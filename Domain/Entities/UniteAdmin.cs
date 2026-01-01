using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Entities;

public class UniteAdmin
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public int UnitId { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;


    public ApplicationUser User { get; set; } = default!;
    public Unit Unit { get; set; } = default!;
}
