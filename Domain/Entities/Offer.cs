using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Offer
{
    public int Id { get; set; }

    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    // Image Storage
    [Required]
    public string? ImageUrl { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string? S3Key { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string S3Bucket { get; set; } = "hujjzy-bucket";


    // Optional Unit Association
    public int? UnitId { get; set; }
    public Unit? Unit { get; set; }

    [MaxLength(2000)]
    public string? Link { get; set; }  
    public bool IsFeatured { get; set; } = false;

    // Date Range
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Discount Information (optional)
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }

    // Status
    public bool IsActive { get; set; } = true;

    // Tracking
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Soft Delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Auto-deactivate if expired
    public bool IsExpired => DateTime.UtcNow > EndDate;
}