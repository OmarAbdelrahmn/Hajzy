// Application/Contracts/hoteladmincont/HotelAdminContracts.cs

using Application.Service.Avilabilaties;
using Domain;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.Contracts.hoteladmincont;

// ============================================================================
// POLICY MANAGEMENT CONTRACTS
// ============================================================================



public record CreateUnitCustomPolicyRequest
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Category { get; init; }
    public int DisplayOrder { get; init; } = 0;
}

public record UpdateUnitCustomPolicyRequest
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? Category { get; init; }
    public int? DisplayOrder { get; init; }
    public bool? IsActive { get; init; }
}

public record UnitCustomPolicyResponse
{
    public int Id { get; init; }
    public int UnitId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Category { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record UpdateUnitOptionsRequest
{
    public List<string> Options { get; init; } = new();
}

public record UpdateUnitCurrencyRequest
{
    public PriceCurrency Currency { get; init; }
}

#region Policy Requests
public record CreateOfferRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public IFormFile? ImageFile { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public record UpdateOfferRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public IFormFile? ImageFile { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsActive { get; set; }
}

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
public record CreatePolicyRequest(
    [Required] string Title,
    [Required] string Description,
    GeneralPolicyName PolicyType,
    GeneralPolicyCategory? PolicyCategory = null,
    string? CustomPolicyName = null,
    int? CancellationPolicyId = null,
    bool IsMandatory = false,
    bool IsHighlighted = false
);

public record UpdatePolicyRequest(
    string? Title = null,
    string? Description = null,
    GeneralPolicyCategory? PolicyCategory = null,
    int? CancellationPolicyId = null,
    bool? IsMandatory = null,
    bool? IsHighlighted = null,
    bool? IsActive = null
);

#endregion

#region Policy Responses

public record PolicyDetailResponse
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string PolicyType { get; init; } = string.Empty;
    public string? PolicyCategory { get; init; }
    public string? CustomPolicyName { get; init; }
    public int? CancellationPolicyId { get; init; }
    public string? CancellationPolicyName { get; init; }
    public bool IsMandatory { get; init; }
    public bool IsHighlighted { get; init; }
    public bool IsActive { get; init; }
    public int? UnitId { get; init; }
    public int? SubUnitId { get; init; }
}

#endregion

// ============================================================================
// CANCELLATION POLICY CONTRACTS
// ============================================================================

#region Cancellation Policy Requests

public record CreateCancellationPolicyRequest(
    [Required][MaxLength(100)] string Name,
    [Required][MaxLength(1000)] string Description,
    [Range(0, 365)] int FullRefundDays,
    [Range(0, 365)] int PartialRefundDays,
    [Range(0, 100)] decimal PartialRefundPercentage,
    bool IsDefault = false
);

#endregion

#region Cancellation Policy Responses

public record CancellationPolicyResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int FullRefundDays { get; init; }
    public int PartialRefundDays { get; init; }
    public decimal PartialRefundPercentage { get; init; }
    public bool IsActive { get; init; }
    public bool IsDefault { get; init; }
    public DateTime CreatedAt { get; init; }
}

#endregion

// ============================================================================
// AVAILABILITY MANAGEMENT CONTRACTS (EXTENDED)
// ============================================================================

#region Availability Requests

public record SetUnitAvailabilityRequest(
    int UnitId,
    DateTime StartDate,
    DateTime EndDate,
    bool IsAvailable,
    UnavailabilityReason? Reason = null,
    string? UpdatedByUserId = null
);

public record SetSubUnitAvailabilityRequest(
    int SubUnitId,
    DateTime StartDate,
    DateTime EndDate,
    bool IsAvailable,
    UnavailabilityReason? Reason = null,
    decimal? SpecialPrice = null,
    decimal? WeekendPrice = null,
    string? UpdatedByUserId = null
);

public record BlockDatesRequest(
    int SubUnitId,
    List<DateRange> DateRanges,
    UnavailabilityReason Reason,
    string UpdatedByUserId
);

public record BlockUnitDatesRequest(
    int UnitId,
    List<DateRange> DateRanges,
    UnavailabilityReason Reason,
    string UpdatedByUserId
);

public record SetSpecialPricingRequest(
    int SubUnitId,
    List<PriceRange> PriceRanges,
    string UpdatedByUserId
);

public record BulkAvailabilityUpdateRequest(
    decimal SpecialPrice,
    decimal WeekendPrice,
    List<int> SubUnitIds,
    DateTime StartDate,
    DateTime EndDate,
    bool IsAvailable,
    UnavailabilityReason? Reason = null
);

public record BulkPricingUpdateRequest(
    List<int> SubUnitIds,
    DateTime StartDate,
    DateTime EndDate,
    decimal? WeekdayPrice = null,
    decimal? WeekendPrice = null
);

public record CopyAvailabilityRequest(
    int SourceSubUnitId,
    List<int> TargetSubUnitIds,
    DateTime StartDate,
    DateTime EndDate
);

#endregion

#region Availability Responses

public record AvailabilityResponse(
    int Id,
    DateTime StartDate,
    DateTime EndDate,
    bool IsAvailable,
    string? Reason,
    decimal? SpecialPrice,
    decimal? WeekendPrice
);

public record AvailabilityBlockResponse
{
    public int Id { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public bool IsAvailable { get; init; }
    public string? Reason { get; init; }
    public decimal? SpecialPrice { get; init; }
    public decimal? WeekendPrice { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public string? UpdatedByUserId { get; init; }
}

public record UnitAvailabilityStatus
{
    public bool IsAvailable { get; init; }
    public string? Reason { get; init; }
    public bool HasManualBlock { get; init; }
    public bool HasActiveBooking { get; init; }
    public bool IsStandaloneUnit { get; init; }
    public int AvailableSubUnitsCount { get; init; }
    public int TotalSubUnitsCount { get; init; }
}

public record SubUnitAvailabilityStatus
{
    public bool IsAvailable { get; init; }
    public string? Reason { get; init; }
    public bool HasManualBlock { get; init; }
    public bool HasActiveBooking { get; init; }
    public decimal? CurrentPrice { get; init; }
    public decimal? SpecialPrice { get; init; }
}

public record UnitDayAvailability
{
    public DateTime Date { get; init; }
    public bool IsAvailable { get; init; }
    public bool IsBooked { get; init; }
    public bool IsManuallyBlocked { get; init; }
    public bool IsStandaloneUnit { get; init; }
    public int AvailableSubUnits { get; init; }
    public int TotalSubUnits { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public UnavailabilityReason? UnavailabilityReason { get; init; }
}

public record SubUnitDayAvailability
{
    public DateTime Date { get; init; }
    public bool IsAvailable { get; init; }
    public bool IsBooked { get; init; }
    public bool IsManuallyBlocked { get; init; }
    public decimal Price { get; init; }
    public bool IsWeekend { get; init; }
    public decimal? SpecialPrice { get; init; }
    public UnavailabilityReason? UnavailabilityReason { get; init; }
}

#endregion

// ============================================================================
// COMPREHENSIVE REPORTS CONTRACTS
// ============================================================================

#region Report Filters

public record FinancialReportFilter(
    DateTime StartDate,
    DateTime EndDate,
    int? UnitId = null,
    bool IncludeProjections = false
);

public record OccupancyFilter(
    DateTime StartDate,
    DateTime EndDate,
    int? UnitId = null,
    int? SubUnitId = null
);

public record TrendsFilter(
    DateTime StartDate,
    DateTime EndDate,
    int? UnitId = null,
    TrendsPeriod Period = TrendsPeriod.Daily
);

public record InsightsFilter(
    DateTime StartDate,
    DateTime EndDate,
    int? UnitId = null
);

public record ComparisonFilter(
    DateTime StartDate,
    DateTime EndDate,
    List<int>? UnitIds = null
);

public record RevenueBreakdownFilter(
    DateTime StartDate,
    DateTime EndDate,
    int? UnitId = null
);

public record CancellationFilter(
    DateTime StartDate,
    DateTime EndDate,
    int? UnitId = null
);

public record ExportReportRequest(
    ReportType ReportType,
    DateTime StartDate,
    DateTime EndDate,
    int? UnitId = null,
    ExportFormat Format = ExportFormat.Excel
);

public enum TrendsPeriod
{
    Daily,
    Weekly,
    Monthly
}

public enum ReportType
{
    Financial,
    Occupancy,
    Booking,
    Revenue,
    Customer,
    Cancellation,
    Performance
}

public enum ExportFormat
{
    Excel,
    PDF,
    CSV
}

#endregion

#region Report Responses

public record FinancialReportResponse
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public decimal TotalRevenue { get; init; }
    public decimal TotalExpenses { get; init; }
    public decimal NetIncome { get; init; }

    public decimal AverageDailyRevenue { get; init; }
    public decimal ProjectedMonthlyRevenue { get; init; }
    public decimal YearToDateRevenue { get; init; }
    public decimal GrowthRate { get; init; }
    public List<RevenueByUnit> RevenueByUnit { get; init; } = [];
    public List<RevenueByMonth> MonthlyBreakdown { get; init; } = [];
    public List<PaymentMethodBreakdown> PaymentMethods { get; init; } = [];
    public List<RevenueSource> RevenueSources { get; init; } = [];
}

public record OccupancyStatisticsResponse
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public decimal OverallOccupancyRate { get; init; }
    public decimal AverageDailyRate { get; init; }
    public decimal RevPAR { get; init; } // Revenue Per Available Room
    public int TotalAvailableRoomNights { get; init; }
    public int TotalOccupiedRoomNights { get; init; }
    public List<UnitOccupancyDetail> UnitOccupancy { get; init; } = [];
    public List<DailyOccupancy> DailyOccupancy { get; init; } = [];
    public List<SubUnitOccupancyDetail> TopPerformingRooms { get; init; } = [];
    public List<SubUnitOccupancyDetail> LowPerformingRooms { get; init; } = [];
}

public record BookingTrendsResponse
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int TotalBookings { get; init; }
    public decimal AverageLeadTime { get; init; }
    public decimal AverageLengthOfStay { get; init; }
    public decimal BookingConversionRate { get; init; }
    public List<BookingByPeriod> BookingsByPeriod { get; init; } = [];
    public List<BookingByDayOfWeek> BookingsByDayOfWeek { get; init; } = [];
    public List<BookingBySource> BookingsBySource { get; init; } = [];
    public List<SeasonalTrend> SeasonalTrends { get; init; } = [];
    public List<PeakPeriod> PeakPeriods { get; init; } = [];
}

public record CustomerInsightsResponse
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int TotalUniqueGuests { get; init; }
    public int ReturningGuests { get; init; }
    public decimal ReturnGuestRate { get; init; }
    public decimal AverageGuestsPerBooking { get; init; }
    public decimal AverageBookingValue { get; init; }
    public decimal CustomerLifetimeValue { get; init; }
    public List<GuestDemographic> Demographics { get; init; } = [];
    public List<TopGuest> TopGuests { get; init; } = [];
    public List<GuestPreference> Preferences { get; init; } = [];
    public List<ReviewSummary> ReviewInsights { get; init; } = [];
}

