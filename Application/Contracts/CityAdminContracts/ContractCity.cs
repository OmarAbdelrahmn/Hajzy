using Application.Contracts.Availability;
using Application.Contracts.hoteladmincont;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Contracts.CityAdminContracts;

internal class ContractCity
{
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
    public decimal BasePrice { get; set; }
    public int ImageCount { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
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
}

public record AdResponse
{
    public int Id { get; set; }
    public string? Title { get; set; }
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
    public string? PhoneNumber { get; set; }
    public int TotalBookings { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime LastBookingDate { get; set; }
    public DateTime CreatedAt { get; set; }
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

public record UploadDepartmentImageRequest
{
    public string ImageUrl { get; set; } = string.Empty;
    public string S3Key { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public string ImageType { get; set; } = "General";
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
}

public record PolicyFilter
{
    public int? UnitId { get; set; }
    public GeneralPolicyName? PolicyType { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public record OfferFilter
{
    public int? UnitId { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsFeatured { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public record AdFilter
{
    public int? UnitId { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public record CouponFilter
{
    public bool? IsActive { get; set; }
    public CouponType? Type { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public record UserFilter
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? MinBookings { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

