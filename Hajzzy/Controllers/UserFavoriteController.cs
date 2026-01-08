using Application.Contracts.Fav;
using Application.Extensions;
using Application.Service.fav;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hajzzy.Controllers;

[Route("api/favorites")]
[ApiController]
[Authorize]
public class UserFavoriteController(IFavService service) : ControllerBase
{
    private readonly IFavService _service = service;

    // ============= CRUD =============

    /// <summary>
    /// Add a unit or subunit to favorites
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AddFavorite([FromBody] AddFavoriteRequest request)
    {
        var result = await _service.AddFavoriteAsync(User.GetUserId()!, request);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Remove a favorite by ID
    /// </summary>
    [HttpDelete("{favoriteId:int}")]
    public async Task<IActionResult> RemoveFavorite(
        int favoriteId)
    {
        var result = await _service.RemoveFavoriteAsync(User.GetUserId()!, favoriteId);
        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }

    // ============= QUERY =============

    /// <summary>
    /// Get all user favorites with filtering
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetFavorites([FromQuery] FavoriteFilter filter)
    {
        var result = await _service.GetUserFavoritesAsync(User.GetUserId()!, filter);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get favorite details by ID
    /// </summary>
    [HttpGet("{favoriteId:int}")]
    public async Task<IActionResult> GetFavoriteDetails(
        int favoriteId)
    {
        var result = await _service.GetFavoriteDetailsAsync(User.GetUserId()!, favoriteId);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get all unit favorites
    /// </summary>
    [HttpGet("units")]
    public async Task<IActionResult> GetUnitFavorites()
    {
        var filter = new FavoriteFilter { Type = FavoriteType.Unit };
        var result = await _service.GetUserFavoritesAsync(User.GetUserId()!, filter);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get all subunit favorites
    /// </summary>
    [HttpGet("subunits")]
    public async Task<IActionResult> GetSubUnitFavorites()
    {
        var filter = new FavoriteFilter { Type = FavoriteType.SubUnit };
        var result = await _service.GetUserFavoritesAsync(User.GetUserId()!, filter);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    // ============= VALIDATION =============

    /// <summary>
    /// Get favorite count for a unit (public)
    /// </summary>
    [HttpGet("count/unit/{unitId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUnitFavoriteCount(int unitId)
    {
        var result = await _service.GetUnitFavoriteCountAsync(unitId);
        return result.IsSuccess
            ? Ok(new { count = result.Value })
            : result.ToProblem();
    }

    /// <summary>
    /// Get favorite count for a subunit (public)
    /// </summary>
    [HttpGet("count/subunit/{subUnitId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSubUnitFavoriteCount(int subUnitId)
    {
        var result = await _service.GetSubUnitFavoriteCountAsync(subUnitId);
        return result.IsSuccess
            ? Ok(new { count = result.Value })
            : result.ToProblem();
    }

    // ============= STATISTICS =============

    /// <summary>
    /// Get user's favorite statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        var result = await _service.GetUserStatisticsAsync(User.GetUserId()!);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get global favorite statistics (admin only)
    /// </summary>
    [HttpGet("statistics/global")]
    [Authorize(Roles = "SuperAdmin,CityAdmin")]
    public async Task<IActionResult> GetGlobalStatistics()
    {
        var result = await _service.GetGlobalStatisticsAsync();
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    // ============= BULK OPERATIONS =============

    /// <summary>
    /// Clear all favorites
    /// </summary>
    [HttpDelete("clear")]
    public async Task<IActionResult> ClearAllFavorites()
    {
        var result = await _service.ClearAllFavoritesAsync(User.GetUserId()!);
        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }
}