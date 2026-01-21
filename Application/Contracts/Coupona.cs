using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Contracts;

internal class Coupona
{
}


// ========== REQUEST DTOs ==========
public record CreateCouponForUnitAdminRequest(
    string Code,
    string? Description,
    CouponType Type,
    decimal DiscountAmount,
    decimal? MinimumSpend,
    decimal? MaximumDiscount,
    int? MaxUsageCount,
    int? MaxUsagePerUser,
    DateTime ValidFrom,
    DateTime ValidUntil,
    bool IsActive = true,
    int? SpecificUnitId = null  // Optional: if admin manages multiple units and wants to target a specific one
);
public record CreateCouponRequest(
    string Code,
    string? Description,
    CouponType Type,
    decimal DiscountAmount,
    decimal? MinimumSpend,
    decimal? MaximumDiscount,
    int? MaxUsageCount,
    int? MaxUsagePerUser,
    DateTime ValidFrom,
    DateTime ValidUntil,
    int? TargetUnitId,
    int? TargetCityId,
    int? TargetUnitTypeId,
    bool IsActive = true
);

public record UpdateCouponRequest(
    string? Code,
    string? Description,
    CouponType? Type,
    decimal? DiscountAmount,
    decimal? MinimumSpend,
    decimal? MaximumDiscount,
    int? MaxUsageCount,
    int? MaxUsagePerUser,
    DateTime? ValidFrom,
    DateTime? ValidUntil,
    int? TargetUnitId,
    int? TargetCityId,
    int? TargetUnitTypeId,
    bool? IsActive
);

public record ApplyCouponRequest(
    string CouponCode,
    decimal BookingAmount,
    int? UnitId,
    int? CityId,
    int? UnitTypeId,
    string UserId
);

public record ValidateCouponRequest(
    string CouponCode,
    decimal BookingAmount,
    int? UnitId,
    int? CityId,
    int? UnitTypeId,
    string UserId,
    DateTime? CheckInDate,
    DateTime? CheckOutDate
);

// ========== RESPONSE DTOs ==========

public record CouponResponse
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Type { get; init; } = string.Empty;
    public decimal DiscountAmount { get; init; }
    public decimal? MinimumSpend { get; init; }
    public decimal? MaximumDiscount { get; init; }
    public int? MaxUsageCount { get; init; }
    public int CurrentUsageCount { get; init; }
    public int? MaxUsagePerUser { get; init; }
    public DateTime ValidFrom { get; init; }
    public DateTime ValidUntil { get; init; }
    public bool IsActive { get; init; }
    public int? TargetUnitId { get; init; }
    public string? TargetUnitName { get; init; }
    public int? TargetCityId { get; init; }
    public string? TargetCityName { get; init; }
    public int? TargetUnitTypeId { get; init; }
    public string? TargetUnitTypeName { get; init; }
    public decimal TotalDiscountGiven { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public bool IsExpired { get; init; }
    public bool IsValid { get; init; }
}

public record CouponDetailsResponse : CouponResponse
{
    public List<CouponUsageInfo> RecentUsages { get; init; } = [];
    public CouponStatistics Statistics { get; init; } = new();
}

public record CouponUsageInfo
{
    public int BookingId { get; init; }
    public string BookingNumber { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public decimal DiscountApplied { get; init; }
    public DateTime AppliedAt { get; init; }
}

public record CouponStatistics
{
    public int TotalUsageCount { get; init; }
    public decimal TotalDiscountGiven { get; init; }
    public decimal AverageDiscountPerBooking { get; init; }
    public int UniqueUsersCount { get; init; }
    public decimal ConversionRate { get; init; }
    public Dictionary<string, int> UsageByMonth { get; init; } = new();
}

public record CouponValidationResponse
{
    public bool IsValid { get; init; }
    public string? ErrorMessage { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal FinalPrice { get; init; }
    public CouponResponse? Coupon { get; init; }
}

public record ApplyCouponResponse
{
    public int CouponId { get; init; }
    public string CouponCode { get; init; } = string.Empty;
    public decimal OriginalPrice { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal FinalPrice { get; init; }
    public string DiscountType { get; init; } = string.Empty;
}

// ========== FILTER DTOs ==========

public record CouponFilter
{
    public string? Code { get; init; }
    public CouponType? Type { get; init; }
    public bool? IsActive { get; init; }
    public bool? IsExpired { get; init; }
    public int? TargetUnitId { get; init; }
    public int? TargetCityId { get; init; }
    public int? TargetUnitTypeId { get; init; }
    public DateTime? ValidFrom { get; init; }
    public DateTime? ValidUntil { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SortBy { get; init; } // "code", "usage", "discount", "expiry"
    public bool SortDescending { get; init; } = true;
}

public record PagedCouponResponse
{
    public List<CouponResponse> Coupons { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}

public record CouponReportRequest
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int? UnitId { get; init; }
    public int? CityId { get; init; }
    public CouponType? Type { get; init; }
}

public record CouponReportResponse
{
    public int TotalCoupons { get; init; }
    public int ActiveCoupons { get; init; }
    public int ExpiredCoupons { get; init; }
    public int TotalUsageCount { get; init; }
    public decimal TotalDiscountGiven { get; init; }
    public decimal TotalRevenueGenerated { get; init; }
    public decimal AverageDiscountPerCoupon { get; init; }
    public List<TopCouponInfo> TopPerformingCoupons { get; init; } = [];
    public Dictionary<string, decimal> DiscountByMonth { get; init; } = new();
}

public record TopCouponInfo
{
    public string Code { get; init; } = string.Empty;
    public int UsageCount { get; init; }
    public decimal TotalDiscount { get; init; }
}