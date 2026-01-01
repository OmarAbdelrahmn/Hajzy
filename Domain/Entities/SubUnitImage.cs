using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Entities;

//public class SubUnitImage
//{
//    public int Id { get; set; }

//    public int SubUnitId { get; set; }

//    [Required]
//    public string ImageUrl { get; set; } = string.Empty;

//    public bool IsPrimary { get; set; } = false;

//    public int DisplayOrder { get; set; }

//    [MaxLength(200)]
//    public string? Caption { get; set; }

//    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

//    // Navigation
//    public SubUnit SubUnit { get; set; } = default!;
//}

public class SubUnitImage : BaseImage
{
    public int SubUnitId { get; set; }

    public SubUnitImageType ImageType { get; set; } = SubUnitImageType.General;

    // Navigation
    public SubUnit SubUnit { get; set; } = default!;
}

public enum SubUnitImageType
{
    Primary,
    Bedroom,
    Bathroom,
    Kitchen,
    LivingRoom,
    Balcony,
    View,
    Amenities,
    General
}
