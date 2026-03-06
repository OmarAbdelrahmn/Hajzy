using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Package
{
    public int Id { get; set; }

    public int UnitId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public decimal Price { get; set; }

    // Stored as JSON: ["Feature A", "Feature B"]
    public string FeaturesJson { get; set; } = "[]";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Unit Unit { get; set; } = default!;
}