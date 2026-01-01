using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Entities;

public class Coupon
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }

    public CouponType Type { get; set; }
    public decimal DiscountAmount { get; set; } // Fixed amount or percentage

    public decimal? MinimumSpend { get; set; }
    public decimal? MaximumDiscount { get; set; }

    public int? MaxUsageCount { get; set; }
    public int CurrentUsageCount { get; set; }

    public int? MaxUsagePerUser { get; set; }

    public DateTime ValidFrom { get; set; } = DateTime.UtcNow;
    public DateTime ValidUntil { get; set; }

    public bool IsActive { get; set; } = true;

    // Targeting
    public int? TargetUnitId { get; set; }
    public int? TargetCityId { get; set; }
    public int? TargetUnitTypeId { get; set; }

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Unit? TargetUnit { get; set; }
    public Department? TargetCity { get; set; }
    public UnitType? TargetUnitType { get; set; }

    public ICollection<BookingCoupon> BookingCoupons { get; set; } = [];
}