using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Entities;


/// <summary>
/// Stores every email that has subscribed to the newsletter.
/// </summary>
public class NewsletterSubscriber
{
    public int Id { get; set; }

    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Populated when the subscriber is a registered user.</summary>
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Token sent in unsubscribe links so no auth is required.</summary>
    [Required, MaxLength(128)]
    public string UnsubscribeToken { get; set; } = Guid.NewGuid().ToString("N");

    [MaxLength(2000)]
    public string? Link { get; set; }

    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UnsubscribedAt { get; set; }
}

/// <summary>
/// One newsletter blast created by an admin.
/// </summary>
public class NewsletterCampaign
{
    public int Id { get; set; }

    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public NewsletterCampaignStatus Status { get; set; } = NewsletterCampaignStatus.Pending;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Set when Hangfire enqueues the send job.</summary>
    public DateTime? QueuedAt { get; set; }

    /// <summary>Set when the last email in the batch is dispatched.</summary>
    public DateTime? CompletedAt { get; set; }

    // Stats
    public int TotalRecipients { get; set; }
    public int SentCount { get; set; }
    public int FailedCount { get; set; }

    /// <summary>Hangfire job ID so the status can be tracked.</summary>
    [MaxLength(128)]
    public string? HangfireJobId { get; set; }

    [MaxLength(2000)]
    public string? Link { get; set; }
    public ICollection<NewsletterSendLog> SendLogs { get; set; } = [];
    // ── Targeting Filters (all nullable = no filter applied) ──────────────
    /// <summary>Only send to subscribers who booked/visited a unit in this city.</summary>
    public int? FilterCityId { get; set; }

    /// <summary>Only send to subscribers who booked/visited this specific unit.</summary>
    public int? FilterUnitId { get; set; }

    /// <summary>Only send to subscribers who joined/booked after this date.</summary>
    public DateTime? FilterFromDate { get; set; }

    /// <summary>Only send to subscribers who joined/booked before this date.</summary>
    public DateTime? FilterToDate { get; set; }

    /// <summary>Only send to subscribers who are also registered users (not anonymous).</summary>
    public bool? FilterRegisteredUsersOnly { get; set; }
}

/// <summary>
/// Per-recipient send record so we can retry failures and track delivery.
/// </summary>
public class NewsletterSendLog
{
    public int Id { get; set; }

    public int CampaignId { get; set; }
    public NewsletterCampaign Campaign { get; set; } = default!;

    [Required, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    public bool IsSuccess { get; set; }

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}

public enum NewsletterCampaignStatus
{
    Pending = 0,
    Queued = 1,
    Sending = 2,
    Completed = 3,
    Failed = 4
}