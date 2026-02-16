namespace Domain.Entities;

public class UnitAmenity
{
    public int Id { get; set; }
    public int UnitId { get; set; }
    public int AmenityId { get; set; }

    public bool IsAvailable { get; set; } = true;

    // Navigation
    public Unit Unit { get; set; } = default!;
    public Amenity Amenity { get; set; } = default!;
}