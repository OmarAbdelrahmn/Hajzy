using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Entities;

public class LoyaltyTransaction
{
    public int Id { get; set; }

    public int LoyaltyProgramId { get; set; }
    public int? BookingId { get; set; }

    public int PointsEarned { get; set; }
    public int PointsRedeemed { get; set; }

    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public LoyaltyProgram LoyaltyProgram { get; set; } = default!;
    public Booking? Booking { get; set; }
}