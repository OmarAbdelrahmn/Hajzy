using Domain;
using Domain.Entities;

namespace Application.Contracts.Bookin;

internal class Booking
{
}


/// <summary>
/// Represents a single option selected by the user
/// </summary>
public record SelectedOptionDto
{
    public string OptionName { get; init; } = string.Empty;
    public string? Value { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// Request to validate selected options
/// </summary>
public record ValidateOptionsRequest
{
    public int UnitId { get; init; }
    public List<SelectedOptionDto> SelectedOptions { get; init; } = new();
}

/// <summary>
/// Response for option validation
/// </summary>
public record ValidateOptionsResponse
{
    public bool IsValid { get; init; }
    public List<string> AvailableOptions { get; init; } = new();
    public List<string> InvalidOptions { get; init; } = new();
    public string? ErrorMessage { get; init; }
}
public class BookingAdminFilter
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 5;

    public BookingType? BookingType { get; set; }
    public BookingStatus? Status { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }

    public int? UnitId { get; set; }
    public int? CityId { get; set; }
    public string? UserId { get; set; }
    public string? BookingNumber { get; set; }

    public DateTime? CheckInFrom { get; set; }
    public DateTime? CheckInTo { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }

    public string? SortBy { get; set; } = "BookingDate"; // BookingDate, CheckIn, TotalPrice, Status
    public bool SortDescending { get; set; } = true;
}

public class PaginatedBookingsResponse
{
    public List<AdminBookingResponse> Bookings { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public BookingStatistics Statistics { get; set; } = new();
}

public class AdminBookingResponse
{
    // Basic Info
    public int Id { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public string BookingType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;

    // Unit Info
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string UnitAddress { get; set; } = string.Empty;
    public string UnitType { get; set; } = string.Empty;
    public string CityName { get; set; } = string.Empty;
    public int CityId { get; set; }

    // SubUnit Info (only for SubUnit bookings)
    public List<SubUnitSummary> SubUnits { get; set; } = new();

    // Guest Info
    public string UserId { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
    public string? GuestPhone { get; set; }

    // Booking Details
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; }
    public int NumberOfNights { get; set; }

    // Financial Info
    public decimal TotalPrice { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }

    // Coupon Info
    public string? CouponCode { get; set; }
    public decimal? CouponDiscount { get; set; }

    // Payment Info
    public List<PaymentSummary> Payments { get; set; } = new();

    // Additional Info
    public string? SpecialRequests { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class SubUnitSummary
{
    public int SubUnitId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string SubUnitType { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public int NumberOfNights { get; set; }
    public decimal Subtotal { get; set; }
    public int MaxOccupancy { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public decimal? Size { get; set; }
}

public class PaymentSummary
{
    public int Id { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class BookingStatistics
{
    public int TotalBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PaidRevenue { get; set; }
    public decimal PendingRevenue { get; set; }

    public int PendingCount { get; set; }
    public int ConfirmedCount { get; set; }
    public int CheckedInCount { get; set; }
    public int CompletedCount { get; set; }
    public int CancelledCount { get; set; }

    public int UnitBookingsCount { get; set; }
    public int SubUnitBookingsCount { get; set; }

    public decimal AverageBookingValue { get; set; }
    public double AverageNightsPerBooking { get; set; }
}

//public record CreateSubUnitBookingRequest(
//    int UnitId,
//    List<int> SubUnitIds, // Specific rooms to book
//    string UserId,
//    DateTime CheckInDate,
//    DateTime CheckOutDate,
//    int NumberOfGuests,
//    string? SpecialRequests = null
//);

//public record CalculateSubUnitBookingPriceRequest(
//    List<int> SubUnitIds,
//    DateTime CheckInDate,
//    DateTime CheckOutDate
//);

//public record SubUnitBookingStatisticsFilter(
//    DateTime? StartDate = null,
//    DateTime? EndDate = null,
//    int? UnitId = null,
//    int? SubUnitId = null,
//    BookingStatus? Status = null
//);

//// ============= RESPONSE MODELS =============

//public record SubUnitBookingResponse
//{
//    public int Id { get; init; }
//    public string BookingNumber { get; init; } = string.Empty;
//    public int UnitId { get; init; }
//    public string UnitName { get; init; } = string.Empty;
//    public List<BookedSubUnitInfo> SubUnits { get; init; } = new();
//    public string UserId { get; init; } = string.Empty;
//    public string UserName { get; init; } = string.Empty;
//    public DateTime CheckInDate { get; init; }
//    public DateTime CheckOutDate { get; init; }
//    public int NumberOfGuests { get; init; }
//    public int NumberOfNights { get; init; }
//    public decimal TotalPrice { get; init; }
//    public decimal PaidAmount { get; init; }
//    public BookingStatus Status { get; init; }
//    public PaymentStatus PaymentStatus { get; init; }
//    public DateTime CreatedAt { get; init; }
//}

//public record SubUnitBookingDetailsResponse : SubUnitBookingResponse
//{
//    public string UnitAddress { get; init; } = string.Empty;
//    public string UserEmail { get; init; } = string.Empty;
//    public string? UserPhone { get; init; }
//    public string? SpecialRequests { get; init; }
//    public string? CancellationReason { get; init; }
//    public DateTime? CancelledAt { get; init; }
//    public List<PaymentInfo> Payments { get; init; } = new();
//    public DateTime? UpdatedAt { get; init; }
//}

//public record BookedSubUnitInfo
//{
//    public int SubUnitId { get; init; }
//    public string RoomNumber { get; init; } = string.Empty;
//    public decimal PricePerNight { get; init; }
//    public int NumberOfNights { get; init; }
//    public decimal SubTotal { get; init; }
//}

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

//public record SubUnitBookingStatisticsResponse
//{
//    public int TotalBookings { get; init; }
//    public int PendingBookings { get; init; }
//    public int ConfirmedBookings { get; init; }
//    public int CheckedInBookings { get; init; }
//    public int CompletedBookings { get; init; }
//    public int CancelledBookings { get; init; }
//    public decimal TotalRevenue { get; init; }
//    public decimal PendingRevenue { get; init; }
//    public decimal AverageBookingValue { get; init; }
//    public double AverageNightsPerBooking { get; init; }
//    public int TotalRoomsBooked { get; init; }
//    public double AverageRoomsPerBooking { get; init; }
//    public Dictionary<string, int> BookingsByStatus { get; init; } = new();
//    public Dictionary<string, MonthlyBookingStats> BookingsByMonth { get; init; } = new();
//}
//public record CreateUnitBookingRequest(
//    int UnitId,
//    string UserId,
//    DateTime CheckInDate,
//    DateTime CheckOutDate,
//    int NumberOfGuests,
//    string? SpecialRequests = null
//);

//public record CalculateUnitBookingPriceRequest(
//    int UnitId,
//    DateTime CheckInDate,
//    DateTime CheckOutDate
//);

//public record UnitBookingStatisticsFilter(
//    DateTime? StartDate = null,
//    DateTime? EndDate = null,
//    int? UnitId = null,
//    BookingStatus? Status = null
//);

//// ============= RESPONSE MODELS =============

//public record UnitBookingResponse
//{
//    public int Id { get; init; }
//    public string BookingNumber { get; init; } = string.Empty;
//    public int UnitId { get; init; }
//    public string UnitName { get; init; } = string.Empty;
//    public string UserId { get; init; } = string.Empty;
//    public string UserName { get; init; } = string.Empty;
//    public DateTime CheckInDate { get; init; }
//    public DateTime CheckOutDate { get; init; }
//    public int NumberOfGuests { get; init; }
//    public int NumberOfNights { get; init; }
//    public decimal TotalPrice { get; init; }
//    public decimal PaidAmount { get; init; }
//    public BookingStatus Status { get; init; }
//    public PaymentStatus PaymentStatus { get; init; }
//    public DateTime CreatedAt { get; init; }
//}

//public record UnitBookingDetailsResponse : UnitBookingResponse
//{
//    public string UnitAddress { get; init; } = string.Empty;
//    public string UserEmail { get; init; } = string.Empty;
//    public string? UserPhone { get; init; }
//    public string? SpecialRequests { get; init; }
//    public string? CancellationReason { get; init; }
//    public DateTime? CancelledAt { get; init; }
//    public List<PaymentInfo> Payments { get; init; } = new();
//    public DateTime? UpdatedAt { get; init; }
//}

//public record UnitBookingStatisticsResponse
//{
//    public int TotalBookings { get; init; }
//    public int PendingBookings { get; init; }
//    public int ConfirmedBookings { get; init; }
//    public int CheckedInBookings { get; init; }
//    public int CompletedBookings { get; init; }
//    public int CancelledBookings { get; init; }
//    public decimal TotalRevenue { get; init; }
//    public decimal PendingRevenue { get; init; }
//    public decimal AverageBookingValue { get; init; }
//    public double AverageNightsPerBooking { get; init; }
//    public Dictionary<string, int> BookingsByStatus { get; init; } = new();
//    public Dictionary<string, MonthlyBookingStats> BookingsByMonth { get; init; } = new();
//}

//public record MonthlyBookingStats
//{
//    public int Count { get; init; }
//    public decimal Revenue { get; init; }
//}

//public record PaymentInfo
//{
//    public int Id { get; init; }
//    public decimal Amount { get; init; }
//    public string PaymentMethod { get; init; } = string.Empty;
//    public string Status { get; init; } = string.Empty;
//    public DateTime PaymentDate { get; init; }
//}

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

//public record BookingFilter(
//    BookingStatus? Status = null,
//    PaymentStatus? PaymentStatus = null,
//    DateTime? StartDate = null,
//    DateTime? EndDate = null,
//    int Page = 1,
//    int PageSize = 10
//);

//public record ProcessPaymentRequest(
//    string TransactionId,
//    decimal Amount,
//    PaymentMethod PaymentMethod,
//    string? Notes = null
//);
//public record CreateBookingRequest
//{
//    public int UnitId { get; init; }
//    public string UserId { get; init; } = string.Empty;
//    public DateTime CheckInDate { get; init; }
//    public DateTime CheckOutDate { get; init; }
//    public int NumberOfGuests { get; init; }
//    public int NumberOfRooms { get; init; }
//    public string? SpecialRequests { get; init; }
//}
//public record BookingStatisticsFilter
//{
//    public DateTime? StartDate { get; init; }
//    public DateTime? EndDate { get; init; }
//    public int? UnitId { get; init; }
//    public BookingStatus? Status { get; init; }
//}

//public record CalculateBookingPriceRequest
//{
//    public int UnitId { get; init; }
//    public DateTime CheckInDate { get; init; }
//    public DateTime CheckOutDate { get; init; }
//    public int NumberOfRooms { get; init; }
//}

//public class BookingResponse
//{
//    public int Id { get; set; }
//    public string BookingNumber { get; set; } = string.Empty;
//    public int UnitId { get; set; }
//    public string UnitName { get; set; } = string.Empty;
//    public string UserId { get; set; } = string.Empty;
//    public string UserName { get; set; } = string.Empty;
//    public DateTime CheckInDate { get; set; }
//    public DateTime CheckOutDate { get; set; }
//    public int NumberOfGuests { get; set; }
//    public int NumberOfNights { get; set; }
//    public decimal TotalPrice { get; set; }
//    public decimal PaidAmount { get; set; }
//    public BookingStatus Status { get; set; }
//    public PaymentStatus PaymentStatus { get; set; }
//    public DateTime CreatedAt { get; set; }
//}

//public class BookingDetailsResponse : BookingResponse
//{
//    public string UnitAddress { get; set; } = string.Empty;
//    public string UserEmail { get; set; } = string.Empty;
//    public string? UserPhone { get; set; }
//    public string? SpecialRequests { get; set; }
//    public string? CancellationReason { get; set; }
//    public DateTime? CancelledAt { get; set; }
//    public List<BookingRoomInfo> Rooms { get; set; } = new();
//    public List<PaymentInfo> Payments { get; set; } = new();
//    public DateTime? UpdatedAt { get; set; }
//}

//public record BookingRoomInfo
//{
//    public int RoomId { get; init; }
//    public string RoomNumber { get; init; } = string.Empty;
//    public decimal PricePerNight { get; init; }
//    public int NumberOfNights { get; init; }
//}

//public class BookingStatisticsResponse
//{
//    public int TotalBookings { get; set; }
//    public int PendingBookings { get; set; }
//    public int ConfirmedBookings { get; set; }
//    public int CheckedInBookings { get; set; }
//    public int CompletedBookings { get; set; }
//    public int CancelledBookings { get; set; }
//    public decimal TotalRevenue { get; set; }
//    public decimal PendingRevenue { get; set; }
//    public decimal AverageBookingValue { get; set; }
//    public double AverageNightsPerBooking { get; set; }
//    public Dictionary<string, int> BookingsByStatus { get; set; } = new();
//    public Dictionary<string, MonthlyBookingStats> BookingsByMonth { get; set; } = new();
//}
