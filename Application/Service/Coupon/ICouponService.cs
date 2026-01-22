using Application.Abstraction;
using Application.Contracts.couponcontract;
using Domain;

namespace Application.Service.PromoCode;

public interface ICouponService
{
    // ============= CRUD OPERATIONS =============
    Task<Result<CouponResponse>> CreateCouponForMyUnitsAsync(
    CreateCouponForUnitAdminRequest request,
    string userId);

    /// <summary>
    /// Get coupons created by current unit admin for their units
    /// </summary>
    Task<Result<PagedCouponResponse>> GetMyCouponsAsAdminAsync(
        string userId,
        CouponFilter filter);

    /// <summary>
    /// Create a new coupon/promo code
    /// </summary>
    Task<Result<CouponResponse>> CreateCouponAsync(CreateCouponRequest request, string userId);

    /// <summary>
    /// Update an existing coupon
    /// </summary>
    Task<Result<CouponResponse>> UpdateCouponAsync(int couponId, UpdateCouponRequest request, string userId);

    /// <summary>
    /// Delete a coupon (soft delete)
    /// </summary>
    Task<Result> DeleteCouponAsync(int couponId);

    /// <summary>
    /// Get coupon by ID
    /// </summary>
    Task<Result<CouponDetailsResponse>> GetCouponByIdAsync(int couponId);

    /// <summary>
    /// Get coupon by code
    /// </summary>
    Task<Result<CouponDetailsResponse>> GetCouponByCodeAsync(string code);

    /// <summary>
    /// Get all coupons with filtering and pagination
    /// </summary>
    Task<Result<PagedCouponResponse>> GetCouponsAsync(CouponFilter filter);

    // ============= VALIDATION & APPLICATION =============

    /// <summary>
    /// Validate if a coupon can be applied to a booking
    /// </summary>
    Task<Result<CouponValidationResponse>> ValidateCouponAsync(ValidateCouponRequest request);

    /// <summary>
    /// Calculate discount amount for a coupon
    /// </summary>
    Task<Result<ApplyCouponResponse>> CalculateCouponDiscountAsync(ApplyCouponRequest request);

    /// <summary>
    /// Apply coupon to a booking (called internally by booking service)
    /// </summary>
    Task<Result<ApplyCouponResponse>> ApplyCouponToBookingAsync(
        int bookingId,
        string couponCode,
        decimal originalPrice,
        string userId);

    /// <summary>
    /// Remove coupon from a booking
    /// </summary>
    Task<Result> RemoveCouponFromBookingAsync(int bookingId);

    // ============= USER OPERATIONS =============

    /// <summary>
    /// Get available coupons for a user
    /// </summary>
    Task<Result<List<CouponResponse>>> GetAvailableCouponsForUserAsync(
        string userId,
        int? unitId = null,
        int? cityId = null);

    /// <summary>
    /// Get user's coupon usage history
    /// </summary>
    Task<Result<List<CouponUsageInfo>>> GetUserCouponHistoryAsync(string userId);

    /// <summary>
    /// Check how many times user has used a specific coupon
    /// </summary>
    Task<Result<int>> GetUserCouponUsageCountAsync(string userId, string couponCode);

    // ============= ADMIN/STATISTICS =============

    /// <summary>
    /// Get coupon statistics
    /// </summary>
    Task<Result<CouponStatistics>> GetCouponStatisticsAsync(int couponId);

    /// <summary>
    /// Get coupon usage report
    /// </summary>
    Task<Result<CouponReportResponse>> GetCouponReportAsync(CouponReportRequest request);

    /// <summary>
    /// Activate/Deactivate a coupon
    /// </summary>
    Task<Result> ToggleCouponStatusAsync(int couponId, bool isActive);

    /// <summary>
    /// Get expiring coupons (expiring within X days)
    /// </summary>
    Task<Result<List<CouponResponse>>> GetExpiringCouponsAsync(int days = 7);

    /// <summary>
    /// Get most used coupons
    /// </summary>
    Task<Result<List<CouponResponse>>> GetTopPerformingCouponsAsync(int count = 10);
}