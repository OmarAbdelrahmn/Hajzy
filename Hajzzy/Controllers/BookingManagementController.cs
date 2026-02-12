using Application.Abstraction;
using Application.Contracts.BookingManagement;
using Application.Extensions;
using Application.Service.Booking;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hajzzy.Controllers;

/// <summary>
/// Controller for managing bookings (both Unit and SubUnit bookings)
/// </summary>
[Route("api/bookings")]
[ApiController]
public class BookingManagementController(IBookingManagementService service) : ControllerBase
{
    private readonly IBookingManagementService _service = service;

    #region BOOKING QUERIES

    /// <summary>
    /// Get all bookings with filters (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> GetAllBookings([FromQuery] UnifiedBookingFilter filter)
    {
        var result = await _service.GetAllBookingsAsync(filter);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get bookings for the authenticated user
    /// </summary>
    [HttpGet("my-bookings")]
    [Authorize]
    public async Task<IActionResult> GetMyBookings([FromQuery] UnifiedBookingFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _service.GetUserBookingsAsync(userId!, filter);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get bookings for a specific user (Admin only)
    /// </summary>
    [HttpGet("users/{userId}")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> GetUserBookings(
        string userId,
        [FromQuery] UnifiedBookingFilter filter)
    {
        var result = await _service.GetUserBookingsAsync(userId, filter);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get bookings for a specific unit (Admin only)
    /// </summary>
    [HttpGet("units/{unitId}")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> GetUnitBookings(
        int unitId,
        [FromQuery] UnifiedBookingFilter filter)
    {
        var result = await _service.GetUnitBookingsAsync(unitId, filter);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get booking details by ID
    /// </summary>
    [HttpGet("{bookingId}")]
    [Authorize]
    public async Task<IActionResult> GetBookingById(int bookingId)
    {
        var result = await _service.GetBookingByIdAsync(bookingId);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get booking details by booking number
    /// </summary>
    [HttpGet("number/{bookingNumber}")]
    [Authorize]
    public async Task<IActionResult> GetBookingByNumber(string bookingNumber)
    {
        var result = await _service.GetBookingByNumberAsync(bookingNumber);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Search bookings by guest name, email, or booking number (Admin only)
    /// </summary>
    [HttpGet("search")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> SearchBookings(
        [FromQuery] string searchTerm,
        [FromQuery] UnifiedBookingFilter filter)
    {
        var result = await _service.SearchBookingsAsync(searchTerm, filter);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    #endregion

    #region BOOKING MANAGEMENT

    /// <summary>
    /// Confirm a booking (Admin only)
    /// </summary>
    [HttpPost("{bookingId}/confirm")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> ConfirmBooking(int bookingId)
    {
        var adminUserId = User.GetUserId();
        var result = await _service.ConfirmBookingAsync(bookingId, adminUserId!);

        return result.IsSuccess
            ? Ok(new { message = "Booking confirmed successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Check-in a booking (Admin only)
    /// </summary>
    [HttpPost("{bookingId}/check-in")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> CheckInBooking(
        int bookingId,
        [FromBody] CheckInRequest? request = null)
    {
        var result = await _service.CheckInBookingAsync(bookingId, request);

        return result.IsSuccess
            ? Ok(new { message = "Booking checked-in successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Check-out a booking (Admin only)
    /// </summary>
    [HttpPost("{bookingId}/check-out")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> CheckOutBooking(
        int bookingId,
        [FromBody] CheckOutRequest? request = null)
    {
        var result = await _service.CheckOutBookingAsync(bookingId, request);

        return result.IsSuccess
            ? Ok(new { message = "Booking checked-out successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Cancel a booking
    /// </summary>
    [HttpPost("{bookingId}/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelBooking(
        int bookingId,
        [FromBody] CancelBookingRequest request)
    {
        var userId = User.GetUserId();
        var result = await _service.CancelBookingAsync(
            bookingId,
            request.CancellationReason,
            userId!);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Modify booking dates
    /// </summary>
    [HttpPut("{bookingId}/dates")]
    [Authorize]
    public async Task<IActionResult> ModifyBookingDates(
        int bookingId,
        [FromBody] ModifyBookingDatesRequest request)
    {
        var userId = User.GetUserId();
        var result = await _service.ModifyBookingDatesAsync(
            bookingId,
            request.NewCheckIn,
            request.NewCheckOut,
            userId!);

        return result.IsSuccess
            ? Ok(new { message = "Booking dates modified successfully" })
            : result.ToProblem();
    }

    #endregion

    #region PAYMENT OPERATIONS

    /// <summary>
    /// Process payment for a booking (Admin only)
    /// </summary>
    [HttpPost("{bookingId}/payments")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> ProcessPayment(
        int bookingId,
        [FromBody] ProcessPaymentRequest request)
    {
        var result = await _service.ProcessPaymentAsync(bookingId, request);

        return result.IsSuccess
            ? Ok(new { message = "Payment processed successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Refund a booking (Admin only)
    /// </summary>
    [HttpPost("{bookingId}/refund")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> RefundBooking(
        int bookingId,
        [FromBody] RefundRequest request)
    {
        var result = await _service.RefundBookingAsync(bookingId, request);

        return result.IsSuccess
            ? Ok(new { message = "Refund processed successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Get payment history for a booking
    /// </summary>
    [HttpGet("{bookingId}/payments")]
    [Authorize]
    public async Task<IActionResult> GetPaymentHistory(int bookingId)
    {
        var result = await _service.GetBookingPaymentHistoryAsync(bookingId);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get pending payments (Admin only)
    /// </summary>
    [HttpGet("pending-payments")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> GetPendingPayments([FromQuery] PendingPaymentFilter filter)
    {
        var result = await _service.GetPendingPaymentsAsync(filter);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    #endregion

    #region CALENDAR & SCHEDULING

    /// <summary>
    /// Get upcoming check-ins (Admin only)
    /// </summary>
    [HttpGet("upcoming-check-ins")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> GetUpcomingCheckIns(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int? unitId = null)
    {
        var result = await _service.GetUpcomingCheckInsAsync(startDate, endDate, unitId);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get upcoming check-outs (Admin only)
    /// </summary>
    [HttpGet("upcoming-check-outs")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> GetUpcomingCheckOuts(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int? unitId = null)
    {
        var result = await _service.GetUpcomingCheckOutsAsync(startDate, endDate, unitId);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get current guests (checked-in bookings) (Admin only)
    /// </summary>
    [HttpGet("current-guests")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> GetCurrentGuests([FromQuery] int? unitId = null)
    {
        var result = await _service.GetCurrentGuestsAsync(unitId);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get booking timeline for a date range (Admin only)
    /// </summary>
    [HttpGet("timeline")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> GetBookingTimeline(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int? unitId = null)
    {
        var result = await _service.GetBookingTimelineAsync(startDate, endDate, unitId);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    #endregion

    #region STATISTICS & ANALYTICS

    /// <summary>
    /// Get booking statistics (Admin only)
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> GetBookingStatistics([FromQuery] BookingStatisticsFilter filter)
    {
        var result = await _service.GetBookingStatisticsAsync(filter);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get revenue analytics (Admin only)
    /// </summary>
    [HttpGet("analytics/revenue")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> GetRevenueAnalytics([FromQuery] RevenueAnalyticsFilter filter)
    {
        var result = await _service.GetRevenueAnalyticsAsync(filter);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get occupancy statistics (Admin only)
    /// </summary>
    [HttpGet("analytics/occupancy")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> GetOccupancyStatistics([FromQuery] OccupancyFilter filter)
    {
        var result = await _service.GetOccupancyStatisticsAsync(filter);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get cancellation analytics (Admin only)
    /// </summary>
    [HttpGet("analytics/cancellations")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> GetCancellationAnalytics([FromQuery] CancellationAnalyticsFilter filter)
    {
        var result = await _service.GetCancellationAnalyticsAsync(filter);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get booking trends over time (Admin only)
    /// </summary>
    [HttpGet("analytics/trends")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> GetBookingTrends([FromQuery] BookingTrendsFilter filter)
    {
        var result = await _service.GetBookingTrendsAsync(filter);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    #endregion

    #region REPORTS

    /// <summary>
    /// Generate daily booking report (Admin only)
    /// </summary>
    [HttpGet("reports/daily")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> GetDailyReport([FromQuery] DateTime date)
    {
        var result = await _service.GetDailyReportAsync(date);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Generate monthly booking report (Admin only)
    /// </summary>
    [HttpGet("reports/monthly")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> GetMonthlyReport(
        [FromQuery] int year,
        [FromQuery] int month)
    {
        var result = await _service.GetMonthlyReportAsync(year, month);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Generate custom date range report (Admin only)
    /// </summary>
    [HttpPost("reports/custom")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> GetCustomReport([FromBody] CustomReportRequest request)
    {
        var result = await _service.GetCustomReportAsync(
            request.StartDate,
            request.EndDate,
            request.Options);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    #endregion

    #region BULK OPERATIONS

    /// <summary>
    /// Bulk confirm bookings (Admin only)
    /// </summary>
    [HttpPost("bulk/confirm")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> BulkConfirmBookings([FromBody] BulkBookingRequest request)
    {
        var adminUserId = User.GetUserId();
        var result = await _service.BulkConfirmBookingsAsync(request.BookingIds, adminUserId!);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Bulk cancel bookings (Admin only)
    /// </summary>
    [HttpPost("bulk/cancel")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> BulkCancelBookings([FromBody] BulkCancelRequest request)
    {
        var userId = User.GetUserId();
        var result = await _service.BulkCancelBookingsAsync(
            request.BookingIds,
            request.CancellationReason,
            userId!);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    #endregion

    #region NOTIFICATIONS

    /// <summary>
    /// Send booking reminder (Admin only)
    /// </summary>
    [HttpPost("{bookingId}/reminders")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> SendBookingReminder(
        int bookingId,
        [FromBody] SendReminderRequest request)
    {
        var result = await _service.SendBookingReminderAsync(bookingId, request.ReminderType);

        return result.IsSuccess
            ? Ok(new { message = "Reminder sent successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Send custom message to guest (Admin only)
    /// </summary>
    [HttpPost("{bookingId}/messages")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> SendCustomMessage(
        int bookingId,
        [FromBody] SendCustomMessageRequest request)
    {
        var result = await _service.SendCustomMessageToGuestAsync(
            bookingId,
            request.Subject,
            request.Message);

        return result.IsSuccess
            ? Ok(new { message = "Message sent successfully" })
            : result.ToProblem();
    }

    #endregion

    #region VALIDATION & UTILITIES

    /// <summary>
    /// Validate if booking can be modified
    /// </summary>
    [HttpPost("{bookingId}/validate-modification")]
    [Authorize]
    public async Task<IActionResult> ValidateBookingModification(
        int bookingId,
        [FromBody] ModifyBookingDatesRequest request)
    {
        var result = await _service.ValidateBookingModificationAsync(
            bookingId,
            request.NewCheckIn,
            request.NewCheckOut);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Calculate refund amount for a booking
    /// </summary>
    [HttpGet("{bookingId}/calculate-refund")]
    [Authorize]
    public async Task<IActionResult> CalculateRefundAmount(
        int bookingId,
        [FromQuery] DateTime? cancellationDate = null)
    {
        var result = await _service.CalculateRefundAmountAsync(bookingId, cancellationDate);

        return result.IsSuccess
            ? Ok(new { refundAmount = result.Value })
            : result.ToProblem();
    }

    /// <summary>
    /// Get dashboard summary (Admin only)
    /// </summary>
    [HttpGet("dashboard/summary")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> GetDashboardSummary(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var result = await _service.GetDashboardSummaryAsync(startDate, endDate);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    #endregion
}

// ============= REQUEST MODELS =============


public record ModifyBookingDatesRequest(DateTime NewCheckIn, DateTime NewCheckOut);

public record CustomReportRequest(
    DateTime StartDate,
    DateTime EndDate,
    ReportOptions Options);

public record BulkBookingRequest(List<int> BookingIds);

public record BulkCancelRequest(
    List<int> BookingIds,
    string CancellationReason);

public record SendReminderRequest(ReminderType ReminderType);

public record SendCustomMessageRequest(
    string Subject,
    string Message);