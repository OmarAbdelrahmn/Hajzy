using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Entities;

public class Review
{
    public int Id { get; set; }

    public int UnitId { get; set; }
    public int BookingId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Range(1, 5)]
    public int Rating { get; set; }

    [Range(1, 5)]
    public int CleanlinessRating { get; set; }

    [Range(1, 5)]
    public int LocationRating { get; set; }

    [Range(1, 5)]
    public int ServiceRating { get; set; }

    [Range(1, 5)]
    public int ValueRating { get; set; }

    [MaxLength(2000)]
    public string? Comment { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? OwnerResponse { get; set; }

    public DateTime? OwnerResponseDate { get; set; }

    public bool IsVerified { get; set; } = true; // Auto-verified since based on booking

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CheckInDate { get; set; }
    public DateTime? CheckOutDate { get; set; }
    // Navigation
    public Unit Unit { get; set; } = default!;
    public Booking Booking { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;

    public ICollection<ReviewImage> Images { get; set; } = [];
}