public record PerformanceComparisonResponse
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public List<UnitPerformance> UnitPerformances { get; init; } = [];
    public List<MetricComparison> MetricComparisons { get; init; } = [];
    public UnitPerformance BestPerformer { get; init; } = default!;
    public UnitPerformance WorstPerformer { get; init; } = default!;
}

public record RevenueBreakdownResponse
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public decimal TotalRevenue { get; init; }
    public List<RevenueByRoomType> ByRoomType { get; init; } = [];
    public List<RevenueByBookingType> ByBookingType { get; init; } = [];
    public List<RevenueByPaymentMethod> ByPaymentMethod { get; init; } = [];
    public List<RevenueByDayType> ByDayType { get; init; } = []; // Weekday vs Weekend
    public List<RevenueByMonth> ByMonth { get; init; } = [];
}

public record CancellationAnalyticsResponse
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int TotalCancellations { get; init; }
    public decimal CancellationRate { get; init; }
    public decimal LostRevenue { get; init; }
    public decimal AverageCancellationLeadTime { get; init; }
    public List<CancellationByReason> ByReason { get; init; } = [];
    public List<CancellationByTimeframe> ByTimeframe { get; init; } = [];
    public List<CancellationByUnit> ByUnit { get; init; } = [];
    public List<RefundSummary> RefundSummary { get; init; } = [];
}

