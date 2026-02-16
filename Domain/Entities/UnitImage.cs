namespace Domain.Entities;

//public class UnitImage
//{
//    public int Id { get; set; }

//    public int UnitId { get; set; }

//    [Required]
//    public string ImageUrl { get; set; } = string.Empty;

//    public bool IsPrimary { get; set; } = false;

//    public int DisplayOrder { get; set; }

//    [MaxLength(200)]
//    public string? Caption { get; set; }

//    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

//    // Navigation
//    public Unit Unit { get; set; } = default!;
//}
public class UnitImage : BaseImage
{
    public int UnitId { get; set; }

    public UnitImageType ImageType { get; set; } = UnitImageType.Gallery;

    public Unit Unit { get; set; } = default!;
}

public enum UnitImageType
{
    Primary,        // Main hero image
    Gallery,        // Gallery images
    Exterior,       // Building exterior
    Interior,       // Interior shots
    Amenities,      // Amenity photos
    Dining,         // Restaurant/dining area
    Recreation,     // Pool, gym, etc.
    Views,          // View from property
    Other
}