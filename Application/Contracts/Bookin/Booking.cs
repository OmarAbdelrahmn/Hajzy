using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Contracts.Bookin;

internal class Booking
{
}
public record CreateBookingRequest
{
    public int UnitId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public DateTime CheckInDate { get; init; }
    public DateTime CheckOutDate { get; init; }
    public int NumberOfGuests { get; init; }
    public int NumberOfRooms { get; init; }
    public string? SpecialRequests { get; init; }
}

public record ProcessPaymentRequest
{
    public string TransactionId { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public string? Notes { get; init; }
}

public record BookingFilter
{
    public BookingStatus? Status { get; init; }
    public PaymentStatus? PaymentStatus { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public record BookingStatisticsFilter
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int? UnitId { get; init; }
    public BookingStatus? Status { get; init; }
}

public record CalculateBookingPriceRequest
{
    public int UnitId { get; init; }
    public DateTime CheckInDate { get; init; }
    public DateTime CheckOutDate { get; init; }
    public int NumberOfRooms { get; init; }
}

public class BookingResponse
{
    public int Id { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; }
    public int NumberOfNights { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal PaidAmount { get; set; }
    public BookingStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class BookingDetailsResponse : BookingResponse
{
    public string UnitAddress { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string? UserPhone { get; set; }
    public string? SpecialRequests { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }
    public List<BookingRoomInfo> Rooms { get; set; } = new();
    public List<PaymentInfo> Payments { get; set; } = new();
    public DateTime? UpdatedAt { get; set; }
}

public record BookingRoomInfo
{
    public int RoomId { get; init; }
    public string RoomNumber { get; init; } = string.Empty;
    public decimal PricePerNight { get; init; }
    public int NumberOfNights { get; init; }
}

public record PaymentInfo
{
    public int Id { get; init; }
    public decimal Amount { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime PaymentDate { get; init; }
}

public class BookingStatisticsResponse
{
    public int TotalBookings { get; set; }
    public int PendingBookings { get; set; }
    public int ConfirmedBookings { get; set; }
    public int CheckedInBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PendingRevenue { get; set; }
    public decimal AverageBookingValue { get; set; }
    public double AverageNightsPerBooking { get; set; }
    public Dictionary<string, int> BookingsByStatus { get; set; } = new();
    public Dictionary<string, MonthlyBookingStats> BookingsByMonth { get; set; } = new();
}

public class MonthlyBookingStats
{
    public int Count { get; set; }
    public decimal Revenue { get; set; }
}


//==================== Department Admin Dto
public record DABookingFilter
{
    public BookingStatus? Status { get; init; }
    public PaymentStatus? PaymentStatus { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public record DABookingStatisticsFilter
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int? UnitId { get; init; }
    public BookingStatus? Status { get; init; }
}
public class DABookingResponse
{
    public int Id { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; }
    public int NumberOfNights { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal PaidAmount { get; set; }
    public BookingStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public DateTime CreatedAt { get; set; }
}
public class DABookingDetailsResponse : DABookingResponse
{
    public string UnitAddress { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string? UserPhone { get; set; }
    public string? SpecialRequests { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }
    public List<BookingRoomInfo> Rooms { get; set; } = new();
    public List<PaymentInfo> Payments { get; set; } = new();
    public DateTime? UpdatedAt { get; set; }
}

public class DABookingStatisticsResponse
{
    public int TotalBookings { get; set; }
    public int PendingBookings { get; set; }
    public int ConfirmedBookings { get; set; }
    public int CheckedInBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PendingRevenue { get; set; }
    public decimal AverageBookingValue { get; set; }
    public double AverageNightsPerBooking { get; set; }
    public Dictionary<string, int> BookingsByStatus { get; set; } = new();
    public Dictionary<string, MonthlyBookingStats> BookingsByMonth { get; set; } = new();
}