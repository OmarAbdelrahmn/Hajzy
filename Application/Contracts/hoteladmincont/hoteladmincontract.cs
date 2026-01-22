// Application/Contracts/hoteladmincont/HotelAdminContracts.cs

using Domain;

namespace Application.Contracts.hoteladmincont;

#region DASHBOARD RESPONSES

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
}

public class BookingFilter
{
    public BookingStatus? Status { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
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
}

public class PaymentFilter
{
    public int? UnitId { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

#endregion