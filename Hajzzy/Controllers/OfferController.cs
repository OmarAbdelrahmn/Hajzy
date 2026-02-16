using Application.Contracts.Offer;
using Application.Extensions;
using Application.Service.OfferService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hajzzy.Controllers;

[Route("api/offers")]
[ApiController]
public class OfferController(IOfferService offerService) : ControllerBase
{
    private readonly IOfferService _offerService = offerService;

    /// <summary>
    /// Create a new offer (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,CityAdmin")]
    public async Task<IActionResult> CreateOffer([FromForm] CreateOfferRequest request)
    {
        var userId = User.GetUserId();
        var result = await _offerService.CreateOfferAsync(request, userId!);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Update an existing offer (Admin only)
    /// </summary>
    [HttpPut("{offerId}")]
    [Authorize(Roles = "SuperAdmin,CityAdmin")]
    public async Task<IActionResult> UpdateOffer(int offerId, [FromForm] UpdateOfferRequest request)
    {
        var userId = User.GetUserId();
        var result = await _offerService.UpdateOfferAsync(offerId, request, userId);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Delete an offer (Admin only)
    /// </summary>
    [HttpDelete("{offerId}")]
    [Authorize(Roles = "SuperAdmin,CityAdmin")]
    public async Task<IActionResult> DeleteOffer(int offerId)
    {
        var result = await _offerService.DeleteOfferAsync(offerId);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    /// <summary>
    /// Get offer by ID
    /// </summary>
    [HttpGet("{offerId}")]
    public async Task<IActionResult> GetOfferById(int offerId)
    {
        var result = await _offerService.GetOfferByIdAsync(offerId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get all offers with filtering (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,CityAdmin")]
    public async Task<IActionResult> GetAllOffers([FromQuery] OfferListFilter filter)
    {
        var result = await _offerService.GetAllOffersAsync(filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("featured")]
    [Authorize(Roles = "SuperAdmin,CityAdmin")]
    public async Task<IActionResult> GetFeatueredOffers()
    {
        var result = await _offerService.GetFeaturedOffersAsync();
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get current active offers (Public endpoint)
    /// </summary>
    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCurrentActiveOffers()
    {
        var result = await _offerService.GetActiveOffersAsync();
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get inactive/expired offers (Admin only)
    /// </summary>
    [HttpGet("inactive")]
    [Authorize(Roles = "SuperAdmin,CityAdmin")]
    public async Task<IActionResult> GetInactiveOffers()
    {
        var result = await _offerService.GetInactiveOffersAsync();
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Manually trigger deactivation of expired offers (Admin only)
    /// </summary>
    [HttpPost("deactivate-expired")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeactivateExpiredOffers()
    {
        var result = await _offerService.DeactivateExpiredOffersAsync();
        return result.IsSuccess ? Ok(new { message = "Expired offers deactivated" }) : result.ToProblem();
    }
    [HttpPut("Toggle-Featured")]
    [Authorize(Roles = "SuperAdmin,CityAdmin")]
    public async Task<IActionResult> ToggleStatus(int OfferId)
    {
        var result = await _offerService.ToggleFeatured(OfferId);
        return result.IsSuccess ? Ok(new { message = "Done" }) : result.ToProblem();
    }
}