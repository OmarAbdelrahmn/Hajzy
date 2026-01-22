using Application.Abstraction;
using Application.Contracts.hoteladmincont;
using Application.Contracts.Unit;
using Application.Service.Availability;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Service.HotelAdmin;

public interface IHotelAdminService
{
    /// <summary>
    /// Get comprehensive dashboard data for hotel admin
    /// </summary>
    Task<Result<HotelAdminDashboardResponse>> GetDashboardAsync(string userId);

    /// <summary>
    /// Get quick statistics for hotel admin's units
    /// </summary>
    Task<Result<HotelAdminStatsResponse>> GetQuickStatsAsync(string userId);

    // ============= UNIT MANAGEMENT =============

    /// <summary>
    /// Get all units managed by this admin with full details
    /// </summary>
    Task<Result<IEnumerable<UnitComprehensiveResponse>>> GetMyUnitsAsync(
        string userId,
        UnitFilter filter);

    /// <summary>
    /// Get specific unit details (only if admin manages it)
    /// </summary>
    Task<Result<UnitComprehensiveResponse>> GetMyUnitDetailsAsync(
        string userId,
        int unitId);

    /// <summary>
    /// Update unit information (only if admin manages it)
    /// </summary>
    Task<Result<UnitResponse>> UpdateMyUnitAsync(
        string userId,
        int unitId,
        UpdateUnitRequest request);

    // ============= BOOKING MANAGEMENT =============

    /// <summary>
    /// Get all bookings for admin's units
    /// </summary>
    Task<Result<IEnumerable<BookingComprehensiveResponse>>> GetMyUnitBookingsAsync(
        string userId,
        BookingFilter filter);

    /// <summary>
    /// Get specific booking details (only if for admin's unit)
    /// </summary>
    Task<Result<BookingDetailsResponse>> GetMyBookingDetailsAsync(
        string userId,
        int bookingId);

    /// <summary>
    /// Get bookings by booking number
    /// </summary>
    Task<Result<BookingDetailsResponse>> GetMyBookingByNumberAsync(
        string userId,
        string bookingNumber);

    /// <summary>
    /// Confirm a booking for admin's unit
    /// </summary>
    Task<Result> ConfirmBookingAsync(
        string userId,
        int bookingId);

    /// <summary>
    /// Check-in a booking
    /// </summary>
    Task<Result> CheckInBookingAsync(
        string userId,
        int bookingId);

    /// <summary>
    /// Check-out a booking
    /// </summary>
    Task<Result> CheckOutBookingAsync(
        string userId,
        int bookingId);

    /// <summary>
    /// Cancel a booking with reason
    /// </summary>
    Task<Result> CancelBookingAsync(
        string userId,
        int bookingId,
        string cancellationReason);

    // ============= SUBUNIT MANAGEMENT =============

    /// <summary>
    /// Get all subunits for admin's units
    /// </summary>
    Task<Result<IEnumerable<SubUnitComprehensiveDetail>>> GetMySubUnitsAsync(
        string userId,
        int? unitId = null);

    /// <summary>
    /// Get specific subunit details
    /// </summary>
    Task<Result<SubUnitComprehensiveDetail>> GetMySubUnitDetailsAsync(
        string userId,
        int subUnitId);

    // ============= AVAILABILITY MANAGEMENT =============

    /// <summary>
    /// Get availability calendar for admin's units
    /// </summary>
    Task<Result<AvailabilityCalendarResponse>> GetMyUnitsAvailabilityAsync(
        string userId,
        int unitId,
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Update subunit availability
    /// </summary>
    Task<Result> UpdateSubUnitAvailabilityAsync(
        string userId,
        int subUnitId,
        UpdateAvailabilityRequest request);

    // ============= REVENUE & ANALYTICS =============

    /// <summary>
    /// Get revenue report for admin's units
    /// </summary>
    Task<Result<RevenueReportResponse>> GetRevenueReportAsync(
        string userId,
        RevenueReportFilter filter);

    /// <summary>
    /// Get booking analytics for admin's units
    /// </summary>
    Task<Result<BookingAnalyticsResponse>> GetBookingAnalyticsAsync(
        string userId,
        AnalyticsFilter filter);

    /// <summary>
    /// Get occupancy rate for admin's units
    /// </summary>
    Task<Result<OccupancyReportResponse>> GetOccupancyReportAsync(
        string userId,
        int? unitId,
        DateTime startDate,
        DateTime endDate);

    // ============= REVIEWS MANAGEMENT =============

    /// <summary>
    /// Get all reviews for admin's units
    /// </summary>
    Task<Result<IEnumerable<ReviewResponse>>> GetMyUnitsReviewsAsync(
        string userId,
        ReviewFilter filter);

    /// <summary>
    /// Respond to a review
    /// </summary>
    Task<Result> RespondToReviewAsync(
        string userId,
        int reviewId,
        string response);

    // ============= CALENDAR & SCHEDULING =============

    /// <summary>
    /// Get upcoming check-ins for today/tomorrow
    /// </summary>
    Task<Result<IEnumerable<UpcomingCheckInResponse>>> GetUpcomingCheckInsAsync(
        string userId,
        int days = 1);

    /// <summary>
    /// Get upcoming check-outs for today/tomorrow
    /// </summary>
    Task<Result<IEnumerable<UpcomingCheckOutResponse>>> GetUpcomingCheckOutsAsync(
        string userId,
        int days = 1);

    // ============= PAYMENT MANAGEMENT =============

    /// <summary>
    /// Get payment history for admin's bookings
    /// </summary>
    Task<Result<IEnumerable<PaymentHistoryResponse>>> GetPaymentHistoryAsync(
        string userId,
        PaymentFilter filter);

    /// <summary>
    /// Get pending payments for admin's bookings
    /// </summary>
    Task<Result<IEnumerable<PendingPaymentResponse>>> GetPendingPaymentsAsync(
        string userId);

    // ============= HELPER/VALIDATION =============

    /// <summary>
    /// Verify if user is admin of a specific unit
    /// </summary>
    Task<Result<bool>> IsAdminOfUnitAsync(string userId, int unitId);

    /// <summary>
    /// Verify if booking belongs to admin's unit
    /// </summary>
    Task<Result<bool>> IsBookingForMyUnitAsync(string userId, int bookingId);
}