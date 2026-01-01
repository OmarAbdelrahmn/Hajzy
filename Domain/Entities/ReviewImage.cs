using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Entities;

public class ReviewImage : BaseImage
{
    public int ReviewId { get; set; }

    // Reviews are guest photos - categorize them
    public ReviewImageType ImageType { get; set; } = ReviewImageType.General;

    // Navigation
    public Review Review { get; set; } = default!;
}

public enum ReviewImageType
{
    Room,
    Bathroom,
    Food,
    Amenities,
    View,
    Experience,
    General
}