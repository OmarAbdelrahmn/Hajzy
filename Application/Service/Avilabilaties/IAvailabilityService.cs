using Application.Abstraction;
using Application.Contracts.Availability;
using Application.Contracts.SubUnit;
using Domain;
using Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Application.Service.Avilabilaties;

/// <summary>
/// Service for managing both Unit and SubUnit availability with smart defaults
/// </summary>
public interface IAvailabilityService
{
    /// <summary>
    /// Set manual availability override for a unit (maintenance, seasonal closure, etc.)
    /// </summary>
    Task<Result> SetUnitAvailabilityAsync(SetUnitAvailabilityRequest request);

    /// <summary>
    /// Get unit availability for a date range
    /// </summary>
    Task<Result<List<AvailabilityResponse>>> GetUnitAvailabilityAsync(
        int unitId,
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Check if entire unit is available (no bookings and no blocks)
    /// </summary>
    Task<Result<Contracts.hoteladmincont.UnitAvailabilityStatus>> CheckUnitAvailabilityAsync(
        int unitId,
        DateTime checkIn,
        DateTime checkOut);

    /// <summary>
    /// Get detailed availability calendar for unit
    /// </summary>
    Task<Result<Dictionary<DateTime, UnitDayAvailability>>> GetUnitAvailabilityCalendarAsync(
        int unitId,
        int year,
        int month);

    // ============= SUBUNIT AVAILABILITY =============

    /// <summary>
    /// Set manual availability override for a subunit
    /// </summary>
    Task<Result> SetSubUnitAvailabilityAsync(SetSubUnitAvailabilityRequest request);

    /// <summary>
    /// Get subunit availability for a date range
    /// </summary>
    Task<Result<List<AvailabilityResponse>>> GetSubUnitAvailabilityAsync(
        int subUnitId,
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Check if specific subunit is available
    /// </summary>
    Task<Result<Contracts.hoteladmincont.SubUnitAvailabilityStatus>> CheckSubUnitAvailabilityAsync(
        int subUnitId,
        DateTime checkIn,
        DateTime checkOut);

    /// <summary>
    /// Get detailed availability calendar for subunit
    /// </summary>
    Task<Result<Dictionary<DateTime, SubUnitDayAvailability>>> GetSubUnitAvailabilityCalendarAsync(
        int subUnitId,
        int year,
        int month);

    // ============= BATCH OPERATIONS =============

    /// <summary>
    /// Block multiple dates at once for a subunit
    /// </summary>
    Task<Result> BlockSubUnitDatesAsync(BlockDatesRequest request);

    /// <summary>
    /// Block multiple dates for an entire unit
    /// </summary>
    Task<Result> BlockUnitDatesAsync(BlockUnitDatesRequest request);

    /// <summary>
    /// Set special pricing for multiple dates
    /// </summary>
    Task<Result> SetSpecialPricingAsync(SetSpecialPricingRequest request);

    /// <summary>
    /// Unblock previously blocked dates
    /// </summary>
    Task<Result> UnblockDatesAsync(int availabilityId);

    // ============= BOOKING INTEGRATION =============

    /// <summary>
    /// Mark dates as booked when booking is confirmed (called internally)
    /// </summary>
    Task<Result> MarkDatesAsBookedAsync(MarkDatesAsBookedRequest request);

    /// <summary>
    /// Free up dates when booking is cancelled (called internally)
    /// </summary>
    Task<Result> FreeDatesFromBookingAsync(int bookingId);

    // ============= BULK QUERIES =============

    /// <summary>
    /// Get all available subunits for a unit in a date range
    /// </summary>
    Task<Result<List<int>>> GetAvailableSubUnitIdsAsync(
        int unitId,
        DateTime checkIn,
        DateTime checkOut);

    /// <summary>
    /// Check multiple subunits availability at once
    /// </summary>
    Task<Result<Dictionary<int, bool>>> CheckMultipleSubUnitsAvailabilityAsync(
        List<int> subUnitIds,
        DateTime checkIn,
        DateTime checkOut);

    // ============= INITIALIZATION =============

    /// <summary>
    /// Initialize default availability for new subunit
    /// </summary>
    Task<Result> InitializeSubUnitAvailabilityAsync(int subUnitId, int daysAhead = 365);

    /// <summary>
    /// Initialize default availability for new unit
    /// </summary>
    Task<Result> InitializeUnitAvailabilityAsync(int unitId, int daysAhead = 365);

}





#region Unit Booking Requests

public record CreateUnitBookingRequest(
    int UnitId,
    string UserId,
    DateTime CheckInDate,
    DateTime CheckOutDate,
    int NumberOfGuests,
    string? CouponCode,  // ADDED: Optional coupon code
    string? SpecialRequests = null
);

public record CalculateUnitBookingPriceRequest(
    int UnitId,
    DateTime CheckInDate,
    DateTime CheckOutDate,
    string? CouponCode  // ADDED: Optional coupon code for price calculation
);

public record UnitBookingStatisticsFilter(
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int? UnitId = null,
    BookingStatus? Status = null
);

#endregion

#region Unit Booking Responses

public record UnitBookingResponse
{
    public int Id { get; init; }
    public string BookingNumber { get; init; } = string.Empty;
    public int UnitId { get; init; }
    public string UnitName { get; init; } = string.Empty;
    public string CouponDiscount { get; init; } = string.Empty;
    public string AppliedCouponCode { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public DateTime CheckInDate { get; init; }
    public DateTime CheckOutDate { get; init; }
    public int NumberOfGuests { get; init; }
    public int NumberOfNights { get; init; }
    public decimal TotalPrice { get; init; }
    public decimal PaidAmount { get; init; }
    public BookingStatus Status { get; init; }
    public PaymentStatus PaymentStatus { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record UnitBookingDetailsResponse : UnitBookingResponse
{
    public string UnitAddress { get; init; } = string.Empty;
    public string UserEmail { get; init; } = string.Empty;
    public string? UserPhone { get; init; }
    public string? SpecialRequests { get; init; }
    public string? CancellationReason { get; init; }
    public DateTime? CancelledAt { get; init; }
    public List<PaymentInfo> Payments { get; init; } = new();
    public DateTime? UpdatedAt { get; init; }

}

public record UnitBookingStatisticsResponse
{
    public int TotalBookings { get; init; }
    public int PendingBookings { get; init; }
    public int ConfirmedBookings { get; init; }
    public int CheckedInBookings { get; init; }
    public int CompletedBookings { get; init; }
    public int CancelledBookings { get; init; }
    public decimal TotalRevenue { get; init; }
    public decimal PendingRevenue { get; init; }
    public decimal AverageBookingValue { get; init; }
    public double AverageNightsPerBooking { get; init; }
    public Dictionary<string, int> BookingsByStatus { get; init; } = new();
    public Dictionary<string, MonthlyBookingStats> BookingsByMonth { get; init; } = new();
}

#endregion

// ============================================================================
// SUBUNIT BOOKING CONTRACTS
// ============================================================================

#region SubUnit Booking Requests

public record CreateSubUnitBookingRequest(
    int UnitId,
    List<int> SubUnitIds,
    string UserId,
    DateTime CheckInDate,
    DateTime CheckOutDate,
    int NumberOfGuests,
    string? CouponCode,  // ADDED: Optional coupon code
    string? SpecialRequests = null
);

public record CalculateSubUnitBookingPriceRequest(
    List<int> SubUnitIds,
    DateTime CheckInDate,
    DateTime CheckOutDate,
    string? CouponCode  // ADDED: Optional coupon code for price calculation
);

public record SubUnitBookingStatisticsFilter(
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int? UnitId = null,
    int? SubUnitId = null,
    BookingStatus? Status = null
);

#endregion

#region SubUnit Booking Responses

public record SubUnitBookingResponse
{
    public int Id { get; init; }
    public string BookingNumber { get; init; } = string.Empty;
    public int UnitId { get; init; }
    public string UnitName { get; init; } = string.Empty;
    public List<BookedSubUnitInfo> SubUnits { get; init; } = new();
    public string UserId { get; init; } = string.Empty;
    public string AppliedCouponCode { get; init; } = string.Empty;
    public string CouponDiscount { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public DateTime CheckInDate { get; init; }
    public DateTime CheckOutDate { get; init; }
    public int NumberOfGuests { get; init; }
    public int NumberOfNights { get; init; }
    public decimal TotalPrice { get; init; }
    public decimal PaidAmount { get; init; }
    public BookingStatus Status { get; init; }
    public PaymentStatus PaymentStatus { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record SubUnitBookingDetailsResponse : SubUnitBookingResponse
{
    public string UnitAddress { get; init; } = string.Empty;
    public string UserEmail { get; init; } = string.Empty;
    public string? UserPhone { get; init; }
    public string? SpecialRequests { get; init; }
    public string? CancellationReason { get; init; }
    public DateTime? CancelledAt { get; init; }
    public List<PaymentInfo> Payments { get; init; } = new();
    public DateTime? UpdatedAt { get; init; }
}

public record BookedSubUnitInfo
{
    public int SubUnitId { get; init; }
    public string RoomNumber { get; init; } = string.Empty;
    public decimal PricePerNight { get; init; }
    public int NumberOfNights { get; init; }
    public decimal SubTotal { get; init; }
}

//public record AvailableSubUnitInfo
//{
//    public int Id { get; init; }
//    public string RoomNumber { get; init; } = string.Empty;
//    public int TypeId { get; init; }
//    public decimal PricePerNight { get; init; }
//    public int MaxOccupancy { get; init; }
//    public bool IsAvailable { get; init; }
//    public decimal? SpecialPrice { get; init; }
//}

public record SubUnitBookingStatisticsResponse
{
    public int TotalBookings { get; init; }
    public int PendingBookings { get; init; }
    public int ConfirmedBookings { get; init; }
    public int CheckedInBookings { get; init; }
    public int CompletedBookings { get; init; }
    public int CancelledBookings { get; init; }
    public decimal TotalRevenue { get; init; }
    public decimal PendingRevenue { get; init; }
    public decimal AverageBookingValue { get; init; }
    public double AverageNightsPerBooking { get; init; }
    public int TotalRoomsBooked { get; init; }
    public double AverageRoomsPerBooking { get; init; }
    public Dictionary<string, int> BookingsByStatus { get; init; } = new();
    public Dictionary<string, MonthlyBookingStats> BookingsByMonth { get; init; } = new();
}

#endregion

// ============================================================================
// SHARED BOOKING CONTRACTS
// ============================================================================

public record ProcessPaymentRequest(
    [Required] string TransactionId,
    [Required][Range(0.01, double.MaxValue)] decimal Amount,
    [Required] PaymentMethod PaymentMethod,
    string? Notes
);

public record PaymentInfo
{
    public int Id { get; init; }
    public decimal Amount { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime PaymentDate { get; init; }
}

public record MonthlyBookingStats
{
    public int Count { get; init; }
    public decimal Revenue { get; init; }
}

public record BookingFilter(
    BookingStatus? Status = null,
    PaymentStatus? PaymentStatus = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int Page = 1,
    int PageSize = 10
);

// ============================================================================
// AVAILABILITY CONTRACTS
// ============================================================================


#region Availability Requests

//public record SetUnitAvailabilityRequest(
//    int UnitId,
//    DateTime StartDate,
//    DateTime EndDate,
//    bool IsAvailable,
//    UnavailabilityReason? Reason = null,
//    string? UpdatedByUserId = null
//);

//public record SetSubUnitAvailabilityRequest(
//    int SubUnitId,
//    DateTime StartDate,
//    DateTime EndDate,
//    bool IsAvailable,
//    UnavailabilityReason? Reason = null,
//    decimal? SpecialPrice = null,
//    decimal? WeekendPrice = null,
//    string? UpdatedByUserId = null
//);

//public record BlockDatesRequest(
//    int SubUnitId,
//    List<DateRange> DateRanges,
//    UnavailabilityReason Reason,
//    string UpdatedByUserId
//);

//public record BlockUnitDatesRequest(
//    int UnitId,
//    List<DateRange> DateRanges,
//    UnavailabilityReason Reason,
//    string UpdatedByUserId
//);

//public record SetSpecialPricingRequest(
//    int SubUnitId,
//    List<PriceRange> PriceRanges,
//    string UpdatedByUserId
//);

public record DateRange(
    DateTime StartDate,
    DateTime EndDate
);

public record PriceRange(
    DateTime StartDate,
    DateTime EndDate,
    decimal WeekdayPrice,
    decimal? WeekendPrice = null
);

//public record MarkDatesAsBookedRequest(
//    BookingType BookingType,
//    int? UnitId = null,
//    List<int>? SubUnitIds = null,
//    int BookingId = 0,
//    DateTime CheckInDate = default,
//    DateTime CheckOutDate = default
//);

#endregion


//public record AvailabilityResponse(
//    int Id,
//    DateTime StartDate,
//    DateTime EndDate,
//    bool IsAvailable,
//    string? Reason,
//    decimal? SpecialPrice,
//    decimal? WeekendPrice
//);

//public record UnitAvailabilityStatus
//{
//    public bool IsAvailable { get; init; }
//    public string? Reason { get; init; }
//    public bool HasManualBlock { get; init; }
//    public bool HasActiveBooking { get; init; }
//    public int AvailableSubUnitsCount { get; init; }
//    public int TotalSubUnitsCount { get; init; }
//}

//public record SubUnitAvailabilityStatus
//{
//    public bool IsAvailable { get; init; }
//    public string? Reason { get; init; }
//    public bool HasManualBlock { get; init; }
//    public bool HasActiveBooking { get; init; }
//    public decimal? CurrentPrice { get; init; }
//    public decimal? SpecialPrice { get; init; }
//}

//public record UnitDayAvailability
//{
//    public DateTime Date { get; init; }
//    public bool IsAvailable { get; init; }
//    public bool IsBooked { get; init; }
//    public bool IsManuallyBlocked { get; init; }
//    public int AvailableSubUnits { get; init; }
//    public int TotalSubUnits { get; init; }
//    public decimal? MinPrice { get; init; }
//    public decimal? MaxPrice { get; init; }
//    public UnavailabilityReason? UnavailabilityReason { get; init; }
//}

//public record SubUnitDayAvailability
//{
//    public DateTime Date { get; init; }
//    public bool IsAvailable { get; init; }
//    public bool IsBooked { get; init; }
//    public bool IsManuallyBlocked { get; init; }
//    public decimal Price { get; init; }
//    public bool IsWeekend { get; init; }
//    public decimal? SpecialPrice { get; init; }
//    public UnavailabilityReason? UnavailabilityReason { get; init; }
//}

//#endregion

//// ============================================================================
//// ENUMS
//// ============================================================================

////public enum BookingType
////{
////    UnitBooking = 1,
////    SubUnitBooking = 2
////}

////public enum BookingStatus
////{
////    Pending = 1,
////    Confirmed = 2,
////    CheckedIn = 3,
////    Completed = 4,
////    Cancelled = 5
////}

////public enum PaymentStatus
////{
////    Pending = 1,
////    PartiallyPaid = 2,
////    Paid = 3,
////    Refunded = 4
////}

////public enum PaymentMethod
////{
////    CreditCard = 1,
////    DebitCard = 2,
////    Cash = 3,
////    BankTransfer = 4
////}

////public enum UnavailabilityReason
////{
////    Booked = 1,
////    Maintenance = 2,
////    OwnerBlocked = 3,
////    SeasonallyClosed = 4,
////    Other = 5
////}}
///
