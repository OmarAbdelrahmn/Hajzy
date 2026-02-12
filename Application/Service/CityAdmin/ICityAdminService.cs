using Application.Abstraction;
using Application.Contracts.CityAdminContracts;
using Application.Service.Avilabilaties;

namespace Application.Service.CityAdmin;

public interface ICityAdminService
{
    // ============= DASHBOARD & OVERVIEW =============

    // ============= REVIEWS MANAGEMENT =============

    Task<Result<PaginatedResponse<ReviewResponse>>> GetNonVisibleReviewsAsync(int page = 1, int pageSize = 10);


    // ADD THIS NEW METHOD
    /// <summary>
    /// Toggle review visibility status
    /// </summary>
    Task<Result> ToggleReviewVisibilityAsync(
        string userId,
        int reviewId);

    /// <summary>
    /// Get department details by ID
    /// </summary>
    Task<Result<DepartmentDetailsResponse>> GetDepartmentDetailsByIdAsync(
        string userId,
        int departmentId);

    /// <summary>
    /// 
                              /// Get comprehensive dashboard data for city admin
                              /// </summary>
    Task<Result<CityAdminDashboardResponse>> GetDashboardAsync(string userId);

    /// <summary>
    /// Get quick statistics for city admin's department
    /// </summary>
    Task<Result<CityAdminStatsResponse>> GetQuickStatsAsync(string userId);

    // ============= DEPARTMENT/CITY MANAGEMENT =============

    /// <summary>
    /// Get department details that admin manages
    /// </summary>
    Task<Result<DepartmentDetailsResponse>> GetMyDepartmentDetailsAsync(string userId);

    /// <summary>
    /// Update department information
    /// </summary>
    Task<Result<DepartmentResponse>> UpdateMyDepartmentAsync(
        string userId,
        int departmentId,
        UpdateDepartmentRequest request);

    // ============= UNIT REGISTRATION REQUESTS =============

    /// <summary>
    /// Get all unit registration requests for the city
    /// </summary>
    Task<Result<PaginatedResponse<UnitRegistrationRequestResponse>>> GetRegistrationRequestsAsync(
        string userId,
        RegistrationRequestFilter filter);
    /// <summary>
    /// Get specific registration request details
    /// </summary>
    Task<Result<UnitRegistrationRequestDetailsResponse>> GetRegistrationRequestDetailsAsync(
        string userId,
        int requestId);

    /// <summary>
    /// Approve a unit registration request
    /// </summary>
    Task<Result> ApproveRegistrationRequestAsync(
        string userId,
        int requestId);

    /// <summary>
    /// Reject a unit registration request
    /// </summary>
    Task<Result> RejectRegistrationRequestAsync(
        string userId,
        int requestId,
        string rejectionReason);

    // ============= UNITS MANAGEMENT =============

    /// <summary>
    /// Get all units in the city
    /// </summary>
    Task<Result<PaginatedResponse<UnitComprehensiveResponse>>> GetCityUnitsAsync(
        string userId,
        UnitFilter filter);

    /// <summary>
    /// Get specific unit details
    /// </summary>
    Task<Result<UnitComprehensiveResponse>> GetUnitDetailsAsync(
        string userId,
        int unitId);

    /// <summary>
    /// Verify/Unverify a unit
    /// </summary>
    Task<Result> ToggleUnitVerificationAsync(
        string userId,
        int unitId);

    /// <summary>
    /// Feature/Unfeature a unit
    /// </summary>
    Task<Result> ToggleUnitFeaturedAsync(
        string userId,
        int unitId);

    /// <summary>
    /// Activate/Deactivate a unit
    /// </summary>
    Task<Result> ToggleUnitStatusAsync(
        string userId,
        int unitId);

    // ============= BOOKING MANAGEMENT =============

    /// <summary>
    /// Get all bookings in the city
    /// </summary>
    Task<Result<PaginatedResponse<BookingComprehensiveResponse>>> GetCityBookingsAsync(
        string userId,
        Contracts.CityAdminContracts.BookingFilter filter);

    /// <summary>
    /// Get specific booking details
    /// </summary>
    Task<Result<BookingDetailsResponse>> GetBookingDetailsAsync(
        string userId,
        int bookingId);

