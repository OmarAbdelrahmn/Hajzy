using Application.Abstraction;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.News;

public interface INewsletterService
{
    /// <summary>
    /// Subscribe an email address to the newsletter.
    /// If the email already exists but was unsubscribed, it is reactivated.
    /// </summary>
    Task<Result> SubscribeAsync(string email, string? userId = null);

    /// <summary>
    /// Unsubscribe via the token that arrives in email links — no auth required.
    /// </summary>
    Task<Result> UnsubscribeAsync(string token);

    /// <summary>
    /// Create a campaign record and enqueue a Hangfire job to send it.
    /// Only admins should call this.
    /// </summary>
    Task<Result<int>> CreateAndQueueCampaignAsync(
        string title,
        string description,
        string adminUserId,
    string? link = null,   // ── new targeting filters ──
    int? filterCityId = null,
    int? filterUnitId = null,
    DateTime? filterFromDate = null,
    DateTime? filterToDate = null,
    bool? filterRegisteredUsersOnly = null);
}


// ── Inbound ──────────────────────────────────────────────────────────────────

public record SubscribeRequest
{
    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; init; } = string.Empty;
}

public record SendNewsletterRequest
{
    [Required, MaxLength(300)]
    public string Title { get; init; } = string.Empty;

    [Required]
    public string Description { get; init; } = string.Empty;

    // NEW
    [MaxLength(2000)]
    public string? Link { get; init; }
    // ── Targeting Filters (all optional) ─────────────────────────────────
    public int? FilterCityId { get; init; }
    public int? FilterUnitId { get; init; }
    public DateTime? FilterFromDate { get; init; }
    public DateTime? FilterToDate { get; init; }
    public bool? FilterRegisteredUsersOnly { get; init; }
}

// ── Outbound ─────────────────────────────────────────────────────────────────

public record CampaignCreatedResponse(
    int CampaignId,
    string Message,
    string HangfireStatus = "Queued"
    ,string? link = null,
    int? FilterCityId = null,
    int? FilterUnitId = null,
    DateTime? FilterFromDate = null,
    DateTime? FilterToDate = null,
    bool? FilterRegisteredUsersOnly = null);