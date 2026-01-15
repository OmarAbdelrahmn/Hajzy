using Application.Contracts.AD;
using Application.Extensions;
using Application.Service.AdService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Hajzzy.Controllers;

[Route("api/ads")]
[ApiController]
public class AdController : ControllerBase
{
    private readonly IAdService _adService;

    public AdController(IAdService adService)
    {
        _adService = adService;
    }

    /// <summary>
    /// Create a new ad (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,CityAdmin")]
    public async Task<IActionResult> CreateAd([FromForm] CreateAdRequest request)
    {
        var userId = User.GetUserId();
        var result = await _adService.CreateAdAsync(request, userId);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Update an existing ad (Admin only)
    /// </summary>
    [HttpPut("{adId}")]
    [Authorize(Roles = "SuperAdmin,CityAdmin")]
    public async Task<IActionResult> UpdateAd(int adId, [FromForm] UpdateAdRequest request)
    {
        var userId = User.GetUserId();
        var result = await _adService.UpdateAdAsync(adId, request, userId);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Delete an ad (Admin only)
    /// </summary>
    [HttpDelete("{adId}")]
    [Authorize(Roles = "SuperAdmin,CityAdmin")]
    public async Task<IActionResult> DeleteAd(int adId)
    {
        var result = await _adService.DeleteAdAsync(adId);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    /// <summary>
    /// Get ad by ID
    /// </summary>
    [HttpGet("{adId}")]
    public async Task<IActionResult> GetAdById(int adId)
    {
        var result = await _adService.GetAdByIdAsync(adId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get all ads with filtering (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,CityAdmin")]
    public async Task<IActionResult> GetAllAds([FromQuery] AdListFilter filter)
    {
        var result = await _adService.GetAllAdsAsync(filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get current active ads (Public endpoint)
    /// </summary>
    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCurrentActiveAds()
    {
        var result = await _adService.GetCurrentActiveAdsAsync();
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get inactive/expired ads (Admin only)
    /// </summary>
    [HttpGet("inactive")]
    [Authorize(Roles = "SuperAdmin,CityAdmin")]
    public async Task<IActionResult> GetInactiveAds()
    {
        var result = await _adService.GetInactiveAdsAsync();
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Manually trigger deactivation of expired ads (Admin only)
    /// </summary>
    [HttpPost("deactivate-expired")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeactivateExpiredAds()
    {
        var result = await _adService.DeactivateExpiredAdsAsync();
        return result.IsSuccess ? Ok(new { message = "Expired ads deactivated" }) : result.ToProblem();
    }
}