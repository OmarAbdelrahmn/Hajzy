using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Contracts.publicuser;

  internal class publicuser
{
}

// Application/Contracts/Public/PublicContracts.cs


public record PublicUnitResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }

    public string CityName { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string UnitTypeName { get; init; } = string.Empty;

    public decimal BasePrice { get; init; }
    public int? MaxGuests { get; init; }
    public int? Bedrooms { get; init; }
    public int? Bathrooms { get; init; }

    public decimal AverageRating { get; init; }
    public int TotalReviews { get; init; }

    public string? PrimaryImageUrl { get; init; }
    public bool IsAvailable { get; init; }
    public bool IsFeatured { get; init; }
    public List<string> Options { get; init; } = new();
    public string Currency { get; init; } = "SAR";
    public List<PublicCustomPolicyInfo> CustomPolicies { get; init; } = new();
}

public record PublicUnitDetailsResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }

    public PublicCityInfo City { get; init; } = default!;
    public string UnitTypeName { get; init; } = string.Empty;

    public decimal BasePrice { get; init; }
    public int? MaxGuests { get; init; }
    public int? Bedrooms { get; init; }
    public int? Bathrooms { get; init; }

    public decimal AverageRating { get; init; }
    public int TotalReviews { get; init; }

    public List<PublicImageInfo> Images { get; init; } = new();
    public List<PublicAmenityInfo> Amenities { get; init; } = new();
    public List<PublicSubUnitSummary> SubUnits { get; init; } = new();
    public List<PublicReviewSummary> RecentReviews { get; init; } = new();

    public PublicCancellationPolicy? CancellationPolicy { get; init; }
    public List<PublicPolicyInfo> Policies { get; init; } = new();
}

public record PublicUnitDetailsResponses
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public bool IsStandaloneUnit { get; init; } 

    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }

    public PublicCityInfo City { get; init; } = default!;
    public string UnitTypeName { get; init; } = string.Empty;

    public decimal BasePrice { get; init; }
    public int? MaxGuests { get; init; }
    public int? Bedrooms { get; init; }
    public int? Bathrooms { get; init; }

    public decimal AverageRating { get; init; }
    public int TotalReviews { get; init; }

    public List<PublicImageInfo> Images { get; init; } = new();
    public List<PublicAmenityInfo> Amenities { get; init; } = new();
    public List<PublicSubUnitSummary> SubUnits { get; init; } = new();
    public List<PublicReviewSummary> RecentReviews { get; init; } = new();

    public PublicCancellationPolicy? CancellationPolicy { get; init; }
    public List<PublicPolicyInfo> Policies { get; init; } = new();
    public List<string> Options { get; init; } = new();
    public string Currency { get; init; } = "SAR";
    public List<PublicCustomPolicyInfo> CustomPolicies { get; init; } = new();
}
public record PublicCustomPolicyInfo
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Category { get; init; }
}

// ============= SUBUNIT RESPONSES =============

public record PublicSubUnitDetailsResponse
{
    public int Id { get; init; }
    public int UnitId { get; init; }
    public string UnitName { get; init; } = string.Empty;
    public string RoomNumber { get; init; } = string.Empty;
    public int TypeId { get; init; }
    public decimal PricePerNight { get; init; }
    public int MaxOccupancy { get; init; }
    public int? Bedrooms { get; init; }
    public int? Bathrooms { get; init; }
    public decimal? Size { get; init; }
    public string? Description { get; init; }
    public bool IsAvailable { get; init; }

    public List<PublicImageInfo> Images { get; init; } = new();
    public List<PublicAmenityInfo> Amenities { get; init; } = new();
}

// ============= CITY/DEPARTMENT RESPONSES =============

public record PublicCityResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public int TotalUnits { get; init; }
    public decimal? AverageRating { get; init; }
}

// ============= SUPPORTING TYPES =============

public record PublicCityInfo(
    int Id,
    string Name,
    string Country,
    string? Description,
    decimal Latitude,
    decimal Longitude
);

public record PublicImageInfo
{
    public string ImageUrl { get; init; } = string.Empty;
    public string? ThumbnailUrl { get; init; }
    public string? MediumUrl { get; init; }
    public bool IsPrimary { get; init; }
    public int DisplayOrder { get; init; }
    public string? Caption { get; init; }
}

public record PublicAmenityInfo(
    string Name,
    string? Description,
    string Category
);

public record PublicSubUnitSummary(
    int Id,
    string RoomNumber,
    int TypeId,
    decimal PricePerNight,
    int MaxOccupancy,
    bool IsAvailable,
    string? PrimaryImageUrl
);

public record PublicReviewSummary
{
    public int Rating { get; init; }
    public string? Comment { get; init; }
    public DateTime CreatedAt { get; init; }
    public string ReviewerName { get; init; } = "Anonymous"; // Never expose real names
    public List<string> ImageUrls { get; init; } = new();
}

public record PublicCancellationPolicy(
    string Name,
    string Description,
    int FullRefundDays,
    int PartialRefundDays,
    decimal PartialRefundPercentage
);

public record PublicPolicyInfo(
    string Title,
    string Description,
    string PolicyType,
    bool IsMandatory
);

// ============= FILTER/SEARCH REQUESTS =============

public record PublicUnitFilter
{
    public string? CityName { get; init; }
    public string? Country { get; init; }
    public int? CityId { get; init; }
    public int? UnitTypeId { get; init; }

    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public decimal? MinRating { get; init; }
    public int? MinGuests { get; init; }

    public DateTime? CheckIn { get; init; }
    public DateTime? CheckOut { get; init; }

    public List<string>? Amenities { get; init; }

    public string? SortBy { get; init; } = "Name";
    public string? SortDirection { get; init; } = "ASC";

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public record PublicSearchRequest
{
    public string Keyword { get; init; } = string.Empty;
    public int? CityId { get; init; }
    public DateTime? CheckIn { get; init; }
    public DateTime? CheckOut { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

// ============= FAVORITES (for non-authenticated users) =============

public record AddToFavoritesRequest
{
    public int UnitId { get; init; }
    public string? SessionId { get; init; } // For tracking anonymous users
}

public record FavoritesResponse
{
    public List<PublicUnitResponse> Units { get; init; } = new();
    public int TotalCount { get; init; }
}

// ============= AVAILABILITY CHECK =============

public record CheckAvailabilityRequest
{
    public int UnitId { get; init; }
    public DateTime CheckIn { get; init; }
    public DateTime CheckOut { get; init; }
    public int NumberOfGuests { get; init; }
}

public record AvailabilityCheckResponse
{
    public bool IsAvailable { get; init; }
    public int AvailableRooms { get; init; }
    public decimal EstimatedPrice { get; init; }
    public List<PublicSubUnitSummary> AvailableSubUnits { get; init; } = new();
}

// ============= FEATURED UNITS =============

public record FeaturedUnitsResponse
{
    public List<PublicUnitResponse> Units { get; init; } = new();
    public string Criteria { get; init; } = string.Empty; // "Top Rated", "Most Booked", etc.
}