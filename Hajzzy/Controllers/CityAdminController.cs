using Application.Abstraction.Consts;
using Application.Contracts.CityAdminContracts;
using Application.Extensions;
using Application.Service.CityAdmin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hajzzy.Controllers;

[Route("api/city-admin")]
[ApiController]
[Authorize(Roles = DefaultRoles.CityAdmin)]
public class CityAdminController(ICityAdminService cityAdminService) : ControllerBase
{
    private readonly ICityAdminService _cityAdminService = cityAdminService;

    #region DASHBOARD & OVERVIEW

    /// <summary>
    /// Get comprehensive dashboard data for city admin
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetDashboardAsync(userId!);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get quick statistics for city admin's department
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetQuickStats()
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetQuickStatsAsync(userId!);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    #endregion

    #region DEPARTMENT/CITY MANAGEMENT

    /// <summary>
    /// Get department details that admin manages
    /// </summary>
    [HttpGet("my-department")]
    public async Task<IActionResult> GetMyDepartmentDetails()
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetMyDepartmentDetailsAsync(userId!);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Update department information
    /// </summary>
    [HttpPut("departments/{departmentId}")]
    public async Task<IActionResult> UpdateMyDepartment(
        int departmentId,
        [FromBody] UpdateDepartmentRequest request)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.UpdateMyDepartmentAsync(userId!, departmentId, request);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    #endregion

    #region UNIT REGISTRATION REQUESTS

