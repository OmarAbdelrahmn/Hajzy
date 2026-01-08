using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Entities;

public class RoomStateHistory
{
    public int Id { get; set; }
    public int BookingRoomId { get; set; }

    //public RoomState FromState { get; set; }
    //public RoomState ToState { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    [Required, MaxLength(450)]
    public string ChangedBy { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation
    public BookingRoom BookingRoom { get; set; } = default!;
    public ApplicationUser ChangedByUser { get; set; } = default!;
}