using Application.Abstraction;
using Application.Abstraction.Consts;
using Application.Contracts.hoteladmincont;
using Application.Extensions;
using Application.Service.HotelAdmin;
using Domain.Consts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hajzzy.Controllers;

[Route("api/hotel-admin")]
[ApiController]
[Authorize(Roles = DefaultRoles.HotelAdmin)]
public class HotelAdminController(IHotelAdminService hotelAdminService) : ControllerBase
{
    private readonly IHotelAdminService _hotelAdminService = hotelAdminService;

    #region DASHBOARD & OVERVIEW

    /// <summary>
    /// Get comprehensive dashboard data for hotel admin
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetDashboardAsync(userId!);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get quick statistics for hotel admin's units
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetQuickStats()
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetQuickStatsAsync(userId!);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    #endregion

    #region UNIT MANAGEMENT

    /// <summary>
    /// Get all units managed by this admin with full details
    /// </summary>
    [HttpGet("units")]
    public async Task<IActionResult> GetMyUnits([FromQuery] UnitFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetMyUnitsAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get specific unit details (only if admin manages it)
    /// </summary>
    [HttpGet("units/{unitId}")]
    public async Task<IActionResult> GetMyUnitDetails(int unitId)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetMyUnitDetailsAsync(userId!, unitId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Update unit information (only if admin manages it)
    /// </summary>
    [HttpPut("units/{unitId}")]
    public async Task<IActionResult> UpdateMyUnit(int unitId, [FromBody] UpdateUnitRequest request)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.UpdateMyUnitAsync(userId!, unitId, request);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Verify if user is admin of a specific unit
    /// </summary>
    [HttpGet("units/{unitId}/verify-access")]
    public async Task<IActionResult> VerifyUnitAccess(int unitId)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.IsAdminOfUnitAsync(userId!, unitId);
        return result.IsSuccess ? Ok(new { hasAccess = result.Value }) : result.ToProblem();
    }

    #endregion

    #region BOOKING MANAGEMENT

    /// <summary>
    /// Get all bookings for admin's units
    /// </summary>
    [HttpGet("bookings")]
    public async Task<IActionResult> GetMyUnitBookings([FromQuery] BookingFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetMyUnitBookingsAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get specific booking details (only if for admin's unit)
    /// </summary>
    [HttpGet("bookings/{bookingId}")]
    public async Task<IActionResult> GetMyBookingDetails(int bookingId)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetMyBookingDetailsAsync(userId!, bookingId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get booking by booking number
    /// </summary>
    [HttpGet("bookings/number/{bookingNumber}")]
    public async Task<IActionResult> GetMyBookingByNumber(string bookingNumber)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetMyBookingByNumberAsync(userId!, bookingNumber);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Confirm a booking for admin's unit
    /// </summary>
    [HttpPost("bookings/{bookingId}/confirm")]
    public async Task<IActionResult> ConfirmBooking(int bookingId)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.ConfirmBookingAsync(userId!, bookingId);
        return result.IsSuccess ? Ok(new { message = "Booking confirmed successfully" }) : result.ToProblem();
    }

    /// <summary>
    /// Check-in a booking
    /// </summary>
    [HttpPost("bookings/{bookingId}/check-in")]
    public async Task<IActionResult> CheckInBooking(int bookingId)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.CheckInBookingAsync(userId!, bookingId);
        return result.IsSuccess ? Ok(new { message = "Booking checked in successfully" }) : result.ToProblem();
    }

    /// <summary>
    /// Check-out a booking
    /// </summary>
    [HttpPost("bookings/{bookingId}/check-out")]
    public async Task<IActionResult> CheckOutBooking(int bookingId)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.CheckOutBookingAsync(userId!, bookingId);
        return result.IsSuccess ? Ok(new { message = "Booking checked out successfully" }) : result.ToProblem();
    }

    /// <summary>
    /// Cancel a booking with reason
    /// </summary>
    [HttpPost("bookings/{bookingId}/cancel")]
    public async Task<IActionResult> CancelBooking(int bookingId, [FromBody] CancelBookingRequest request)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.CancelBookingAsync(userId!, bookingId, request.CancellationReason);
        return result.IsSuccess ? Ok(new { message = "Booking cancelled successfully" }) : result.ToProblem();
    }

