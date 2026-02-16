using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.Contracts.Unit;

internal class Unit
{
}

// Add these to your Application/Contracts/Unit folder


/// <summary>
/// Comprehensive unit response with all related data
/// </summary>
public class UnitComprehensiveResponse
{
    // Basic Info
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }

    // Location
    public int CityId { get; set; }
    public string CityName { get; set; } = string.Empty;
    public string CityCountry { get; set; } = string.Empty;

    // Type
    public int UnitTypeId { get; set; }
    public string UnitTypeName { get; set; } = string.Empty;
    public string? UnitTypeDescription { get; set; }

    // Pricing & Capacity
    public decimal BasePrice { get; set; }
    public int? MaxGuests { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }

    // Status
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public bool IsFeatuered { get; set; }

    // Ratings
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }

    // Admins
    public List<UnitAdminInfo> Admins { get; set; } = new();

    // Images
    public List<UnitImageDetail> Images { get; set; } = new();

    // Amenities
    public List<AmenityDetail> Amenities { get; set; } = new();

    // Policies
    public List<GeneralPolicyDetail> GeneralPolicies { get; set; } = new();
    public CancellationPolicyDetail? CancellationPolicy { get; set; }

    // SubUnits
    public List<SubUnitComprehensiveDetail> SubUnits { get; set; } = new();
    public int TotalSubUnits { get; set; }
    public int AvailableSubUnits { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<string> Options { get; init; } = new();
    public string Currency { get; init; } = "SAR";
    public List<CustomPolicyDetail> CustomPolicies { get; init; } = new();

    // ... rest of existing properties ...
}

public record CustomPolicyDetail
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Category { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
}

/// <summary>
/// Detailed image information
/// </summary>
public class UnitImageDetail
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? MediumUrl { get; set; }
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
    public string? Caption { get; set; }
    public string? AltText { get; set; }
    public string ImageType { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public long FileSizeBytes { get; set; }
}

/// <summary>
/// Detailed amenity information
/// </summary>
public class AmenityDetail
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
}

/// <summary>
/// General policy details
/// </summary>
public class GeneralPolicyDetail
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PolicyType { get; set; } = string.Empty;
    public string? PolicyCategory { get; set; }
    public string? CustomPolicyName { get; set; }
    public bool IsMandatory { get; set; }
    public bool IsHighlighted { get; set; }
    public string? CancellationPolicyName { get; set; }
}

/// <summary>
/// Cancellation policy details
/// </summary>
public class CancellationPolicyDetail
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int FullRefundDays { get; set; }
    public int PartialRefundDays { get; set; }
    public decimal PartialRefundPercentage { get; set; }
}

/// <summary>
/// Comprehensive SubUnit details
/// </summary>
public class SubUnitComprehensiveDetail
{
    public int Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int  TypeId { get; set; }
    public decimal PricePerNight { get; set; }
    public int MaxOccupancy { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public decimal? Size { get; set; }
    public string? Description { get; set; }
    public bool IsAvailable { get; set; }

    // SubUnit Images
    public List<SubUnitImageDetail> Images { get; set; } = new();

    // SubUnit Amenities
    public List<AmenityDetail> Amenities { get; set; } = new();

    // Current Availability Status
    public AvailabilityInfo? CurrentAvailability { get; set; }
}

/// <summary>
/// SubUnit image details
/// </summary>
public class SubUnitImageDetail
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? SmallUrl { get; set; }
    public string? MediumUrl { get; set; }
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
    public string? Caption { get; set; }
    public string ImageType { get; set; } = string.Empty;
}

/// <summary>
/// Availability information
/// </summary>
public class AvailabilityInfo
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsAvailable { get; set; }
    public string? Reason { get; set; }
    public decimal? SpecialPrice { get; set; }
    public decimal? WeekendPrice { get; set; }
}

/// <summary>
/// Lightweight response for list views (original GetAllAsync can use this)
/// </summary>
public class UnitSummaryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string CityName { get; set; } = string.Empty;
    public string UnitTypeName { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public int TotalSubUnits { get; set; }
    public int AvailableSubUnits { get; set; }
}
// ============= REQUESTS =============

