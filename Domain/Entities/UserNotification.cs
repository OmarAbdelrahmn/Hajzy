using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Entities;

public class UserNotification
{
    public int Id { get; set; }

    public int NotificationId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Notification Notification { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;
}