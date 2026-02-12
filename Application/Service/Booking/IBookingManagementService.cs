using Application.Abstraction;
using Application.Contracts.BookingManagement;
using Application.Service.Avilabilaties;
using Domain.Entities;
using System.Runtime.Serialization;

namespace Application.Service.Booking;

/// <summary>
/// Unified booking management service for both Unit and SubUnit bookings
/// Provides comprehensive booking operations, queries, and analytics
/// </summary>
public interface IBookingManagementService
{
    // ============= BOOKING QUERIES =============

    /// <summary>
    /// Get all bookings with filters (both unit and subunit bookings)
    /// </summary>
    Task<Result<PaginatedResponse<UnifiedBookingResponse>>> GetAllBookingsAsync(
        UnifiedBookingFilter filter);

    /// <summary>
    /// Get bookings for a specific user
    /// </summary>
    Task<Result<PaginatedResponse<UnifiedBookingResponse>>> GetUserBookingsAsync(
        string userId,
        UnifiedBookingFilter filter);

    /// <summary>
    /// Get bookings for a specific unit (both full unit and subunit bookings)
    /// </summary>
    Task<Result<PaginatedResponse<UnifiedBookingResponse>>> GetUnitBookingsAsync(
        int unitId,
        UnifiedBookingFilter filter);

    /// <summary>
    /// Get booking details by ID (auto-detects booking type)
    /// </summary>
    Task<Result<UnifiedBookingDetailsResponse>> GetBookingByIdAsync(int bookingId);

    /// <summary>
    /// Get booking details by booking number
    /// </summary>
    Task<Result<UnifiedBookingDetailsResponse>> GetBookingByNumberAsync(string bookingNumber);

    /// <summary>
    /// Search bookings by guest name, email, or booking number
    /// </summary>
    Task<Result<PaginatedResponse<UnifiedBookingResponse>>> SearchBookingsAsync(
        string searchTerm,
        UnifiedBookingFilter filter);

    // ============= BOOKING MANAGEMENT =============

    /// <summary>
    /// Confirm a booking (works for both unit and subunit bookings)
    /// </summary>
    Task<Result> ConfirmBookingAsync(int bookingId, string adminUserId);

    /// <summary>
    /// Check-in a booking
    /// </summary>
    Task<Result> CheckInBookingAsync(int bookingId, CheckInRequest? request = null);

    /// <summary>
    /// Check-out a booking
    /// </summary>
    Task<Result> CheckOutBookingAsync(int bookingId, CheckOutRequest? request = null);

    /// <summary>
    /// Cancel a booking with refund calculation
    /// </summary>
    Task<Result<CancellationResponse>> CancelBookingAsync(
        int bookingId,
        string cancellationReason,
        string userId);

    /// <summary>
    /// Modify booking dates (if allowed by policy)
    /// </summary>
    Task<Result> ModifyBookingDatesAsync(
        int bookingId,
        DateTime newCheckIn,
        DateTime newCheckOut,
        string userId);

    // ============= PAYMENT OPERATIONS =============

    /// <summary>
    /// Process payment for a booking
    /// </summary>
    Task<Result> ProcessPaymentAsync(int bookingId, Contracts.BookingManagement.ProcessPaymentRequest request);

    /// <summary>
    /// Refund a booking
    /// </summary>
    Task<Result> RefundBookingAsync(int bookingId, RefundRequest request);

    /// <summary>
    /// Get payment history for a booking
    /// </summary>
    Task<Result<IEnumerable<PaymentHistoryItem>>> GetBookingPaymentHistoryAsync(int bookingId);

    /// <summary>
    /// Get pending payments
    /// </summary>
    Task<Result<PaginatedResponse<PendingPaymentResponse>>> GetPendingPaymentsAsync(
        PendingPaymentFilter filter);

    // ============= CALENDAR & SCHEDULING =============

