using Application.Abstraction.Consts;
using Application.Contracts.hoteladmincont;
using Application.Extensions;
using Application.Service.HotelAdmin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hajzzy.Controllers;

[Route("api/hotel-admin")]
[ApiController]
//[Authorize(Roles = DefaultRoles.HotelAdmin)]
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

   

    #region Policy Management

    /// <summary>
    /// Get all policies for a unit
    /// </summary>
    [HttpGet("units/{unitId}/policies")]
    public async Task<IActionResult> GetUnitPolicies(int unitId)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetUnitPoliciesAsync(userId!, unitId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Create a new policy for a unit
    /// </summary>
    [HttpPost("units/{unitId}/policies")]
    public async Task<IActionResult> CreateUnitPolicy(int unitId, [FromBody] CreatePolicyRequest request)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.CreateUnitPolicyAsync(userId!, unitId, request);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Update an existing policy
    /// </summary>
    [HttpPut("policies/{policyId}")]
    public async Task<IActionResult> UpdatePolicy(int policyId, [FromBody] UpdatePolicyRequest request)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.UpdateUnitPolicyAsync(userId!, policyId, request);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Delete a policy
    /// </summary>
    [HttpDelete("policies/{policyId}")]
    public async Task<IActionResult> DeletePolicy(int policyId)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.DeleteUnitPolicyAsync(userId!, policyId);
        return result.IsSuccess ? Ok(new { message = "Policy deleted successfully" }) : result.ToProblem();
    }

    /// <summary>
    /// Get policies for a subunit
    /// </summary>
    [HttpGet("subunits/{subUnitId}/policies")]
    public async Task<IActionResult> GetSubUnitPolicies(int subUnitId)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetSubUnitPoliciesAsync(userId!, subUnitId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Create a policy for a subunit
    /// </summary>
    [HttpPost("subunits/{subUnitId}/policies")]
    public async Task<IActionResult> CreateSubUnitPolicy(int subUnitId, [FromBody] CreatePolicyRequest request)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.CreateSubUnitPolicyAsync(userId!, subUnitId, request);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    #endregion

    // ============================================================================
    // CANCELLATION POLICY MANAGEMENT ENDPOINTS
    // ============================================================================

    #region Cancellation Policy Management

    /// <summary>
    /// Get all available cancellation policies
    /// </summary>
    [HttpGet("cancellation-policies")]
    public async Task<IActionResult> GetAvailableCancellationPolicies()
    {
        var result = await _hotelAdminService.GetAvailableCancellationPoliciesAsync();
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get unit's current cancellation policy
    /// </summary>
    [HttpGet("units/{unitId}/cancellation-policy")]
    public async Task<IActionResult> GetUnitCancellationPolicy(int unitId)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetUnitCancellationPolicyAsync(userId!, unitId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Set cancellation policy for a unit
    /// </summary>
    [HttpPut("units/{unitId}/cancellation-policy/{policyId}")]
    public async Task<IActionResult> SetUnitCancellationPolicy(int unitId, int policyId)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.SetUnitCancellationPolicyAsync(userId!, unitId, policyId);
        return result.IsSuccess
            ? Ok(new { message = "Cancellation policy set successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Create a custom cancellation policy
    /// </summary>
    [HttpPost("cancellation-policies/custom")]
    public async Task<IActionResult> CreateCustomCancellationPolicy(
        [FromBody] Application.Contracts.hoteladmincont.CreateCancellationPolicyRequest request)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.CreateCustomCancellationPolicyAsync(userId!, request);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    #endregion

    // ============================================================================
    // COMPREHENSIVE AVAILABILITY MANAGEMENT ENDPOINTS
    // ============================================================================

    #region Availability Management

    /// <summary>
    /// Get unit availability calendar
    /// </summary>
    [HttpGet("units/{unitId}/availability-calendar")]
    public async Task<IActionResult> GetUnitAvailabilityCalendar(
        int unitId,
        [FromQuery] int year,
        [FromQuery] int month)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetUnitAvailabilityCalendarAsync(userId!, unitId, year, month);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get subunit availability calendar
    /// </summary>
    [HttpGet("subunits/{subUnitId}/availability-calendar")]
    public async Task<IActionResult> GetSubUnitAvailabilityCalendar(
        int subUnitId,
        [FromQuery] int year,
        [FromQuery] int month)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetSubUnitAvailabilityCalendarAsync(userId!, subUnitId, year, month);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Set unit availability
    /// </summary>
    [HttpPost("units/availability")]
    public async Task<IActionResult> SetUnitAvailability([FromBody] Application.Contracts.Availability.SetUnitAvailabilityRequest request)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.SetUnitAvailabilityAsync(userId!, request);
        return result.IsSuccess
            ? Ok(new { message = "Availability set successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Set subunit availability
    /// </summary>
    [HttpPost("subunits/availability")]
    public async Task<IActionResult> SetSubUnitAvailability([FromBody] Application.Contracts.Availability.SetSubUnitAvailabilityRequest request)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.SetSubUnitAvailabilityAsync(userId!, request);
        return result.IsSuccess
            ? Ok(new { message = "Availability set successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Block multiple dates for a unit
    /// </summary>
    [HttpPost("units/block-dates")]
    public async Task<IActionResult> BlockUnitDates([FromBody] Application.Contracts.Availability.BlockUnitDatesRequest request)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.BlockUnitDatesAsync(userId!, request);
        return result.IsSuccess
            ? Ok(new { message = "Dates blocked successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Block multiple dates for a subunit
    /// </summary>
    [HttpPost("subunits/block-dates")]
    public async Task<IActionResult> BlockSubUnitDates([FromBody] Application.Contracts.Availability.BlockDatesRequest request)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.BlockSubUnitDatesAsync(userId!, request);
        return result.IsSuccess
            ? Ok(new { message = "Dates blocked successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Set special pricing for subunit
    /// </summary>
    [HttpPost("subunits/special-pricing")]
    public async Task<IActionResult> SetSubUnitSpecialPricing([FromBody] Application.Contracts.Availability.SetSpecialPricingRequest request)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.SetSubUnitSpecialPricingAsync(userId!, request);
        return result.IsSuccess
            ? Ok(new { message = "Special pricing set successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Unblock dates
    /// </summary>
    [HttpDelete("availability/{availabilityId}")]
    public async Task<IActionResult> UnblockDates(int availabilityId)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.UnblockDatesAsync(userId!, availabilityId);
        return result.IsSuccess
            ? Ok(new { message = "Dates unblocked successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Get all blocked dates for a unit
    /// </summary>
    [HttpGet("units/{unitId}/blocked-dates")]
    public async Task<IActionResult> GetUnitBlockedDates(
        int unitId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetUnitBlockedDatesAsync(userId!, unitId, startDate, endDate);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get all blocked dates for a subunit
    /// </summary>
    [HttpGet("subunits/{subUnitId}/blocked-dates")]
    public async Task<IActionResult> GetSubUnitBlockedDates(
        int subUnitId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetSubUnitBlockedDatesAsync(userId!, subUnitId, startDate, endDate);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Check unit availability for specific dates
    /// </summary>
    [HttpGet("units/{unitId}/check-availability")]
    public async Task<IActionResult> CheckUnitAvailability(
        int unitId,
        [FromQuery] DateTime checkIn,
        [FromQuery] DateTime checkOut)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.CheckUnitAvailabilityAsync(userId!, unitId, checkIn, checkOut);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Check subunit availability for specific dates
    /// </summary>
    [HttpGet("subunits/{subUnitId}/check-availability")]
    public async Task<IActionResult> CheckSubUnitAvailability(
        int subUnitId,
        [FromQuery] DateTime checkIn,
        [FromQuery] DateTime checkOut)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.CheckSubUnitAvailabilityAsync(userId!, subUnitId, checkIn, checkOut);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get available subunits for a date range
    /// </summary>
    [HttpGet("units/{unitId}/available-subunits")]
    public async Task<IActionResult> GetAvailableSubUnits(
        int unitId,
        [FromQuery] DateTime checkIn,
        [FromQuery] DateTime checkOut)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetAvailableSubUnitsAsync(userId!, unitId, checkIn, checkOut);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    #endregion

    // ============================================================================
    // COMPREHENSIVE REPORTS ENDPOINTS
    // ============================================================================

    #region Comprehensive Reports

    /// <summary>
    /// Get detailed financial report
    /// </summary>
    [HttpGet("reports/financial")]
    public async Task<IActionResult> GetFinancialReport([FromQuery] FinancialReportFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetFinancialReportAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get occupancy statistics
    /// </summary>
    [HttpGet("reports/occupancy-statistics")]
    public async Task<IActionResult> GetOccupancyStatistics([FromQuery] OccupancyFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetOccupancyStatisticsAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get booking trends
    /// </summary>
    [HttpGet("reports/booking-trends")]
    public async Task<IActionResult> GetBookingTrends([FromQuery] TrendsFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetBookingTrendsAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get customer insights
    /// </summary>
    [HttpGet("reports/customer-insights")]
    public async Task<IActionResult> GetCustomerInsights([FromQuery] InsightsFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetCustomerInsightsAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get performance comparison
    /// </summary>
    [HttpGet("reports/performance-comparison")]
    public async Task<IActionResult> GetPerformanceComparison([FromQuery] ComparisonFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetPerformanceComparisonAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get revenue breakdown
    /// </summary>
    [HttpGet("reports/revenue-breakdown")]
    public async Task<IActionResult> GetRevenueBreakdown([FromQuery] RevenueBreakdownFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetRevenueBreakdownAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get cancellation analytics
    /// </summary>
    [HttpGet("reports/cancellation-analytics")]
    public async Task<IActionResult> GetCancellationAnalytics([FromQuery] CancellationFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetCancellationAnalyticsAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get pricing optimization suggestions
    /// </summary>
    [HttpGet("units/{unitId}/pricing-optimization")]
    public async Task<IActionResult> GetPricingOptimization(
        int unitId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetPricingOptimizationSuggestionsAsync(userId!, unitId, startDate, endDate);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Export report to Excel
    /// </summary>
    [HttpPost("reports/export/excel")]
    public async Task<IActionResult> ExportReportToExcel([FromBody] ExportReportRequest request)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.ExportReportToExcelAsync(userId!, request);

        if (!result.IsSuccess)
            return result.ToProblem();

        return File(result.Value, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"report_{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    /// <summary>
    /// Export report to PDF
    /// </summary>
    [HttpPost("reports/export/pdf")]
    public async Task<IActionResult> ExportReportToPdf([FromBody] ExportReportRequest request)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.ExportReportToPdfAsync(userId!, request);

        if (!result.IsSuccess)
            return result.ToProblem();

        return File(result.Value, "application/pdf", $"report_{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    #endregion

    // ============================================================================
    // SUBUNIT MANAGEMENT (EXTENDED) ENDPOINTS
    // ============================================================================

    #region SubUnit Management Extended

    /// <summary>
    /// Update subunit details
    /// </summary>
    [HttpPut("subunits/{subUnitId}")]
    public async Task<IActionResult> UpdateSubUnit(int subUnitId, [FromBody] UpdateSubUnitRequest request)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.UpdateSubUnitAsync(userId!, subUnitId, request);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Update subunit pricing
    /// </summary>
    [HttpPut("subunits/{subUnitId}/pricing")]
    public async Task<IActionResult> UpdateSubUnitPricing(
        int subUnitId,
        [FromBody] UpdateSubUnitPricingRequest request)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.UpdateSubUnitPricingAsync(userId!, subUnitId, request);
        return result.IsSuccess
            ? Ok(new { message = "Pricing updated successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Toggle subunit status (available/unavailable)
    /// </summary>
    [HttpPatch("subunits/{subUnitId}/toggle-status")]
    public async Task<IActionResult> ToggleSubUnitStatus(int subUnitId)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.ToggleSubUnitStatusAsync(userId!, subUnitId);
        return result.IsSuccess
            ? Ok(new { message = "Status updated successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Get subunit booking history
    /// </summary>
    [HttpGet("subunits/{subUnitId}/booking-history")]
    public async Task<IActionResult> GetSubUnitBookingHistory(
        int subUnitId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetSubUnitBookingHistoryAsync(userId!, subUnitId, startDate, endDate);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    #endregion

    // ============================================================================
    // AMENITIES MANAGEMENT ENDPOINTS
    // ============================================================================

    #region Amenities Management

    /// <summary>
    /// Get all available amenities
    /// </summary>
    [HttpGet("amenities")]
    public async Task<IActionResult> GetAvailableAmenities()
    {
        var result = await _hotelAdminService.GetAvailableAmenitiesAsync();
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get unit amenities
    /// </summary>
    [HttpGet("units/{unitId}/amenities")]
    public async Task<IActionResult> GetUnitAmenities(int unitId)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetUnitAmenitiesAsync(userId!, unitId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Update unit amenities
    /// </summary>
    [HttpPut("units/{unitId}/amenities")]
    public async Task<IActionResult> UpdateUnitAmenities(int unitId, [FromBody] UpdateAmenitiesRequest request)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.UpdateUnitAmenitiesAsync(userId!, unitId, request);
        return result.IsSuccess
            ? Ok(new { message = "Amenities updated successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Get subunit amenities
    /// </summary>
    [HttpGet("subunits/{subUnitId}/amenities")]
    public async Task<IActionResult> GetSubUnitAmenities(int subUnitId)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetSubUnitAmenitiesAsync(userId!, subUnitId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Update subunit amenities
    /// </summary>
    [HttpPut("subunits/{subUnitId}/amenities")]
    public async Task<IActionResult> UpdateSubUnitAmenities(
        int subUnitId,
        [FromBody] UpdateAmenitiesRequest request)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.UpdateSubUnitAmenitiesAsync(userId!, subUnitId, request);
        return result.IsSuccess
            ? Ok(new { message = "Amenities updated successfully" })
            : result.ToProblem();
    }

    #endregion

    // ============================================================================
    // PART 2: Add these endpoints to HotelAdminController.cs
    // ============================================================================

    #region offers

    /// <summary>
    /// Get all reviews for admin's units
    /// </summary>
    [HttpGet("offers")]
    public async Task<IActionResult> GetMyUnitsReviws()
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetMyUnitOffersAsync(userId!);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Respond to a review
    /// </summary>
    [HttpPost("offers")]
    [Consumes("multipart/form-data")]  // Changed from application/json
    public async Task<IActionResult> RespondTdoReview([FromForm] CreateOfferRequest request)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.CreateUnitOfferAsync(userId!,request);
        return result.IsSuccess ? Ok(new { message = "Done" }) : result.ToProblem();
    }

    [HttpDelete("offers")]
    public async Task<IActionResult> RespondTsdoReview(int offerId)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.DeleteOfferAsync(userId!,offerId);
        return result.IsSuccess ? Ok(new { message = "Done" }) : result.ToProblem();
    }

    [HttpPut("offers")]
    [Consumes("multipart/form-data")]  // Changed from application/json
    public async Task<IActionResult> RespondTsdsoReview(int offerId, [FromForm] UpdateOfferRequest request)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.UpdateOfferAsync(userId!,offerId,request);
        return result.IsSuccess ? Ok(new { message = "Done" }) : result.ToProblem();
    }


    #endregion

    #region IMAGE MANAGEMENT ENDPOINTS

    /// <summary>
    /// Get all images for a unit
    /// </summary>
    [HttpGet("units/{unitId}/images")]
    public async Task<IActionResult> GetUnitImages(int unitId)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetUnitImagesAsync(userId!, unitId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Update image order and primary status for a unit
    /// </summary>
    [HttpPut("units/{unitId}/images/order")]
    public async Task<IActionResult> UpdateImageOrder(int unitId, [FromBody] UpdateImageOrderRequest request)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.UpdateImageOrderAsync(userId!, unitId, request);
        return result.IsSuccess
            ? Ok(new { message = "Image order updated successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Delete a unit image
    /// </summary>
    [HttpDelete("images/{imageId}")]
    public async Task<IActionResult> DeleteUnitImage(int imageId)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.DeleteUnitImageAsync(userId!, imageId);
        return result.IsSuccess
            ? Ok(new { message = "Image deleted successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Get all images for a subunit
    /// </summary>
    [HttpGet("subunits/{subUnitId}/images")]
    public async Task<IActionResult> GetSubUnitImages(int subUnitId)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.GetSubUnitImagesAsync(userId!, subUnitId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Delete a subunit image
    /// </summary>
    [HttpDelete("subunits/images/{imageId}")]
    public async Task<IActionResult> DeleteSubUnitImage(int imageId)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.DeleteSubUnitImageAsync(userId!, imageId);
        return result.IsSuccess
            ? Ok(new { message = "SubUnit image deleted successfully" })
            : result.ToProblem();
    }

    #endregion

    #region NOTIFICATIONS & ALERTS ENDPOINTS

    /// <summary>
    /// Get admin notifications with filtering
    /// </summary>
    //[HttpGet("notifications")]
    //public async Task<IActionResult> GetAdminNotifications([FromQuery] NotificationFilter filter)
    //{
    //    var userId = User.GetUserId();
    //    var result = await _hotelAdminService.GetAdminNotificationsAsync(userId!, filter);
    //    return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    //}

    ///// <summary>
    ///// Mark a notification as read
    ///// </summary>
    //[HttpPatch("notifications/{notificationId}/mark-read")]
    //public async Task<IActionResult> MarkNotificationAsRead(int notificationId)
    //{
    //    var userId = User.GetUserId();
    //    var result = await _hotelAdminService.MarkNotificationAsReadAsync(userId!, notificationId);
    //    return result.IsSuccess
    //        ? Ok(new { message = "Notification marked as read" })
    //        : result.ToProblem();
    //}

    ///// <summary>
    ///// Get alert settings for the admin
    ///// </summary>
    //[HttpGet("alerts/settings")]
    //public async Task<IActionResult> GetAlertSettings()
    //{
    //    var userId = User.GetUserId();
    //    var result = await _hotelAdminService.GetAlertSettingsAsync(userId!);
    //    return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    //}

    ///// <summary>
    ///// Update alert settings
    ///// </summary>
    //[HttpPut("alerts/settings")]
    //public async Task<IActionResult> UpdateAlertSettings([FromBody] UpdateAlertSettingsRequest request)
    //{
    //    var userId = User.GetUserId();
    //    var result = await _hotelAdminService.UpdateAlertSettingsAsync(userId!, request);
    //    return result.IsSuccess
    //        ? Ok(new { message = "Alert settings updated successfully" })
    //        : result.ToProblem();
    //}

    //#endregion

    //#region BULK OPERATIONS ENDPOINTS

    ///// <summary>
    ///// Bulk update availability for multiple subunits
    ///// </summary>
    //[HttpPost("subunits/bulk-availability")]
    //public async Task<IActionResult> BulkUpdateSubUnitAvailability([FromBody] BulkAvailabilityUpdateRequest request)
    //{
    //    var userId = User.GetUserId();
    //    var result = await _hotelAdminService.BulkUpdateSubUnitAvailabilityAsync(userId!, request);
    //    return result.IsSuccess
    //        ? Ok(new { message = $"Availability updated for {request.SubUnitIds.Count} subunits" })
    //        : result.ToProblem();
    //}

    ///// <summary>
    ///// Bulk update pricing for multiple subunits
    ///// </summary>
    //[HttpPost("subunits/bulk-pricing")]
    //public async Task<IActionResult> BulkUpdatePricing([FromBody] BulkPricingUpdateRequest request)
    //{
    //    var userId = User.GetUserId();
    //    var result = await _hotelAdminService.BulkUpdatePricingAsync(userId!, request);
    //    return result.IsSuccess
    //        ? Ok(new { message = $"Pricing updated for {request.SubUnitIds.Count} subunits" })
    //        : result.ToProblem();
    //}

    ///// <summary>
    ///// Copy availability settings from one subunit to others
    ///// </summary>
    //[HttpPost("subunits/copy-availability")]
    //public async Task<IActionResult> CopyAvailabilitySettings([FromBody] CopyAvailabilityRequest request)
    //{
    //    var userId = User.GetUserId();
    //    var result = await _hotelAdminService.CopyAvailabilitySettingsAsync(userId!, request);
    //    return result.IsSuccess
    //        ? Ok(new { message = $"Availability copied to {request.TargetSubUnitIds.Count} subunits" })
    //        : result.ToProblem();
    //}

    #endregion

    /// <summary>
    /// Upload new image for a unit
    /// </summary>
    /// 
    [HttpPost("unit/images/upload")]
    public async Task<IActionResult> UploadUnitImage(
        [FromForm] UploadDto request)
    {
        var userId = User.GetUserId();
        var result = await _hotelAdminService.UploadUnitImageAsync(userId!, request.image,request.caption);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    public record UploadDto(IFormFile image, string? caption);
  
}