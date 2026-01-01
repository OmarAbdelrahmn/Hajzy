using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Entities;

public class PricingRule
{
    public int Id { get; set; }

    public int UnitId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public PricingRuleType Type { get; set; }

    // Conditions
    public int? MinDaysBeforeCheckIn { get; set; } // Early bird
    public int? MaxDaysBeforeCheckIn { get; set; } // Last minute
    public int? MinNights { get; set; } // Long stay discount
    public DayOfWeek? DayOfWeek { get; set; } // Weekend pricing
    public DateTime? SeasonStart { get; set; } // Seasonal pricing
    public DateTime? SeasonEnd { get; set; }

    // Adjustment
    public PricingAdjustmentType AdjustmentType { get; set; }
    public decimal AdjustmentValue { get; set; }

    public int Priority { get; set; } // For rule ordering
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Unit Unit { get; set; } = default!;
}