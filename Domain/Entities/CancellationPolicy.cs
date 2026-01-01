using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Entities;

public class CancellationPolicy
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    // Refund tiers based on days before check-in
    public int FullRefundDays { get; set; } // Full refund if cancelled X days before
    public int PartialRefundDays { get; set; } // Partial refund if cancelled X days before
    public decimal PartialRefundPercentage { get; set; } // Percentage to refund

    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Unit> Units { get; set; } = [];
}