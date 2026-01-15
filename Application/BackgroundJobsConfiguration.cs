using Application.Service.AdService;
using Application.Service.OfferService;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application;

public static class BackgroundJobsConfiguration
{
    public static void ConfigureRecurringJobs()
    {
        // Run every hour to deactivate expired ads
        RecurringJob.AddOrUpdate<AdExpirationJob>(
            "deactivate-expired-ads",
            job => job.DeactivateExpiredAds(),
            Cron.Hourly);

        // Run every hour to deactivate expired offers
        RecurringJob.AddOrUpdate<OfferExpirationJob>(
            "deactivate-expired-offers",
            job => job.DeactivateExpiredOffers(),
            Cron.Hourly);
    }
}

public class AdExpirationJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AdExpirationJob> _logger;

    public AdExpirationJob(
        IServiceProvider serviceProvider,
        ILogger<AdExpirationJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DeactivateExpiredAds()
    {
        try
        {
            _logger.LogInformation("Starting expired ads deactivation job");

            using var scope = _serviceProvider.CreateScope();
            var adService = scope.ServiceProvider.GetRequiredService<IAdService>();

            var result = await adService.DeactivateExpiredAdsAsync();

            if (result.IsSuccess)
                _logger.LogInformation("Expired ads deactivation job completed successfully");
            else
                _logger.LogError("Expired ads deactivation job failed: {Error}", result.Error.Description);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in expired ads deactivation job");
        }
    }
}

/// <summary>
/// Background job for deactivating expired offers
/// </summary>
public class OfferExpirationJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OfferExpirationJob> _logger;

    public OfferExpirationJob(
        IServiceProvider serviceProvider,
        ILogger<OfferExpirationJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DeactivateExpiredOffers()
    {
        try
        {
            _logger.LogInformation("Starting expired offers deactivation job");

            using var scope = _serviceProvider.CreateScope();
            var offerService = scope.ServiceProvider.GetRequiredService<IOfferService>();

            var result = await offerService.DeactivateExpiredOffersAsync();

            if (result.IsSuccess)
                _logger.LogInformation("Expired offers deactivation job completed successfully");
            else
                _logger.LogError("Expired offers deactivation job failed: {Error}", result.Error.Description);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in expired offers deactivation job");
        }
    }
}
