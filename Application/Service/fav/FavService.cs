using Application.Abstraction;
using Application.Contracts.Fav;
using Domain;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Service.fav;

public class FavService(
    ApplicationDbcontext context) : IFavService
{
    private readonly ApplicationDbcontext _context = context;

    #region CRUD Operations

    public async Task<Result<FavoriteResponse>> AddFavoriteAsync(
        string userId,
        AddFavoriteRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {

             UserFavorite favorite;

            if (request.Type.ToString() == 1)
            {
                // Validate unit exists
                var unitExists = await _context.Units
                    .AnyAsync(u => u.Id == request.UnitId.Value && !u.IsDeleted);

                if (!unitExists)
                    return Result.Failure<FavoriteResponse>(
                        new Error("NotFound", "Unit not found", 404));

                // Check if already favorited
                var existing = await _context.Set<UserFavorite>()
                    .FirstOrDefaultAsync(f => f.UserId == userId &&
                                             f.UnitId == request.UnitId.Value);

                if (existing != null)
                    return Result.Failure<FavoriteResponse>(
                        new Error("AlreadyFavorited", "Unit is already in favorites", 400));

                favorite = new UserFavorite
                {
                    UserId = userId,
                    UnitId = request.UnitId.Value,
                    Type = FavoriteType.Unit,
                    Notes = request.Notes,
                    AddedAt = DateTime.UtcNow.AddHours(3)
                };
            }
            else // SubUnitId
            {
                // Validate subunit exists
                var subUnitExists = await _context.SubUnits
                    .AnyAsync(s => s.Id == request.SubUnitId!.Value && !s.IsDeleted);

                if (!subUnitExists)
                    return Result.Failure<FavoriteResponse>(
                        new Error("NotFound", "SubUnit not found", 404));

                // Check if already favorited
                var existing = await _context.Set<UserFavorite>()
                    .FirstOrDefaultAsync(f => f.UserId == userId &&
                                             f.SubUnitId == request.SubUnitId.Value);

                if (existing != null)
                    return Result.Failure<FavoriteResponse>(
                        new Error("AlreadyFavorited", "SubUnit is already in favorites", 400));

                favorite = new UserFavorite
                {
                    UserId = userId,
                    SubUnitId = request.SubUnitId.Value,
                    Type = FavoriteType.SubUnit,
                    Notes = request.Notes,
                    AddedAt = DateTime.UtcNow.AddHours(3)
                };
            }

            await _context.Set<UserFavorite>().AddAsync(favorite);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "User {UserId} added {Type} favorite: {ItemId}",
                userId, favorite.Type, favorite.UnitId ?? favorite.SubUnitId);

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

    public async Task<r> RemoveFavoriteAsync(string userId, int favoriteId)
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

    public async Task<r> RemoveUnitFavoriteAsync(string userId, int unitId)
    {
        try
        {
            var favorite = await _context.Set<UserFavorite>()
                .FirstOrDefaultAsync(f => f.UserId == userId && f.UnitId == unitId);

            if (favorite == null)
                return Result.Failure(
                    new Error("NotFound", "Unit favorite not found", 404));

            _context.Set<UserFavorite>().Remove(favorite);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "User {UserId} removed unit favorite {UnitId}",
                userId, unitId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing unit favorite {UnitId}", unitId);
            return Result.Failure(
                new Error("RemoveFailed", "Failed to remove unit favorite", 500));
        }
    }

    public async Task<r> RemoveSubUnitFavoriteAsync(string userId, int subUnitId)
    {
        try
        {
            var favorite = await _context.Set<UserFavorite>()
                .FirstOrDefaultAsync(f => f.UserId == userId && f.SubUnitId == subUnitId);

            if (favorite == null)
                return Result.Failure(
                    new Error("NotFound", "SubUnit favorite not found", 404));

            _context.Set<UserFavorite>().Remove(favorite);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "User {UserId} removed subunit favorite {SubUnitId}",
                userId, subUnitId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing subunit favorite {SubUnitId}", subUnitId);
            return Result.Failure(
                new Error("RemoveFailed", "Failed to remove subunit favorite", 500));
        }
    }

    public async Task<Result<ToggleFavoriteResponse>> ToggleFavoriteAsync(
        string userId,
        int? unitId,
        int? subUnitId)
    {
        try
        {
            if (unitId.HasValue)
            {
                var existing = await _context.Set<UserFavorite>()
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.UnitId == unitId.Value);

                if (existing != null)
                {
                    _context.Set<UserFavorite>().Remove(existing);
                    await _context.SaveChangesAsync();
                    return Result.Success(new ToggleFavoriteResponse(false, "Removed from favorites"));
                }
                else
                {
                    await AddFavoriteAsync(userId, new AddFavoriteRequest(unitId, null));
                    return Result.Success(new ToggleFavoriteResponse(true, "Added to favorites"));
                }
            }
            else if (subUnitId.HasValue)
            {
                var existing = await _context.Set<UserFavorite>()
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.SubUnitId == subUnitId.Value);

                if (existing != null)
                {
                    _context.Set<UserFavorite>().Remove(existing);
                    await _context.SaveChangesAsync();
                    return Result.Success(new ToggleFavoriteResponse(false, "Removed from favorites"));
                }
                else
                {
                    await AddFavoriteAsync(userId, new AddFavoriteRequest(null, subUnitId));
                    return Result.Success(new ToggleFavoriteResponse(true, "Added to favorites"));
                }
            }

            return Result.Failure<ToggleFavoriteResponse>(
                new Error("InvalidRequest", "Must provide either UnitId or SubUnitId", 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling favorite");
            return Result.Failure<ToggleFavoriteResponse>(
                new Error("ToggleFailed", "Failed to toggle favorite", 500));
        }
    }

    public async Task<Result<FavoriteResponse>> UpdateNotesAsync(
        string userId,
        int favoriteId,
        UpdateFavoriteNotesRequest request)
    {
        try
        {
            var favorite = await _context.Set<UserFavorite>()
                .FirstOrDefaultAsync(f => f.Id == favoriteId && f.UserId == userId);

            if (favorite == null)
                return Result.Failure<FavoriteResponse>(
                    new Error("NotFound", "Favorite not found", 404));

            favorite.Notes = request.Notes;
            await _context.SaveChangesAsync();

            var response = await GetFavoriteResponseAsync(favoriteId);
            return Result.Success(response!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating favorite notes");
            return Result.Failure<FavoriteResponse>(
                new Error("UpdateFailed", "Failed to update notes", 500));
        }
    }

    #endregion

    #region Query Operations

    public async Task<Result<IEnumerable<FavoriteResponse>>> GetUserFavoritesAsync(
        string userId,
        FavoriteFilter filter)
    {
        try
        {
            var query = _context.Set<UserFavorite>()
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
                .Where(f => f.UserId == userId)
                .AsQueryable();

            // Apply filters
            if (filter.Type.HasValue)
                query = query.Where(f => f.Type == filter.Type.Value);

            if (filter.DepartmentId.HasValue)
                query = query.Where(f =>
                    (f.UnitId.HasValue && f.Unit!.CityId == filter.DepartmentId.Value) ||
                    (f.SubUnitId.HasValue && f.SubUnit!.Unit.CityId == filter.DepartmentId.Value));

            if (filter.UnitTypeId.HasValue)
                query = query.Where(f =>
                    (f.UnitId.HasValue && f.Unit!.UnitTypeId == filter.UnitTypeId.Value) ||
                    (f.SubUnitId.HasValue && f.SubUnit!.Unit.UnitTypeId == filter.UnitTypeId.Value));

            if (filter.AddedFrom.HasValue)
                query = query.Where(f => f.AddedAt >= filter.AddedFrom.Value);

            if (filter.AddedTo.HasValue)
                query = query.Where(f => f.AddedAt <= filter.AddedTo.Value);

            if (!string.IsNullOrWhiteSpace(filter.SearchKeyword))
            {
                var keyword = filter.SearchKeyword.ToLower();
                query = query.Where(f =>
                    (f.UnitId.HasValue && f.Unit!.Name.ToLower().Contains(keyword)) ||
                    (f.SubUnitId.HasValue && f.SubUnit!.RoomNumber.ToLower().Contains(keyword)) ||
                    (f.SubUnitId.HasValue && f.SubUnit!.Unit.Name.ToLower().Contains(keyword)));
            }

            // Apply sorting
            query = ApplySorting(query, filter.SortBy, filter.SortDirection);

            // Pagination
            var skip = (filter.Page - 1) * filter.PageSize;
            var favorites = await query
                .Skip(skip)
                .Take(filter.PageSize)
                .AsNoTracking()
                .ToListAsync();

            var responses = favorites.Select(MapToResponse).ToList();
            return Result.Success<IEnumerable<FavoriteResponse>>(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user favorites");
            return Result.Failure<IEnumerable<FavoriteResponse>>(
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
                .FirstOrDefaultAsync(f => f.Id == favoriteId && f.UserId == userId);

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

    public async Task<Result<IEnumerable<FavoriteResponse>>> GetUnitFavoritesAsync(string userId)
    {
        return await GetUserFavoritesAsync(userId, new FavoriteFilter { Type = FavoriteType.Unit });
    }

    public async Task<Result<IEnumerable<FavoriteResponse>>> GetSubUnitFavoritesAsync(string userId)
    {
        return await GetUserFavoritesAsync(userId, new FavoriteFilter { Type = FavoriteType.SubUnit });
    }

    #endregion

    #region Validation Operations

    public async Task<Result<bool>> IsUnitFavoritedAsync(string userId, int unitId)
    {
        var exists = await _context.Set<UserFavorite>()
            .AnyAsync(f => f.UserId == userId && f.UnitId == unitId);

        return Result.Success(exists);
    }

    public async Task<Result<bool>> IsSubUnitFavoritedAsync(string userId, int subUnitId)
    {
        var exists = await _context.Set<UserFavorite>()
            .AnyAsync(f => f.UserId == userId && f.SubUnitId == subUnitId);

        return Result.Success(exists);
    }

    public async Task<Result<int>> GetUnitFavoriteCountAsync(int unitId)
    {
        var count = await _context.Set<UserFavorite>()
            .CountAsync(f => f.UnitId == unitId);

        return Result.Success(count);
    }

    public async Task<Result<int>> GetSubUnitFavoriteCountAsync(int subUnitId)
    {
        var count = await _context.Set<UserFavorite>()
            .CountAsync(f => f.SubUnitId == subUnitId);

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
                .Where(f => f.UnitId.HasValue)
                .GroupBy(f => new { f.UnitId, f.Unit!.Name })
                .Select(g => new TopFavoriteItem(
                    g.Key.UnitId!.Value,
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

    public async Task<r> ClearAllFavoritesAsync(string userId)
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

    public async Task<r> ClearUnitFavoritesAsync(string userId)
    {
        try
        {
            var favorites = await _context.Set<UserFavorite>()
                .Where(f => f.UserId == userId && f.Type == FavoriteType.Unit)
                .ToListAsync();

            if (!favorites.Any())
                return Result.Success();

            _context.Set<UserFavorite>().RemoveRange(favorites);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Cleared {Count} unit favorites for user {UserId}",
                favorites.Count, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing unit favorites");
            return Result.Failure(
                new Error("ClearFailed", "Failed to clear unit favorites", 500));
        }
    }

    public async Task<r> ClearSubUnitFavoritesAsync(string userId)
    {
        try
        {
            var favorites = await _context.Set<UserFavorite>()
                .Where(f => f.UserId == userId && f.Type == FavoriteType.SubUnit)
                .ToListAsync();

            if (!favorites.Any())
                return Result.Success();

            _context.Set<UserFavorite>().RemoveRange(favorites);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Cleared {Count} subunit favorites for user {UserId}",
                favorites.Count, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing subunit favorites");
            return Result.Failure(
                new Error("ClearFailed", "Failed to clear subunit favorites", 500));
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
            Notes = favorite.Notes,

            // Unit info
            UnitId = favorite.UnitId,
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
            SubUnitId = favorite.SubUnitId,
            SubUnitRoomNumber = favorite.SubUnit?.RoomNumber,
            SubUnitType = favorite.SubUnit?.Type.ToString(),
            SubUnitPricePerNight = favorite.SubUnit?.PricePerNight,
            SubUnitMaxOccupancy = favorite.SubUnit?.MaxOccupancy,
            SubUnitIsAvailable = favorite.SubUnit?.IsAvailable,
            SubUnitPrimaryImageUrl = favorite.SubUnit?.SubUnitImages?.FirstOrDefault()?.ImageUrl,

            // Parent unit for subunits
            ParentUnitId = favorite.SubUnit?.UnitId,
            ParentUnitName = favorite.SubUnit?.Unit.Name
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
            Notes = baseResponse.Notes,
            UnitId = baseResponse.UnitId,
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
            SubUnitId = baseResponse.SubUnitId,
            SubUnitRoomNumber = baseResponse.SubUnitRoomNumber,
            SubUnitType = baseResponse.SubUnitType,
            SubUnitPricePerNight = baseResponse.SubUnitPricePerNight,
            SubUnitMaxOccupancy = baseResponse.SubUnitMaxOccupancy,
            SubUnitIsAvailable = baseResponse.SubUnitIsAvailable,
            SubUnitPrimaryImageUrl = baseResponse.SubUnitPrimaryImageUrl,
            ParentUnitId = baseResponse.ParentUnitId,
            ParentUnitName = baseResponse.ParentUnitName,

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
} : IFavService
{
}
