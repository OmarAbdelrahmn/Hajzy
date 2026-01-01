using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities;

public class BookingRoom
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int RoomId { get; set; }

    public decimal PricePerNight { get; set; }
    public int NumberOfNights { get; set; }

    // Navigation
    public Booking Booking { get; set; } = default!;
    public SubUnit Room { get; set; } = default!;
}