    /// <summary>
    /// Verify if booking belongs to admin's unit
    /// </summary>
    [HttpGet("bookings/{bookingId}/verify-access")]
    public async Task<IActionResult> VerifyBookingAccess(int bookingId)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.IsBookingForMyUnitAsync(userId!, bookingId);
        return result.IsSuccess ? Ok(new { hasAccess = result.Value }) : result.ToProblem();
    }

    #endregion

    #region SUBUNIT MANAGEMENT

    /// <summary>
    /// Get all subunits for admin's units (optionally filtered by unit)
    /// </summary>
    [HttpGet("subunits")]
    public async Task<IActionResult> GetMySubUnits([FromQuery] int? unitId = null)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetMySubUnitsAsync(userId!, unitId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get specific subunit details
    /// </summary>
    [HttpGet("subunits/{subUnitId}")]
    public async Task<IActionResult> GetMySubUnitDetails(int subUnitId)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetMySubUnitDetailsAsync(userId!, subUnitId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    #endregion

    #region AVAILABILITY MANAGEMENT

    /// <summary>
    /// Get availability calendar for admin's unit
    /// </summary>
    [HttpGet("units/{unitId}/availability")]
    public async Task<IActionResult> GetMyUnitsAvailability(
        int unitId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetMyUnitsAvailabilityAsync(userId!, unitId, startDate, endDate);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Update subunit availability
    /// </summary>
    [HttpPost("subunits/{subUnitId}/availability")]
    public async Task<IActionResult> UpdateSubUnitAvailability(
        int subUnitId,
        [FromBody] UpdateAvailabilityRequest request)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.UpdateSubUnitAvailabilityAsync(userId!, subUnitId, request);
        return result.IsSuccess ? Ok(new { message = "Availability updated successfully" }) : result.ToProblem();
    }

    #endregion

    #region REVENUE & ANALYTICS

    /// <summary>
    /// Get revenue report for admin's units
    /// </summary>
    [HttpGet("reports/revenue")]
    public async Task<IActionResult> GetRevenueReport([FromQuery] RevenueReportFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetRevenueReportAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get booking analytics for admin's units
    /// </summary>
    [HttpGet("reports/analytics")]
    public async Task<IActionResult> GetBookingAnalytics([FromQuery] AnalyticsFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetBookingAnalyticsAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get occupancy report for admin's units
    /// </summary>
    [HttpGet("reports/occupancy")]
    public async Task<IActionResult> GetOccupancyReport(
        [FromQuery] int? unitId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetOccupancyReportAsync(userId!, unitId, startDate, endDate);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    #endregion

    #region REVIEWS MANAGEMENT

    /// <summary>
    /// Get all reviews for admin's units
    /// </summary>
    [HttpGet("reviews")]
    public async Task<IActionResult> GetMyUnitsReviews([FromQuery] ReviewFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetMyUnitsReviewsAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Respond to a review
    /// </summary>
    [HttpPost("reviews/{reviewId}/respond")]
    public async Task<IActionResult> RespondToReview(int reviewId, [FromBody] ReviewResponseRequest request)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.RespondToReviewAsync(userId!, reviewId, request.Response);
        return result.IsSuccess ? Ok(new { message = "Response submitted successfully" }) : result.ToProblem();
    }

    #endregion

    #region CALENDAR & SCHEDULING

    /// <summary>
    /// Get upcoming check-ins (default: today, can specify days ahead)
    /// </summary>
    [HttpGet("schedule/check-ins")]
    public async Task<IActionResult> GetUpcomingCheckIns([FromQuery] int days = 1)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetUpcomingCheckInsAsync(userId!, days);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get upcoming check-outs (default: today, can specify days ahead)
    /// </summary>
    [HttpGet("schedule/check-outs")]
    public async Task<IActionResult> GetUpcomingCheckOuts([FromQuery] int days = 1)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetUpcomingCheckOutsAsync(userId!, days);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    #endregion

    #region PAYMENT MANAGEMENT

    /// <summary>
    /// Get payment history for admin's bookings
    /// </summary>
    [HttpGet("payments/history")]
    public async Task<IActionResult> GetPaymentHistory([FromQuery] PaymentFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetPaymentHistoryAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get pending payments for admin's bookings
    /// </summary>
    [HttpGet("payments/pending")]
    public async Task<IActionResult> GetPendingPayments()
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetPendingPaymentsAsync(userId!);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    #endregion
}