    // ============= BOOKING MANAGEMENT =============

    /// <summary>
    /// Confirm a booking (both unit and subunit bookings)
    /// </summary>
    Task<Result> ConfirmBookingAsync(string userId, int bookingId);

    /// <summary>
    /// Check-in a booking
    /// </summary>
    Task<Result> CheckInBookingAsync(string userId, int bookingId);

    /// <summary>
    /// Check-out a booking
    /// </summary>
    Task<Result> CheckOutBookingAsync(string userId, int bookingId);

    /// <summary>
    /// Cancel a booking
    /// </summary>
    Task<Result> CancelBookingAsync(string userId, int bookingId, string cancellationReason);

    /// <summary>
    /// Process payment for a booking
    /// </summary>
    Task<Result> ProcessBookingPaymentAsync(string userId, int bookingId, ProcessPaymentRequest request);

    /// <summary>
    /// Refund a booking
    /// </summary>
    Task<Result> RefundBookingAsync(string userId, int bookingId, decimal refundAmount, string reason);

    // ============= REVIEWS MANAGEMENT =============

    /// <summary>
    /// Get all reviews in the city
    /// </summary>
    Task<Result<PaginatedResponse<ReviewResponse>>> GetCityReviewsAsync(
        string userId,
        ReviewFilter filter);

    // ============= REVENUE & ANALYTICS =============

    /// <summary>
    /// Get revenue report for the city
    /// </summary>
    Task<Result<RevenueReportResponse>> GetCityRevenueReportAsync(
        string userId,
        RevenueReportFilter filter);

    /// <summary>
    /// Get booking analytics for the city
    /// </summary>
    Task<Result<BookingAnalyticsResponse>> GetCityBookingAnalyticsAsync(
        string userId,
        AnalyticsFilter filter);

    /// <summary>
    /// Get occupancy report for the city
    /// </summary>
    Task<Result<OccupancyReportResponse>> GetCityOccupancyReportAsync(
        string userId,
        DateTime startDate,
        DateTime endDate);

    // ============= SUBUNIT MANAGEMENT =============

    /// <summary>
    /// Get all subunits in the city
    /// </summary>
    Task<Result<PaginatedResponse<SubUnitComprehensiveDetail>>> GetCitySubUnitsAsync(
        string userId,
        int? unitId = null,
        int page = 1,
        int pageSize = 10);

    /// <summary>
    /// Get specific subunit details
    /// </summary>
    Task<Result<SubUnitComprehensiveDetail>> GetSubUnitDetailsAsync(
        string userId,
        int subUnitId);

    // ============= PAYMENT MANAGEMENT =============

    /// <summary>
    /// Get payment history for all city bookings
    /// </summary>
    Task<Result<IEnumerable<PaymentHistoryResponse>>> GetCityPaymentHistoryAsync(
        string userId,
        PaymentFilter filter);

    /// <summary>
    /// Get pending payments for city bookings
    /// </summary>
    Task<Result<IEnumerable<PendingPaymentResponse>>> GetCityPendingPaymentsAsync(string userId);

    // ============= DEPARTMENT IMAGE MANAGEMENT =============

    /// <summary>
    /// Get all images for the department
    /// </summary>
    Task<Result<IEnumerable<DepartmentImageResponse>>> GetDepartmentImagesAsync(
        string userId,
        int departmentId);

    /// <summary>
    /// Update department image order and primary status
    /// </summary>
    Task<Result> UpdateDepartmentImageOrderAsync(
        string userId,
        int departmentId,
        UpdateImageOrderRequest request);

    /// <summary>
    /// Delete department image
    /// </summary>
    Task<Result> DeleteDepartmentImageAsync(
        string userId,
        int imageId);

    /// <summary>
    /// Upload new department image
    /// </summary>
    Task<Result> UploadDepartmentImageAsync(
        string userId,
        UploadDepartmentImageRequest request);

    // ============= UNIT IMAGES MANAGEMENT =============

    /// <summary>
    /// Get all images for a unit in the city
    /// </summary>
    Task<Result<IEnumerable<ImageDetailResponse>>> GetUnitImagesAsync(
        string userId,
        int unitId);

