using Application.Extensions;
using Application.Service.Avilabilaties;
using Application.Service.Booking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hajzzy.Controllers;

/// <summary>
/// Controller for managing SubUnit-level bookings (individual room bookings)
/// </summary>
[Route("api/bookings/rooms")]
[ApiController]
public class SubUnitBookingController(ISubUnitBookingServices service) : ControllerBase
{
    private readonly ISubUnitBookingServices _service = service;

    #region CREATE BOOKING

    /// <summary>
    /// Create a new subunit booking (book specific rooms)
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateSubUnitBooking([FromBody] CreateSubUnitBookingRequest request)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { error = "User not authenticated" });

        var requestWithUser = request with { UserId = userId };
        var result = await _service.CreateSubUnitBookingAsync(requestWithUser);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Calculate price for a subunit booking
    /// </summary>
    [HttpPost("calculate-price")]
    [AllowAnonymous]
    public async Task<IActionResult> CalculateSubUnitBookingPrice([FromBody] CalculateSubUnitBookingPriceRequest request)
    {
        var result = await _service.CalculateSubUnitBookingPriceAsync(request);

        return result.IsSuccess
            ? Ok(new { totalPrice = result.Value })
            : result.ToProblem();
    }

    #endregion

    #region BOOKING MANAGEMENT

    /// <summary>
    /// Confirm a pending subunit booking (Admin only)
    /// </summary>
    [HttpPut("{bookingId}/confirm")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> ConfirmBooking(int bookingId)
    {
        var adminUserId = User.GetUserId()!;
        var result = await _service.ConfirmSubUnitBookingAsync(bookingId, adminUserId);

        return result.IsSuccess
            ? Ok(new { message = "Booking confirmed successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Check-in a subunit booking
    /// </summary>
    [HttpPut("{bookingId}/check-in")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> CheckIn(int bookingId)
    {
        var result = await _service.CheckInSubUnitAsync(bookingId);

        return result.IsSuccess
            ? Ok(new { message = "Check-in successful" })
            : result.ToProblem();
    }

    /// <summary>
    /// Check-out a subunit booking
    /// </summary>
    [HttpPut("{bookingId}/check-out")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> CheckOut(int bookingId)
    {
        var result = await _service.CheckOutSubUnitAsync(bookingId);

        return result.IsSuccess
            ? Ok(new { message = "Check-out successful" })
            : result.ToProblem();
    }

    /// <summary>
    /// Cancel a subunit booking
    /// </summary>
    [HttpPut("{bookingId}/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelBooking(
        int bookingId,
        [FromBody] CancelBookingRequest request)
    {
        var userId = User.GetUserId()!;
        var result = await _service.CancelSubUnitBookingAsync(
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
    /// Get subunit booking by ID
    /// </summary>
    [HttpGet("{bookingId}")]
    [Authorize]
    public async Task<IActionResult> GetBookingById(int bookingId)
    {
        var result = await _service.GetSubUnitBookingByIdAsync(bookingId);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get subunit booking by booking number
    /// </summary>
    [HttpGet("by-number/{bookingNumber}")]
    [Authorize]
    public async Task<IActionResult> GetBookingByNumber(string bookingNumber)
    {
        var result = await _service.GetSubUnitBookingByNumberAsync(bookingNumber);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get current user's subunit bookings
    /// </summary>
    [HttpGet("my-bookings")]
    [Authorize]
    public async Task<IActionResult> GetMyBookings([FromQuery] BookingFilter filter)
    {
        var userId = User.GetUserId()!;
        var result = await _service.GetUserSubUnitBookingsAsync(userId, filter);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get all bookings for a specific room (Admin only)
    /// </summary>
    [HttpGet("room/{subUnitId}")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> GetRoomBookings(
        int subUnitId,
        [FromQuery] BookingFilter filter)
    {
        var result = await _service.GetSubUnitBookingsForRoomAsync(subUnitId, filter);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get all subunit bookings for a property (Admin only)
    /// </summary>
    [HttpGet("unit/{unitId}")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> GetUnitBookings(
        int unitId,
        [FromQuery] BookingFilter filter)
    {
        var result = await _service.GetSubUnitBookingsForUnitAsync(unitId, filter);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    #endregion

    #region PAYMENT OPERATIONS

    /// <summary>
    /// Process payment for a subunit booking
    /// </summary>
    [HttpPost("{bookingId}/payments")]
    [Authorize]
    public async Task<IActionResult> ProcessPayment(
        int bookingId,
        [FromBody] ProcessPaymentRequest request)
    {
        var result = await _service.ProcessSubUnitPaymentAsync(bookingId, request);

        return result.IsSuccess
            ? Ok(new { message = "Payment processed successfully" })
            : result.ToProblem();
    }

    #endregion

    #region STATISTICS

    /// <summary>
    /// Get subunit booking statistics (Admin only)
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "SuperAdmin,CityAdmin")]
    public async Task<IActionResult> GetStatistics([FromQuery] SubUnitBookingStatisticsFilter filter)
    {
        var result = await _service.GetSubUnitBookingStatisticsAsync(filter);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    #endregion

    #region AVAILABILITY

    /// <summary>
    /// Check if specific subunits are available
    /// </summary>
    [HttpPost("check-availability")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckSubUnitsAvailability([FromBody] CheckSubUnitsAvailabilityRequest request)
    {
        var result = await _service.AreSubUnitsAvailableAsync(
            request.SubUnitIds,
            request.CheckIn,
            request.CheckOut);

        return result.IsSuccess
            ? Ok(new { isAvailable = result.Value })
            : result.ToProblem();
    }

    /// <summary>
    /// Get available subunits for a unit
    /// </summary>
    [HttpGet("available")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailableSubUnits(
        [FromQuery] int unitId,
        [FromQuery] DateTime checkIn,
        [FromQuery] DateTime checkOut,
        [FromQuery] int count = 10)
    {
        var result = await _service.GetAvailableSubUnitsAsync(unitId, checkIn, checkOut, count);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    #endregion
}

// ============= REQUEST MODELS =============

public record CheckSubUnitsAvailabilityRequest(
    List<int> SubUnitIds,
    DateTime CheckIn,
    DateTime CheckOut
);