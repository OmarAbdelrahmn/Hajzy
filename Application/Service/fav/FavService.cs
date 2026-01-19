using Application.Abstraction;
using Application.Contracts.Fav;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Service.fav;

public class FavService(
    ApplicationDbcontext context,
    ILogger<FavService> logger) : IFavService
{
    private readonly ApplicationDbcontext _context = context;
    private readonly ILogger<FavService> _logger = logger;

    #region CRUD Operations

    public async Task<Result<FavoriteResponse>> AddFavoriteAsync(
        string userId,
        AddFavoriteRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            UserFavorite favorite;

            if (request.Type == FavoriteType.Unit)
            {
                var unitExists = await _context.Units
                    .AnyAsync(u => u.Id == request.FavId!.Value && !u.IsDeleted);

                if (!unitExists)
                    return Result.Failure<FavoriteResponse>(
                        new Error("NotFound", "Unit not found", 404));

                var existing = await _context.Set<UserFavorite>()
                    .FirstOrDefaultAsync(f => f.UserId == userId &&
                                             f.FavId == request.FavId.Value &&
                                             f.Type == FavoriteType.Unit);

                if (existing != null)
                    return Result.Failure<FavoriteResponse>(
                        new Error("AlreadyFavorited", "Unit is already in favorites", 400));

                favorite = new UserFavorite
                {
                    UserId = userId,
                    FavId = request.FavId.Value,
                    Type = FavoriteType.Unit,
                    AddedAt = DateTime.UtcNow.AddHours(3)
                };
            }
            else
            {
                var subUnitExists = await _context.SubUnits
                    .AnyAsync(s => s.Id == request.FavId!.Value && !s.IsDeleted);

                if (!subUnitExists)
                    return Result.Failure<FavoriteResponse>(
                        new Error("NotFound", "SubUnit not found", 404));

                var existing = await _context.Set<UserFavorite>()
                    .FirstOrDefaultAsync(f => f.UserId == userId &&
                                             f.FavId == request.FavId.Value &&
                                             f.Type == FavoriteType.SubUnit);

                if (existing != null)
                    return Result.Failure<FavoriteResponse>(
                        new Error("AlreadyFavorited", "SubUnit is already in favorites", 400));

                favorite = new UserFavorite
                {
                    UserId = userId,
                    FavId = request.FavId.Value,
                    Type = FavoriteType.SubUnit,
                    AddedAt = DateTime.UtcNow.AddHours(3)
                };
            }

            await _context.Set<UserFavorite>().AddAsync(favorite);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "User {UserId} added {Type} favorite: {ItemId}",
                userId, favorite.Type, favorite.FavId);

            var response = await GetFavoriteResponseAsync(favorite.Id);
            return Result.Success(response!);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error adding favorite for user {UserId}", userId);
            return Result.Failure<FavoriteResponse>(
                new Error("AddFailed", "Failed to add favorite", 500));
        }
    }

    public async Task<Result> RemoveFavoriteAsync(string userId, int favoriteId)
    {
        try
        {
            var favorite = await _context.Set<UserFavorite>()
                .FirstOrDefaultAsync(f => f.Id == favoriteId && f.UserId == userId);

            if (favorite == null)
                return Result.Failure(
                    new Error("NotFound", "Favorite not found", 404));

            _context.Set<UserFavorite>().Remove(favorite);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "User {UserId} removed favorite {FavoriteId}",
                userId, favoriteId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing favorite {FavoriteId}", favoriteId);
            return Result.Failure(
                new Error("RemoveFailed", "Failed to remove favorite", 500));
        }
    }

    #endregion

    #region Query Operations

    public async Task<Result<List<FavoriteResponse>>> GetUserFavoritesAsync(
        string userId, FavoriteFilter filter)
    {
        try
        {
            var query = _context.Set<UserFavorite>()
                .Where(f => f.UserId == userId)
                .AsQueryable();

            // Apply type filter
            if (filter.Type.HasValue)
                query = query.Where(f => f.Type == filter.Type.Value);

            var favorites = await query
                .OrderByDescending(f => f.AddedAt)
                .ToListAsync();

            // Separate unit and subunit IDs
            var unitIds = favorites
                .Where(f => f.Type == FavoriteType.Unit && f.FavId != null)
                .Select(f => f.FavId!)
                .Distinct()
                .ToList();

            var subUnitIds = favorites
                .Where(f => f.Type == FavoriteType.SubUnit && f.FavId != null)
                .Select(f => f.FavId!)
                .Distinct()
                .ToList();

            // Load Units with their data
            Dictionary<int,Domain.Entities.Unit> unitsDict = new();
            if (unitIds.Any())
            {
                var units = await _context.Units
                    .Include(u => u.City)
                    .Include(u => u.UnitType)
                    .Include(u => u.Images.Where(i => !i.IsDeleted && i.IsPrimary))
                    .Where(u => unitIds.Contains(u.Id))
                    .AsNoTracking()
                    .ToListAsync();

                unitsDict = units.ToDictionary(u => u.Id);
            }

            // Load SubUnits with their data
            Dictionary<int, Domain.Entities.SubUnit> subUnitsDict = new();
            if (subUnitIds.Any())
            {
                var subUnits = await _context.SubUnits
                    .Include(s => s.Unit)
                        .ThenInclude(u => u.City)
                    .Include(s => s.SubUnitType)
                    .Include(s => s.SubUnitImages.Where(i => !i.IsDeleted && i.IsPrimary))
                    .Where(s => subUnitIds.Contains(s.Id))
                    .AsNoTracking()
                    .ToListAsync();

                subUnitsDict = subUnits.ToDictionary(s => s.Id);
            }

            // Map to responses
            var responses = new List<FavoriteResponse>();
            foreach (var fav in favorites)
            {
                if (fav.Type == FavoriteType.Unit && fav.FavId != 0)
                {
                    if (unitsDict.TryGetValue(fav.FavId, out var unit))
                    {
                        fav.Unit = unit;
                        responses.Add(MapToResponse(fav));
                    }
                }
                else if (fav.Type == FavoriteType.SubUnit && fav.FavId !=0)
                {
                    if (subUnitsDict.TryGetValue(fav.FavId, out var subUnit))
                    {
                        fav.SubUnit = subUnit;
                        responses.Add(MapToResponse(fav));
                    }
                }
            }

            return Result.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user favorites for user {UserId}", userId);
            return Result.Failure<List<FavoriteResponse>>(
                new Error("QueryFailed", "Failed to get favorites", 500));
        }
    }

    public async Task<Result<FavoriteDetailsResponse>> GetFavoriteDetailsAsync(
        string userId,
        int favoriteId)
    {
        try
        {
            var favorite = await _context.Set<UserFavorite>()
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == favoriteId && f.UserId == userId);

            if (favorite == null)
                return Result.Failure<FavoriteDetailsResponse>(
                    new Error("NotFound", "Favorite not found", 404));

            // Load the appropriate entity with all details
            if (favorite.Type == FavoriteType.Unit && favorite.FavId != 0)
            {
                favorite.Unit = await _context.Units
                    .Include(u => u.City)
                    .Include(u => u.UnitType)
                    .Include(u => u.Images.Where(i => !i.IsDeleted))
                    .Include(u => u.UnitAmenities)
                        .ThenInclude(ua => ua.Amenity)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == favorite.FavId);
            }
            else if (favorite.Type == FavoriteType.SubUnit && favorite.FavId != 0)
            {
                favorite.SubUnit = await _context.SubUnits
                    .Include(s => s.Unit)
                        .ThenInclude(u => u.City)
                    .Include(s => s.SubUnitType)
                    .Include(s => s.SubUnitImages.Where(i => !i.IsDeleted))
                    .Include(s => s.SubUnitAmenities)
                        .ThenInclude(sa => sa.Amenity)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == favorite.FavId);
            }

            var response = MapToDetailsResponse(favorite);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting favorite details for favorite {FavoriteId}", favoriteId);
            return Result.Failure<FavoriteDetailsResponse>(
                new Error("QueryFailed", "Failed to get favorite details", 500));
        }
    }

    #endregion

    #region Validation Operations

    public async Task<Result<int>> GetUnitFavoriteCountAsync(int unitId)
    {
        try
        {
            var count = await _context.Set<UserFavorite>()
                .CountAsync(f => f.FavId == unitId && f.Type == FavoriteType.Unit);

            return Result.Success(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unit favorite count for unit {UnitId}", unitId);
            return Result.Failure<int>(new Error("CountFailed", "Failed to get favorite count", 500));
        }
    }

    public async Task<Result<int>> GetSubUnitFavoriteCountAsync(int subUnitId)
    {
        try
        {
            var count = await _context.Set<UserFavorite>()
                .CountAsync(f => f.FavId == subUnitId && f.Type == FavoriteType.SubUnit);

            return Result.Success(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subunit favorite count for subunit {SubUnitId}", subUnitId);
            return Result.Failure<int>(new Error("CountFailed", "Failed to get favorite count", 500));
        }
    }

    #endregion

    #region Statistics Operations

    public async Task<Result<FavoriteStatisticsResponse>> GetUserStatisticsAsync(string userId)
    {
        try
        {
            var favorites = await _context.Set<UserFavorite>()
                .Where(f => f.UserId == userId)
                .AsNoTracking()
                .ToListAsync();

            var now = DateTime.UtcNow;
            var weekAgo = now.AddDays(-7);
            var monthAgo = now.AddMonths(-1);

            // Load necessary data for statistics
            var unitIds = favorites
                .Where(f => f.Type == FavoriteType.Unit && f.FavId !=0)
                .Select(f => f.FavId!)
                .ToList();

            var subUnitIds = favorites
                .Where(f => f.Type == FavoriteType.SubUnit && f.FavId !=0)
                .Select(f => f.FavId!)
                .ToList();

            var units = unitIds.Any() ? await _context.Units
                .Include(u => u.City)
                .Include(u => u.UnitType)
                .Where(u => unitIds.Contains(u.Id))
                .AsNoTracking()
                .ToListAsync() : new List<Domain.Entities.Unit>();

            var subUnits = subUnitIds.Any() ? await _context.SubUnits
                .Include(s => s.Unit)
                    .ThenInclude(u => u.City)
                .Include(s => s.Unit)
                    .ThenInclude(u => u.UnitType)
                .Where(s => subUnitIds.Contains(s.Id))
                .AsNoTracking()
                .ToListAsync() : new List<Domain.Entities.SubUnit>();

            var favoritesByDepartment = new Dictionary<string, int>();
            var favoritesByUnitType = new Dictionary<string, int>();

            // Aggregate units
            foreach (var unit in units)
            {
                var deptName = unit.City?.Name ?? "Unknown";
                var typeName = unit.UnitType?.Name ?? "Unknown";

                favoritesByDepartment[deptName] = favoritesByDepartment.GetValueOrDefault(deptName) + 1;
                favoritesByUnitType[typeName] = favoritesByUnitType.GetValueOrDefault(typeName) + 1;
            }

            // Aggregate subunits
            foreach (var subUnit in subUnits)
            {
                var deptName = subUnit.Unit?.City?.Name ?? "Unknown";
                var typeName = subUnit.Unit?.UnitType?.Name ?? "Unknown";

                favoritesByDepartment[deptName] = favoritesByDepartment.GetValueOrDefault(deptName) + 1;
                favoritesByUnitType[typeName] = favoritesByUnitType.GetValueOrDefault(typeName) + 1;
            }

            var stats = new FavoriteStatisticsResponse(
                TotalFavorites: favorites.Count,
                UnitFavorites: favorites.Count(f => f.Type == FavoriteType.Unit),
                SubUnitFavorites: favorites.Count(f => f.Type == FavoriteType.SubUnit),
                FavoritesThisWeek: favorites.Count(f => f.AddedAt >= weekAgo),
                FavoritesThisMonth: favorites.Count(f => f.AddedAt >= monthAgo),
                FavoritesByDepartment: favoritesByDepartment,
                FavoritesByUnitType: favoritesByUnitType,
                TopFavoriteUnits: new List<TopFavoriteItem>()
            );

            return Result.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user statistics for user {UserId}", userId);
            return Result.Failure<FavoriteStatisticsResponse>(
                new Error("StatsFailed", "Failed to get statistics", 500));
        }
    }

    public async Task<Result<FavoriteStatisticsResponse>> GetGlobalStatisticsAsync()
    {
        try
        {
            var favorites = await _context.Set<UserFavorite>()
                .AsNoTracking()
                .ToListAsync();

            var now = DateTime.UtcNow;
            var weekAgo = now.AddDays(-7);
            var monthAgo = now.AddMonths(-1);

            // Get top favorited units
            var topUnits = await _context.Set<UserFavorite>()
                .Where(f => f.Type == FavoriteType.Unit && f.FavId != 0)
                .GroupBy(f => f.FavId!)
                .Select(g => new { UnitId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            var topUnitIds = topUnits.Select(t => t.UnitId).ToList();
            var topUnitDetails = await _context.Units
                .Where(u => topUnitIds.Contains(u.Id))
                .AsNoTracking()
                .ToDictionaryAsync(u => u.Id);

            var topFavoriteUnits = topUnits
                .Where(t => topUnitDetails.ContainsKey(t.UnitId))
                .Select(t => new TopFavoriteItem(
                    t.UnitId,
                    topUnitDetails[t.UnitId].Name,
                    FavoriteType.Unit,
                    t.Count))
                .ToList();

            // Calculate statistics
            var unitIds = favorites
                .Where(f => f.Type == FavoriteType.Unit && f.FavId != 0)
                .Select(f => f.FavId!)
                .Distinct()
                .ToList();

            var subUnitIds = favorites
                .Where(f => f.Type == FavoriteType.SubUnit && f.FavId != 0)
                .Select(f => f.FavId!)
                .Distinct()
                .ToList();

            var units = unitIds.Any() ? await _context.Units
                .Include(u => u.City)
                .Include(u => u.UnitType)
                .Where(u => unitIds.Contains(u.Id))
                .AsNoTracking()
                .ToListAsync() : new List<Domain.Entities.Unit>();

            var subUnits = subUnitIds.Any() ? await _context.SubUnits
                .Include(s => s.Unit)
                    .ThenInclude(u => u.City)
                .Include(s => s.Unit)
                    .ThenInclude(u => u.UnitType)
                .Where(s => subUnitIds.Contains(s.Id))
                .AsNoTracking()
                .ToListAsync() : new List<Domain.Entities.SubUnit>();

            var favoritesByDepartment = new Dictionary<string, int>();
            var favoritesByUnitType = new Dictionary<string, int>();

            foreach (var unit in units)
            {
                var favCount = favorites.Count(f => f.Type == FavoriteType.Unit && f.FavId == unit.Id);
                var deptName = unit.City?.Name ?? "Unknown";
                var typeName = unit.UnitType?.Name ?? "Unknown";

                favoritesByDepartment[deptName] = favoritesByDepartment.GetValueOrDefault(deptName) + favCount;
                favoritesByUnitType[typeName] = favoritesByUnitType.GetValueOrDefault(typeName) + favCount;
            }

            foreach (var subUnit in subUnits)
            {
                var favCount = favorites.Count(f => f.Type == FavoriteType.SubUnit && f.FavId == subUnit.Id);
                var deptName = subUnit.Unit?.City?.Name ?? "Unknown";
                var typeName = subUnit.Unit?.UnitType?.Name ?? "Unknown";

                favoritesByDepartment[deptName] = favoritesByDepartment.GetValueOrDefault(deptName) + favCount;
                favoritesByUnitType[typeName] = favoritesByUnitType.GetValueOrDefault(typeName) + favCount;
            }

            var stats = new FavoriteStatisticsResponse(
                TotalFavorites: favorites.Count,
                UnitFavorites: favorites.Count(f => f.Type == FavoriteType.Unit),
                SubUnitFavorites: favorites.Count(f => f.Type == FavoriteType.SubUnit),
                FavoritesThisWeek: favorites.Count(f => f.AddedAt >= weekAgo),
                FavoritesThisMonth: favorites.Count(f => f.AddedAt >= monthAgo),
                FavoritesByDepartment: favoritesByDepartment,
                FavoritesByUnitType: favoritesByUnitType,
                TopFavoriteUnits: topFavoriteUnits
            );

            return Result.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting global statistics");
            return Result.Failure<FavoriteStatisticsResponse>(
                new Error("StatsFailed", "Failed to get statistics", 500));
        }
    }

    #endregion

    #region Bulk Operations

    public async Task<Result> ClearAllFavoritesAsync(string userId)
    {
        try
        {
            var favorites = await _context.Set<UserFavorite>()
                .Where(f => f.UserId == userId)
                .ToListAsync();

            if (!favorites.Any())
                return Result.Success();

            _context.Set<UserFavorite>().RemoveRange(favorites);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Cleared all {Count} favorites for user {UserId}",
                favorites.Count, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing all favorites for user {UserId}", userId);
            return Result.Failure(
                new Error("ClearFailed", "Failed to clear favorites", 500));
        }
    }

    #endregion

    #region Helper Methods

    private async Task<FavoriteResponse?> GetFavoriteResponseAsync(int favoriteId)
    {
        try
        {
            var favorite = await _context.Set<UserFavorite>()
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == favoriteId);

            if (favorite == null)
                return null;

            if (favorite.Type == FavoriteType.Unit && favorite.FavId != 0)
            {
                favorite.Unit = await _context.Units
                    .Include(u => u.City)
                    .Include(u => u.UnitType)
                    .Include(u => u.Images.Where(i => !i.IsDeleted && i.IsPrimary))
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == favorite.FavId);
            }
            else if (favorite.Type == FavoriteType.SubUnit && favorite.FavId != 0)
            {
                favorite.SubUnit = await _context.SubUnits
                    .Include(s => s.Unit)
                        .ThenInclude(u => u.City)
                    .Include(s => s.SubUnitType)
                    .Include(s => s.SubUnitImages.Where(i => !i.IsDeleted && i.IsPrimary))
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == favorite.FavId);
            }

            return MapToResponse(favorite);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting favorite response for favorite {FavoriteId}", favoriteId);
            return null;
        }
    }

    private static FavoriteResponse MapToResponse(UserFavorite favorite)
    {
        return new FavoriteResponse
        {
            Id = favorite.Id,
            Type = favorite.Type,
            AddedAt = favorite.AddedAt,
            FavId = favorite.FavId,

            // Unit info
            UnitName = favorite.Unit?.Name,
            UnitAddress = favorite.Unit?.Address,
            UnitBasePrice = favorite.Unit?.BasePrice,
            UnitAverageRating = favorite.Unit?.AverageRating,
            UnitTotalReviews = favorite.Unit?.TotalReviews,
            UnitPrimaryImageUrl = favorite.Unit?.Images?.FirstOrDefault()?.ImageUrl,
            UnitIsActive = favorite.Unit?.IsActive,
            UnitIsVerified = favorite.Unit?.IsVerified,
            DepartmentName = favorite.Unit?.City?.Name,
            UnitTypeName = favorite.Unit?.UnitType?.Name,

            // SubUnit info
            SubUnitRoomNumber = favorite.SubUnit?.RoomNumber,
            SubUnitType = favorite.SubUnit?.SubUnitType?.Name,
            SubUnitPricePerNight = favorite.SubUnit?.PricePerNight,
            SubUnitMaxOccupancy = favorite.SubUnit?.MaxOccupancy,
            SubUnitIsAvailable = favorite.SubUnit?.IsAvailable,
            SubUnitPrimaryImageUrl = favorite.SubUnit?.SubUnitImages?.FirstOrDefault()?.ImageUrl,
        };
    }

    private static FavoriteDetailsResponse MapToDetailsResponse(UserFavorite favorite)
    {
        var baseResponse = MapToResponse(favorite);

        return new FavoriteDetailsResponse
        {
            Id = baseResponse.Id,
            Type = baseResponse.Type,
            AddedAt = baseResponse.AddedAt,
            FavId = baseResponse.FavId,
            UnitName = baseResponse.UnitName,
            UnitAddress = baseResponse.UnitAddress,
            UnitBasePrice = baseResponse.UnitBasePrice,
            UnitAverageRating = baseResponse.UnitAverageRating,
            UnitTotalReviews = baseResponse.UnitTotalReviews,
            UnitPrimaryImageUrl = baseResponse.UnitPrimaryImageUrl,
            UnitIsActive = baseResponse.UnitIsActive,
            UnitIsVerified = baseResponse.UnitIsVerified,
            DepartmentName = baseResponse.DepartmentName,
            UnitTypeName = baseResponse.UnitTypeName,
            SubUnitRoomNumber = baseResponse.SubUnitRoomNumber,
            SubUnitType = baseResponse.SubUnitType,
            SubUnitPricePerNight = baseResponse.SubUnitPricePerNight,
            SubUnitMaxOccupancy = baseResponse.SubUnitMaxOccupancy,
            SubUnitIsAvailable = baseResponse.SubUnitIsAvailable,
            SubUnitPrimaryImageUrl = baseResponse.SubUnitPrimaryImageUrl,

            // Additional details
            UnitImageUrls = favorite.Unit?.Images?
                .OrderBy(i => i.DisplayOrder)
                .Select(i => i.ImageUrl)
                .ToList(),

            SubUnitImageUrls = favorite.SubUnit?.SubUnitImages?
                .OrderBy(i => i.DisplayOrder)
                .Select(i => i.ImageUrl)
                .ToList(),

            Amenities = (favorite.Type == FavoriteType.Unit
                ? favorite.Unit?.UnitAmenities?.Select(ua => new AmenityInfo(
                    ua.AmenityId,
                    ua.Amenity?.Name ?? "Unknown",
                    ua.Amenity?.Category ?? "Unknown",
                    ua.IsAvailable))
                : favorite.SubUnit?.SubUnitAmenities?.Select(sa => new AmenityInfo(
                    sa.AmenityId,
                    sa.Amenity?.Name ?? "Unknown",
                    sa.Amenity?.Category ?? "Unknown",
                    sa.IsAvailable)))?.ToList(),

            Description = favorite.Type == FavoriteType.Unit
                ? favorite.Unit?.Description
                : favorite.SubUnit?.Description
        };
    }

    #endregion
}