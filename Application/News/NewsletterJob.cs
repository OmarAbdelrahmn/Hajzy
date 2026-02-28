using Application.Helpers;
using Domain;
using Domain.Entities;
using Hangfire;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Newsletter;

/// <summary>
/// Executed by Hangfire. Fetches all active subscribers in batches and sends
/// the newsletter email for each one, logging success / failure per recipient.
/// </summary>
public class NewsletterJob(
    ApplicationDbcontext context,
    IEmailSender emailSender,
    ILogger<NewsletterJob> logger)
{
    private const int BatchSize = 50; // emails per mini-batch

    [Queue("newsletters")]
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [60, 300, 600])]
    public async Task SendCampaignAsync(int campaignId)
    {
        var campaign = await context.NewsletterCampaigns
            .FirstOrDefaultAsync(c => c.Id == campaignId);

        if (campaign is null)
        {
            logger.LogWarning("NewsletterJob: campaign {Id} not found.", campaignId);
            return;
        }

        campaign.Status = NewsletterCampaignStatus.Sending;
        await context.SaveChangesAsync();

        // ── Collect all active subscriber emails ─────────────────────────────
        var emails = await BuildTargetedEmailListAsync(campaign);


        campaign.TotalRecipients = emails.Count;
        await context.SaveChangesAsync();

        logger.LogInformation(
            "NewsletterJob: sending campaign {Id} to {Count} subscribers.",
            campaignId, emails.Count);

        int sent = 0, failed = 0;

        // ── Send in batches to avoid hammering the SMTP server ───────────────
        for (var i = 0; i < emails.Count; i += BatchSize)
        {
            var batch = emails.Skip(i).Take(BatchSize);
            var logs = new List<NewsletterSendLog>(BatchSize);

            foreach (var recipient in batch)
            {
                try
                {
                    var body = BuildEmailBody(campaign, recipient.UnsubscribeToken);

                    await emailSender.SendEmailAsync(
                        recipient.Email,
                        campaign.Title,
                        body);

                    logs.Add(new NewsletterSendLog
                    {
                        CampaignId = campaignId,
                        Email = recipient.Email,
                        IsSuccess = true
                    });
                    sent++;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        "NewsletterJob: failed to send to {Email} for campaign {Id}",
                        recipient.Email, campaignId);

                    logs.Add(new NewsletterSendLog
                    {
                        CampaignId = campaignId,
                        Email = recipient.Email,
                        IsSuccess = false,
                        ErrorMessage = ex.Message[..Math.Min(ex.Message.Length, 1000)]
                    });
                    failed++;
                }
            }

            // Persist the batch logs and update running counters
            context.NewsletterSendLogs.AddRange(logs);
            campaign.SentCount = sent;
            campaign.FailedCount = failed;
            await context.SaveChangesAsync();

            // Small delay between batches to be kind to the mail server
            if (i + BatchSize < emails.Count)
                await Task.Delay(TimeSpan.FromSeconds(2));
        }

        // ── Finalise ─────────────────────────────────────────────────────────
        campaign.Status = failed == emails.Count && emails.Count > 0
            ? NewsletterCampaignStatus.Failed
            : NewsletterCampaignStatus.Completed;

        campaign.CompletedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        logger.LogInformation(
            "NewsletterJob: campaign {Id} finished. Sent={Sent} Failed={Failed}",
            campaignId, sent, failed);
    }
    private async Task<List<(string Email, string UnsubscribeToken)>> BuildTargetedEmailListAsync(
    NewsletterCampaign campaign)
    {
        // Start from active subscribers only
        var query = context.NewsletterSubscribers
            .Where(s => s.IsActive)
            .AsQueryable();

        // Filter: registered users only
        if (campaign.FilterRegisteredUsersOnly == true)
            query = query.Where(s => s.UserId != null);

        // Filter: subscribed/joined within date range
        if (campaign.FilterFromDate.HasValue)
            query = query.Where(s => s.SubscribedAt >= campaign.FilterFromDate.Value);

        if (campaign.FilterToDate.HasValue)
            query = query.Where(s => s.SubscribedAt <= campaign.FilterToDate.Value);

        // Filter: users who have a booking at a unit in the given city
        if (campaign.FilterCityId.HasValue)
        {
            var userIdsInCity = await context.Bookings
                .Where(b => b.Unit.CityId == campaign.FilterCityId.Value
                         && b.UserId != null)
                .Select(b => b.UserId)
                .Distinct()
                .ToListAsync();

            query = query.Where(s => s.UserId != null
                                  && userIdsInCity.Contains(s.UserId!));
        }

        // Filter: users who have a booking at a specific unit
        if (campaign.FilterUnitId.HasValue)
        {
            var userIdsAtUnit = await context.Bookings
                .Where(b => b.UnitId == campaign.FilterUnitId.Value
                         && b.UserId != null)
                .Select(b => b.UserId)
                .Distinct()
                .ToListAsync();

            query = query.Where(s => s.UserId != null
                                  && userIdsAtUnit.Contains(s.UserId!));
        }

        return await query
            .Select(s => new { s.Email, s.UnsubscribeToken })
            .AsNoTracking()
            .ToListAsync()
            .ContinueWith(t => t.Result
                .Select(x => (x.Email, x.UnsubscribeToken))
                .ToList());
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Email body builder
    // ─────────────────────────────────────────────────────────────────────────
    private static string BuildEmailBody(NewsletterCampaign campaign, string unsubscribeToken)
    {
        var placeholders = new Dictionary<string, string>
    {
        { "{{title}}",             campaign.Title },
        { "{{description}}",       campaign.Description },
        { "{{link_button}}",       BuildLinkButton(campaign.Link) },
        { "{{unsubscribe_token}}", unsubscribeToken },
        { "{{year}}",              DateTime.UtcNow.Year.ToString() }
    };

        try
        {
            return EmailBodyBuilder.GenerateEmailBody("Newsletter", placeholders);
        }
        catch
        {
            return $"""
            <!DOCTYPE html>
            <html>
            <body style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto;padding:20px;">
              <h1 style="color:#2c3e50;">{campaign.Title}</h1>
              <div style="color:#555;line-height:1.6;">{campaign.Description}</div>
              {BuildLinkButton(campaign.Link)}
              <hr style="margin:30px 0;border:none;border-top:1px solid #eee;" />
              <p style="font-size:12px;color:#aaa;">
                You are receiving this email because you subscribed to our newsletter.<br/>
                <a href="https://hujjzy.com/newsletter/unsubscribe/{unsubscribeToken}"
                   style="color:#aaa;">Unsubscribe</a>
              </p>
            </body>
            </html>
            """;
        }
    }

    private static string BuildLinkButton(string? link) =>
        string.IsNullOrWhiteSpace(link)
            ? string.Empty
            : $"""
          <div style="text-align:center;margin:28px 0;">
            <a href="{link}"
               style="background-color:#2c3e50;color:#ffffff;padding:12px 28px;
                      text-decoration:none;border-radius:6px;font-size:15px;
                      font-weight:bold;display:inline-block;">
              Learn More
            </a>
          </div>
          """;
}