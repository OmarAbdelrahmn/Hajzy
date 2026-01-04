using Application.Contracts.Unit;
using Domain;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.Contracts.SubUnit;

internal class SubUnit
{
}



// ============= REQUESTS =============

public record CreateSubUnitRequest
{
    [Required]
    public int UnitId { get; init; }

    [Required, MaxLength(100)]
    public string RoomNumber { get; init; } = string.Empty;

    [Required]
    public SubUnitType Type { get; init; }

    [Required]
    public decimal PricePerNight { get; init; }

    [Required]
    public int MaxOccupancy { get; init; }

    public int? Bedrooms { get; init; }
    public int? Bathrooms { get; init; }
    public decimal? Size { get; init; }

    [MaxLength(500)]
    public string? Description { get; init; }

    public bool IsAvailable { get; init; } = true;
}

public record UpdateSubUnitRequest
{
    public string? RoomNumber { get; init; }
    public SubUnitType? Type { get; init; }
    public decimal? PricePerNight { get; init; }
    public int? MaxOccupancy { get; init; }
    public int? Bedrooms { get; init; }
    public int? Bathrooms { get; init; }
    public decimal? Size { get; init; }
    public string? Description { get; init; }
    public bool? IsAvailable { get; init; }
}

public record SetAvailabilityRequest
{
    [Required]
    public DateTime StartDate { get; init; }

    [Required]
    public DateTime EndDate { get; init; }

    [Required]
    public bool IsAvailable { get; init; }

    public UnavailabilityReason? Reason { get; init; }
    public decimal? SpecialPrice { get; init; }
    public decimal? WeekendPrice { get; init; }
    public string? UpdatedByUserId { get; init; }
}

public record SetSpecialPricingRequestsss
{
    [Required]
    public DateTime StartDate { get; init; }

    [Required]
    public DateTime EndDate { get; init; }

    public decimal? SpecialPrice { get; init; }
    public decimal? WeekendPrice { get; init; }
    public string? UpdatedByUserId { get; init; }
}

// ============= RESPONSES =============

public class SubUnitResponse
{
    public int Id { get; init; }
    public int UnitId { get; init; }
    public string UnitName { get; init; } = string.Empty;
    public string RoomNumber { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public decimal PricePerNight { get; init; }
    public int MaxOccupancy { get; init; }
    public int? Bedrooms { get; init; }
    public int? Bathrooms { get; init; }
    public decimal? Size { get; init; }
    public string? Description { get; init; }
    public bool IsAvailable { get; init; }
    public string? PrimaryImageUrl { get; init; }
}

public class SubUnitDetailsResponse
{
    public int Id { get; init; }
    public int UnitId { get; init; }
    public string UnitName { get; init; } = string.Empty;
    public string RoomNumber { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public decimal PricePerNight { get; init; }
    public int MaxOccupancy { get; init; }
    public int? Bedrooms { get; init; }
    public int? Bathrooms { get; init; }
    public decimal? Size { get; init; }
    public string? Description { get; init; }
    public bool IsAvailable { get; init; }
    public List<SubUnitImageResponse> Images { get; init; } = new();
    public List<AmenityInfo> Amenities { get; init; } = new();
}

public class SubUnitImageResponse
{
    public int Id { get; init; }
    public string ImageUrl { get; init; } = string.Empty;
    public string? ThumbnailUrl { get; init; }
    public bool IsPrimary { get; init; }
    public int DisplayOrder { get; init; }
}
public class SubUnitImageResponses
{
    public int Id { get; init; }
    public string ImageUrl { get; init; } = string.Empty;
    public string? ThumbnailUrl { get; init; }
    public string? MediumUrl { get; init; }
    public bool IsPrimary { get; init; }
    public int DisplayOrder { get; init; }
    public string Caption { get; init; }
}

public record AvailabilityResponse(
    int Id,
    DateTime StartDate,
    DateTime EndDate,
    bool IsAvailable,
    string? Reason,
    decimal? SpecialPrice,
    decimal? WeekendPrice
);
