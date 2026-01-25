using System.ComponentModel.DataAnnotations;

namespace Application.Contracts.Review;

// ========== REQUEST DTOs ==========



public record AllReviewsFilter
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;

    // Filter by Unit
    public int? UnitId { get; init; }

    // Filter by User
    public string? UserId { get; init; }

    // Filter by Department/City
    public int? DepartmentId { get; init; }

    // Filter by Unit Type
    public int? UnitTypeId { get; init; }

    // Rating Filters
    public int? MinRating { get; init; }
    public int? MaxRating { get; init; }

    // Date Filters
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }

    // Verification Status
    public bool? IsVerified { get; init; }

    // Owner Response
    public bool? HasOwnerResponse { get; init; }

    // Search
    public string? SearchTerm { get; init; } // Search in comment or unit name

    // Sorting
    public string? SortBy { get; init; } = "date"; // date, rating, unit
    public bool SortDescending { get; init; } = true;

    // Additional Filters
    public bool? HasImages { get; init; }
    public int? MinCleanlinessRating { get; init; }
    public int? MinLocationRating { get; init; }
    public int? MinServiceRating { get; init; }
    public int? MinValueRating { get; init; }
}

public record CreateReviewRequest
{
    [Required]
    public int UnitId { get; set; }

    public string UserId { get; set; } = string.Empty; // Set by controller

    [Required]
    [Range(1, 5, ErrorMessage = "Overall rating must be between 1 and 5")]
    public int Rating { get; set; }

    [Required]
    [Range(1, 5, ErrorMessage = "Cleanliness rating must be between 1 and 5")]
    public int CleanlinessRating { get; set; }

    [Required]
    [Range(1, 5, ErrorMessage = "Location rating must be between 1 and 5")]
    public int LocationRating { get; set; }

    [Required]
    [Range(1, 5, ErrorMessage = "Service rating must be between 1 and 5")]
    public int ServiceRating { get; set; }

    [Required]
    [Range(1, 5, ErrorMessage = "Value rating must be between 1 and 5")]
    public int ValueRating { get; set; }

    [MinLength(10, ErrorMessage = "Comment must be at least 10 characters")]
    [MaxLength(2000, ErrorMessage = "Comment must not exceed 2000 characters")]
    public string? Comment { get; set; }

    public List<int>? ImageIds { get; set; }
}

public record UpdateReviewRequest
{
    [Range(1, 5, ErrorMessage = "Overall rating must be between 1 and 5")]
    public int? Rating { get; set; }

    [Range(1, 5, ErrorMessage = "Cleanliness rating must be between 1 and 5")]
    public int? CleanlinessRating { get; set; }

    [Range(1, 5, ErrorMessage = "Location rating must be between 1 and 5")]
    public int? LocationRating { get; set; }

    [Range(1, 5, ErrorMessage = "Service rating must be between 1 and 5")]
    public int? ServiceRating { get; set; }

    [Range(1, 5, ErrorMessage = "Value rating must be between 1 and 5")]
    public int? ValueRating { get; set; }

    [MinLength(10, ErrorMessage = "Comment must be at least 10 characters")]
    [MaxLength(2000, ErrorMessage = "Comment must not exceed 2000 characters")]
    public string? Comment { get; set; }
}

public record AddOwnerResponseRequest
{
    [Required]
    [MinLength(5, ErrorMessage = "Response must be at least 5 characters")]
    [MaxLength(1000, ErrorMessage = "Response must not exceed 1000 characters")]
    public string Response { get; set; } = string.Empty;
}

public record ReviewFilter
{
    public int? MinRating { get; set; }
    public int? MaxRating { get; set; }
    public bool? IsVerified { get; set; }
    public bool? HasOwnerResponse { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SortBy { get; set; } = "CreatedAt"; // CreatedAt, Rating
    public bool SortDescending { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

// ========== RESPONSE DTOs ==========

public record ReviewResponse
{
    public int Id { get; set; }
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public int BookingId { get; set; }

    public ReviewerInfo Reviewer { get; set; } = default!;

    public int Rating { get; set; }
    public int CleanlinessRating { get; set; }
    public int LocationRating { get; set; }
    public int ServiceRating { get; set; }
    public int ValueRating { get; set; }

    public string? Comment { get; set; }

    public bool IsVerified { get; set; }
    public int? StayDuration { get; set; }
    public DateTime? CheckInDate { get; set; }
    public DateTime? CheckOutDate { get; set; }

    public string? OwnerResponse { get; set; }
    public DateTime? OwnerResponseDate { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<ReviewImageResponse> Images { get; set; } = new();
}

public record ReviewDetailsResponse : ReviewResponse
{
    public string ModerationStatus { get; set; } = string.Empty;
}

public record ReviewerInfo
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public int TotalReviews { get; set; }
    public string? Nationality { get; set; }
}

public record ReviewImageResponse
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? Caption { get; set; }
}

public record ReviewEligibilityResponse
{
    public bool CanReview { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int? BookingId { get; set; }
    public DateTime? CheckOutDate { get; set; }
    public int? DaysRemaining { get; set; }
}

public record ReviewStatisticsResponse
{
    public int TotalReviews { get; set; }
    public decimal AverageRating { get; set; }
    public decimal AverageCleanlinessRating { get; set; }
    public decimal AverageLocationRating { get; set; }
    public decimal AverageServiceRating { get; set; }
    public decimal AverageValueRating { get; set; }

    public Dictionary<int, int> RatingDistribution { get; set; } = new();
    public int VerifiedReviewsCount { get; set; }
    public int ReviewsWithPhotosCount { get; set; }
    public int ReviewsWithOwnerResponseCount { get; set; }
}

public record PagedReviewResponse
{
    public List<ReviewResponse> Reviews { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}