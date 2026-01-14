using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Entities;

public class DepartmentAdmin
{public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public int CityId { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public bool IsPrimary { get; set; } = false;

    public bool IsActive { get; set; } = true;

    // Navigation
    public ApplicationUser User { get; set; } = default!;
    public Department City { get; set; } = default!;
}

    