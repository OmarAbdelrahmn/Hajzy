using Application.Abstraction;
using Application.Helpers;
using Application.News;
using Domain;
using Domain.Entities;
using Hangfire;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Newsletter;

public class NewsletterService(
    ApplicationDbcontext context,
    IBackgroundJobClient backgroundJobClient,
    ILogger<NewsletterService> logger) : INewsletterService
{
    // ─────────────────────────────────────────────────────────────────────────
    // SUBSCRIBE
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<Result> SubscribeAsync(string email, string? userId = null)
    {
        try
        {
            email = email.Trim().ToLowerInvariant();

            var existing = await context.NewsletterSubscribers
                .FirstOrDefaultAsync(s => s.Email == email);

            if (existing is not null)
            {
                if (existing.IsActive)
                    return Result.Failure(
                        new Error("AlreadySubscribed", "This email is already subscribed.", 409));

                // Re-activate
                existing.IsActive = true;
                existing.UnsubscribedAt = null;
                existing.UnsubscribeToken = Guid.NewGuid().ToString("N");
                existing.SubscribedAt = DateTime.UtcNow;
                if (userId is not null) existing.UserId = userId;

                await context.SaveChangesAsync();
                logger.LogInformation("Re-activated newsletter subscriber {Email}", email);
                return Result.Success();
            }

            var subscriber = new NewsletterSubscriber
            {
                Email = email,
                UserId = userId,
                UnsubscribeToken = Guid.NewGuid().ToString("N")
            };

            context.NewsletterSubscribers.Add(subscriber);
            await context.SaveChangesAsync();

            logger.LogInformation("New newsletter subscriber: {Email}", email);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to subscribe email {Email}", email);
            return Result.Failure(new Error("SubscribeFailed", "Failed to subscribe.", 500));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UNSUBSCRIBE
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<Result> UnsubscribeAsync(string token)
    {
        try
        {
            var subscriber = await context.NewsletterSubscribers
                .FirstOrDefaultAsync(s => s.UnsubscribeToken == token && s.IsActive);

            if (subscriber is null)
                return Result.Failure(
                    new Error("NotFound", "Invalid or already used unsubscribe token.", 404));

            subscriber.IsActive = false;
            subscriber.UnsubscribedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            logger.LogInformation("Unsubscribed {Email}", subscriber.Email);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to unsubscribe token {Token}", token);
            return Result.Failure(new Error("UnsubscribeFailed", "Failed to unsubscribe.", 500));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CREATE & QUEUE CAMPAIGN
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<Result<int>> CreateAndQueueCampaignAsync(
        string title,
        string description,
        string adminUserId)
    {
        try
        {
            var campaign = new NewsletterCampaign
            {
                Title = title,
                Description = description,
                CreatedByUserId = adminUserId,
                Status = NewsletterCampaignStatus.Queued,
                QueuedAt = DateTime.UtcNow
            };

            context.NewsletterCampaigns.Add(campaign);
            await context.SaveChangesAsync();

            // Enqueue the actual sending to Hangfire
            var jobId = backgroundJobClient.Enqueue<NewsletterJob>(
                j => j.SendCampaignAsync(campaign.Id));

            campaign.HangfireJobId = jobId;
            await context.SaveChangesAsync();

            logger.LogInformation(
                "Newsletter campaign {CampaignId} queued. Hangfire job: {JobId}",
                campaign.Id, jobId);

            return Result.Success(campaign.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create newsletter campaign");
            return Result.Failure<int>(
                new Error("CampaignFailed", "Failed to create campaign.", 500));
        }
    }
}