    /// <summary>
    /// Delete unit image
    /// </summary>
    Task<Result> DeleteUnitImageAsync(
        string userId,
        int imageId);

    // ============= CALENDAR & SCHEDULING =============

    /// <summary>
    /// Get upcoming check-ins for the city
    /// </summary>
    Task<Result<IEnumerable<UpcomingCheckInResponse>>> GetCityUpcomingCheckInsAsync(
        string userId,
        int days = 1);

    /// <summary>
    /// Get upcoming check-outs for the city
    /// </summary>
    Task<Result<IEnumerable<UpcomingCheckOutResponse>>> GetCityUpcomingCheckOutsAsync(
        string userId,
        int days = 1);

    // ============= COMPREHENSIVE REPORTS
    // =============

    /// <summary>
    /// Get detailed financial report for the city
    /// </summary>
    Task<Result<FinancialReportResponse>> GetCityFinancialReportAsync(
        string userId,
        FinancialReportFilter filter);

    /// <summary>
    /// Get occupancy statistics for the city
    /// </summary>
    Task<Result<OccupancyStatisticsResponse>> GetCityOccupancyStatisticsAsync(
        string userId,
        OccupancyFilter filter);

    /// <summary>
    /// Get booking trends for the city
    /// </summary>
    Task<Result<BookingTrendsResponse>> GetCityBookingTrendsAsync(
        string userId,
        TrendsFilter filter);

    /// <summary>
    /// Get customer insights for the city
    /// </summary>
    Task<Result<CustomerInsightsResponse>> GetCityCustomerInsightsAsync(
        string userId,
        InsightsFilter filter);

    /// <summary>
    /// Get performance comparison between units in the city
    /// </summary>
    Task<Result<PerformanceComparisonResponse>> GetCityPerformanceComparisonAsync(
        string userId,
        ComparisonFilter filter);

    /// <summary>
    /// Get revenue breakdown for the city
    /// </summary>
    Task<Result<RevenueBreakdownResponse>> GetCityRevenueBreakdownAsync(
        string userId,
        RevenueBreakdownFilter filter);

    /// <summary>
    /// Get cancellation analytics for the city
    /// </summary>
    Task<Result<CancellationAnalyticsResponse>> GetCityCancellationAnalyticsAsync(
        string userId,
        CancellationFilter filter);

    /// <summary>
    /// Get amenities usage statistics in the city
    /// </summary>
    Task<Result<AmenitiesStatisticsResponse>> GetCityAmenitiesStatisticsAsync(string userId);

    // ============= UNIT ADMINS MANAGEMENT =============

    /// <summary>
    /// Get all unit admins in the city
    /// </summary>
    Task<Result<PaginatedResponse<UnitAdminResponse>>> GetCityUnitAdminsAsync(
        string userId,
        UnitAdminFilter filter);
    /// <summary>
    /// Assign admin to a unit
    /// </summary>
    Task<Result> AssignUnitAdminAsync(
        string userId,
        AssignUnitAdminRequest request);

    /// <summary>
    /// Remove admin from a unit
    /// </summary>
    Task<Result> RemoveUnitAdminAsync(
        string userId,
        int unitAdminId);

    /// <summary>
    /// Toggle unit admin status
    /// </summary>
    Task<Result> ToggleUnitAdminStatusAsync(
        string userId,
        int unitAdminId);

    // ============= POLICIES MANAGEMENT =============

    /// <summary>
    /// Get all policies in the city
    /// </summary>
    Task<Result<IEnumerable<PolicyDetailResponse>>> GetCityPoliciesAsync(
        string userId,
        PolicyFilter filter);

    /// <summary>
    /// Get cancellation policies usage in the city
    /// </summary>
    Task<Result<CancellationPolicyUsageResponse>> GetCityCancellationPolicyUsageAsync(
        string userId);

    // ============= AVAILABILITY OVERVIEW =============

    /// <summary>
    /// Get city-wide availability overview
    /// </summary>
    Task<Result<CityAvailabilityOverviewResponse>> GetCityAvailabilityOverviewAsync(
        string userId,
        DateTime startDate,
        DateTime endDate);

    // ============= OFFERS & ADS MANAGEMENT =============

    /// <summary>
    /// Get all offers in the city
    /// </summary>
    Task<Result<PaginatedResponse<OfferResponse>>> GetCityOffersAsync(
       string userId,
       OfferFilter filter);

