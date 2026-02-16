using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class LoyaltyProgram
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public int TotalPoints { get; set; }
    public int LifetimePoints { get; set; }

    public LoyaltyTier Tier { get; set; } = LoyaltyTier.Bronze;

    public DateTime? TierAchievedAt { get; set; }
    public DateTime? NextTierEligibleAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ApplicationUser User { get; set; } = default!;
    public ICollection<LoyaltyTransaction> Transactions { get; set; } = [];
}