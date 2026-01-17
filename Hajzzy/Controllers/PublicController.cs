// Hajzzy/Controllers/PublicController.cs
using Application.Contracts.publicuser;
using Application.Extensions;
using Application.Service.OfferService;
using Application.Service.publicuser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hajzzy.Controllers;

/// <summary>
/// Public API endpoints - No authentication required
/// </summary>
[Route("api/public")]
[ApiController]
[AllowAnonymous] // All endpoints in this controller are public
public class PublicController(IPublicServise service,IOfferService service1) : ControllerBase
{
    private readonly IPublicServise _service = service;
    private readonly IOfferService service1 = service1;


    #region UNITS

    [HttpPost("units")]
    public async Task<IActionResult> GetAllUnits([FromBody] PublicUnitFilter filter)
    {
        var result = await _service.GetAllUnitsAsync(filter);
        return result.IsSuccess ?
            Ok(result.Value) :
            result.ToProblem();
    }

    /// <summary>
    /// Get detailed information about a specific unit
    /// </summary>
    [HttpGet("units/{unitId}")]
    public async Task<IActionResult> GetUnitDetails(int unitId)
    {
        var result = await _service.GetUnitDetailsAsync(unitId);
        return result.IsSuccess ?
            Ok(result.Value) :
            result.ToProblem();
    }

    /// <summary>
    /// Search units by keyword
    /// </summary>
    [HttpPost("units/search")]
    public async Task<IActionResult> SearchUnits([FromBody] PublicSearchRequest request)
    {
        var result = await _service.SearchUnitsAsync(request);
        return result.IsSuccess ?
            Ok(result.Value) :
            result.ToProblem();
    }

    /// <summary>
    /// Get featured/top-rated units
    /// </summary>
    [HttpGet("units/featured")]
    public async Task<IActionResult> GetFeaturedUnits()
    {
        var result = await _service.GetFeaturedUnitsAsync();
        return result.IsSuccess ?
            Ok(result.Value) :
            result.ToProblem();
    }

    /// <summary>
    /// Get units near a specific location
    /// </summary>
    [HttpGet("units/nearby")]
    public async Task<IActionResult> GetNearbyUnits(
        [FromQuery] decimal latitude,
        [FromQuery] decimal longitude,
        [FromQuery] int radiusKm = 50)
    {
        var result = await _service.GetNearbyUnitsAsync(latitude, longitude, radiusKm);
        return result.IsSuccess ?
            Ok(result.Value) :
            result.ToProblem();
    }

    #endregion

    #region SUBUNITS

    /// <summary>
    /// Get detailed information about a specific room/subunit
    /// </summary>
    [HttpGet("subunits/{subUnitId}")]
    public async Task<IActionResult> GetSubUnitDetails(int subUnitId)
    {
        var result = await _service.GetSubUnitDetailsAsync(subUnitId);
        return result.IsSuccess ?
            Ok(result.Value) :
            result.ToProblem();
    }

    /// <summary>
    /// Get available rooms for a unit on specific dates
    /// </summary>
    [HttpGet("units/{unitId}/available-rooms")]
    public async Task<IActionResult> GetAvailableRooms(
        int unitId,
        [FromQuery] DateTime checkIn,
        [FromQuery] DateTime checkOut)
    {
        if (checkIn >= checkOut)
            return BadRequest(new { Message = "Check-out date must be after check-in date" });

        var result = await _service.GetAvailableSubUnitsAsync(unitId, checkIn, checkOut);
        return result.IsSuccess ?
            Ok(result.Value) :
            result.ToProblem();
    }

    #endregion

    #region CITIES

    /// <summary>
    /// Get all available cities
    /// </summary>
    [HttpGet("cities")]
    public async Task<IActionResult> GetAllCities()
    {
        var result = await _service.GetAllCitiesAsync();
        return result.IsSuccess ?
            Ok(result.Value) :
            result.ToProblem();
    }

    /// <summary>
    /// Get detailed information about a specific city
    /// </summary>
    [HttpGet("cities/{cityId}")]
    public async Task<IActionResult> GetCityDetails(int cityId)
    {
        var result = await _service.GetCityDetailsAsync(cityId);
        return result.IsSuccess ?
            Ok(result.Value) :
            result.ToProblem();
    }

