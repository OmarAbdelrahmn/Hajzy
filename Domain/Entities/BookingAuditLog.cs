using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;


public class BookingAuditLog
{
    public int Id { get; set; }
    public int BookingId { get; set; }

    [Required, MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    [Required]
    public string PerformedBy { get; set; } = string.Empty;

    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON

    [MaxLength(45)]
    public string? IpAddress { get; set; }

    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Booking Booking { get; set; } = default!;
    public ApplicationUser PerformedByUser { get; set; } = default!;
}
