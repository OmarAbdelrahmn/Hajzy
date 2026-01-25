using Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.Contracts.UnitRegisteration;

internal class UnitRegisteration
{
}

/// <summary>
/// Anonymous user submits this to register a new unit
/// </summary>
public record SubmitUnitRegistrationRequest
{
    // Owner Information
    [Required, MaxLength(200)]
    public string OwnerFullName { get; init; } = string.Empty;

    [Required, EmailAddress]
    public string OwnerEmail { get; init; } = string.Empty;

    [Required, Phone]
    public string OwnerPhoneNumber { get; init; } = string.Empty;

    [Required, MinLength(8)]
    public string OwnerPassword { get; init; } = string.Empty;

    // Unit Information
    [Required, MaxLength(200)]
    public string UnitName { get; init; } = string.Empty;

    [Required, MinLength(50)]
    public string Description { get; init; } = string.Empty;

    [Required, MaxLength(500)]
    public string Address { get; init; } = string.Empty;

    [Required]
    public int DepartmentId { get; init; }

    [Required]
    public int UnitTypeId { get; init; }

    [Range(-90, 90)]
    public decimal Latitude { get; init; }

    [Range(-180, 180)]
    public decimal Longitude { get; init; }

    [Range(0, double.MaxValue)]
    public decimal BasePrice { get; init; }

    [Range(1, 1000)]
    public int? MaxGuests { get; init; }

    [Range(0, 100)]
    public int? Bedrooms { get; init; }

    [Range(0, 100)]
    public int? Bathrooms { get; init; }

}

// ============= RESPONSES =============

public record UnitRegistrationResponse
{
    public int Id { get; init; }
    public RegistrationRequestStatus Status { get; init; }
    public string StatusDisplay { get; init; } = string.Empty;

    // Owner Info
    public string OwnerFullName { get; init; } = string.Empty;
    public string OwnerEmail { get; init; } = string.Empty;
    public string OwnerPhoneNumber { get; init; } = string.Empty;

    // Unit Info
    public string UnitName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public int DepartmentId { get; init; }
    public string DepartmentName { get; init; } = string.Empty;
    public int UnitTypeId { get; init; }
    public string UnitTypeName { get; init; } = string.Empty;
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public decimal BasePrice { get; init; }
    public int? MaxGuests { get; init; }
    public int? Bedrooms { get; init; }
    public int? Bathrooms { get; init; }

    // Images
    public List<string> ImageUrls { get; init; } = new();
    public int ImageCount { get; init; }

    // Tracking
    public DateTime SubmittedAt { get; init; }
    public DateTime? ReviewedAt { get; init; }
    public string? ReviewedByAdminName { get; init; }
    public string? RejectionReason { get; init; }

    // After Approval
    public string? CreatedUserId { get; init; }
    public int? CreatedUnitId { get; init; }
}

// ============= ADMIN ACTIONS =============

public record ReviewUnitRegistrationRequest
{
    [Required]
    public int RequestId { get; init; }

    [Required]
    public bool Approve { get; init; }

    public string? RejectionReason { get; init; }
}

public record UnitRegistrationListFilter
{
    public RegistrationRequestStatus? Status { get; init; }
    public int? DepartmentId { get; init; }
    public int? UnitTypeId { get; init; }
    public DateTime? SubmittedFrom { get; init; }
    public DateTime? SubmittedTo { get; init; }
    public string? SearchKeyword { get; init; }

    // Pagination
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

// ============= STATISTICS =============

public record UnitRegistrationStatistics
{
    public int TotalRequests { get; init; }
    public int PendingRequests { get; init; }
    public int UnderReviewRequests { get; init; }
    public int ApprovedRequests { get; init; }
    public int RejectedRequests { get; init; }
    public int CancelledRequests { get; init; }

    public int RequestsThisWeek { get; init; }
    public int RequestsThisMonth { get; init; }

    public Dictionary<string, int> RequestsByDepartment { get; init; } = new();
    public Dictionary<string, int> RequestsByUnitType { get; init; } = new();
}

// ============= ENUMS =============


public record ApprovalResult
{
    public string CreatedUserId { get; init; } = string.Empty;
    public string CreatedUserEmail { get; init; } = string.Empty;
    public int CreatedUnitId { get; init; }
    public string CreatedUnitName { get; init; } = string.Empty;
    public bool EmailSent { get; init; }
}


public record DAUnitRegistrationResponse
{
    public int Id { get; init; }
    public RegistrationRequestStatus Status { get; init; }
    public string StatusDisplay { get; init; } = string.Empty;

    // Owner Info
    public string OwnerFullName { get; init; } = string.Empty;
    public string OwnerEmail { get; init; } = string.Empty;
    public string OwnerPhoneNumber { get; init; } = string.Empty;

    // Unit Info
    public string UnitName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string DepartmentName { get; init; } = string.Empty;
    public int UnitTypeId { get; init; }
    public string UnitTypeName { get; init; } = string.Empty;
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public decimal BasePrice { get; init; }
    public int? MaxGuests { get; init; }
    public int? Bedrooms { get; init; }
    public int? Bathrooms { get; init; }

    // Images
    public List<string> ImageUrls { get; init; } = new();
    public int ImageCount { get; init; }

    // Tracking
    public DateTime SubmittedAt { get; init; }
    public DateTime? ReviewedAt { get; init; }
    public string? ReviewedByAdminName { get; init; }
    public string? RejectionReason { get; init; }

    // After Approval
    public string? CreatedUserId { get; init; }
    public int? CreatedUnitId { get; init; }
}

public record DAUnitRegistrationListFilter
{
    public RegistrationRequestStatus? Status { get; init; }
    public int? UnitTypeId { get; init; }
    public DateTime? SubmittedFrom { get; init; }
    public DateTime? SubmittedTo { get; init; }
    public string? SearchKeyword { get; init; }

    // Pagination
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public record DAapprovalResult
{
    public string CreatedUserId { get; init; } = string.Empty;
    public string CreatedUserEmail { get; init; } = string.Empty;
    public int CreatedUnitId { get; init; }
    public string CreatedUnitName { get; init; } = string.Empty;
    public bool EmailSent { get; init; }
}

public record DAUnitRegistrationStatistics
{
    public int TotalRequests { get; init; }
    public int PendingRequests { get; init; }
    public int UnderReviewRequests { get; init; }
    public int ApprovedRequests { get; init; }
    public int RejectedRequests { get; init; }
    public int CancelledRequests { get; init; }

    public int RequestsThisWeek { get; init; }
    public int RequestsThisMonth { get; init; }

    public Dictionary<string, int> RequestsByDepartment { get; init; } = new();
    public Dictionary<string, int> RequestsByUnitType { get; init; } = new();
}