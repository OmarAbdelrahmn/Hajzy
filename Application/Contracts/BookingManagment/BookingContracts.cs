using Domain;
using Domain.Entities;

namespace Application.Contracts.BookingManagement;

// ============= FILTERS =============

public record UnifiedBookingFilter
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public BookingStatus? Status { get; init; }
    public PaymentStatus? PaymentStatus { get; init; }
    public BookingType? BookingType { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int? UnitId { get; init; }
    public string? UserId { get; init; }
}

public record PendingPaymentFilter
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int? UnitId { get; init; }
    public int? DaysUntilCheckIn { get; init; }
}

public record BookingStatisticsFilter
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int? UnitId { get; init; }
    public BookingType? BookingType { get; init; }
}

public record RevenueAnalyticsFilter
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int? UnitId { get; init; }
}

public record OccupancyFilter
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int? UnitId { get; init; }
}

public record CancellationAnalyticsFilter
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int? UnitId { get; init; }
}

public record BookingTrendsFilter
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int? UnitId { get; init; }
}

public record ReportOptions
{
    public int? UnitId { get; init; }
    public BookingStatus? Status { get; init; }
    public BookingType? BookingType { get; init; }
    public bool IncludePayments { get; init; } = false;
    public bool IncludeGuests { get; init; } = false;
}

// ============= REQUESTS =============

public record CheckInRequest
{
    public string? Notes { get; init; }
    public DateTime? ActualCheckInTime { get; init; }
}

public record CheckOutRequest
{
    public string? Notes { get; init; }
    public DateTime? ActualCheckOutTime { get; init; }
    public decimal? ExtraCharges { get; init; }
}

public record ProcessPaymentRequest
{
    public string TransactionId { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public int PaymentMethod { get; init; }
    public string? Notes { get; init; }
}

public record RefundRequest
{
    public decimal Amount { get; init; }
    public string Reason { get; init; } = string.Empty;
    public RefundMethod RefundMethod { get; init; }
}

public enum RefundMethod
{
    OriginalPaymentMethod = 1,
    BankTransfer = 2,
    Cash = 3,
    StoreCredit = 4
}

// ============= RESPONSES =============

public class UnifiedBookingResponse
{
    public int Id { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public string BookingType { get; set; } = string.Empty;

    // Unit Info
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;

    // Guest Info
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;

    // Booking Details
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; }
    public int NumberOfNights { get; set; }

