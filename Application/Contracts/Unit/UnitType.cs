using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Contracts.Unit;

internal class UnitType
{
}

// ============= RESPONSE MODELS =============

public record UnitTypeResponse(
    int Id,
    string Name,
    string? Description,
    bool IsActive,
    int TotalUnits
);

public record UnitTypeDetailsResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public int TotalUnits { get; init; }
    public int ActiveUnits { get; init; }
    public int InactiveUnits { get; init; }
    public List<UnitBasicInfo> Units { get; init; } = new();
}

public record UnitBasicInfo(
    int Id,
    string Name,
    bool IsActive,
    bool IsVerified
);

// ============= REQUEST MODELS =============

public record CreateUnitTypeRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; } = true;
}

public record UpdateUnitTypeRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public bool? IsActive { get; init; }
}

public record UnitTypeFilter
{
    public string? Name { get; init; }
    public bool? IsActive { get; init; }
    public int? MinUnits { get; init; }
    public int? MaxUnits { get; init; }

    // Sorting
    public string? SortBy { get; init; } = "Name"; // Name, TotalUnits
    public string? SortDirection { get; init; } = "ASC"; // ASC or DESC

    // Pagination
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

// ============= STATISTICS =============

public record UnitTypeStatisticsResponse(
    int TotalTypes,
    int ActiveTypes,
    int InactiveTypes,
    int TotalUnitsAcrossAllTypes,
    Dictionary<string, int> UnitCountByType
);