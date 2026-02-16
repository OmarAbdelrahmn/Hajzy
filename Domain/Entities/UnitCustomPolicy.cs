// Domain/Entities/UnitCustomPolicy.cs
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class UnitCustomPolicy
{
    public int Id { get; set; }

    public int UnitId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Category { get; set; } // e.g., "House Rules", "Check-in", "Parking"

    public int DisplayOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    [Required]
    public string CreatedByUserId { get; set; } = string.Empty;

    // Navigation
    public Unit Unit { get; set; } = default!;
    public ApplicationUser CreatedBy { get; set; } = default!;
}