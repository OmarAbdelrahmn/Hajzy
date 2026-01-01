using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Contracts.Unit;

internal class Unit
{
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
    string Type,
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