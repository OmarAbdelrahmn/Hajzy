using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

/// <summary>
/// Represents a configurable currency managed by platform admins.
/// Replaces the static <see cref="PriceCurrency"/> enum so names, codes,
/// and symbols can be maintained at runtime without a code deploy.
/// </summary>
public class Currency
{
    public int Id { get; set; }

    /// <summary>ISO 4217 code, e.g. "USD", "SAR", "YER".</summary>
    [Required, MaxLength(10)]
    public string Code { get; set; } = string.Empty;

    /// <summary>English display name, e.g. "US Dollar".</summary>
    [Required, MaxLength(100)]
    public string NameEnglish { get; set; } = string.Empty;

    /// <summary>Arabic display name, e.g. "دولار أمريكي".</summary>
    [Required, MaxLength(100)]
    public string NameArabic { get; set; } = string.Empty;

    /// <summary>Symbol shown in UI, e.g. "$", "﷼", "€".</summary>
    [Required, MaxLength(10)]
    public string Symbol { get; set; } = string.Empty;

    /// <summary>Display order in drop-downs (lower = higher priority).</summary>
    public int DisplayOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    /// <summary>Only one currency may be the platform default.</summary>
    public bool IsDefault { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation — units that use this currency
    public ICollection<Unit> Units { get; set; } = [];
}