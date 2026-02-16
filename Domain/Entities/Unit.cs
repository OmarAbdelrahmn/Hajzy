using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Unit
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }

    public int CityId { get; set; }
    public int UnitTypeId { get; set; }
    public decimal BasePrice { get; set; }

    public int? MaxGuests { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsVerified { get; set; } = false;

    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }

    public bool IsFeatured { get; set; } = false;

    public string OptionsJson { get; set; } = "[]"; // Store as JSON array

    // NEW: Price currency
    public PriceCurrency PriceCurrency { get; set; } = PriceCurrency.SAR;

    [Range(1, 5)]
    public int? Rank { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public Department City { get; set; } = default!;
    public UnitType UnitType { get; set; } = default!;

    public int? CancellationPolicyId { get; set; }
    public CancellationPolicy? CancellationPolicy { get; set; }
    public ICollection<UniteAdmin> Admins { get; set; } = [];
    public ICollection<UnitImage> Images { get; set; } = [];
    public ICollection<UnitAmenity> UnitAmenities { get; set; } = [];
    public ICollection<SubUnit> Rooms { get; set; } = [];
    public ICollection<Booking> Bookings { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<UnitAvailability> Availabilities { get; set; } = [];
    public ICollection<UnitCustomPolicy> CustomPolicies { get; set; } = [];
}

public enum PriceCurrency
{
    USD = 1,  // US Dollar
    SAR = 2,  // Saudi Riyal
    YER = 3,  // Yemeni Rial
    AED = 4,  // UAE Dirham
    EGP = 5,  // Egyptian Pound
    EUR = 6,  // Euro
    GBP = 7   // British Pound
}