    /// <summary>
    /// Get upcoming check-ins
    /// </summary>
    Task<Result<IEnumerable<UpcomingCheckInResponse>>> GetUpcomingCheckInsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? unitId = null);

    /// <summary>
    /// Get upcoming check-outs
    /// </summary>
    Task<Result<IEnumerable<UpcomingCheckOutResponse>>> GetUpcomingCheckOutsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? unitId = null);

    /// <summary>
    /// Get current guests (checked-in bookings)
    /// </summary>
    Task<Result<IEnumerable<CurrentGuestResponse>>> GetCurrentGuestsAsync(int? unitId = null);

    /// <summary>
    /// Get booking timeline for a specific date range
    /// </summary>
    Task<Result<BookingTimelineResponse>> GetBookingTimelineAsync(
        DateTime startDate,
        DateTime endDate,
        int? unitId = null);

    // ============= STATISTICS & ANALYTICS =============

    /// <summary>
    /// Get comprehensive booking statistics
    /// </summary>
    Task<Result<BookingStatisticsResponse>> GetBookingStatisticsAsync(
        BookingStatisticsFilter filter);

    /// <summary>
    /// Get revenue analytics
    /// </summary>
    Task<Result<RevenueAnalyticsResponse>> GetRevenueAnalyticsAsync(
        RevenueAnalyticsFilter filter);

    /// <summary>
    /// Get occupancy statistics
    /// </summary>
    Task<Result<OccupancyStatisticsResponse>> GetOccupancyStatisticsAsync(
        OccupancyFilter filter);

    /// <summary>
    /// Get cancellation analytics
    /// </summary>
    Task<Result<CancellationAnalyticsResponse>> GetCancellationAnalyticsAsync(
        CancellationAnalyticsFilter filter);

    /// <summary>
    /// Get booking trends over time
    /// </summary>
    Task<Result<BookingTrendsResponse>> GetBookingTrendsAsync(
        BookingTrendsFilter filter);

    // ============= REPORTS =============

    /// <summary>
    /// Generate daily report
    /// </summary>
    Task<Result<DailyBookingReportResponse>> GetDailyReportAsync(DateTime date);

    /// <summary>
    /// Generate monthly report
    /// </summary>
    Task<Result<MonthlyBookingReportResponse>> GetMonthlyReportAsync(int year, int month);

    /// <summary>
    /// Generate custom date range report
    /// </summary>
    Task<Result<CustomBookingReportResponse>> GetCustomReportAsync(
        DateTime startDate,
        DateTime endDate,
        ReportOptions options);

    // ============= BULK OPERATIONS =============

    /// <summary>
    /// Bulk confirm bookings
    /// </summary>
    Task<Result<BulkOperationResult>> BulkConfirmBookingsAsync(
        List<int> bookingIds,
        string adminUserId);

    /// <summary>
    /// Bulk cancel bookings
    /// </summary>
    Task<Result<BulkOperationResult>> BulkCancelBookingsAsync(
        List<int> bookingIds,
        string cancellationReason,
        string userId);

    // ============= NOTIFICATIONS =============

    /// <summary>
    /// Send reminder to guest
    /// </summary>
    Task<Result> SendBookingReminderAsync(int bookingId, ReminderType reminderType);

    /// <summary>
    /// Send custom message to guest
    /// </summary>
    Task<Result> SendCustomMessageToGuestAsync(
        int bookingId,
        string subject,
        string message);

    // ============= VALIDATION & UTILITIES =============

    /// <summary>
    /// Validate if booking can be modified
    /// </summary>
    Task<Result<BookingModificationValidation>> ValidateBookingModificationAsync(
        int bookingId,
        DateTime newCheckIn,
        DateTime newCheckOut);

    /// <summary>
    /// Calculate refund amount for a booking
    /// </summary>
    Task<Result<decimal>> CalculateRefundAmountAsync(
        int bookingId,
        DateTime? cancellationDate = null);

    /// <summary>
    /// Get booking summary for dashboard
    /// </summary>
    Task<Result<BookingDashboardSummary>> GetDashboardSummaryAsync(
        DateTime? startDate = null,
        DateTime? endDate = null);

    // ============= SUPPORTING CLASSES =============

    public class PaginatedResponse<T>
    {
        public IEnumerable<T> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int? NextPage { get; set; }
        public int? PrevPage { get; set; }
    }
}

// ============= ENUMS =============

public enum ReminderType
{
    CheckInReminder,
    CheckOutReminder,
    PaymentReminder,
    Custom
}