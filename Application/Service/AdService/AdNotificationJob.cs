using Domain;
using Domain.Entities;
using Hangfire;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Service.AdService;

public class AdNotificationJob(
    ApplicationDbcontext context,
    IEmailSender emailSender,
    ILogger<AdNotificationJob> logger)
{
    private const int BatchSize = 50;

    [Queue("ad-notifications")]
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [60, 300, 600])]
    public async Task SendAdNotificationAsync(
        int adId,
        int? filterCityId,
        int? filterUnitId)
    {
        var ad = await context.Set<Ad>()
            .FirstOrDefaultAsync(a => a.Id == adId);

        if (ad is null)
        {
            logger.LogWarning("AdNotificationJob: ad {Id} not found.", adId);
            return;
        }

        var emails = await BuildTargetedEmailListAsync(
            filterCityId,
            filterUnitId
            );

        if (emails.Count == 0)
        {
            logger.LogInformation("AdNotificationJob: no recipients found for ad {Id}.", adId);
            return;
        }

        logger.LogInformation(
            "AdNotificationJob: sending ad {Id} notification to {Count} recipients.",
            adId, emails.Count);

        for (var i = 0; i < emails.Count; i += BatchSize)
        {
            var batch = emails.Skip(i).Take(BatchSize);

            foreach (var recipient in batch)
            {
                try
                {
                    var body = BuildEmailBody(ad, recipient.UnsubscribeToken);

                    await emailSender.SendEmailAsync(
                        recipient.Email,
                        $"New Ad: {ad.Title}",
                        body);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        "AdNotificationJob: failed to send to {Email} for ad {Id}",
                        recipient.Email, adId);
                }
            }

            if (i + BatchSize < emails.Count)
                await Task.Delay(TimeSpan.FromSeconds(2));
        }

        logger.LogInformation("AdNotificationJob: ad {Id} notifications done.", adId);
    }

    // ── Build recipient list (same logic as NewsletterJob) ───────────────────

    private async Task<List<(string Email, string UnsubscribeToken)>> BuildTargetedEmailListAsync(
        int? filterCityId,
        int? filterUnitId)
    {
        var query = context.NewsletterSubscribers
            .Where(s => s.IsActive)
            .AsQueryable();


        // Users who booked in the given city
        if (filterCityId.HasValue)
        {
            var userIdsInCity = await context.Bookings
                .Where(b => b.Unit.CityId == filterCityId.Value && b.UserId != null)
                .Select(b => b.UserId)
                .Distinct()
                .ToListAsync();

            query = query.Where(s => s.UserId != null
                                  && userIdsInCity.Contains(s.UserId!));
        }

        // Users who booked the given unit
        if (filterUnitId.HasValue)
        {
            var userIdsAtUnit = await context.Bookings
                .Where(b => b.UnitId == filterUnitId.Value && b.UserId != null)
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

    // ── Email body ────────────────────────────────────────────────────────────

    private static string BuildEmailBody(Ad ad, string unsubscribeToken)
    {
        return $"""
            <!DOCTYPE html>
            <html>
            <body style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto;padding:20px;">
              <h1 style="color:#2c3e50;">{ad.Title}</h1>
              <div style="color:#555;line-height:1.6;">{ad.Description}</div>
              {BuildLinkButton(ad.Link)}
              <hr style="margin:30px 0;border:none;border-top:1px solid #eee;" />
              <p style="font-size:12px;color:#aaa;">
                You are receiving this because you subscribed to our newsletter.<br/>
                <a href="https://hujjzy.com/newsletter/unsubscribe/{unsubscribeToken}"
                   style="color:#aaa;">Unsubscribe</a>
              </p>
            </body>
            </html>
            """;
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
                  View Ad
                </a>
              </div>
              """;
}