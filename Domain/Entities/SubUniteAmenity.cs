namespace Domain.Entities;

public class SubUniteAmenity
{
    public int Id { get; set; }
    public int SubUnitId { get; set; }
    public int AmenityId { get; set; }

    public bool IsAvailable { get; set; } = true;

    // Navigation
    public SubUnit SubUnit { get; set; } = default!;
    public Amenity Amenity { get; set; } = default!;
}
