using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Entities;

public class UnitRegistrationRequest
{
    public int Id { get; set; }

    // Request Status
    public RegistrationRequestStatus Status { get; set; } = RegistrationRequestStatus.Pending;

    [MaxLength(1000)]
    public string? RejectionReason { get; set; }

    // Owner Contact Information
    [Required, MaxLength(200)]
    public string OwnerFullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(256)]
    public string OwnerEmail { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string OwnerPhoneNumber { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string OwnerPassword { get; set; } = string.Empty; // Hashed before storing

    // Unit Information
    [Required, MaxLength(200)]
    public string UnitName { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    [Required]
    public int DepartmentId { get; set; }

    [Required]
    public int UnitTypeId { get; set; }

    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }

    public decimal BasePrice { get; set; }

    public int? MaxGuests { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }

    // Images - Stored as JSON array of S3 keys
    [Required]
    public string ImageS3Keys { get; set; } = "[]"; // JSON array: ["temp/request-123/img1.jpg", ...]

    public int ImageCount { get; set; }

    // Tracking
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedByAdminId { get; set; }

    // After Approval
    public string? CreatedUserId { get; set; }
    public int? CreatedUnitId { get; set; }

    // Navigation
    public Department Department { get; set; } = default!;
    public UnitType UnitType { get; set; } = default!;
    public ApplicationUser? ReviewedByAdmin { get; set; }
    public ApplicationUser? CreatedUser { get; set; }
    public Unit? CreatedUnit { get; set; }
}

public enum RegistrationRequestStatus
{
    Pending = 1,
    UnderReview = 2,
    Approved = 3,
    Rejected = 4,
    Cancelled = 5
}