    /// <summary>
    /// Get all unit registration requests for the city
    /// </summary>
    [HttpGet("registration-requests")]
    public async Task<IActionResult> GetRegistrationRequests([FromQuery] RegistrationRequestFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetRegistrationRequestsAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get specific registration request details
    /// </summary>
    [HttpGet("registration-requests/{requestId}")]
    public async Task<IActionResult> GetRegistrationRequestDetails(int requestId)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetRegistrationRequestDetailsAsync(userId!, requestId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Approve a unit registration request
    /// </summary>
    [HttpPost("registration-requests/{requestId}/approve")]
    public async Task<IActionResult> ApproveRegistrationRequest(int requestId)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.ApproveRegistrationRequestAsync(userId!, requestId);
        return result.IsSuccess
            ? Ok(new { message = "Registration request approved successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Reject a unit registration request
    /// </summary>
    [HttpPost("registration-requests/{requestId}/reject")]
    public async Task<IActionResult> RejectRegistrationRequest(
        int requestId,
        string RejectionReason)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.RejectRegistrationRequestAsync(
            userId!, requestId, RejectionReason);
        return result.IsSuccess
            ? Ok(new { message = "Registration request rejected successfully" })
            : result.ToProblem();
    }

    #endregion

    #region UNITS MANAGEMENT

    /// <summary>
    /// Get all units in the city
    /// </summary>
    [HttpGet("units")]
    public async Task<IActionResult> GetCityUnits([FromQuery] UnitFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetCityUnitsAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get specific unit details
    /// </summary>
    [HttpGet("units/{unitId}")]
    public async Task<IActionResult> GetUnitDetails(int unitId)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetUnitDetailsAsync(userId!, unitId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Toggle unit verification status
    /// </summary>
    [HttpPatch("units/{unitId}/verify")]
    public async Task<IActionResult> ToggleUnitVerification(
        int unitId,
        [FromBody] bool isVerified)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.ToggleUnitVerificationAsync(userId!, unitId, isVerified);
        return result.IsSuccess
            ? Ok(new { message = $"Unit {(isVerified ? "verified" : "unverified")} successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Toggle unit featured status
    /// </summary>
    [HttpPatch("units/{unitId}/feature")]
    public async Task<IActionResult> ToggleUnitFeatured(
        int unitId,
        [FromBody] bool isFeatured)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.ToggleUnitFeaturedAsync(userId!, unitId, isFeatured);
        return result.IsSuccess
            ? Ok(new { message = $"Unit {(isFeatured ? "featured" : "unfeatured")} successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Toggle unit active status
    /// </summary>
    [HttpPatch("units/{unitId}/status")]
    public async Task<IActionResult> ToggleUnitStatus(
        int unitId,
        [FromBody] bool isActive)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.ToggleUnitStatusAsync(userId!, unitId, isActive);
        return result.IsSuccess
            ? Ok(new { message = $"Unit {(isActive ? "activated" : "deactivated")} successfully" })
            : result.ToProblem();
    }

    #endregion

    #region BOOKING MANAGEMENT

    /// <summary>
    /// Get all bookings in the city
    /// </summary>
    [HttpGet("bookings")]
    public async Task<IActionResult> GetCityBookings([FromQuery] BookingFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetCityBookingsAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get specific booking details
    /// </summary>
    [HttpGet("bookings/{bookingId}")]
    public async Task<IActionResult> GetBookingDetails(int bookingId)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetBookingDetailsAsync(userId!, bookingId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    #endregion

    #region REVIEWS MANAGEMENT

    /// <summary>
    /// Get all reviews in the city
    /// </summary>
    [HttpGet("reviews")]
    public async Task<IActionResult> GetCityReviews([FromQuery] ReviewFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetCityReviewsAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    #endregion

    #region REVENUE & ANALYTICS

    /// <summary>
    /// Get revenue report for the city
    /// </summary>
    [HttpGet("reports/revenue")]
    public async Task<IActionResult> GetCityRevenueReport([FromQuery] RevenueReportFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetCityRevenueReportAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get booking analytics for the city
    /// </summary>
    [HttpGet("reports/analytics")]
    public async Task<IActionResult> GetCityBookingAnalytics([FromQuery] AnalyticsFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetCityBookingAnalyticsAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get occupancy report for the city
    /// </summary>
    [HttpGet("reports/occupancy")]
    public async Task<IActionResult> GetCityOccupancyReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetCityOccupancyReportAsync(userId!, startDate, endDate);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    #endregion

    #region SUBUNIT MANAGEMENT

    /// <summary>
    /// Get all subunits in the city
    /// </summary>
    [HttpGet("subunits")]
    public async Task<IActionResult> GetCitySubUnits([FromQuery] int? unitId = null)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetCitySubUnitsAsync(userId!, unitId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get specific subunit details
    /// </summary>
    [HttpGet("subunits/{subUnitId}")]
    public async Task<IActionResult> GetSubUnitDetails(int subUnitId)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetSubUnitDetailsAsync(userId!, subUnitId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    #endregion

    #region PAYMENT MANAGEMENT

    /// <summary>
    /// Get payment history for all city bookings
    /// </summary>
    [HttpGet("payments/history")]
    public async Task<IActionResult> GetCityPaymentHistory([FromQuery] PaymentFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetCityPaymentHistoryAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get pending payments for city bookings
    /// </summary>
    [HttpGet("payments/pending")]
    public async Task<IActionResult> GetCityPendingPayments()
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetCityPendingPaymentsAsync(userId!);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    #endregion

    #region DEPARTMENT IMAGE MANAGEMENT

    /// <summary>
    /// Get all images for the department
    /// </summary>
    [HttpGet("departments/{departmentId}/images")]
    public async Task<IActionResult> GetDepartmentImages(int departmentId)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetDepartmentImagesAsync(userId!, departmentId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Update department image order and primary status
    /// </summary>
    [HttpPut("departments/{departmentId}/images/order")]
    public async Task<IActionResult> UpdateDepartmentImageOrder(
        int departmentId,
        [FromBody] UpdateImageOrderRequest request)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.UpdateDepartmentImageOrderAsync(userId!, departmentId, request);
        return result.IsSuccess
            ? Ok(new { message = "Image order updated successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Delete department image
    /// </summary>
    [HttpDelete("departments/images/{imageId}")]
    public async Task<IActionResult> DeleteDepartmentImage(int imageId)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.DeleteDepartmentImageAsync(userId!, imageId);
        return result.IsSuccess
            ? Ok(new { message = "Image deleted successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Upload new department image
    /// </summary>
    [HttpPost("departments/{departmentId}/images")]
    public async Task<IActionResult> UploadDepartmentImage(
        int departmentId,
        [FromBody] UploadDepartmentImageRequest request)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.UploadDepartmentImageAsync(userId!, departmentId, request);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    #endregion

    #region UNIT IMAGES MANAGEMENT

    /// <summary>
    /// Get all images for a unit in the city
    /// </summary>
    [HttpGet("units/{unitId}/images")]
    public async Task<IActionResult> GetUnitImages(int unitId)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetUnitImagesAsync(userId!, unitId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Delete unit image
    /// </summary>
    [HttpDelete("units/images/{imageId}")]
    public async Task<IActionResult> DeleteUnitImage(int imageId)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.DeleteUnitImageAsync(userId!, imageId);
        return result.IsSuccess
            ? Ok(new { message = "Unit image deleted successfully" })
            : result.ToProblem();
    }

    #endregion

    #region CALENDAR & SCHEDULING

    /// <summary>
    /// Get upcoming check-ins for the city
    /// </summary>
    [HttpGet("schedule/check-ins")]
    public async Task<IActionResult> GetCityUpcomingCheckIns([FromQuery] int days = 1)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetCityUpcomingCheckInsAsync(userId!, days);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get upcoming check-outs for the city
    /// </summary>
    [HttpGet("schedule/check-outs")]
    public async Task<IActionResult> GetCityUpcomingCheckOuts([FromQuery] int days = 1)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetCityUpcomingCheckOutsAsync(userId!, days);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    #endregion

    #region COMPREHENSIVE REPORTS

    /// <summary>
    /// Get detailed financial report for the city
    /// </summary>
    [HttpGet("reports/financial")]
    public async Task<IActionResult> GetCityFinancialReport([FromQuery] FinancialReportFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetCityFinancialReportAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get occupancy statistics for the city
    /// </summary>
    [HttpGet("reports/occupancy-statistics")]
    public async Task<IActionResult> GetCityOccupancyStatistics([FromQuery] OccupancyFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetCityOccupancyStatisticsAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get booking trends for the city
    /// </summary>
    [HttpGet("reports/booking-trends")]
    public async Task<IActionResult> GetCityBookingTrends([FromQuery] TrendsFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetCityBookingTrendsAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get customer insights for the city
    /// </summary>
    [HttpGet("reports/customer-insights")]
    public async Task<IActionResult> GetCityCustomerInsights([FromQuery] InsightsFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetCityCustomerInsightsAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get performance comparison between units in the city
    /// </summary>
    [HttpGet("reports/performance-comparison")]
    public async Task<IActionResult> GetCityPerformanceComparison([FromQuery] ComparisonFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetCityPerformanceComparisonAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get revenue breakdown for the city
    /// </summary>
    [HttpGet("reports/revenue-breakdown")]
    public async Task<IActionResult> GetCityRevenueBreakdown([FromQuery] RevenueBreakdownFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetCityRevenueBreakdownAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get cancellation analytics for the city
    /// </summary>
    [HttpGet("reports/cancellation-analytics")]
    public async Task<IActionResult> GetCityCancellationAnalytics([FromQuery] CancellationFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetCityCancellationAnalyticsAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    #endregion

    #region AMENITIES MANAGEMENT

    /// <summary>
    /// Get amenities usage statistics in the city
    /// </summary>
    [HttpGet("amenities/statistics")]
    public async Task<IActionResult> GetCityAmenitiesStatistics()
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetCityAmenitiesStatisticsAsync(userId!);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    #endregion

    #region UNIT ADMINS MANAGEMENT

    /// <summary>
    /// Get all unit admins in the city
    /// </summary>
    [HttpGet("unit-admins")]
    public async Task<IActionResult> GetCityUnitAdmins([FromQuery] UnitAdminFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetCityUnitAdminsAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Assign admin to a unit
    /// </summary>
    [HttpPost("unit-admins")]
    public async Task<IActionResult> AssignUnitAdmin([FromBody] AssignUnitAdminRequest request)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.AssignUnitAdminAsync(userId!, request);
        return result.IsSuccess
            ? Ok(new { message = "Unit admin assigned successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Remove admin from a unit
    /// </summary>
    [HttpDelete("unit-admins/{unitAdminId}")]
    public async Task<IActionResult> RemoveUnitAdmin(int unitAdminId)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.RemoveUnitAdminAsync(userId!, unitAdminId);
        return result.IsSuccess
            ? Ok(new { message = "Unit admin removed successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Toggle unit admin status
    /// </summary>
    [HttpPatch("unit-admins/{unitAdminId}/status")]
    public async Task<IActionResult> ToggleUnitAdminStatus(
        int unitAdminId,
        [FromBody] bool isActive)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.ToggleUnitAdminStatusAsync(userId!, unitAdminId, isActive);
        return result.IsSuccess
            ? Ok(new { message = $"Unit admin {(isActive ? "activated" : "deactivated")} successfully" })
            : result.ToProblem();
    }

    #endregion

    #region POLICIES MANAGEMENT

    /// <summary>
    /// Get all policies in the city
    /// </summary>
    [HttpGet("policies")]
    public async Task<IActionResult> GetCityPolicies([FromQuery] PolicyFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetCityPoliciesAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get cancellation policies usage in the city
    /// </summary>
    [HttpGet("policies/cancellation/usage")]
    public async Task<IActionResult> GetCityCancellationPolicyUsage()
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetCityCancellationPolicyUsageAsync(userId!);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    #endregion

    #region AVAILABILITY OVERVIEW

    /// <summary>
    /// Get city-wide availability overview
    /// </summary>
    [HttpGet("availability/overview")]
    public async Task<IActionResult> GetCityAvailabilityOverview(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetCityAvailabilityOverviewAsync(userId!, startDate, endDate);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    #endregion

    #region OFFERS & ADS MANAGEMENT

    /// <summary>
    /// Get all offers in the city
    /// </summary>
    [HttpGet("offers")]
    public async Task<IActionResult> GetCityOffers([FromQuery] OfferFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetCityOffersAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get all ads in the city
    /// </summary>
    [HttpGet("ads")]
    public async Task<IActionResult> GetCityAds([FromQuery] AdFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetCityAdsAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Approve/reject an offer
    /// </summary>
    [HttpPatch("offers/{offerId}/manage")]
    public async Task<IActionResult> ManageOffer(
        int offerId,
        [FromBody] bool isApproved)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.ManageOfferAsync(userId!, offerId, isApproved);
        return result.IsSuccess
            ? Ok(new { message = $"Offer {(isApproved ? "approved" : "rejected")} successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Approve/reject an ad
    /// </summary>
    [HttpPatch("ads/{adId}/manage")]
    public async Task<IActionResult> ManageAd(
        int adId,
        [FromBody] bool isApproved)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.ManageAdAsync(userId!, adId, isApproved);
        return result.IsSuccess
            ? Ok(new { message = $"Ad {(isApproved ? "approved" : "rejected")} successfully" })
            : result.ToProblem();
    }

    #endregion

    #region COUPONS MANAGEMENT

    /// <summary>
    /// Get all coupons for the city
    /// </summary>
    [HttpGet("coupons")]
    public async Task<IActionResult> GetCityCoupons([FromQuery] CouponFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetCityCouponsAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Create city-wide coupon
    /// </summary>
    [HttpPost("coupons")]
    public async Task<IActionResult> CreateCityCoupon([FromBody] CreateCouponRequest request)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.CreateCityCouponAsync(userId!, request);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Deactivate a coupon
    /// </summary>
    [HttpPatch("coupons/{couponId}/deactivate")]
    public async Task<IActionResult> DeactivateCoupon(int couponId)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.DeactivateCouponAsync(userId!, couponId);
        return result.IsSuccess
            ? Ok(new { message = "Coupon deactivated successfully" })
            : result.ToProblem();
    }

    #endregion

    #region NOTIFICATIONS MANAGEMENT

    /// <summary>
    /// Send notification to all unit admins in the city
    /// </summary>
    [HttpPost("notifications/send")]
    public async Task<IActionResult> SendCityWideNotification([FromBody] SendNotificationRequest request)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.SendCityWideNotificationAsync(userId!, request);
        return result.IsSuccess
            ? Ok(new { message = "Notification sent successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Get city admin notifications
    /// </summary>
    [HttpGet("notifications")]
    public async Task<IActionResult> GetMyNotifications([FromQuery] NotificationFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetMyNotificationsAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Mark notification as read
    /// </summary>
    [HttpPatch("notifications/{notificationId}/mark-read")]
    public async Task<IActionResult> MarkNotificationAsRead(int notificationId)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.MarkNotificationAsReadAsync(userId!, notificationId);
        return result.IsSuccess
            ? Ok(new { message = "Notification marked as read" })
            : result.ToProblem();
    }

    #endregion

    #region USER/GUEST MANAGEMENT

    /// <summary>
    /// Get all users who booked in the city
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetCityUsers([FromQuery] UserFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetCityUsersAsync(userId!, filter);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get user booking history in the city
    /// </summary>
    [HttpGet("users/{targetUserId}/booking-history")]
    public async Task<IActionResult> GetUserBookingHistory(string targetUserId)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetUserBookingHistoryAsync(userId!, targetUserId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    #endregion

    #region LOYALTY PROGRAM

    /// <summary>
    /// Get loyalty program statistics for the city
    /// </summary>
    [HttpGet("loyalty/statistics")]
    public async Task<IActionResult> GetCityLoyaltyStatistics()
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetCityLoyaltyStatisticsAsync(userId!);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    #endregion

    #region HELPER/VALIDATION

    /// <summary>
    /// Verify if user is admin of a specific department
    /// </summary>
    [HttpGet("departments/{departmentId}/verify-access")]
    public async Task<IActionResult> VerifyDepartmentAccess(int departmentId)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.IsCityAdminAsync(userId!, departmentId);
        return result.IsSuccess ? Ok(new { hasAccess = result.Value }) : result.ToProblem();
    }

    /// <summary>
    /// Verify if unit belongs to admin's city
    /// </summary>
    [HttpGet("units/{unitId}/verify-access")]
    public async Task<IActionResult> VerifyUnitAccess(int unitId)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.IsUnitInMyCityAsync(userId!, unitId);
        return result.IsSuccess ? Ok(new { hasAccess = result.Value }) : result.ToProblem();
    }

    /// <summary>
    /// Get admin's department ID
    /// </summary>
    [HttpGet("my-department-id")]
    public async Task<IActionResult> GetAdminDepartmentId()
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.GetAdminDepartmentIdAsync(userId!);
        return result.IsSuccess ? Ok(new { departmentId = result.Value }) : result.ToProblem();
    }

    #endregion

    #region BULK OPERATIONS

    /// <summary>
    /// Bulk verify units
    /// </summary>
    [HttpPost("units/bulk-verify")]
    public async Task<IActionResult> BulkVerifyUnits([FromBody] BulkUnitActionRequest request)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.BulkVerifyUnitsAsync(userId!, request);
        return result.IsSuccess
            ? Ok(new { message = $"{request.UnitIds.Count} units verified successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Bulk feature units
    /// </summary>
    [HttpPost("units/bulk-feature")]
    public async Task<IActionResult> BulkFeatureUnits([FromBody] BulkUnitActionRequest request)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.BulkFeatureUnitsAsync(userId!, request);
        return result.IsSuccess
            ? Ok(new { message = $"{request.UnitIds.Count} units featured successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Bulk activate/deactivate units
    /// </summary>
    [HttpPost("units/bulk-toggle-status")]
    public async Task<IActionResult> BulkToggleUnitStatus([FromBody] BulkUnitActionRequest request)
    {
        var userId = User.GetUserId();
        var result = await _cityAdminService.BulkToggleUnitStatusAsync(userId!, request);
        return result.IsSuccess
            ? Ok(new { message = $"{request.UnitIds.Count} units status updated successfully" })
            : result.ToProblem();
    }
}

#endregion