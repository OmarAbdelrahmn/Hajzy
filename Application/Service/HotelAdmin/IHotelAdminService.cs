using Application.Abstraction;
using Application.Contracts.hoteladmincont;

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

    /// <summary>
    /// Get all policies for admin's unit
    /// </summary>
    Task<Result<IEnumerable<PolicyDetailResponse>>> GetUnitPoliciesAsync(
        string userId,
        int unitId);

    /// <summary>
    /// Create new policy for unit
    /// </summary>
    Task<Result<PolicyDetailResponse>> CreateUnitPolicyAsync(
        string userId,
        int unitId,
        CreatePolicyRequest request);

    /// <summary>
    /// Update existing policy
    /// </summary>
    Task<Result<PolicyDetailResponse>> UpdateUnitPolicyAsync(
        string userId,
        int policyId,
        UpdatePolicyRequest request);

    /// <summary>
    /// Delete/deactivate policy
    /// </summary>
    Task<Result> DeleteUnitPolicyAsync(
        string userId,
        int policyId);

    /// <summary>
    /// Get all policies for a subunit
    /// </summary>
    Task<Result<IEnumerable<PolicyDetailResponse>>> GetSubUnitPoliciesAsync(
        string userId,
        int subUnitId);

    /// <summary>
    /// Create policy for subunit
    /// </summary>
    Task<Result<PolicyDetailResponse>> CreateSubUnitPolicyAsync(
        string userId,
        int subUnitId,
        CreatePolicyRequest request);

    // ============= CANCELLATION POLICY MANAGEMENT =============

    /// <summary>
    /// Get all available cancellation policies
    /// </summary>
    Task<Result<IEnumerable<CancellationPolicyResponse>>> GetAvailableCancellationPoliciesAsync();

    /// <summary>
    /// Get unit's current cancellation policy
    /// </summary>
    Task<Result<CancellationPolicyResponse>> GetUnitCancellationPolicyAsync(
        string userId,
        int unitId);

    /// <summary>
    /// Set/Update cancellation policy for unit
    /// </summary>
    Task<Result> SetUnitCancellationPolicyAsync(
        string userId,
        int unitId,
        int cancellationPolicyId);

    /// <summary>
    /// Create custom cancellation policy
    /// </summary>
    Task<Result<CancellationPolicyResponse>> CreateCustomCancellationPolicyAsync(
        string userId,
        CreateCancellationPolicyRequest request);

    // ============= COMPREHENSIVE AVAILABILITY MANAGEMENT =============

    /// <summary>
    /// Get unit availability calendar with all details (bookings, blocks, prices)
    /// </summary>
    Task<Result<Dictionary<DateTime, Contracts.Availability.UnitDayAvailability>>> GetUnitAvailabilityCalendarAsync(
        string userId,
        int unitId,
        int year,
        int month);

    /// <summary>
    /// Get subunit availability calendar
    /// </summary>
    Task<Result<Dictionary<DateTime, Contracts.Availability.SubUnitDayAvailability>>> GetSubUnitAvailabilityCalendarAsync(
        string userId,
        int subUnitId,
        int year,
        int month);

    /// <summary>
    /// Set unit availability (block/unblock dates)
    /// </summary>
    Task<Result> SetUnitAvailabilityAsync(
        string userId,
        Contracts.Availability.SetUnitAvailabilityRequest request);

    /// <summary>
    /// Set subunit availability with pricing
    /// </summary>
    Task<Result> SetSubUnitAvailabilityAsync(
        string userId,
        Contracts.Availability.SetSubUnitAvailabilityRequest request);

    /// <summary>
    /// Block multiple dates for unit
    /// </summary>
    Task<Result> BlockUnitDatesAsync(
        string userId,
        Contracts.Availability.BlockUnitDatesRequest request);

    /// <summary>
    /// Block multiple dates for subunit
    /// </summary>
    Task<Result> BlockSubUnitDatesAsync(
        string userId,
        Contracts.Availability.BlockDatesRequest request);

    /// <summary>
    /// Set special pricing for subunit date ranges
    /// </summary>
    Task<Result> SetSubUnitSpecialPricingAsync(
        string userId,
        Contracts.Availability.SetSpecialPricingRequest request);

    /// <summary>
    /// Unblock dates (remove availability block)
    /// </summary>
    Task<Result> UnblockDatesAsync(
        string userId,
        int availabilityId);

    /// <summary>
    /// Get all blocked dates for unit
    /// </summary>
    Task<Result<IEnumerable<AvailabilityBlockResponse>>> GetUnitBlockedDatesAsync(
        string userId,
        int unitId,
        DateTime? startDate = null,
        DateTime? endDate = null);

    /// <summary>
    /// Get all blocked dates for subunit
    /// </summary>
    Task<Result<IEnumerable<AvailabilityBlockResponse>>> GetSubUnitBlockedDatesAsync(
        string userId,
        int subUnitId,
        DateTime? startDate = null,
        DateTime? endDate = null);

    /// <summary>
    /// Check unit availability for specific dates
    /// </summary>
    Task<Result<UnitAvailabilityStatus>> CheckUnitAvailabilityAsync(
        string userId,
        int unitId,
        DateTime checkIn,
        DateTime checkOut);

    /// <summary>
    /// Check subunit availability for specific dates
    /// </summary>
    Task<Result<SubUnitAvailabilityStatus>> CheckSubUnitAvailabilityAsync(
        string userId,
        int subUnitId,
        DateTime checkIn,
        DateTime checkOut);

    /// <summary>
    /// Get available subunits for a date range
    /// </summary>
    Task<Result<List<AvailableSubUnitInfo>>> GetAvailableSubUnitsAsync(
        string userId,
        int unitId,
        DateTime checkIn,
        DateTime checkOut);

    // ============= COMPREHENSIVE REPORTS =============

    /// <summary>
    /// Get detailed financial report
    /// </summary>
    Task<Result<FinancialReportResponse>> GetFinancialReportAsync(
        string userId,
        FinancialReportFilter filter);

    /// <summary>
    /// Get occupancy statistics report
    /// </summary>
    Task<Result<OccupancyStatisticsResponse>> GetOccupancyStatisticsAsync(
        string userId,
        OccupancyFilter filter);

    /// <summary>
    /// Get booking trends and patterns
    /// </summary>
    Task<Result<BookingTrendsResponse>> GetBookingTrendsAsync(
        string userId,
        TrendsFilter filter);

    /// <summary>
    /// Get customer insights report
    /// </summary>
    Task<Result<CustomerInsightsResponse>> GetCustomerInsightsAsync(
        string userId,
        InsightsFilter filter);

    /// <summary>
    /// Get performance comparison report (compare units)
    /// </summary>
    Task<Result<PerformanceComparisonResponse>> GetPerformanceComparisonAsync(
        string userId,
        ComparisonFilter filter);

    /// <summary>
    /// Get revenue breakdown by source
    /// </summary>
    Task<Result<RevenueBreakdownResponse>> GetRevenueBreakdownAsync(
        string userId,
        RevenueBreakdownFilter filter);

    /// <summary>
    /// Get cancellation analytics
    /// </summary>
    Task<Result<CancellationAnalyticsResponse>> GetCancellationAnalyticsAsync(
        string userId,
        CancellationFilter filter);

    /// <summary>
    /// Get pricing optimization suggestions
    /// </summary>
    Task<Result<PricingOptimizationResponse>> GetPricingOptimizationSuggestionsAsync(
        string userId,
        int unitId,
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Export comprehensive report to Excel
    /// </summary>
    Task<Result<byte[]>> ExportReportToExcelAsync(
        string userId,
        ExportReportRequest request);

    /// <summary>
    /// Export report to PDF
    /// </summary>
    Task<Result<byte[]>> ExportReportToPdfAsync(
        string userId,
        ExportReportRequest request);

    // ============= SUBUNIT MANAGEMENT (Extended) =============

    /// <summary>
    /// Update subunit details
    /// </summary>
    Task<Result<SubUnitResponse>> UpdateSubUnitAsync(
        string userId,
        int subUnitId,
        UpdateSubUnitRequest request);

    /// <summary>
    /// Update subunit pricing
    /// </summary>
    Task<Result> UpdateSubUnitPricingAsync(
        string userId,
        int subUnitId,
        UpdateSubUnitPricingRequest request);

    /// <summary>
    /// Toggle subunit active status
    /// </summary>
    Task<Result> ToggleSubUnitStatusAsync(
        string userId,
        int subUnitId,
        bool isAvailable);

    /// <summary>
    /// Get subunit booking history
    /// </summary>
    Task<Result<IEnumerable<SubUnitBookingHistoryResponse>>> GetSubUnitBookingHistoryAsync(
        string userId,
        int subUnitId,
        DateTime? startDate = null,
        DateTime? endDate = null);

    // ============= AMENITIES MANAGEMENT =============

    /// <summary>
    /// Get all available amenities
    /// </summary>
    Task<Result<IEnumerable<AmenityResponse>>> GetAvailableAmenitiesAsync();

    /// <summary>
    /// Get unit amenities
    /// </summary>
    Task<Result<IEnumerable<AmenityResponse>>> GetUnitAmenitiesAsync(
        string userId,
        int unitId);

    /// <summary>
    /// Update unit amenities
    /// </summary>
    Task<Result> UpdateUnitAmenitiesAsync(
        string userId,
        int unitId,
        UpdateAmenitiesRequest request);

    /// <summary>
    /// Get subunit amenities
    /// </summary>
    Task<Result<IEnumerable<AmenityResponse>>> GetSubUnitAmenitiesAsync(
        string userId,
        int subUnitId);

    /// <summary>
    /// Update subunit amenities
    /// </summary>
    Task<Result> UpdateSubUnitAmenitiesAsync(
        string userId,
        int subUnitId,
        UpdateAmenitiesRequest request);

    // ============= IMAGE MANAGEMENT =============

    /// <summary>
    /// Get unit images
    /// </summary>
    Task<Result<IEnumerable<ImageDetailResponse>>> GetUnitImagesAsync(
        string userId,
        int unitId);

    /// <summary>
    /// Update image order and primary status
    /// </summary>
    Task<Result> UpdateImageOrderAsync(
        string userId,
        int unitId,
        UpdateImageOrderRequest request);

    /// <summary>
    /// Delete unit image
    /// </summary>
    Task<Result> DeleteUnitImageAsync(
        string userId,
        int imageId);

    /// <summary>
    /// Get subunit images
    /// </summary>
    Task<Result<IEnumerable<ImageDetailResponse>>> GetSubUnitImagesAsync(
        string userId,
        int subUnitId);

    /// <summary>
    /// Delete subunit image
    /// </summary>
    Task<Result> DeleteSubUnitImageAsync(
        string userId,
        int imageId);

    // ============= NOTIFICATIONS & ALERTS =============

    /// <summary>
    /// Get admin notifications
    /// </summary>
    //Task<Result<IEnumerable<AdminNotificationResponse>>> GetAdminNotificationsAsync(
    //    string userId,
    //    NotificationFilter filter);

    ///// <summary>
    ///// Mark notification as read
    ///// </summary>
    //Task<Result> MarkNotificationAsReadAsync(
    //    string userId,
    //    int notificationId);

    ///// <summary>
    ///// Get alert settings
    ///// </summary>
    //Task<Result<AlertSettingsResponse>> GetAlertSettingsAsync(string userId);

    ///// <summary>
    ///// Update alert settings
    ///// </summary>
    //Task<Result> UpdateAlertSettingsAsync(
    //    string userId,
    //    UpdateAlertSettingsRequest request);

    // ============= BULK OPERATIONS =============

    /// <summary>
    /// Bulk update subunit availability
    /// </summary>
    //Task<Result> BulkUpdateSubUnitAvailabilityAsync(
    //    string userId,
    //    BulkAvailabilityUpdateRequest request);

    ///// <summary>
    ///// Bulk update pricing
    ///// </summary>
    //Task<Result> BulkUpdatePricingAsync(
    //    string userId,
    //    BulkPricingUpdateRequest request);

    /// <summary>
    /// Copy availability from one subunit to others
    /// </summary>
    //Task<Result> CopyAvailabilitySettingsAsync(
    //    string userId,
    //    CopyAvailabilityRequest request);
}