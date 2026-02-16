using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Department
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Country { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }

    public bool IsActive { get; set; } = true;


    public int TotalUnits { get; set; }
    public int TotalBookings { get; set; }
    public decimal? AverageRating { get; set; }


    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Unit> Units { get; set; } = [];
    public ICollection<DepartmentAdmin> DepartmentAdmins { get; set; } = [];
    public ICollection<DepartmentImage> DepartmentImages { get; set; } = [];
}