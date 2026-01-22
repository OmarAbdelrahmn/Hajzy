using Application.Service.Availability;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Contracts.hoteladmincont;

public class hoteladmincontract
{
}


public record HotelAdminDashboardResponse
{
    public List<UnitSummary> Units { get; set; } = [];
    public DashboardStats Stats { get; set; } = new();
    public List<RecentBooking> RecentBookings { get; set; } = [];
    public List<UpcomingCheckIn> TodayCheckIns { get; set; } = [];
    public List<UpcomingCheckOut> TodayCheckOuts { get; set; } = [];
    public RevenueOverview RevenueOverview { get; set; } = new();
}

public record HotelAdminStatsResponse
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

public record UnitSummary
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

public record DashboardStats
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

public record UpcomingCheckIn
{
    public int BookingId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string GuestPhone { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public List<string> RoomNumbers { get; set; } = [];
    public DateTime CheckInDate { get; set; }
    public int NumberOfGuests { get; set; }
    public string? SpecialRequests { get; set; }
}

public record UpcomingCheckOut
{
    public int BookingId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public List<string> RoomNumbers { get; set; } = [];
    public DateTime CheckOutDate { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceDue { get; set; }
}

public record RevenueOverview
{
    public decimal TodayRevenue { get; set; }
    public decimal WeekRevenue { get; set; }
    public decimal MonthRevenue { get; set; }
    public decimal YearRevenue { get; set; }
    public decimal PendingPayments { get; set; }
    public List<MonthlyRevenueData> MonthlyTrend { get; set; } = [];
}

public record MonthlyRevenueData
{
    public string Month { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
}

public record BookingComprehensiveResponse
{
    public int Id { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public string BookingType { get; set; } = string.Empty;
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string? UserPhone { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; }
    public int NumberOfNights { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal PaidAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public List<BookedRoomInfo> Rooms { get; set; } = [];
    public string? SpecialRequests { get; set; }
    public string? AppliedCouponCode { get; set; }
    public string? CouponDiscount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public record BookedRoomInfo
{
    public int SubUnitId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public int NumberOfNights { get; set; }
    public decimal SubTotal { get; set; }
}

public record BookingDetailsResponse : BookingComprehensiveResponse
{
    public string UnitAddress { get; set; } = string.Empty;
    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }
    public List<PaymentInfo> Payments { get; set; } = [];
    public DateTime? UpdatedAt { get; set; }
}

public record AvailabilityCalendarResponse
{
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<SubUnitAvailabilityInfo> SubUnits { get; set; } = [];
}

public record SubUnitAvailabilityInfo
{
    public int SubUnitId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public List<DateAvailability> Calendar { get; set; } = [];
}

public record DateAvailability
{
    public DateTime Date { get; set; }
    public bool IsAvailable { get; set; }
    public string? Reason { get; set; }
    public decimal? SpecialPrice { get; set; }
    public int? BookingId { get; set; }
    public string? BookingNumber { get; set; }
}

public record UpdateAvailabilityRequest(
    DateTime StartDate,
    DateTime EndDate,
    bool IsAvailable,
    string? Reason,
    decimal? SpecialPrice,
    decimal? WeekendPrice
);

public record RevenueReportResponse
{
    public decimal TotalRevenue { get; set; }
    public decimal CompletedRevenue { get; set; }
    public decimal PendingRevenue { get; set; }
    public decimal RefundedAmount { get; set; }
    public int TotalBookings { get; set; }
    public decimal AverageBookingValue { get; set; }
    public List<DailyRevenue> DailyBreakdown { get; set; } = [];
    public List<UnitRevenue> ByUnit { get; set; } = [];
    public List<MonthlyRevenue> MonthlyBreakdown { get; set; } = [];
}

public record DailyRevenue
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
}

public record UnitRevenue
{
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
    public decimal OccupancyRate { get; set; }
}

public record MonthlyRevenue
{
    public string Month { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
}

public record BookingAnalyticsResponse
{
    public int TotalBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public decimal CancellationRate { get; set; }
    public decimal AverageLeadTime { get; set; }
    public decimal AverageStayDuration { get; set; }
    public List<BookingSourceData> BySource { get; set; } = [];
    public List<BookingStatusData> ByStatus { get; set; } = [];
    public List<DailyBookingData> DailyTrend { get; set; } = [];
}

public record BookingSourceData
{
    public string Source { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Revenue { get; set; }
}

public record BookingStatusData
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public record DailyBookingData
{
    public DateTime Date { get; set; }
    public int BookingCount { get; set; }
    public int CheckIns { get; set; }
    public int CheckOuts { get; set; }
}

public record OccupancyReportResponse
{
    public decimal OverallOccupancyRate { get; set; }
    public int TotalRoomNights { get; set; }
    public int OccupiedRoomNights { get; set; }
    public List<UnitOccupancy> ByUnit { get; set; } = [];
    public List<DailyOccupancy> DailyBreakdown { get; set; } = [];
}

public record UnitOccupancy
{
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public decimal OccupancyRate { get; set; }
    public int TotalSubUnits { get; set; }
    public int AverageOccupiedSubUnits { get; set; }
}

public record DailyOccupancy
{
    public DateTime Date { get; set; }
    public decimal OccupancyRate { get; set; }
    public int OccupiedRooms { get; set; }
    public int TotalRooms { get; set; }
}

public record ReviewResponse
{
    public int Id { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public int CleanlinessRating { get; set; }
    public int LocationRating { get; set; }
    public int ServiceRating { get; set; }
    public int ValueRating { get; set; }
    public string? Comment { get; set; }
    public string? OwnerResponse { get; set; }
    public DateTime? OwnerResponseDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
}

public record UpcomingCheckInResponse : UpcomingCheckIn;
public record UpcomingCheckOutResponse : UpcomingCheckOut;

public record PaymentHistoryResponse
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
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
    public string GuestName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceDue { get; set; }
    public DateTime CheckInDate { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
}

// ============= FILTER DTOs =============

public record RevenueReportFilter(
    int? UnitId,
    DateTime? StartDate,
    DateTime? EndDate,
    string? GroupBy = "day" // day, week, month
);

public record AnalyticsFilter(
    int? UnitId,
    DateTime? StartDate,
    DateTime? EndDate
);

public record ReviewFilter(
    int? UnitId,
    int? MinRating,
    bool? HasResponse,
    int Page = 1,
    int PageSize = 20
);

public record PaymentFilter(
    int? UnitId,
    int? BookingId,
    string? PaymentStatus,
    DateTime? StartDate,
    DateTime? EndDate,
    int Page = 1,
    int PageSize = 20
);