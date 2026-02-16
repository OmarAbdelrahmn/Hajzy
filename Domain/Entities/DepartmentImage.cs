namespace Domain.Entities;

public class DepartmentImage : BaseImage
{
    public int DepartmentId { get; set; }

    public DepartmentImageType ImageType { get; set; } = DepartmentImageType.General;

    // Navigation
    public Department Department { get; set; } = default!;
}

public enum DepartmentImageType
{
    Primary,        // Main city/department image
    Landmark,       // Famous landmarks
    Attraction,     // Tourist attractions
    Culture,        // Cultural sites
    Nature,         // Natural scenery
    CityView,       // Skyline/panoramic views
    General
}