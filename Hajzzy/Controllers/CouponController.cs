using Application.Contracts.couponcontract;
using Application.Extensions;
using Application.Service.PromoCode;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hajzzy.Controllers;

[Route("api/coupons")]
[ApiController]
public class CouponController(ICouponService couponService) : ControllerBase
{
    private readonly ICouponService _couponService = couponService;

    // ========== CRUD OPERATIONS ==========

    /// <summary>
    /// Create a new coupon/promo code (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> CreateCoupon([FromBody] CreateCouponRequest request)
    {
        var userId = User.GetUserId();
        var result = await _couponService.CreateCouponAsync(request, userId);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetCouponById), new { couponId = result.Value.Id }, result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Update an existing coupon (Admin only)
    /// </summary>
    [HttpPut("{couponId:int}")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> UpdateCoupon(int couponId, [FromBody] UpdateCouponRequest request)
    {
        var userId = User.GetUserId();
        var result = await _couponService.UpdateCouponAsync(couponId, request, userId);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Delete a coupon (Admin only)
    /// </summary>
    [HttpDelete("{couponId:int}")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> DeleteCoupon(int couponId)
    {
        var result = await _couponService.DeleteCouponAsync(couponId);

        return result.IsSuccess
            ? Ok(new { message = "Coupon deleted successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Get coupon by ID
    /// </summary>
    [HttpGet("{couponId:int}")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> GetCouponById(int couponId)
    {
        var result = await _couponService.GetCouponByIdAsync(couponId);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get coupon by code
    /// </summary>
    [HttpGet("code/{code}")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> GetCouponByCode(string code)
    {
        var result = await _couponService.GetCouponByCodeAsync(code);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get all coupons with filtering and pagination (Admin only)
    /// </summary>
    [HttpPost("filter")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> GetCoupons([FromBody] CouponFilter filter)
    {
        var result = await _couponService.GetCouponsAsync(filter);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    // ========== COUPON VALIDATION & APPLICATION ==========

    /// <summary>
    /// Validate a coupon code for a booking
    /// </summary>
    [HttpPost("validate")]
    [Authorize]
    public async Task<IActionResult> ValidateCoupon([FromBody] ValidateCouponRequest request)
    {
        var userId = User.GetUserId();
        request = request with { UserId = userId };

        var result = await _couponService.ValidateCouponAsync(request);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Calculate discount amount for a coupon
    /// </summary>
    [HttpPost("calculate-discount")]
    [Authorize]
    public async Task<IActionResult> CalculateDiscount([FromBody] ApplyCouponRequest request)
    {
        var userId = User.GetUserId();
        request = request with { UserId = userId };

        var result = await _couponService.CalculateCouponDiscountAsync(request);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    // ========== USER OPERATIONS ==========

    /// <summary>
    /// Get available coupons for the current user
    /// </summary>
    [HttpGet("available")]
    [Authorize]
    public async Task<IActionResult> GetAvailableCoupons(
        [FromQuery] int? unitId = null,
        [FromQuery] int? cityId = null)
    {
        var userId = User.GetUserId();
        var result = await _couponService.GetAvailableCouponsForUserAsync(userId, unitId, cityId);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get user's coupon usage history
    /// </summary>
    [HttpGet("my-history")]
    [Authorize]
    public async Task<IActionResult> GetMyCouponHistory()
    {
        var userId = User.GetUserId();
        var result = await _couponService.GetUserCouponHistoryAsync(userId);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Check how many times current user has used a specific coupon
    /// </summary>
    [HttpGet("my-usage/{couponCode}")]
    [Authorize]
    public async Task<IActionResult> GetMyUsageCount(string couponCode)
    {
        var userId = User.GetUserId();
        var result = await _couponService.GetUserCouponUsageCountAsync(userId, couponCode);

        return result.IsSuccess
            ? Ok(new { couponCode, usageCount = result.Value })
            : result.ToProblem();
    }

    // ========== ADMIN OPERATIONS ==========

    /// <summary>
    /// Toggle coupon active status (Admin only)
    /// </summary>
    [HttpPatch("{couponId:int}/toggle-status")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> ToggleCouponStatus(int couponId)
    {
        var result = await _couponService.ToggleCouponStatusAsync(couponId);

        return result.IsSuccess
            ? Ok(new { message = $"done successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Get coupon statistics (Admin only)
    /// </summary>
    [HttpGet("{couponId:int}/statistics")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> GetCouponStatistics(int couponId)
    {
        var result = await _couponService.GetCouponStatisticsAsync(couponId);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get coupon usage report (Admin only)
    /// </summary>
    [HttpPost("report")]
    [Authorize(Roles = "SuperAdmin,CityAdmin")]
    public async Task<IActionResult> GetCouponReport([FromBody] CouponReportRequest request)
    {
        var result = await _couponService.GetCouponReportAsync(request);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get expiring coupons (Admin only)
    /// </summary>
    [HttpGet("expiring")]
    [Authorize(Roles = "SuperAdmin,CityAdmin,HotelAdmin")]
    public async Task<IActionResult> GetExpiringCoupons([FromQuery] int days = 7)
    {
        var result = await _couponService.GetExpiringCouponsAsync(days);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get top performing coupons (Admin only)
    /// </summary>
    [HttpGet("top-performing")]
    [Authorize(Roles = "SuperAdmin,CityAdmin")]
    public async Task<IActionResult> GetTopPerformingCoupons([FromQuery] int count = 10)
    {
        var result = await _couponService.GetTopPerformingCouponsAsync(count);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    // Add these endpoints to CouponController (after the existing CreateCoupon endpoint)

    // ========== UNIT ADMIN SPECIFIC OPERATIONS ==========

    /// <summary>
    /// Create a coupon for your own units (Unit Admin only)
    /// Automatically sets the coupon to target your administered unit(s)
    /// </summary>
    [HttpPost("my-units")]
    [Authorize(Roles = "HotelAdmin")]
    public async Task<IActionResult> CreateCouponForMyUnits([FromBody] CreateCouponForUnitAdminRequest request)
    {
        var userId = User.GetUserId();
        var result = await _couponService.CreateCouponForMyUnitsAsync(request, userId);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetCouponById), new { couponId = result.Value.Id }, result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Get all coupons you created for your units (Unit Admin only)
    /// </summary>
    [HttpGet("my-units")]
    [Authorize(Roles = "HotelAdmin")]
    public async Task<IActionResult> GetMyCouponsAsAdmin([FromQuery] CouponFilter filter)
    {
        var userId = User.GetUserId();
        var result = await _couponService.GetMyCouponsAsAdminAsync(userId, filter);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }
}