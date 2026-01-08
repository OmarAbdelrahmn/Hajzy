using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Contracts.Fav;

internal class Fav
{
}

// ============= REQUESTS =============

public record AddFavoriteRequest(
    int? FavId,
    FavoriteType? Type 
);

public record UpdateFavoriteNotesRequest(
    string? Notes
);

public record FavoriteFilter
{
    public FavoriteType? Type { get; init; }
    public int? FavId { get; init; }
    public DateTime? AddedFrom { get; init; }
    public DateTime? AddedTo { get; init; }
    public string? SearchKeyword { get; init; }
    public string? SortBy { get; init; } = "AddedAt"; // AddedAt, Name, Rating
    public string? SortDirection { get; init; } = "DESC";
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

// ============= RESPONSES =============

public record FavoriteResponse
{
    public int Id { get; init; }
    public FavoriteType Type { get; init; }
    public DateTime AddedAt { get; init; }

    // Unit Information (if Type = Unit)
    public int? FavId { get; init; }
    public string? UnitName { get; init; }
    public string? UnitAddress { get; init; }
    public decimal? UnitBasePrice { get; init; }
    public decimal? UnitAverageRating { get; init; }
    public int? UnitTotalReviews { get; init; }
    public string? UnitPrimaryImageUrl { get; init; }
    public bool? UnitIsActive { get; init; }
    public bool? UnitIsVerified { get; init; }
    public string? DepartmentName { get; init; }
    public string? UnitTypeName { get; init; }

    // SubUnit Information (if Type = SubUnit)
    public string? SubUnitRoomNumber { get; init; }
    public string? SubUnitType { get; init; }
    public decimal? SubUnitPricePerNight { get; init; }
    public int? SubUnitMaxOccupancy { get; init; }
    public bool? SubUnitIsAvailable { get; init; }
    public string? SubUnitPrimaryImageUrl { get; init; }

}

public record FavoriteDetailsResponse : FavoriteResponse
{
    public List<string>? UnitImageUrls { get; init; }
    public List<string>? SubUnitImageUrls { get; init; }
    public List<AmenityInfo>? Amenities { get; init; }
    public string? Description { get; init; }
}

public record FavoriteStatisticsResponse(
    int TotalFavorites,
    int UnitFavorites,
    int SubUnitFavorites,
    int FavoritesThisWeek,
    int FavoritesThisMonth,
    Dictionary<string, int> FavoritesByDepartment,
    Dictionary<string, int> FavoritesByUnitType,
    List<TopFavoriteItem> TopFavoriteUnits
);

public record TopFavoriteItem(
    int ItemId,
    string ItemName,
    FavoriteType Type,
    int FavoriteCount
);

public record AmenityInfo(
    int Id,
    string Name,
    string Category,
    bool IsAvailable
);

public record ToggleFavoriteResponse(
    bool IsFavorited,
    string Message
);