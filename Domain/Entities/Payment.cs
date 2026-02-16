using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Payment
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    [Required]
    public string TransactionId { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation
    public Booking Booking { get; set; } = default!;
}
