using Application.Contracts.hoteladmincont;
using Domain;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.Contracts.CityAdminContracts;

internal class ContractCity
{
}

// CreateCityAdRequest.cs
public record CreateCityAdRequest
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public IFormFile Image { get; init; } = null!;
    public int? UnitId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public bool IsActive { get; init; } = true;
    public string? Link { get; init; }
}

// UpdateCityAdRequest.cs
public record UpdateCityAdRequest
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public IFormFile? Image { get; init; }
    public int? UnitId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public bool? IsActive { get; init; }
    public string? Link { get; init; }
}

public class CreateOfferRequest
{
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    [MaxLength(2000, ErrorMessage = "Link cannot exceed 2000 characters")]
    [Url(ErrorMessage = "Link must be a valid URL")]
    public string? Link { get; set; }

    [Required(ErrorMessage = "Image is required")]
    public IFormFile Image { get; set; } = default!;

    public int? UnitId { get; set; }

    [Required(ErrorMessage = "Start date is required")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "End date is required")]
    public DateTime EndDate { get; set; }

    [Range(0, 100, ErrorMessage = "Discount percentage must be between 0 and 100")]
    public decimal? DiscountPercentage { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Discount amount must be positive")]
    public decimal? DiscountAmount { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsFeatured { get; set; } = false;
}
public record PublicAdResponse
{
    public int Id { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public int? UnitId { get; init; }
    public string? UnitName { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public DateTime CreatedAt { get; init; }
}

// ============= PUBLIC OFFER DTOs =============

public record PublicOfferResponse
{
    public int Id { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public bool? IsFeatured { get; init; }
    public int? UnitId { get; init; }
    public string? UnitName { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public decimal? DiscountPercentage { get; init; }
    public decimal? DiscountAmount { get; init; }
    public DateTime CreatedAt { get; init; }
}

// ============= REVIEW DTOs =============

public record PublicReviewResponse
{
    public int Id { get; init; }
    public int UnitId { get; init; }
    public string UnitName { get; init; } = string.Empty;
    public int Rating { get; init; }
    public int CleanlinessRating { get; init; }
    public int LocationRating { get; init; }
    public int ServiceRating { get; init; }
    public int ValueRating { get; init; }
    public string? Comment { get; init; }
    public string? OwnerResponse { get; init; }
    public DateTime? OwnerResponseDate { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? CheckInDate { get; init; }
    public DateTime? CheckOutDate { get; init; }
    public string ReviewerName { get; init; } = "Guest"; // Never expose real names
    public List<string> ImageUrls { get; init; } = new();
}

// ============= STATISTICS DTOs =============

public record UnitStatisticsResponse
{
    public int UnitId { get; init; }
    public string UnitName { get; init; } = string.Empty;

    // Review Statistics
    public decimal AverageRating { get; init; }
    public int TotalReviews { get; init; }
    public decimal AverageCleanlinessRating { get; init; }
    public decimal AverageLocationRating { get; init; }
    public decimal AverageServiceRating { get; init; }
    public decimal AverageValueRating { get; init; }

    // Rating Distribution
    public RatingDistribution RatingDistribution { get; init; } = new();

    // Booking Statistics
    public int TotalBookings { get; init; }
    public int CompletedBookings { get; init; }
    public int CancelledBookings { get; init; }
    public decimal? OccupancyRate { get; init; } // Percentage

    // Revenue Statistics (optional - may want to hide from public)
    // public decimal TotalRevenue { get; init; }
    // public decimal AverageBookingValue { get; init; }

    // Other Statistics
    public int TotalSubUnits { get; init; }
    public int AvailableSubUnits { get; init; }
    public DateTime? FirstBookingDate { get; init; }
    public DateTime? LastBookingDate { get; init; }
}

public record RatingDistribution
{
    public int FiveStars { get; init; }
    public int FourStars { get; init; }
    public int ThreeStars { get; init; }
    public int TwoStars { get; init; }
    public int OneStar { get; init; }

    public decimal FiveStarsPercentage { get; init; }
    public decimal FourStarsPercentage { get; init; }
    public decimal ThreeStarsPercentage { get; init; }
    public decimal TwoStarsPercentage { get; init; }
    public decimal OneStarPercentage { get; init; }
}


public class PaginatedResponse<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int? NextPage { get; set; }
    public int? PrevPage { get; set; }
    public int TotalCount { get; set; }
}
public record FinancialReportResponse
{
    public string Period { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public decimal PendingRevenue { get; set; }
    public decimal RefundedAmount { get; set; }
    public decimal NetRevenue { get; set; }
    public int TotalTransactions { get; set; }
    public decimal AverageTransactionValue { get; set; }
}

public record OccupancyStatisticsResponse
{
    public string Period { get; set; } = string.Empty;
    public decimal OccupancyRate { get; set; }
}

public record BookingTrendsResponse
{
    public List<MonthlyTrend> MonthlyTrends { get; set; } = [];
    public int TotalBookings { get; set; }
    public string TrendDirection { get; set; } = string.Empty;
}

public record MonthlyTrend
{
    public string Month { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
}

public record CustomerInsightsResponse
{
    public int TotalUniqueGuests { get; set; }
    public int ReturningGuests { get; set; }
    public decimal ReturnGuestRate { get; set; }
    public decimal AverageBookingsPerGuest { get; set; }
}

public record PerformanceComparisonResponse
{
    public decimal CurrentPeriodRevenue { get; set; }
    public decimal PreviousPeriodRevenue { get; set; }
    public decimal GrowthPercentage { get; set; }
}

public record RevenueBreakdownResponse
{
    public decimal TotalRevenue { get; set; }
    public decimal AccommodationRevenue { get; set; }
    public decimal ServiceRevenue { get; set; }
    public decimal TaxRevenue { get; set; }
}

public record CancellationAnalyticsResponse
{
    public int TotalCancellations { get; set; }
    public decimal CancellationRate { get; set; }
    public decimal RefundedAmount { get; set; }
    public decimal AverageCancellationLeadTime { get; set; }
}

public record OccupancyReportResponse
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal OverallOccupancyRate { get; set; }
    public int TotalRooms { get; set; }
    public int TotalRoomNights { get; set; }
    public int BookedRoomNights { get; set; }
    public int AvailableRoomNights { get; set; }
}

public record ReviewResponse
{
    public int Id { get; set; }
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public int BookingId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public int CleanlinessRating { get; set; }
    public int LocationRating { get; set; }
    public int ValueRating { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? OwnerResponse { get; set; }
}

public record SubUnitComprehensiveDetail
{
    public int Id { get; set; }
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal PricePerNight { get; set; }
    public int MaxOccupancy { get; set; }
    public string BedType { get; set; } = string.Empty;
    public int NumberOfBeds { get; set; }
    public decimal? Size { get; set; }
    public bool IsAvailable { get; set; }
    public int? FloorNumber { get; set; }
    public bool HasPrivateBathroom { get; set; }
    public bool HasBalcony { get; set; }
    public bool HasKitchen { get; set; }
    public List<SubUnitImageResponse> Images { get; set; } = [];
    public List<AmenityResponse> Amenities { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public record SubUnitImageResponse
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
    public string? Caption { get; set; }
}

public record PolicyDetailResponse
{
    public int Id { get; set; }
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PolicyType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public record UpdateImageOrderRequest
{
    public List<ImageOrderItem> ImageOrders { get; set; } = [];
    public int? PrimaryImageId { get; set; }
}

public record ImageOrderItem
{
    public int ImageId { get; set; }
    public int Order { get; set; }
}
// ============= DASHBOARD RESPONSES =============

public record CityAdminDashboardResponse
{
    public DepartmentSummary Department { get; set; } = default!;
    public CityDashboardStats Stats { get; set; } = default!;
    public List<UnitPerformance> TopPerformingUnits { get; set; } = [];
    public List<RecentBooking> RecentBookings { get; set; } = [];
    public List<PendingRegistrationRequest> PendingRequests { get; set; } = [];
    public List<UpcomingCheckInResponse> TodayCheckIns { get; set; } = [];
    public List<UpcomingCheckOutResponse> TodayCheckOuts { get; set; } = [];
    public CityRevenueOverview RevenueOverview { get; set; } = default!;
}

public record DepartmentSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int TotalUnits { get; set; }
    public int ActiveUnits { get; set; }
    public int VerifiedUnits { get; set; }
    public int TotalSubUnits { get; set; }
    public decimal? AverageRating { get; set; }
    public int TotalReviews { get; set; }
}

public record CityDashboardStats
{
    public int TotalUnits { get; set; }
    public int ActiveUnits { get; set; }
    public int VerifiedUnits { get; set; }
    public int PendingRegistrations { get; set; }
    public int TodayCheckIns { get; set; }
    public int TodayCheckOuts { get; set; }
    public int CurrentGuests { get; set; }
    public int TotalBookings { get; set; }
    public int ActiveBookings { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal WeekRevenue { get; set; }
    public decimal MonthRevenue { get; set; }
    public decimal OccupancyRate { get; set; }
}

public record CityAdminStatsResponse
{
    public int TotalUnits { get; set; }
    public int ActiveUnits { get; set; }
    public int VerifiedUnits { get; set; }
    public int FeaturedUnits { get; set; }
    public int TotalSubUnits { get; set; }
    public int AvailableSubUnits { get; set; }
    public int TotalBookings { get; set; }
    public int ActiveBookings { get; set; }
    public int CompletedBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PendingRevenue { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public decimal OccupancyRate { get; set; }
    public int TotalUsers { get; set; }
}

public record CityRevenueOverview
{
    public decimal TodayRevenue { get; set; }
    public decimal WeekRevenue { get; set; }
    public decimal MonthRevenue { get; set; }
    public decimal YearRevenue { get; set; }
    public decimal PendingPayments { get; set; }
    public List<MonthlyRevenueData> MonthlyTrend { get; set; } = [];
}

// ============= DEPARTMENT RESPONSES =============

public record DepartmentDetailsResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public bool IsActive { get; set; }
    public int TotalUnits { get; set; }
    public int TotalBookings { get; set; }
    public decimal? AverageRating { get; set; }
    public List<DepartmentImageResponse> Images { get; set; } = [];
    public List<DepartmentAdminInfo> Admins { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}

public record DepartmentResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public record DepartmentAdminInfo
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; }
    public DateTime AssignedAt { get; set; }
}

// ============= DEPARTMENT IMAGE RESPONSES =============

public record DepartmentImageResponse
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
    public string? Caption { get; set; }
    public string ImageType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}

// ============= REGISTRATION REQUEST RESPONSES =============

public record UnitRegistrationRequestResponse
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public string OwnerFullName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string OwnerPhoneNumber { get; set; } = string.Empty;
    public string UnitTypeName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PrimaryImageUrl { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public int ImageCount { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string price { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; }
    public string Images { get; set; }

}

public record UnitRegistrationRequestDetailsResponse
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public string OwnerFullName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string OwnerPhoneNumber { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int UnitTypeId { get; set; }
    public string UnitTypeName { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal BasePrice { get; set; }
    public int? MaxGuests { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public List<string> ImageUrls { get; set; } = [];
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedByAdminId { get; set; }
}

public record PendingRegistrationRequest
{
    public int Id { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public int DaysWaiting { get; set; }
}

// ============= UNIT ADMIN RESPONSES =============

public record UnitAdminResponse
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime AssignedAt { get; set; }
}

// ============= AMENITIES RESPONSES =============

public record AmenitiesStatisticsResponse
{
    public List<AmenityUsage> MostUsedAmenities { get; set; } = [];
    public List<AmenityUsage> LeastUsedAmenities { get; set; } = [];
    public int TotalUniqueAmenities { get; set; }
}

public record AmenityUsage
{
    public int AmenityId { get; set; }
    public string AmenityName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int UnitCount { get; set; }
    public decimal UsagePercentage { get; set; }
}

// ============= AVAILABILITY RESPONSES =============

public record CityAvailabilityOverviewResponse
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalSubUnits { get; set; }
    public int AvailableSubUnits { get; set; }
    public int BookedSubUnits { get; set; }
    public int BlockedSubUnits { get; set; }
    public decimal OverallOccupancyRate { get; set; }
    public List<UnitAvailabilitySummary> UnitSummaries { get; set; } = [];
}

public record UnitAvailabilitySummary
{
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public int TotalRooms { get; set; }
    public int AvailableRooms { get; set; }
    public decimal OccupancyRate { get; set; }
}

// ============= POLICY RESPONSES =============

public record CancellationPolicyUsageResponse
{
    public List<PolicyUsage> Policies { get; set; } = [];
    public int TotalUnits { get; set; }
}

public record PolicyUsage
{
    public int PolicyId { get; set; }
    public string PolicyName { get; set; } = string.Empty;
    public int UnitCount { get; set; }
    public decimal UsagePercentage { get; set; }
}

// ============= OFFER & AD RESPONSES =============

public record OfferResponse
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int? UnitId { get; set; }
    public string? UnitName { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Link { get; set; }
}

public record AdResponse
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Link { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int? UnitId { get; set; }
    public string? UnitName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ============= COUPON RESPONSES =============

public record CouponResponse
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public decimal? MinimumSpend { get; set; }
    public decimal? MaximumDiscount { get; set; }
    public int? MaxUsageCount { get; set; }
    public int CurrentUsageCount { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
    public bool IsActive { get; set; }
}

// ============= USER RESPONSES =============

public record CityUserResponse
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public int TotalBookings { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime LastBookingDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }

}

// ============= LOYALTY RESPONSES =============

public record LoyaltyProgramStatisticsResponse
{
    public int TotalMembers { get; set; }
    public int BronzeMembers { get; set; }
    public int SilverMembers { get; set; }
    public int GoldMembers { get; set; }
    public int PlatinumMembers { get; set; }
    public int TotalPointsIssued { get; set; }
    public int TotalPointsRedeemed { get; set; }
}

// ============= NOTIFICATION RESPONSES =============

public record AdminNotificationResponse
{
    public int Id { get; set; }
    public int NotificationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime ReceivedAt { get; set; }
    public int? TargetUnitId { get; set; }
    public string? TargetUnitName { get; set; }
}

// ============= REQUEST MODELS =============

public record UpdateDepartmentRequest
{
    public string? Name { get; set; }
    public string? Country { get; set; }
    public string? Description { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}

public class UploadDepartmentImageRequest
{
    public IFormFile ImageFile { get; set; } = default!; // ADD THIS
    public string? Caption { get; set; }
    public string? ImageType { get; set; }
}

public record AssignUnitAdminRequest
{
    public int UnitId { get; set; }
    public string AdminUserId { get; set; } = string.Empty;
}

public record CreateCouponRequest
{
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CouponType Type { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal? MinimumSpend { get; set; }
    public decimal? MaximumDiscount { get; set; }
    public int? MaxUsageCount { get; set; }
    public int? MaxUsagePerUser { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
}

public record SendNotificationRequest
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
}

public record BulkUnitActionRequest
{
    public List<int> UnitIds { get; set; } = [];
    public bool Value { get; set; }
}

// ============= FILTER MODELS =============

public record RegistrationRequestFilter
{
    public string? SearchKeyword { get; set; }
    public RegistrationRequestStatus? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public record UnitAdminFilter
{
    public int? UnitId { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchKeyword { get; set; }
}

public record PolicyFilter
{
    public int? UnitId { get; set; }
    public GeneralPolicyName? PolicyType { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchKeyword { get; set; }
}

public record OfferFilter
{
    public int? UnitId { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsFeatured { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchKeyword { get; set; }
}

public record AdFilter
{
    public int? UnitId { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchKeyword { get; set; }
}

public record CouponFilter
{
    public bool? IsActive { get; set; }
    public CouponType? Type { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchKeyword { get; set; }
}

public record UserFilter
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? MinBookings { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchKeyword { get; set; }
}

public record UnitComprehensiveResponse
{
    public int Id { get; set; }
    public int? Rank { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public int CityId { get; set; }
    public string CityName { get; set; } = string.Empty;
    public int UnitTypeId { get; set; }
    public string UnitTypeName { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public int? MaxGuests { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public bool IsFeatured { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int TotalRooms { get; set; }
    public int AvailableRooms { get; set; }
    public List<ImageResponse> Images { get; set; } = [];
    public List<AmenityResponse> Amenities { get; set; } = [];
    public List<PolicyResponse> Policies { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public record ImageResponse
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public string? Caption { get; set; }
}

public record AmenityResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
}

public record PolicyResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PolicyType { get; set; } = string.Empty;
}

// ============= BOOKING RESPONSES =============

public record BookingComprehensiveResponse
{
    public List<string> SelectedOptions { get; set; } = new();
    public int Id { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; }
    public int NumberOfNights { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal PaidAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public List<BookedRoomInfo> Rooms { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}

public record BookingDetailsResponse
{
    public int Id { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
    public string GuestPhone { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; }
    public int NumberOfNights { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingBalance { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string? SpecialRequests { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }
    public List<BookedRoomInfo> Rooms { get; set; } = [];
    public List<PaymentInfo> Payments { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<string> SelectedOptions { get; set; } = new();

}

public record BookedRoomInfo
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
}

public record PaymentInfo
{
    public int Id { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
}


public record UpcomingCheckInResponse { public int BookingId { get; set; } public string BookingNumber { get; set; } = string.Empty; public string UnitName { get; set; } = string.Empty; public string GuestName { get; set; } = string.Empty; public string GuestEmail { get; set; } = string.Empty; public string GuestPhone { get; set; } = string.Empty; public DateTime CheckInDate { get; set; } public DateTime CheckOutDate { get; set; } public int NumberOfGuests { get; set; } public int NumberOfNights { get; set; } public List<string> Rooms { get; set; } = []; public string? SpecialRequests { get; set; } }

public record UpcomingCheckOutResponse { public int BookingId { get; set; } public string BookingNumber { get; set; } = string.Empty; public int UnitId { get; set; } public string UnitName { get; set; } = string.Empty; public string GuestName { get; set; } = string.Empty; public string GuestEmail { get; set; } = string.Empty; public string GuestPhone { get; set; } = string.Empty; public DateTime CheckOutDate { get; set; } public TimeSpan? CheckOutTime { get; set; } public int NumberOfNights { get; set; } public List<string> RoomNumbers { get; set; } = []; public decimal TotalAmount { get; set; } public decimal PaidAmount { get; set; } public decimal BalanceDue { get; set; } }

// ============= PAYMENT RESPONSES =============

public record PaymentHistoryResponse
{
    public int PaymentId { get; set; }
    public int BookingId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public string? Notes { get; set; }
}

public record PendingPaymentResponse
{
    public int BookingId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
    public string GuestPhone { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingBalance { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public int DaysUntilCheckIn { get; set; }
}

// ============= REVENUE & ANALYTICS RESPONSES =============

public record RevenueReportResponse { public DateTime StartDate { get; set; } public DateTime EndDate { get; set; } public decimal TotalRevenue { get; set; } public int TotalBookings { get; set; } public decimal AverageBookingValue { get; set; } public List<UnitRevenueData> RevenueByUnit { get; set; } = []; public List<MonthlyRevenueData> RevenueByMonth { get; set; } = []; }

public record UnitRevenueData
{
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
    public decimal AverageBookingValue { get; set; }
}

public record MonthlyRevenueData
{
    public string Month { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
}

public record BookingAnalyticsResponse { public DateTime StartDate { get; set; } public DateTime EndDate { get; set; } public int TotalBookings { get; set; } public int ConfirmedBookings { get; set; } public int CompletedBookings { get; set; } public int CancelledBookings { get; set; } public decimal CancellationRate { get; set; } public double AverageLeadTime { get; set; } public double AverageStayDuration { get; set; } public List<StatusCount> StatusDistribution { get; set; } = []; public List<DailyBookingData> BookingsByDay { get; set; } = []; }

public record StatusCount
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public record DailyBookingData
{
    public DateTime Date { get; set; }
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
}


public record UnitOccupancyData
{
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public decimal OccupancyRate { get; set; }
    public int TotalRooms { get; set; }
    public int AvailableRooms { get; set; }
}


public record UnitPerformance
{
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public decimal OccupancyRate { get; set; }
    public decimal AverageDailyRate { get; set; }
    public decimal RevPAR { get; set; }
    public decimal Rating { get; set; }
    public int BookingCount { get; set; }
}

public record RecentBooking
{
    public int Id { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ============= IMAGE RESPONSES =============

public record ImageDetailResponse
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
    public string? Caption { get; set; }
    public string? AltText { get; set; }
    public long FileSizeBytes { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public DateTime UploadedAt { get; set; }
}

// ============= REQUEST MODELS =============

public record UnitFilter
{
    public string? SearchKeyword { get; set; }
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsVerified { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public record BookingFilter
{
    public BookingStatus? Status { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchKeyword { get; set; }
}

public record RevenueReportFilter
{
    public int? UnitId { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow.AddMonths(-1);
    public DateTime EndDate { get; set; } = DateTime.UtcNow;
}

public record AnalyticsFilter
{
    public int? UnitId { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow.AddMonths(-1);
    public DateTime EndDate { get; set; } = DateTime.UtcNow;
}

public record ReviewFilter
{
    public int? UnitId { get; set; }
    public int? MinRating { get; set; }
    public bool? HasResponse { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchKeyword { get; set; }
}

public record PaymentFilter
{
    public int? UnitId { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchKeyword { get; set; }

}

public record FinancialReportFilter
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? UnitId { get; set; }
    public bool IncludeProjections { get; set; } = false;
}

public record OccupancyFilter
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? UnitId { get; set; }
    public int? SubUnitId { get; set; }
}

public record TrendsFilter
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? UnitId { get; set; }
    public TrendsPeriod Period { get; set; } = TrendsPeriod.Daily;
}

public record InsightsFilter
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? UnitId { get; set; }
}

public record ComparisonFilter { public DateTime StartDate { get; set; } public DateTime EndDate { get; set; } public List<int>? UnitIds { get; set; } }

public record RevenueBreakdownFilter
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? UnitId { get; set; }
}

public record CancellationFilter
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? UnitId { get; set; }
}

public record NotificationFilter
{
    public bool? IsRead { get; set; }
    public NotificationType? Type { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
