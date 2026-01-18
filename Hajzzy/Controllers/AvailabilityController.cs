using Application.Contracts.Availability;
using Application.Extensions;
using Application.Service.Availability;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hajzzy.Controllers;

/// <summary>
/// Controller for managing Unit and SubUnit availability
/// </summary>
[Route("api/availability")]
[ApiController]
public class AvailabilityController(IAvailabilityService service) : ControllerBase
{
    private readonly IAvailabilityService _service = service;

    #region UNIT AVAILABILITY

    /// <summary>
    /// Set manual availability for a unit (Admin only)
    /// </summary>
    [HttpPost("units")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> SetUnitAvailability([FromBody] SetUnitAvailabilityRequest request)
    {
        var userId = User.GetUserId();
        var requestWithUser = request with { UpdatedByUserId = userId };

        var result = await _service.SetUnitAvailabilityAsync(requestWithUser);

        return result.IsSuccess
            ? Ok(new { message = "Unit availability updated successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Get unit availability for a date range
    /// </summary>
    [HttpGet("units/{unitId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUnitAvailability(
        int unitId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var result = await _service.GetUnitAvailabilityAsync(unitId, startDate, endDate);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Check if unit is available for booking
    /// </summary>
    [HttpGet("units/{unitId}/check")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckUnitAvailability(
        int unitId,
        [FromQuery] DateTime checkIn,
        [FromQuery] DateTime checkOut)
    {
        var result = await _service.CheckUnitAvailabilityAsync(unitId, checkIn, checkOut);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get unit availability calendar for a specific month
    /// </summary>
    [HttpGet("units/{unitId}/calendar")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUnitAvailabilityCalendar(
        int unitId,
        [FromQuery] int year,
        [FromQuery] int month)
    {
        var result = await _service.GetUnitAvailabilityCalendarAsync(unitId, year, month);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    #endregion

    #region SUBUNIT AVAILABILITY

    /// <summary>
    /// Set manual availability for a subunit (Admin only)
    /// </summary>
    [HttpPost("subunits")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> SetSubUnitAvailability([FromBody] SetSubUnitAvailabilityRequest request)
    {
        var userId = User.GetUserId();
        var requestWithUser = request with { UpdatedByUserId = userId };

        var result = await _service.SetSubUnitAvailabilityAsync(requestWithUser);

        return result.IsSuccess
            ? Ok(new { message = "SubUnit availability updated successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Get subunit availability for a date range
    /// </summary>
    [HttpGet("subunits/{subUnitId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSubUnitAvailability(
        int subUnitId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var result = await _service.GetSubUnitAvailabilityAsync(subUnitId, startDate, endDate);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Check if subunit is available for booking
    /// </summary>
    [HttpGet("subunits/{subUnitId}/check")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckSubUnitAvailability(
        int subUnitId,
        [FromQuery] DateTime checkIn,
        [FromQuery] DateTime checkOut)
    {
        var result = await _service.CheckSubUnitAvailabilityAsync(subUnitId, checkIn, checkOut);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get subunit availability calendar for a specific month
    /// </summary>
    [HttpGet("subunits/{subUnitId}/calendar")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSubUnitAvailabilityCalendar(
        int subUnitId,
        [FromQuery] int year,
        [FromQuery] int month)
    {
        var result = await _service.GetSubUnitAvailabilityCalendarAsync(subUnitId, year, month);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    #endregion

    #region BATCH OPERATIONS

    /// <summary>
    /// Block multiple dates for a subunit (Admin only)
    /// </summary>
    [HttpPost("subunits/block")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> BlockSubUnitDates([FromBody] BlockDatesRequest request)
    {
        var userId = User.GetUserId();
        var requestWithUser = request with { UpdatedByUserId = userId! };

        var result = await _service.BlockSubUnitDatesAsync(requestWithUser);

        return result.IsSuccess
            ? Ok(new { message = "Dates blocked successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Block multiple dates for a unit (Admin only)
    /// </summary>
    [HttpPost("units/block")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> BlockUnitDates([FromBody] BlockUnitDatesRequest request)
    {
        var userId = User.GetUserId();
        var requestWithUser = request with { UpdatedByUserId = userId! };

        var result = await _service.BlockUnitDatesAsync(requestWithUser);

        return result.IsSuccess
            ? Ok(new { message = "Unit dates blocked successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Set special pricing for a subunit (Admin only)
    /// </summary>
    [HttpPost("subunits/special-pricing")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> SetSpecialPricing([FromBody] SetSpecialPricingRequest request)
    {
        var userId = User.GetUserId();
        var requestWithUser = request with { UpdatedByUserId = userId! };

        var result = await _service.SetSpecialPricingAsync(requestWithUser);

        return result.IsSuccess
            ? Ok(new { message = "Special pricing set successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Unblock previously blocked dates (Admin only)
    /// </summary>
    [HttpDelete("{availabilityId}")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> UnblockDates(int availabilityId)
    {
        var result = await _service.UnblockDatesAsync(availabilityId);

        return result.IsSuccess
            ? Ok(new { message = "Dates unblocked successfully" })
            : result.ToProblem();
    }

    #endregion

    #region BULK QUERIES

    /// <summary>
    /// Get all available subunits for a unit
    /// </summary>
    [HttpGet("units/{unitId}/available-subunits")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailableSubUnits(
        int unitId,
        [FromQuery] DateTime checkIn,
        [FromQuery] DateTime checkOut)
    {
        var result = await _service.GetAvailableSubUnitIdsAsync(unitId, checkIn, checkOut);

        return result.IsSuccess
            ? Ok(new { availableSubUnitIds = result.Value })
            : result.ToProblem();
    }

    /// <summary>
    /// Check availability of multiple subunits at once
    /// </summary>
    [HttpPost("subunits/check-multiple")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckMultipleSubUnitsAvailability(
        [FromBody] CheckMultipleSubUnitsRequest request)
    {
        var result = await _service.CheckMultipleSubUnitsAvailabilityAsync(
            request.SubUnitIds,
            request.CheckIn,
            request.CheckOut);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    #endregion

    #region INITIALIZATION

    /// <summary>
    /// Initialize default availability for a subunit (Admin only)
    /// </summary>
    [HttpPost("subunits/{subUnitId}/initialize")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> InitializeSubUnitAvailability(
        int subUnitId,
        [FromQuery] int daysAhead = 365)
    {
        var result = await _service.InitializeSubUnitAvailabilityAsync(subUnitId, daysAhead);

        return result.IsSuccess
            ? Ok(new { message = $"Availability initialized for {daysAhead} days" })
            : result.ToProblem();
    }

    /// <summary>
    /// Initialize default availability for a unit (Admin only)
    /// </summary>
    [HttpPost("units/{unitId}/initialize")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> InitializeUnitAvailability(
        int unitId,
        [FromQuery] int daysAhead = 365)
    {
        var result = await _service.InitializeUnitAvailabilityAsync(unitId, daysAhead);

        return result.IsSuccess
            ? Ok(new { message = $"Unit availability initialized for {daysAhead} days" })
            : result.ToProblem();
    }

    #endregion
}

// ============= REQUEST MODELS =============

public record CheckMultipleSubUnitsRequest(
    List<int> SubUnitIds,
    DateTime CheckIn,
    DateTime CheckOut
);