    /// <summary>
    /// Get all units in a specific city
    /// </summary>
    [HttpPost("cities/{cityId}/units")]
    public async Task<IActionResult> GetUnitsByCity(
        int cityId,
        [FromBody] PublicUnitFilter? filter = null)
    {
        var result = await _service.GetUnitsByCityAsync(cityId, filter);
        return result.IsSuccess ?
            Ok(result.Value) :
            result.ToProblem();
    }

    #endregion

    #region AVAILABILITY

    /// <summary>
    /// Check availability and get price estimate for a booking
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/public/availability/check
    ///     {
    ///       "unitId": 1,
    ///       "checkIn": "2024-12-01",
    ///       "checkOut": "2024-12-05",
    ///       "numberOfGuests": 2
    ///     }
    /// </remarks>
    [HttpPost("availability/check")]
    public async Task<IActionResult> CheckAvailability([FromBody] CheckAvailabilityRequest request)
    {
        if (request.CheckIn >= request.CheckOut)
            return BadRequest(new { Message = "Check-out date must be after check-in date" });

        if (request.CheckIn < DateTime.UtcNow.Date)
            return BadRequest(new { Message = "Check-in date cannot be in the past" });

        var result = await _service.CheckAvailabilityAsync(request);
        return result.IsSuccess ?
            Ok(result.Value) :
            result.ToProblem();
    }

    #endregion

    #region FAVORITES (for anonymous users)

    /// <summary>
    /// Get favorites for a session (anonymous users)
    /// </summary>
    /// <remarks>
    /// For unauthenticated users, favorites should be stored in localStorage on the client side.
    /// This endpoint is a placeholder for when the user signs in and wants to sync their favorites.
    /// </remarks>
    //[HttpGet("favorites/unit/{sessionId}")]
    //public async Task<IActionResult> GetFavorites(string sessionId)
    //{
    //    var result = await _service.GetFavoritesAsync(sessionId);
    //    return result.IsSuccess ?
    //        Ok(result.Value) :
    //        result.ToProblem();
    //}
    //[HttpGet("favorites/subunit/{sessionId}")]
    //public async Task<IActionResult> GetaFavorites(string sessionId)
    //{
    //    var result = await _service.GetFavoritesAsync(sessionId);
    //    return result.IsSuccess ?
    //        Ok(result.Value) :
    //        result.ToProblem();
    //}

    #endregion

    #region UTILITY ENDPOINTS

    /// <summary>
    /// Health check for public API
    /// </summary>
    //[HttpGet("health")]
    //public IActionResult HealthCheck()
    //{
    //    return Ok(new
    //    {
    //        Status = "Healthy",
    //        Timestamp = DateTime.UtcNow,
    //        Service = "Public API"
    //    });
    //}

    ///// <summary>
    ///// Get API information
    ///// </summary>
    //[HttpGet("info")]
    //public IActionResult GetApiInfo()
    //{
    //    return Ok(new
    //    {
    //        Name = "Hujjzy Public API",
    //        Version = "1.0",
    //        Description = "Public API for browsing units, cities, and checking availability",
    //        Endpoints = new[]
    //        {
    //            "GET /api/public/units - Get all units",
    //            "GET /api/public/units/{id} - Get unit details",
    //            "POST /api/public/units/search - Search units",
    //            "GET /api/public/units/featured - Get featured units",
    //            "GET /api/public/cities - Get all cities",
    //            "POST /api/public/availability/check - Check availability"
    //        }
    //    });
    //}

    #endregion

    [HttpGet("offers")]
    public async Task<IActionResult> GetActiveOffers()
    {
        var result = await _service.GetActiveOffersAsync();
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    [HttpGet("ads")]
    public async Task<IActionResult> GetActiveAds()
    {
        var result = await _service.GetActiveAdsAsync();
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }
    [HttpGet("aminities")]
    public async Task<IActionResult> getam()
    {
        var result = await _service.GetAminitiesAsync();
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }
    [HttpGet("unit-type")]
    public async Task<IActionResult> getunittype()
    {
        var result = await _service.GetUnitTypesAsync();
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }
    [HttpGet("payment-method")]
    public async Task<IActionResult> getpayment()
    {
        var result = await _service.GetPaymentMethodesAsync();
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    [HttpGet("featured-offers")]
    public async Task<IActionResult> getfeaturedoffers()
    {
        var result = await service1.GetFeaturedOffersAsync();
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }
}