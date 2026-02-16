using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Booking
{
    public int Id { get; set; }

    [Required]
    public string BookingNumber { get; set; } = string.Empty; // BK-2024-001234

    public int UnitId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public BookingType BookingType { get; set; } = BookingType.SubUnitBooking;


    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }

    public int NumberOfGuests { get; set; }
    public int NumberOfNights { get; set; }

    public decimal TotalPrice { get; set; }
    public decimal PaidAmount { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    [MaxLength(1000)]
    public string? SpecialRequests { get; set; }

    public string? SelectedOptionsJson { get; set; } = "[]";


    [MaxLength(1000)]
    public string? CancellationReason { get; set; }

    public DateTime? CancelledAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public Unit Unit { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;

    public ICollection<BookingRoom> BookingRooms { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
    public Review? Review { get; set; }
}

public enum BookingType
{
    UnitBooking = 1,
    SubUnitBooking = 2

}