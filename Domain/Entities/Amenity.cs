using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Amenity
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; }  // WiFi, Pool, Parking

    [MaxLength(200)]
    public string? Description { get; set; }


    public string Category { get; set; }  // Basic, Entertainment, Safety

    // Navigation
    public ICollection<UnitAmenity> UnitAmenities { get; set; } = [];
}