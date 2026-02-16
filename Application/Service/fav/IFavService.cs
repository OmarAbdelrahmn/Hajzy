using Application.Abstraction;
using Application.Contracts.Fav;

namespace Application.Service.fav;

public interface IFavService
{
    // ============= CRUD =============

    /// <summary>
    /// Add a unit or subunit to user's favorites
    /// </summary>
    Task<Result<FavoriteResponse>> AddFavoriteAsync(string userId, AddFavoriteRequest request);

    /// <summary>
    /// Remove a favorite by ID and type
    /// </summary>
    Task<Result> RemoveFavoriteAsync(string userId, int favoriteId);

    // ============= QUERY =============

    /// <summary>
    /// Get user's favorites with filtering and pagination
    /// </summary>
    Task<Result<List<FavoriteResponse>>> GetUserFavoritesAsync(
        string userId,
        FavoriteFilter filter);

    /// <summary>
    /// Get favorite details by ID and type
    /// </summary>
    Task<Result<FavoriteDetailsResponse>> GetFavoriteDetailsAsync(
        string userId,
        int favoriteId);

    // ============= VALIDATION =============

    /// <summary>
    /// Get count of users who favorited a specific unit
    /// </summary>
    Task<Result<int>> GetUnitFavoriteCountAsync(int unitId);

    /// <summary>
    /// Get count of users who favorited a specific subunit
    /// </summary>
    Task<Result<int>> GetSubUnitFavoriteCountAsync(int subUnitId);

    // ============= STATISTICS =============

    /// <summary>
    /// Get user's favorite statistics
    /// </summary>
    Task<Result<FavoriteStatisticsResponse>> GetUserStatisticsAsync(string userId);

    /// <summary>
    /// Get global favorite statistics (admin only)
    /// </summary>
    Task<Result<FavoriteStatisticsResponse>> GetGlobalStatisticsAsync();

    // ============= BULK OPERATIONS =============

    /// <summary>
    /// Remove all favorites for a user
    /// </summary>
    Task<Result> ClearAllFavoritesAsync(string userId);
}