public record PricingOptimizationResponse
{
    public int UnitId { get; init; }
    public string UnitName { get; init; } = string.Empty;
    public DateTime AnalysisPeriodStart { get; init; }
    public DateTime AnalysisPeriodEnd { get; init; }
    public decimal CurrentAverageRate { get; init; }
    public decimal RecommendedAverageRate { get; init; }
    public decimal PotentialRevenueIncrease { get; init; }
    public List<PricingSuggestion> Suggestions { get; init; } = [];
    public List<CompetitorPricing> CompetitorAnalysis { get; init; } = [];
    public List<DemandForecast> DemandForecasts { get; init; } = [];
}

#endregion

#region Report Supporting Types

public record RevenueByUnit
{
    public int UnitId { get; init; }
    public string UnitName { get; init; } = string.Empty;
    public decimal Revenue { get; init; }
    public int BookingCount { get; init; }
    public decimal AverageBookingValue { get; init; }
    public decimal ContributionPercentage { get; init; }
}

public record RevenueByMonth
{
    public string Month { get; init; } = string.Empty;
    public int Year { get; init; }
    public decimal Revenue { get; init; }
    public int BookingCount { get; init; }
    public decimal GrowthRate { get; init; }
}

public record PaymentMethodBreakdown
{
    public string PaymentMethod { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public int TransactionCount { get; init; }
    public decimal Percentage { get; init; }
}

public record RevenueSource
{
    public string Source { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public decimal Percentage { get; init; }
}

public record UnitOccupancyDetail
{
    public int UnitId { get; init; }
    public string UnitName { get; init; } = string.Empty;
    public decimal OccupancyRate { get; init; }
    public int AvailableRoomNights { get; init; }
    public int OccupiedRoomNights { get; init; }
    public decimal AverageDailyRate { get; init; }
    public decimal RevPAR { get; init; }
}

public record DailyOccupancy
{
    public DateTime Date { get; init; }
    public decimal OccupancyRate { get; init; }
    public int OccupiedRooms { get; init; }
    public int AvailableRooms { get; init; }
    public decimal Revenue { get; init; }
}

public record SubUnitOccupancyDetail
{
    public int SubUnitId { get; init; }
    public string RoomNumber { get; init; } = string.Empty;
    public string UnitName { get; init; } = string.Empty;
    public decimal OccupancyRate { get; init; }
    public int BookingCount { get; init; }
    public decimal Revenue { get; init; }
}

public record BookingByPeriod
{
    public string Period { get; init; } = string.Empty;
    public int BookingCount { get; init; }
    public decimal Revenue { get; init; }
    public decimal AverageValue { get; init; }
}

public record BookingByDayOfWeek
{
    public string DayOfWeek { get; init; } = string.Empty;
    public int BookingCount { get; init; }
    public decimal AverageValue { get; init; }
}

public record BookingBySource
{
    public string Source { get; init; } = string.Empty;
    public int Count { get; init; }
    public decimal Percentage { get; init; }
}

public record SeasonalTrend
{
    public string Season { get; init; } = string.Empty;
    public decimal AverageOccupancy { get; init; }
    public decimal AverageRate { get; init; }
    public int BookingCount { get; init; }
}

public record PeakPeriod
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public decimal OccupancyRate { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public record GuestDemographic
{
    public string Category { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public int Count { get; init; }
    public decimal Percentage { get; init; }
}

public record TopGuest
{
    public string UserId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int BookingCount { get; init; }
    public decimal TotalSpent { get; init; }
    public DateTime LastBooking { get; init; }
}

public record GuestPreference
{
    public string Preference { get; init; } = string.Empty;
    public int Count { get; init; }
    public decimal Percentage { get; init; }
}

public record ReviewSummary
{
    public decimal AverageRating { get; init; }
    public int TotalReviews { get; init; }
    public int PositiveReviews { get; init; }
    public int NegativeReviews { get; init; }
    public List<string> CommonPraise { get; init; } = [];
    public List<string> CommonComplaints { get; init; } = [];
}

public record UnitPerformance
{
    public int UnitId { get; init; }
    public string UnitName { get; init; } = string.Empty;
    public decimal Revenue { get; init; }
    public decimal OccupancyRate { get; init; }
    public decimal AverageDailyRate { get; init; }
    public decimal RevPAR { get; init; }
    public decimal Rating { get; init; }
    public int BookingCount { get; init; }
}

public record MetricComparison
{
    public string MetricName { get; init; } = string.Empty;
    public Dictionary<int, decimal> UnitValues { get; init; } = [];
    public decimal Average { get; init; }
    public int BestUnitId { get; init; }
    public int WorstUnitId { get; init; }
}

public record RevenueByRoomType
{
    public string RoomType { get; init; } = string.Empty;
    public decimal Revenue { get; init; }
    public int BookingCount { get; init; }
    public decimal Percentage { get; init; }
}

public record RevenueByBookingType
{
    public string BookingType { get; init; } = string.Empty;
    public decimal Revenue { get; init; }
    public int Count { get; init; }
    public decimal Percentage { get; init; }
}

public record RevenueByPaymentMethod
{
    public string PaymentMethod { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public decimal Percentage { get; init; }
}

public record RevenueByDayType
{
    public string DayType { get; init; } = string.Empty; // "Weekday" or "Weekend"
    public decimal Revenue { get; init; }
    public int NightCount { get; init; }
    public decimal AverageRate { get; init; }
}

public record CancellationByReason
{
    public string Reason { get; init; } = string.Empty;
    public int Count { get; init; }
    public decimal Percentage { get; init; }
    public decimal LostRevenue { get; init; }
}

public record CancellationByTimeframe
{
    public string Timeframe { get; init; } = string.Empty;
    public int Count { get; init; }
    public decimal Percentage { get; init; }
}

public record CancellationByUnit
{
    public int UnitId { get; init; }
    public string UnitName { get; init; } = string.Empty;
    public int CancellationCount { get; init; }
    public decimal CancellationRate { get; init; }
    public decimal LostRevenue { get; init; }
}

public record RefundSummary
{
    public string RefundType { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public int Count { get; init; }
}

public record PricingSuggestion
{
    public DateTime Date { get; init; }
    public decimal CurrentPrice { get; init; }
    public decimal SuggestedPrice { get; init; }
    public string Reason { get; init; } = string.Empty;
    public decimal ExpectedImpact { get; init; }
}

public record CompetitorPricing
{
    public string CompetitorName { get; init; } = string.Empty;
    public decimal AveragePrice { get; init; }
    public decimal OurPrice { get; init; }
    public decimal Difference { get; init; }
}

public record DemandForecast
{
    public DateTime Date { get; init; }
    public decimal ForecastedDemand { get; init; }
    public decimal SuggestedPrice { get; init; }
    public string Confidence { get; init; } = string.Empty;
}

#endregion

// ============================================================================
// SUBUNIT MANAGEMENT CONTRACTS (EXTENDED)
// ============================================================================

#region SubUnit Requests

public record UpdateSubUnitRequest(
    string? RoomNumber = null,
    string? Description = null,
    int? MaxOccupancy = null,
    int? Bedrooms = null,
    int? Bathrooms = null,
    decimal? Size = null
);

public record UpdateSubUnitPricingRequest(
    [Range(0, double.MaxValue)] decimal PricePerNight
);

#endregion

#region SubUnit Responses

public record SubUnitResponse
{
    public int Id { get; init; }
    public int UnitId { get; init; }
    public string RoomNumber { get; init; } = string.Empty;
    public decimal PricePerNight { get; init; }
    public int MaxOccupancy { get; init; }
    public bool IsAvailable { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record SubUnitBookingHistoryResponse
{
    public int BookingId { get; init; }
    public string BookingNumber { get; init; } = string.Empty;
    public DateTime CheckInDate { get; init; }
    public DateTime CheckOutDate { get; init; }
    public string GuestName { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Status { get; init; } = string.Empty;
}

#endregion

// ============================================================================
// AMENITIES MANAGEMENT CONTRACTS
// ============================================================================

public record UpdateAmenitiesRequest(
    List<int> AmenityIds,
    List<int>? UnavailableAmenityIds = null
);

// ============================================================================
// IMAGE MANAGEMENT CONTRACTS
// ============================================================================

public record UpdateImageOrderRequest(
    List<ImageOrderItem> ImageOrders,
    int? PrimaryImageId
);

public record ImageOrderItem(
    int ImageId,
    int Order,
    bool IsPrimary = false
);

public record ImageDetailResponse
{
    public int Id { get; init; }
    public string ImageUrl { get; init; } = string.Empty;
    public string? ThumbnailUrl { get; init; }
    public bool IsPrimary { get; init; }
    public int DisplayOrder { get; init; }
    public string? Caption { get; init; }
    public string? AltText { get; init; }
    public long FileSizeBytes { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public DateTime UploadedAt { get; init; }
}

// ============================================================================
// NOTIFICATIONS & ALERTS CONTRACTS
// ============================================================================

public record NotificationFilter(
    bool? IsRead = null,
    NotificationType? Type = null,
    int Page = 1,
    int PageSize = 20
);

public record AdminNotificationResponse
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public bool IsRead { get; init; }
    public DateTime? ReadAt { get; init; }
    public DateTime ReceivedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record AlertSettingsResponse
{
    public bool NewBookingAlert { get; init; }
    public bool CancellationAlert { get; init; }
    public bool ReviewAlert { get; init; }
    public bool PaymentAlert { get; init; }
    public bool LowAvailabilityAlert { get; init; }
    public bool MaintenanceAlert { get; init; }
    public int LowAvailabilityThreshold { get; init; }
    public string PreferredNotificationMethod { get; init; } = string.Empty;
}

public record UpdateAlertSettingsRequest(
    bool? NewBookingAlert = null,
    bool? CancellationAlert = null,
    bool? ReviewAlert = null,
    bool? PaymentAlert = null,
    bool? LowAvailabilityAlert = null,
    bool? MaintenanceAlert = null,
    int? LowAvailabilityThreshold = null,
    string? PreferredNotificationMethod = null
);


public record MarkDatesAsBookedRequest(
    BookingType BookingType,
    int? UnitId = null,
    List<int>? SubUnitIds = null,
    int BookingId = 0,
    DateTime CheckInDate = default,
    DateTime CheckOutDate = default
);
#region DASHBOARD RESPONSES

public record AvailableSubUnitInfo
{
    public int Id { get; init; }
    public string RoomNumber { get; init; } = string.Empty;
    public int TypeId { get; init; }
    public decimal PricePerNight { get; init; }
    public int MaxOccupancy { get; init; }
    public bool IsAvailable { get; init; }
    public decimal? SpecialPrice { get; init; }
}

public class HotelAdminDashboardResponse
{
    public List<UnitSummary> Units { get; set; } = [];
    public DashboardStats Stats { get; set; } = new();
    public List<RecentBooking> RecentBookings { get; set; } = [];
    public List<UpcomingCheckInResponse> TodayCheckIns { get; set; } = [];
    public List<UpcomingCheckOutResponse> TodayCheckOuts { get; set; } = [];
    public RevenueOverview RevenueOverview { get; set; } = new();
}

public class UnitSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int TotalSubUnits { get; set; }
    public int AvailableSubUnits { get; set; }
    public int ActiveBookings { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal AverageRating { get; set; }
}

public class DashboardStats
{
    public int TodayCheckIns { get; set; }
    public int TodayCheckOuts { get; set; }
    public int CurrentGuests { get; set; }
    public int PendingBookings { get; set; }
    public int UnreadReviews { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal WeekRevenue { get; set; }
    public decimal MonthRevenue { get; set; }
}

public class RecentBooking
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

public class RevenueOverview
{
    public decimal TodayRevenue { get; set; }
    public decimal WeekRevenue { get; set; }
    public decimal MonthRevenue { get; set; }
    public decimal YearRevenue { get; set; }
    public decimal PendingPayments { get; set; }
    public List<MonthlyRevenueData> MonthlyTrend { get; set; } = [];
}

public class MonthlyRevenueData
{
    public string Month { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
}

public class HotelAdminStatsResponse
{
    public int TotalUnits { get; set; }
    public int ActiveUnits { get; set; }
    public int TotalSubUnits { get; set; }
    public int AvailableSubUnits { get; set; }
    public int TotalBookings { get; set; }
    public int ActiveBookings { get; set; }
    public int PendingBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PendingRevenue { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public decimal OccupancyRate { get; set; }
}

#endregion

#region UNIT RESPONSES

public class UnitComprehensiveResponse
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
    public List<string> Options { get; init; } = new();
    public string Currency { get; init; } = "SAR";
    public List<CustomPolicyDetail> CustomPolicies { get; init; } = new();

    // ... rest of existing properties ...
}

public record CustomPolicyDetail
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Category { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
}

public class UnitResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public int? MaxGuests { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public int? Rank { get; set; }
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ImageResponse
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public string? Caption { get; set; }
}

public class AmenityResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
}

public class PolicyResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PolicyType { get; set; } = string.Empty;
}

#endregion

#region BOOKING RESPONSES

public class BookingComprehensiveResponse
{
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
    public string? UserCity { get; set; }
    public string? UserCountry { get; set; }
}

public class BookingDetailsResponse
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
}

public class BookedRoomInfo
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
}

public class PaymentInfo
{
    public int Id { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
}

#endregion

#region SUBUNIT RESPONSES

public class SubUnitComprehensiveDetail
{
    public int Id { get; set; }
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string RoomNumber { get; set; } = string.Empty;
    public string SubUnitTypeName { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public int MaxOccupancy { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public decimal? Size { get; set; }
    public bool IsAvailable { get; set; }
    public string? Description { get; set; }
    public List<ImageResponse> Images { get; set; } = [];
    public List<AmenityResponse> Amenities { get; set; } = [];
}

#endregion

#region AVAILABILITY RESPONSES

public class AvailabilityCalendarResponse
{
    public int UnitId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<SubUnitAvailabilityInfo> SubUnits { get; set; } = [];
}

public class SubUnitAvailabilityInfo
{
    public int SubUnitId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public List<AvailabilityPeriod> AvailabilityPeriods { get; set; } = [];
    public List<BookingPeriod> Bookings { get; set; } = [];
}

public class AvailabilityPeriod
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsAvailable { get; set; }
    public string? Reason { get; set; }
    public decimal? SpecialPrice { get; set; }
    public decimal? WeekendPrice { get; set; }
}

public class BookingPeriod
{
    public int BookingId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

#endregion

#region REVENUE & ANALYTICS RESPONSES

public class RevenueReportResponse
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalBookings { get; set; }
    public decimal AverageBookingValue { get; set; }
    public List<UnitRevenueData> RevenueByUnit { get; set; } = [];
    public List<MonthlyRevenueData> RevenueByMonth { get; set; } = [];
}

public class UnitRevenueData
{
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
    public decimal AverageBookingValue { get; set; }
}

public class BookingAnalyticsResponse
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public decimal CancellationRate { get; set; }
    public double AverageLeadTime { get; set; }
    public double AverageStayDuration { get; set; }
    public List<StatusCount> StatusDistribution { get; set; } = [];
    public List<DailyBookingData> BookingsByDay { get; set; } = [];
}

public class StatusCount
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class DailyBookingData
{
    public DateTime Date { get; set; }
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
}

public class OccupancyReportResponse
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal OverallOccupancyRate { get; set; }
    public List<UnitOccupancyData> OccupancyByUnit { get; set; } = [];
}

public class UnitOccupancyData
{
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public decimal OccupancyRate { get; set; }
    public int TotalRooms { get; set; }
    public int AvailableRooms { get; set; }
}

#endregion

#region REVIEW RESPONSES

public class ReviewResponse
{
    public int Id { get; set; }
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public int BookingId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public int CleanlinessRating { get; set; }
    public int LocationRating { get; set; }
    public int ServiceRating { get; set; }
    public int ValueRating { get; set; }
    public string? Comment { get; set; }
    public string? OwnerResponse { get; set; }
    public DateTime? OwnerResponseDate { get; set; }
    public List<ImageResponse> Images { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}

#endregion

#region SCHEDULE RESPONSES

public class UpcomingCheckInResponse
{
    public int BookingId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
    public string GuestPhone { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; }
    public int NumberOfNights { get; set; }
    public List<RoomInfo> Rooms { get; set; } = [];
    public string? SpecialRequests { get; set; }
}

public class UpcomingCheckOutResponse
{
    public int BookingId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
    public string GuestPhone { get; set; } = string.Empty;
    public DateTime CheckOutDate { get; set; }
    public int NumberOfNights { get; set; }
    public List<RoomInfo> Rooms { get; set; } = [];
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingBalance { get; set; }
}

public class RoomInfo
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
}

#endregion

#region PAYMENT RESPONSES

public class PaymentHistoryResponse
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

public class PendingPaymentResponse
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

#endregion

#region REQUEST MODELS

public class UpdateUnitRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Address { get; set; }
    public decimal? BasePrice { get; set; }
    public int? MaxGuests { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public int? Rank { get; set; }
}

public class UpdateAvailabilityRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsAvailable { get; set; }
    public Domain.Entities.UnavailabilityReason? Reason { get; set; }
    public decimal? SpecialPrice { get; set; }
    public decimal? WeekendPrice { get; set; }
}

public class CancelBookingRequest
{
    public string CancellationReason { get; set; } = string.Empty;
}

public class ReviewResponseRequest
{
    public string Response { get; set; } = string.Empty;
}

#endregion

#region FILTER MODELS

public class UnitFilter
{
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsVerified { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchKeyword { get; set; }

}

public class BookingFilter
{
    public BookingStatus? Status { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchKeyword { get; set; }
}

public class RevenueReportFilter
{
    public int? UnitId { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow.AddMonths(-1);
    public DateTime EndDate { get; set; } = DateTime.UtcNow;
}

public class AnalyticsFilter
{
    public int? UnitId { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow.AddMonths(-1);
    public DateTime EndDate { get; set; } = DateTime.UtcNow;
}

public class ReviewFilter
{
    public int? UnitId { get; set; }
    public int? MinRating { get; set; }
    public bool? HasResponse { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchKeyword { get; set; }
}

public class PaymentFilter
{
    public int? UnitId { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchKeyword { get; set; }
}

#endregion

// ============================================================================
// PART 3: Add these contracts to Application/Contracts/hoteladmincont/
// Create these files in the appropriate directory
// ============================================================================



public enum PricingUpdateType
{
    Fixed = 1,
    Percentage = 2,
    Amount = 3
}
