using Application.Extensions;
using Application.Service.Avilabilaties;
using Application.Service.Booking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hajzzy.Controllers;

/// <summary>
/// Controller for managing Unit-level bookings (entire property bookings)
/// </summary>
[Route("api/bookings/units")]
[ApiController]
public class UnitBookingController(IUnitBookingServices service) : ControllerBase
{
    private readonly IUnitBookingServices _service = service;

    #region CREATE BOOKING

    /// <summary>
    /// Create a new unit booking (book entire property)
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateUnitBooking([FromBody] CreateUnitBookingRequest request)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { error = "User not authenticated" });

        var requestWithUser = request with { UserId = userId };
        var result = await _service.CreateUnitBookingAsync(requestWithUser);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Calculate price for a unit booking
    /// </summary>
    [HttpPost("calculate-price")]
    [AllowAnonymous]
    public async Task<IActionResult> CalculateUnitBookingPrice([FromBody] CalculateUnitBookingPriceRequest request)
    {
        var result = await _service.CalculateUnitBookingPriceAsync(request);

        return result.IsSuccess
            ? Ok(new { totalPrice = result.Value })
            : result.ToProblem();
    }

    #endregion

    #region BOOKING MANAGEMENT

    /// <summary>
    /// Confirm a pending unit booking (Admin only)
    /// </summary>
    [HttpPut("{bookingId}/confirm")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> ConfirmBooking(int bookingId)
    {
        var adminUserId = User.GetUserId()!;
        var result = await _service.ConfirmUnitBookingAsync(bookingId, adminUserId);

        return result.IsSuccess
            ? Ok(new { message = "Booking confirmed successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Check-in a unit booking
    /// </summary>
    [HttpPut("{bookingId}/check-in")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> CheckIn(int bookingId)
    {
        var result = await _service.CheckInUnitAsync(bookingId);

        return result.IsSuccess
            ? Ok(new { message = "Check-in successful" })
            : result.ToProblem();
    }

    /// <summary>
    /// Check-out a unit booking
    /// </summary>
    [HttpPut("{bookingId}/check-out")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> CheckOut(int bookingId)
    {
        var result = await _service.CheckOutUnitAsync(bookingId);

        return result.IsSuccess
            ? Ok(new { message = "Check-out successful" })
            : result.ToProblem();
    }

    /// <summary>
    /// Cancel a unit booking
    /// </summary>
    [HttpPut("{bookingId}/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelBooking(
        int bookingId,
        [FromBody] CancelBookingRequest request)
    {
        var userId = User.GetUserId()!;
        var result = await _service.CancelUnitBookingAsync(
            bookingId,
            request.CancellationReason,
            userId);

        return result.IsSuccess
            ? Ok(new { message = "Booking cancelled successfully" })
            : result.ToProblem();
    }

    #endregion

    #region QUERY OPERATIONS

    /// <summary>
    /// Get unit booking by ID
    /// </summary>
    [HttpGet("{bookingId}")]
    [Authorize]
    public async Task<IActionResult> GetBookingById(int bookingId)
    {
        var result = await _service.GetUnitBookingByIdAsync(bookingId);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get unit booking by booking number
    /// </summary>
    [HttpGet("by-number/{bookingNumber}")]
    [Authorize]
    public async Task<IActionResult> GetBookingByNumber(string bookingNumber)
    {
        var result = await _service.GetUnitBookingByNumberAsync(bookingNumber);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get current user's unit bookings
    /// </summary>
    [HttpGet("my-bookings")]
    [Authorize]
    public async Task<IActionResult> GetMyBookings([FromQuery] BookingFilter filter)
    {
        var userId = User.GetUserId()!;
        var result = await _service.GetUserUnitBookingsAsync(userId, filter);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get all unit bookings for a specific property (Admin only)
    /// </summary>
    [HttpGet("property/{unitId}")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> GetPropertyBookings(
        int unitId,
        [FromQuery] BookingFilter filter)
    {
        var result = await _service.GetUnitBookingsForPropertyAsync(unitId, filter);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    #endregion

    #region PAYMENT OPERATIONS

    /// <summary>
    /// Process payment for a unit booking
    /// </summary>
    [HttpPost("{bookingId}/payments")]
    [Authorize]
    public async Task<IActionResult> ProcessPayment(
        int bookingId,
        [FromBody] ProcessPaymentRequest request)
    {
        var result = await _service.ProcessUnitPaymentAsync(bookingId, request);

        return result.IsSuccess
            ? Ok(new { message = "Payment processed successfully" })
            : result.ToProblem();
    }

    #endregion

    #region STATISTICS

    /// <summary>
    /// Get unit booking statistics (Admin only)
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "SuperAdmin,CityAdmin")]
    public async Task<IActionResult> GetStatistics([FromQuery] UnitBookingStatisticsFilter filter)
    {
        var result = await _service.GetUnitBookingStatisticsAsync(filter);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    #endregion

    #region AVAILABILITY CHECK

    /// <summary>
    /// Check if a unit is available for booking
    /// </summary>
    [HttpGet("check-availability")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckAvailability(
        [FromQuery] int unitId,
        [FromQuery] DateTime checkIn,
        [FromQuery] DateTime checkOut)
    {
        var result = await _service.IsUnitAvailableForBookingAsync(unitId, checkIn, checkOut);

        return result.IsSuccess
            ? Ok(new { isAvailable = result.Value })
            : result.ToProblem();
    }

    #endregion
}

// ============= REQUEST MODELS =============

public record CancelBookingRequest(string CancellationReason);