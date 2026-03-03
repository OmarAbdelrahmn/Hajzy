using Application.Helpers;
using Domain;
using Domain.Entities;
using Hangfire;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Service.HotelAdmin;

/// <summary>
/// Executed by Hangfire after a hotel admin creates an offer.
/// Fetches all users who previously visited the offer's unit and emails them.
/// </summary>
public class OfferEmailJob(
    ApplicationDbcontext context,
    IEmailSender emailSender,
    ILogger<OfferEmailJob> logger)
{
    private const int BatchSize = 50;

    [Queue("offer-emails")]
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [60, 300, 600])]
    public async Task SendOfferEmailsAsync(int offerId)
    {
        var offer = await context.Offers
            .Include(o => o.Unit)
            .FirstOrDefaultAsync(o => o.Id == offerId);

        if (offer is null)
        {
            logger.LogWarning("OfferEmailJob: offer {Id} not found.", offerId);
            return;
        }

        if (offer.UnitId is null)
        {
            logger.LogWarning("OfferEmailJob: offer {Id} has no unit, skipping.", offerId);
            return;
        }

        var recipients = await BuildRecipientListAsync(offer.UnitId.Value);

        if (recipients.Count == 0)
        {
            logger.LogInformation(
                "OfferEmailJob: no past visitors for unit {UnitId}, offer {OfferId}.",
                offer.UnitId.Value, offerId);
            return;
        }

        logger.LogInformation(
            "OfferEmailJob: sending offer {OfferId} to {Count} past visitors of unit {UnitId}.",
            offerId, recipients.Count, offer.UnitId.Value);

        int sent = 0, failed = 0;

        for (var i = 0; i < recipients.Count; i += BatchSize)
        {
            foreach (var recipient in recipients.Skip(i).Take(BatchSize))
            {
                try
                {
                    await emailSender.SendEmailAsync(
                        recipient.Email,
                        BuildSubject(offer),
                        BuildEmailBody(offer, recipient.FullName));
                    sent++;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        "OfferEmailJob: failed to send to {Email} for offer {OfferId}.",
                        recipient.Email, offerId);
                    failed++;
                }
            }

            if (i + BatchSize < recipients.Count)
                await Task.Delay(TimeSpan.FromSeconds(2));
        }

        logger.LogInformation(
            "OfferEmailJob: offer {OfferId} done. Sent={Sent} Failed={Failed}",
            offerId, sent, failed);
    }

    // ── Recipient builder ─────────────────────────────────────────────────────

    private async Task<List<(string Email, string FullName)>> BuildRecipientListAsync(int unitId)
    {
        var visitorStatuses = new[]
        {
            BookingStatus.Completed,
            BookingStatus.CheckedIn,
            BookingStatus.Confirmed
        };

        return await context.Bookings
            .Where(b => b.UnitId == unitId
                     && b.UserId != null
                     && visitorStatuses.Contains(b.Status)
                     && b.User.Email != null
                     && !b.User.IsDisable)
            .Select(b => new { b.User.Email, FullName = b.User.FullName ?? "Valued Guest" })
            .Distinct()
            .AsNoTracking()
            .ToListAsync()
            .ContinueWith(t => t.Result
                .Select(x => (x.Email!, x.FullName))
                .ToList());
    }

    // ── Email builders ────────────────────────────────────────────────────────

    private static string BuildSubject(Offer offer)
        => $"🎉 Exclusive Offer: {offer.Title} at {offer.Unit?.Name}";

    private static string BuildEmailBody(Offer offer, string recipientName)
    {
        var discountHtml = BuildDiscountBadge(offer);
        var linkHtml = BuildLinkButton(offer);
        var validUntil = offer.EndDate.ToString("MMMM dd, yyyy");

        var placeholders = new Dictionary<string, string>
        {
            { "{{recipient_name}}",    recipientName },
            { "{{offer_title}}",       offer.Title ?? string.Empty },
            { "{{unit_name}}",         offer.Unit?.Name ?? string.Empty },
            { "{{offer_description}}", offer.Description ?? string.Empty },
            { "{{discount_badge}}",    discountHtml },
            { "{{link_button}}",       linkHtml },
            { "{{valid_until}}",       validUntil },
            { "{{year}}",              DateTime.UtcNow.Year.ToString() }
        };

        try
        {
            return EmailBodyBuilder.GenerateEmailBody("OfferEmail", placeholders);
        }
        catch
        {
            return $"""
            <!DOCTYPE html>
            <html>
            <body style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto;padding:20px;">
              <h2 style="color:#2c3e50;">Hi {recipientName},</h2>
              <p style="color:#555;">
                Because you previously stayed at <strong>{offer.Unit?.Name}</strong>,
                we have an exclusive offer just for you!
              </p>
              <h1 style="color:#e74c3c;">{offer.Title}</h1>
              {discountHtml}
              <div style="color:#555;line-height:1.6;margin:16px 0;">{offer.Description}</div>
              {linkHtml}
              <p style="color:#888;font-size:13px;margin-top:24px;">
                Offer valid until <strong>{validUntil}</strong>.
              </p>
              <hr style="margin:30px 0;border:none;border-top:1px solid #eee;" />
              <p style="font-size:12px;color:#aaa;">
                You received this email because you previously stayed with us.<br/>
                © {DateTime.UtcNow.Year} All rights reserved.
              </p>
            </body>
            </html>
            """;
        }
    }

    private static string BuildDiscountBadge(Offer offer)
    {
        if (offer.DiscountPercentage is > 0)
            return $"""
            <div style="display:inline-block;background:#e74c3c;color:#fff;
                        padding:8px 18px;border-radius:20px;font-size:18px;
                        font-weight:bold;margin:12px 0;">
              {offer.DiscountPercentage:0}% OFF
            </div>
            """;

        if (offer.DiscountAmount is > 0)
            return $"""
            <div style="display:inline-block;background:#e74c3c;color:#fff;
                        padding:8px 18px;border-radius:20px;font-size:18px;
                        font-weight:bold;margin:12px 0;">
              Save {offer.DiscountAmount:0.##}
            </div>
            """;

        return string.Empty;
    }

    private static string BuildLinkButton(Offer offer) =>
        string.IsNullOrWhiteSpace(offer.Link) ? string.Empty : $"""
        <div style="text-align:center;margin:28px 0;">
          <a href="{offer.Link}"
             style="background-color:#2c3e50;color:#ffffff;padding:12px 28px;
                    text-decoration:none;border-radius:6px;font-size:15px;
                    font-weight:bold;display:inline-block;">
            View Offer
          </a>
        </div>
        """;
}