public record UpdateUnitRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Address { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public int? CityId { get; init; }
    public int? UnitTypeId { get; init; }
    public decimal? BasePrice { get; init; }
    public int? MaxGuests { get; init; }
    public int? Bedrooms { get; init; }
    public int? Bathrooms { get; init; }
    public bool? IsActive { get; init; }
    [Range(1, 5, ErrorMessage = "Rank must be between 1 and 5 stars")]
    public int? Rank { get; set; }
}

public class UnitFilter
{
    public string? Name { get; init; }
    public int? CityId { get; init; }
    public int? UnitTypeId { get; init; }
    public bool? IsActive { get; init; }
    public bool? IsVerified { get; init; }
    public bool? IsDeleted { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public decimal? MinRating { get; init; }
    public int? MinGuests { get; init; }
    public string? SortBy { get; init; } = "Name";
    public string? SortDirection { get; init; } = "ASC";
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

// ============= RESPONSES =============

public class UnitResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public int CityId { get; init; }
    public string CityName { get; init; } = string.Empty;
    public int UnitTypeId { get; init; }
    public string UnitTypeName { get; init; } = string.Empty;
    public decimal BasePrice { get; init; }
    public int? MaxGuests { get; init; }
    public int? Bedrooms { get; init; }
    public int? Bathrooms { get; init; }
    public bool IsActive { get; init; }
    public bool IsVerified { get; init; }
    public decimal AverageRating { get; init; }
    public int TotalReviews { get; init; }
    public string? PrimaryImageUrl { get; init; }
    public int AdminCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public int? Rank { get; set; }

}

public class UnitResponses
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public int CityId { get; init; }
    public string CityName { get; init; } = string.Empty;
    public int UnitTypeId { get; init; }
    public string UnitTypeName { get; init; } = string.Empty;
    public decimal BasePrice { get; init; }
    public int? MaxGuests { get; init; }
    public int? Bedrooms { get; init; }
    public int? Bathrooms { get; init; }
    public bool IsActive { get; init; }
    public bool IsVerified { get; init; }
    public bool IsStandaloneUnit { get; init; }
    public decimal AverageRating { get; init; }
    public int TotalReviews { get; init; }
    public string? PrimaryImageUrl { get; init; }
    public int AdminCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public class UnitDetailsResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public int CityId { get; init; }
    public string CityName { get; init; } = string.Empty;
    public int UnitTypeId { get; init; }
    public string UnitTypeName { get; init; } = string.Empty;
    public decimal BasePrice { get; init; }
    public int? MaxGuests { get; init; }
    public int? Bedrooms { get; init; }
    public int? Bathrooms { get; init; }
    public bool IsActive { get; init; }
    public bool IsVerified { get; init; }
    public decimal AverageRating { get; init; }
    public int TotalReviews { get; init; }
    public int? CancellationPolicyId { get; init; }
    public string? CancellationPolicyName { get; init; }
    public List<UnitAdminInfo> Admins { get; init; } = new();
    public List<UnitImageResponse> Images { get; init; } = new();
    public List<AmenityInfo> Amenities { get; init; } = new();
    public List<SubUnitSummary> Rooms { get; init; } = new();
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public int? Rank { get; set; }

}

public record UnitAdminInfo(
    string UserId,
    string FullName,
    string Email,
    string? PhoneNumber,
    bool IsActive,
    DateTime AssignedAt
);

public class UnitImageResponse
{
    public int Id { get; init; }
    public string ImageUrl { get; init; } = string.Empty;
    public string? ThumbnailUrl { get; init; }
    public bool IsPrimary { get; init; }
    public int DisplayOrder { get; init; }
    public string? Caption { get; init; }
}

public record AmenityInfo(
    int Id,
    string Name,
    string Category,
    bool IsAvailable
);

public record SubUnitSummary(
    int Id,
    string RoomNumber,
    int TypeId,
    decimal PricePerNight,
    int MaxOccupancy,
    bool IsAvailable
);

public record UnitAdminsResponse(
    int UnitId,
    string UnitName,
    int TotalAdmins,
    List<UnitAdminInfo> Admins
);

public record UnitStatisticsResponse(
    int UnitId,
    string UnitName,
    int TotalBookings,
    int CompletedBookings,
    decimal TotalRevenue,
    decimal AverageRating,
    int TotalReviews,
    int TotalRooms,
    decimal OccupancyRate
);