    /// <summary>
    /// Get all ads in the city
    /// </summary>
    Task<Result<PaginatedResponse<AdResponse>>> GetCityAdsAsync(
        string userId,
        AdFilter filter);

    /// <summary>
    /// Approve/reject an offer
    /// </summary>
    Task<Result> ManageOfferAsync(
        string userId,
        int offerId);

    /// <summary>
    /// Approve/reject an ad
    /// </summary>
    Task<Result> ManageAdAsync(
        string userId,
        int adId);

    // ============= COUPONS MANAGEMENT =============

    /// <summary>
    /// Get all coupons for the city
    /// </summary>
    Task<Result<PaginatedResponse<CouponResponse>>> GetCityCouponsAsync(
        string userId,
        CouponFilter filter);
    /// <summary>
    /// Create city-wide coupon
    /// </summary>
    Task<Result<CouponResponse>> CreateCityCouponAsync(
        string userId,
        CreateCouponRequest request);

    /// <summary>
    /// Deactivate a coupon
    /// </summary>
    Task<Result> DeactivateCouponAsync(
        string userId,
        int couponId);

    // ============= NOTIFICATIONS MANAGEMENT =============

    /// <summary>
    /// Send notification to all unit admins in the city
    /// </summary>
    Task<Result> SendCityWideNotificationAsync(
        string userId,
        SendNotificationRequest request);

    /// <summary>
    /// Get city admin notifications
    /// </summary>
    Task<Result<IEnumerable<Application.Contracts.CityAdminContracts.AdminNotificationResponse>>> GetMyNotificationsAsync(
        string userId,
        NotificationFilter filter);

    /// <summary>
    /// Mark notification as read
    /// </summary>
    Task<Result> MarkNotificationAsReadAsync(
        string userId,
        int notificationId);

    // ============= USER/GUEST MANAGEMENT =============

    /// <summary>
    /// Get all users who booked in the city
    /// </summary>
    Task<Result<PaginatedResponse<CityUserResponse>>> GetCityUsersAsync(
        string userId,
        UserFilter filter);

    /// <summary>
    /// Get user booking history in the city
    /// </summary>
    Task<Result<IEnumerable<BookingComprehensiveResponse>>> GetUserBookingHistoryAsync(
        string userId,
        string targetUserId);

    // ============= LOYALTY PROGRAM =============

    /// <summary>
    /// Get loyalty program statistics for the city
    /// </summary>
    Task<Result<LoyaltyProgramStatisticsResponse>> GetCityLoyaltyStatisticsAsync(
        string userId);

    // ============= HELPER/VALIDATION =============

    /// <summary>
    /// Verify if user is admin of a specific department
    /// </summary>
    Task<Result<bool>> IsCityAdminAsync(string userId, int departmentId);

    /// <summary>
    /// Verify if unit belongs to admin's city
    /// </summary>
    Task<Result<bool>> IsUnitInMyCityAsync(string userId, int unitId);

    /// <summary>
    /// Get admin's department ID
    /// </summary>
    Task<Result<int>> GetAdminDepartmentIdAsync(string userId);

    // ============= BULK OPERATIONS =============

    /// <summary>
    /// Bulk verify units
    /// </summary>
    Task<Result> BulkVerifyUnitsAsync(
        string userId,
        BulkUnitActionRequest request);

    /// <summary>
    /// Bulk feature units
    /// </summary>
    Task<Result> BulkFeatureUnitsAsync(
        string userId,
        BulkUnitActionRequest request);

    /// <summary>
    /// Bulk activate/deactivate units
    /// </summary>
    Task<Result> BulkToggleUnitStatusAsync(
        string userId,
        BulkUnitActionRequest request);

    /// <summary>
    /// Get all reviews for a specific unit (paginated)
    /// </summary>
    Task<Result<PaginatedResponse<PublicReviewResponse>>> GetUnitReviewsAsync(
        int unitId,
        int page = 1,
        int pageSize = 10);

    /// <summary>
    /// Get statistics for a specific unit
    /// </summary>
    Task<Result<UnitStatisticsResponse>> GetUnitStatisticsAsync(int unitId);
}