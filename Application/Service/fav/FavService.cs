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
                // Validate unit exists
                var unitExists = await _context.Units
                    .AnyAsync(u => u.Id == request.FavId!.Value && !u.IsDeleted);

                if (!unitExists)
                    return Result.Failure<FavoriteResponse>(
                        new Error("NotFound", "Unit not found", 404));

                // Check if already favorited
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
            else // SubUnit
            {
                // Validate subunit exists
                var subUnitExists = await _context.SubUnits
                    .AnyAsync(s => s.Id == request.FavId!.Value && !s.IsDeleted);

                if (!subUnitExists)
                    return Result.Failure<FavoriteResponse>(
                        new Error("NotFound", "SubUnit not found", 404));

                // Check if already favorited
                var existing = await _context.Set<UserFavorite>()
                    .FirstOrDefaultAsync(f => f.UserId == userId &&
                                             f.FavId == request.FavId!.Value &&
                                             f.Type == FavoriteType.SubUnit);

                if (existing != null)
                    return Result.Failure<FavoriteResponse>(
                        new Error("AlreadyFavorited", "SubUnit is already in favorites", 400));

                favorite = new UserFavorite
                {
                    UserId = userId,
                    FavId = request.FavId!.Value,
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

            // Reload with navigation properties
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
                .FirstOrDefaultAsync(f => f.Id == favoriteId);

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
            var favoritesQuery = _context.Set<UserFavorite>()
                .Where(f => f.UserId == userId);

            // Apply type filter
            if (filter.Type.HasValue)
                favoritesQuery = favoritesQuery.Where(f => f.Type == filter.Type.Value);

            var favorites = await favoritesQuery.ToListAsync();


            // Load Units with their related data
            var unitFavorites = favorites.Where(f => f.FavId != null && f.Type == FavoriteType.Unit).ToList();
            if (unitFavorites.Any())
            {
                var unitIds = unitFavorites.Select(f => f.FavId).Distinct().ToList();
                var units = await _context.Set<Domain.Entities.Unit>()
                    .Include(u => u.City)
                    .Include(u => u.UnitType)
                    .Include(u => u.Images.Where(i => !i.IsDeleted && i.IsPrimary))
                    .Where(u => unitIds.Contains(u.Id))
                    .AsNoTracking()
                    .ToListAsync();

                // Map units back to favorites
                foreach (var fav in unitFavorites)
                {
                    fav.Unit = units.FirstOrDefault(u => u.Id == fav.FavId);
                }
            }

            // Load SubUnits with their related data
            var subUnitFavorites = favorites.Where(f => f.FavId != null && f.Type == FavoriteType.SubUnit).ToList();
            if (subUnitFavorites.Any())
            {
                var subUnitIds = subUnitFavorites.Select(f => f.FavId).Distinct().ToList();
                var subUnits = await _context.Set<Domain.Entities.SubUnit>()
                    .Include(s => s.Unit)
                    .Include(s => s.SubUnitImages.Where(i => !i.IsDeleted && i.IsPrimary))
                    .Where(s => subUnitIds.Contains(s.Id))
                    .AsNoTracking()
                    .ToListAsync();

                // Map subunits back to favorites
                foreach (var fav in subUnitFavorites)
                {
                    fav.SubUnit = subUnits.FirstOrDefault(s => s.Id == fav.FavId);
                }
            }

            var responses = favorites.Select(MapToResponse).ToList();

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
                .Include(f => f.Unit)
                    .ThenInclude(u => u!.City)
                .Include(f => f.Unit)
                    .ThenInclude(u => u!.UnitType)
                .Include(f => f.Unit)
                    .ThenInclude(u => u!.Images.Where(i => !i.IsDeleted))
                .Include(f => f.Unit)
                    .ThenInclude(u => u!.UnitAmenities)
                    .ThenInclude(ua => ua.Amenity)
                .Include(f => f.SubUnit)
                    .ThenInclude(s => s!.Unit)
                .Include(f => f.SubUnit)
                    .ThenInclude(s => s!.SubUnitImages.Where(i => !i.IsDeleted))
                .Include(f => f.SubUnit)
                    .ThenInclude(s => s!.SubUnitAmenities)
                    .ThenInclude(sa => sa.Amenity)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == favoriteId &&
                                         f.UserId == userId);

            if (favorite == null)
                return Result.Failure<FavoriteDetailsResponse>(
                    new Error("NotFound", "Favorite not found", 404));

            var response = MapToDetailsResponse(favorite);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting favorite details");
            return Result.Failure<FavoriteDetailsResponse>(
                new Error("QueryFailed", "Failed to get favorite details", 500));
        }
    }

    #endregion

    #region Validation Operations

    public async Task<Result<int>> GetUnitFavoriteCountAsync(int unitId)
    {
        var count = await _context.Set<UserFavorite>()
            .CountAsync(f => f.FavId == unitId && f.Type == FavoriteType.Unit);

        return Result.Success(count);
    }

    public async Task<Result<int>> GetSubUnitFavoriteCountAsync(int subUnitId)
    {
        var count = await _context.Set<UserFavorite>()
            .CountAsync(f => f.FavId == subUnitId && f.Type == FavoriteType.SubUnit);

        return Result.Success(count);
    }

    #endregion

    #region Statistics Operations

    public async Task<Result<FavoriteStatisticsResponse>> GetUserStatisticsAsync(string userId)
    {
        try
        {
            var favorites = await _context.Set<UserFavorite>()
                .Include(f => f.Unit)
                    .ThenInclude(u => u!.City)
                .Include(f => f.Unit)
                    .ThenInclude(u => u!.UnitType)
                .Include(f => f.SubUnit)
                    .ThenInclude(s => s!.Unit)
                    .ThenInclude(u => u.City)
                .Where(f => f.UserId == userId)
                .AsNoTracking()
                .ToListAsync();

            var now = DateTime.UtcNow;
            var weekAgo = now.AddDays(-7);
            var monthAgo = now.AddMonths(-1);

            var stats = new FavoriteStatisticsResponse(
                TotalFavorites: favorites.Count,
                UnitFavorites: favorites.Count(f => f.Type == FavoriteType.Unit),
                SubUnitFavorites: favorites.Count(f => f.Type == FavoriteType.SubUnit),
                FavoritesThisWeek: favorites.Count(f => f.AddedAt >= weekAgo),
                FavoritesThisMonth: favorites.Count(f => f.AddedAt >= monthAgo),
                FavoritesByDepartment: favorites
                    .GroupBy(f => f.Type == FavoriteType.Unit
                        ? f.Unit!.City.Name
                        : f.SubUnit!.Unit.City.Name)
                    .ToDictionary(g => g.Key, g => g.Count()),
                FavoritesByUnitType: favorites
                    .GroupBy(f => f.Type == FavoriteType.Unit
                        ? f.Unit!.UnitType.Name
                        : f.SubUnit!.Unit.UnitType.Name)
                    .ToDictionary(g => g.Key, g => g.Count()),
                TopFavoriteUnits: new List<TopFavoriteItem>()
            );

            return Result.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user statistics");
            return Result.Failure<FavoriteStatisticsResponse>(
                new Error("StatsFailed", "Failed to get statistics", 500));
        }
    }

    public async Task<Result<FavoriteStatisticsResponse>> GetGlobalStatisticsAsync()
    {
        try
        {
            var favorites = await _context.Set<UserFavorite>()
                .Include(f => f.Unit)
                    .ThenInclude(u => u!.City)
                .Include(f => f.Unit)
                    .ThenInclude(u => u!.UnitType)
                .Include(f => f.SubUnit)
                    .ThenInclude(s => s!.Unit)
                    .ThenInclude(u => u.City)
                .AsNoTracking()
                .ToListAsync();

            var now = DateTime.UtcNow;
            var weekAgo = now.AddDays(-7);
            var monthAgo = now.AddMonths(-1);

            // Get top favorited units
            var topUnits = await _context.Set<UserFavorite>()
                .Where(f => f.Type == FavoriteType.Unit)
                .GroupBy(f => new { f.FavId, f.Unit!.Name })
                .Select(g => new TopFavoriteItem(
                    g.Key.FavId,
                    g.Key.Name,
                    FavoriteType.Unit,
                    g.Count()))
                .OrderByDescending(t => t.FavoriteCount)
                .Take(10)
                .ToListAsync();

            var stats = new FavoriteStatisticsResponse(
                TotalFavorites: favorites.Count,
                UnitFavorites: favorites.Count(f => f.Type == FavoriteType.Unit),
                SubUnitFavorites: favorites.Count(f => f.Type == FavoriteType.SubUnit),
                FavoritesThisWeek: favorites.Count(f => f.AddedAt >= weekAgo),
                FavoritesThisMonth: favorites.Count(f => f.AddedAt >= monthAgo),
                FavoritesByDepartment: favorites
                    .GroupBy(f => f.Type == FavoriteType.Unit
                        ? f.Unit!.City.Name
                        : f.SubUnit!.Unit.City.Name)
                    .ToDictionary(g => g.Key, g => g.Count()),
                FavoritesByUnitType: favorites
                    .GroupBy(f => f.Type == FavoriteType.Unit
                        ? f.Unit!.UnitType.Name
                        : f.SubUnit!.Unit.UnitType.Name)
                    .ToDictionary(g => g.Key, g => g.Count()),
                TopFavoriteUnits: topUnits
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
            _logger.LogError(ex, "Error clearing all favorites");
            return Result.Failure(
                new Error("ClearFailed", "Failed to clear favorites", 500));
        }
    }

    #endregion

    #region Helper Methods

    private async Task<FavoriteResponse?> GetFavoriteResponseAsync(int favoriteId)
    {
        var favorite = await _context.Set<UserFavorite>()
            .Include(f => f.Unit)
                .ThenInclude(u => u!.City)
            .Include(f => f.Unit)
                .ThenInclude(u => u!.UnitType)
            .Include(f => f.Unit)
                .ThenInclude(u => u!.Images.Where(i => !i.IsDeleted && i.IsPrimary))
            .Include(f => f.SubUnit)
                .ThenInclude(s => s!.Unit)
            .Include(f => f.SubUnit)
                .ThenInclude(s => s!.SubUnitImages.Where(i => !i.IsDeleted && i.IsPrimary))
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == favoriteId);

        return favorite != null ? MapToResponse(favorite) : null;
    }

    private static FavoriteResponse MapToResponse(UserFavorite favorite)
    {
        return new FavoriteResponse
        {
            Id = favorite.Id,
            Type = favorite.Type,
            AddedAt = favorite.AddedAt,
            
            // Unit info (when Type is Unit)
            FavId = favorite.FavId,
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

            // SubUnit info (when Type is SubUnit)
            SubUnitRoomNumber = favorite.SubUnit?.RoomNumber,
            SubUnitType = favorite.SubUnit?.Type.ToString(),
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
            UnitImageUrls = favorite.Unit?.Images
                .OrderBy(i => i.DisplayOrder)
                .Select(i => i.ImageUrl)
                .ToList(),

            SubUnitImageUrls = favorite.SubUnit?.SubUnitImages
                .OrderBy(i => i.DisplayOrder)
                .Select(i => i.ImageUrl)
                .ToList(),

            Amenities = (favorite.Type == FavoriteType.Unit
                ? favorite.Unit?.UnitAmenities.Select(ua => new AmenityInfo(
                    ua.AmenityId,
                    ua.Amenity.Name.ToString(),
                    ua.Amenity.Category.ToString(),
                    ua.IsAvailable))
                : favorite.SubUnit?.SubUnitAmenities.Select(sa => new AmenityInfo(
                    sa.AmenityId,
                    sa.Amenity.Name.ToString(),
                    sa.Amenity.Category.ToString(),
                    sa.IsAvailable)))?.ToList(),

            Description = favorite.Type == FavoriteType.Unit
                ? favorite.Unit?.Description
                : favorite.SubUnit?.Description
        };
    }

    private static IQueryable<UserFavorite> ApplySorting(
        IQueryable<UserFavorite> query,
        string? sortBy,
        string? sortDirection)
    {
        var descending = sortDirection?.ToUpper() == "DESC";

        return sortBy switch
        {
            "AddedAt" => descending
                ? query.OrderByDescending(f => f.AddedAt)
                : query.OrderBy(f => f.AddedAt),

            "Name" => descending
                ? query.OrderByDescending(f => f.Unit != null ? f.Unit.Name : f.SubUnit!.RoomNumber)
                : query.OrderBy(f => f.Unit != null ? f.Unit.Name : f.SubUnit!.RoomNumber),

            "Rating" => descending
                ? query.OrderByDescending(f => f.Unit != null ? f.Unit.AverageRating : 0)
                : query.OrderBy(f => f.Unit != null ? f.Unit.AverageRating : 0),

            _ => query.OrderByDescending(f => f.AddedAt)
        };
    }

    #endregion
}