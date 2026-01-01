using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.Contracts.Aminety;

internal class Aminety
{
}


// ============= REQUESTS =============

public record UpdateUnitAmenitiesRequest
{
    [Required]
    public List<int> AmenityIds { get; init; } = new();
}

public record UpdateSubUnitAmenitiesRequest
{
    [Required]
    public List<int> AmenityIds { get; init; } = new();
}

// ============= RESPONSES =============

public record AmenityResponse(
    int Id,
    string Name,
    string? Description,
    string Category,
    bool IsAvailable = true
);


// Application/Contracts/Amenity/CreateAmenityRequest.cs

public record CreateAmenityRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Category { get; init; } = string.Empty;
}

// Application/Contracts/Amenity/UpdateAmenityRequest.cs

public record UpdateAmenityRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Category { get; init; }
}

// Application/Contracts/Amenity/UpdateUnitAmenitiesRequest.cs


// Application/Contracts/Amenity/UpdateSubUnitAmenitiesRequest.cs


// Application/Contracts/Amenity/AttachAmenityRequest.cs

public record AttachAmenityRequest
{
    public int AmenityId { get; init; }
}

// Application/Contracts/Amenity/AttachAmenitiesRequest.cs

public record AttachAmenitiesRequest
{
    public List<int> AmenityIds { get; init; } = new();
}

// Application/Contracts/Amenity/AmenityFilter.cs

public record AmenityFilter
{
    public string? Category { get; init; }
    public string? SearchTerm { get; init; }
    public bool? IsAvailable { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

// Application/Contracts/Amenity/AmenityDetailsResponse.cs

public record AmenityDetailsResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Category { get; init; } = string.Empty;
    public int TotalUnitsUsing { get; init; }
    public int TotalSubUnitsUsing { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}