    // Pricing
    public decimal TotalPrice { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingBalance { get; set; }

    // Status
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;

    // Rooms (for SubUnit bookings)
    public List<BookedRoomInfo> Rooms { get; set; } = new();

    // Coupon
    public string? AppliedCouponCode { get; set; }
    public decimal? CouponDiscount { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
}

public class UnifiedBookingDetailsResponse : UnifiedBookingResponse
{
    public string UnitAddress { get; set; } = string.Empty;
    public string CityName { get; set; } = string.Empty;

    public string? UserPhone { get; set; }

    public string? SpecialRequests { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }

    public List<PaymentHistoryItem> Payments { get; set; } = new();

    public bool HasReview { get; set; }
    public int? ReviewRating { get; set; }

    public string? CancellationPolicyName { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public class BookedRoomInfo
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
}

public class PaymentHistoryItem
{
    public int Id { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public string? Notes { get; set; }
}

public class CancellationResponse
{
    public int BookingId { get; set; }
    public decimal RefundAmount { get; set; }
    public DateTime CancellationDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class PendingPaymentResponse
{
    public int BookingId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public int DaysUntilCheckIn { get; set; }
}

// ============= CALENDAR & SCHEDULING RESPONSES =============

public class UpcomingCheckInResponse
{
    public int BookingId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public string BookingType { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
    public string GuestPhone { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public TimeSpan CheckInTime { get; set; }
    public int NumberOfGuests { get; set; }
    public int NumberOfNights { get; set; }
    public List<string> RoomNumbers { get; set; } = new();
    public string? SpecialRequests { get; set; }
}

public class UpcomingCheckOutResponse
{
    public int BookingId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public string BookingType { get; set; } = string.Empty;
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
    public string GuestPhone { get; set; } = string.Empty;
    public DateTime CheckOutDate { get; set; }
    public TimeSpan CheckOutTime { get; set; }
    public List<string> RoomNumbers { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceDue { get; set; }
}

public class CurrentGuestResponse
{
    public int BookingId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public string BookingType { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
    public string GuestPhone { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public List<string> RoomNumbers { get; set; } = new();
    public int DaysRemaining { get; set; }
}

public class BookingTimelineResponse
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalBookings { get; set; }
    public List<DateBookingSummary> BookingsByDate { get; set; } = new();
    public UnifiedBookingResponse[] Bookings { get; set; } = Array.Empty<UnifiedBookingResponse>();
}

public class DateBookingSummary
{
    public DateTime Date { get; set; }
    public int CheckIns { get; set; }
    public int CheckOuts { get; set; }
}

// ============= STATISTICS & ANALYTICS RESPONSES =============

public class BookingStatisticsResponse
{
    public int TotalBookings { get; set; }
    public int PendingBookings { get; set; }
    public int ConfirmedBookings { get; set; }
    public int CheckedInBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }

    public int UnitBookings { get; set; }
    public int SubUnitBookings { get; set; }

    public decimal TotalRevenue { get; set; }
    public decimal PendingRevenue { get; set; }
    public decimal AverageBookingValue { get; set; }
    public double AverageNightsPerBooking { get; set; }

    public Dictionary<string, int> BookingsByStatus { get; set; } = new();
    public Dictionary<string, int> BookingsByType { get; set; } = new();
    public Dictionary<string, int> BookingsByMonth { get; set; } = new();
}

public class RevenueAnalyticsResponse
{
    public decimal TotalRevenue { get; set; }
    public int TotalBookings { get; set; }
    public decimal AverageRevenuePerBooking { get; set; }

    public Dictionary<string, decimal> RevenueByBookingType { get; set; } = new();
    public List<UnitRevenue> RevenueByUnit { get; set; } = new();
    public List<MonthlyRevenue> RevenueByMonth { get; set; } = new();
}

public class UnitRevenue
{
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
}

public class MonthlyRevenue
{
    public string Month { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
}

public class OccupancyStatisticsResponse
{
    public string Period { get; set; } = string.Empty;
    public decimal OccupancyRate { get; set; }
    public int TotalRoomNights { get; set; }
    public int BookedRoomNights { get; set; }
    public int AvailableRoomNights { get; set; }
}

public class CancellationAnalyticsResponse
{
    public int TotalCancellations { get; set; }
    public decimal CancellationRate { get; set; }
    public decimal RefundedAmount { get; set; }
    public decimal AverageCancellationLeadTime { get; set; }
}

public class BookingTrendsResponse
{
    public List<MonthlyTrend> MonthlyTrends { get; set; } = new();
    public int TotalBookings { get; set; }
    public string TrendDirection { get; set; } = string.Empty;
}

public class MonthlyTrend
{
    public string Month { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
}

// ============= REPORT RESPONSES =============

public class DailyBookingReportResponse
{
    public DateTime Date { get; set; }
    public int NewBookings { get; set; }
    public int CheckIns { get; set; }
    public int CheckOuts { get; set; }
    public decimal Revenue { get; set; }
    public int Cancellations { get; set; }
    public int CurrentOccupancy { get; set; }
}

public class MonthlyBookingReportResponse
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int TotalBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageBookingValue { get; set; }
    public List<DailyBookingCount> BookingsByDay { get; set; } = new();
}

public class DailyBookingCount
{
    public int Day { get; set; }
    public int Count { get; set; }
}

public class CustomBookingReportResponse
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageBookingValue { get; set; }
    public Dictionary<string, int> BookingsByStatus { get; set; } = new();
    public List<UnitBookingCount> TopUnits { get; set; } = new();
}

public class UnitBookingCount
{
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public int BookingCount { get; set; }
}

// ============= VALIDATION & UTILITIES =============

public class BookingModificationValidation
{
    public bool CanModify { get; set; }
    public List<string> Reasons { get; set; } = new();
}

public class BookingDashboardSummary
{
    public int TotalBookings { get; set; }
    public int PendingBookings { get; set; }
    public int ConfirmedBookings { get; set; }
    public int CheckedInBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }

    public int TodayCheckIns { get; set; }
    public int TodayCheckOuts { get; set; }
    public int CurrentGuests { get; set; }

    public decimal TotalRevenue { get; set; }
    public decimal PendingPayments { get; set; }
}

// ============= BULK OPERATIONS =============

public class BulkOperationResult
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<BulkOperationError> Errors { get; set; } = new();
}

public class BulkOperationError
{
    public int Id { get; set; }
    public string Error { get; set; } = string.Empty;
}