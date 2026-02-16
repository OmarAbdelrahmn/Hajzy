using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;



// ===== BASE IMAGE CLASS (Shared properties) =====

public abstract class BaseImage
{
    public int Id { get; set; }

    // S3 Storage Information
    [Required]
    public string ImageUrl { get; set; } = string.Empty; // Full CloudFront URL

    [Required, MaxLength(500)]
    public string S3Key { get; set; } = string.Empty; // S3 object key for operations

    [Required, MaxLength(100)]
    public string S3Bucket { get; set; } = string.Empty;

    public string? CdnUrl { get; set; } // CloudFront URL if different from ImageUrl

    // Image Metadata
    public long FileSizeBytes { get; set; }

    [Required, MaxLength(50)]
    public string MimeType { get; set; } = string.Empty; // image/jpeg, image/png, image/webp

    public int Width { get; set; }
    public int Height { get; set; }

    [MaxLength(100)]
    public string? OriginalFileName { get; set; }

    // Thumbnails (different sizes)
    public string? ThumbnailUrl { get; set; }  // Small preview (150x150)
    public string? ThumbnailS3Key { get; set; }

    public string? MediumUrl { get; set; }      // Standard display (800x600)
    public string? MediumS3Key { get; set; }

    // Display Properties
    public bool IsPrimary { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;

    [MaxLength(500)]
    public string? Caption { get; set; }

    [MaxLength(500)]
    public string? AltText { get; set; } // For SEO & Accessibility

    [MaxLength(200)]
    public string? Title { get; set; }

    // Image Processing Status
    public ImageProcessingStatus ProcessingStatus { get; set; } = ImageProcessingStatus.Pending;
    public string? ProcessingError { get; set; }
    public DateTime? ProcessedAt { get; set; }

    // Tracking & Audit
    [Required]
    public string UploadedByUserId { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Soft Delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedByUserId { get; set; }

    // Analytics (optional)
    public int ViewCount { get; set; } = 0;
    public DateTime? LastViewedAt { get; set; }

    // Navigation (common)
    public ApplicationUser UploadedBy { get; set; } = default!;
    public ApplicationUser? DeletedBy { get; set; }
}

public enum ImageProcessingStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}
