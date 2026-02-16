using Application.Abstraction;
using Application.Contracts.Bookin;
using Application.Service.Avilabilaties;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Service.Booking;

public interface IUnitBookingServices
{
    /// <summary>
    /// Validate selected options against unit's available options
    /// </summary>
    Task<Result<ValidateOptionsResponse>> ValidateSelectedOptionsAsync(
        ValidateOptionsRequest request);

    /// <summary>
    /// Create a booking for an entire unit (all rooms)
    /// </summary>
    Task<Result<UnitBookingResponse>> CreateUnitBookingAsync(CreateUnitBookingRequest request);

    /// <summary>
    /// Calculate price for unit booking
    /// </summary>
    Task<Result<decimal>> CalculateUnitBookingPriceAsync(CalculateUnitBookingPriceRequest request);

    // ============= BOOKING MANAGEMENT =============

    Task<Result> ConfirmUnitBookingAsync(int bookingId, string adminUserId);
    Task<Result> CheckInUnitAsync(int bookingId);
    Task<Result> CheckOutUnitAsync(int bookingId);
    Task<Result> CancelUnitBookingAsync(int bookingId, string cancellationReason, string userId);

    // ============= QUERY OPERATIONS =============

    Task<Result<UnitBookingDetailsResponse>> GetUnitBookingByIdAsync(int bookingId);
    Task<Result<UnitBookingDetailsResponse>> GetUnitBookingByNumberAsync(string bookingNumber);

    Task<Result<PaginatedResponse<UnitBookingResponse>>> GetUserUnitBookingsAsync(
    string userId, BookingFilter filter);
    Task<Result<PaginatedResponse<UnitBookingResponse>>> GetUnitBookingsForPropertyAsync(
        int unitId, BookingFilter filter);

    public class PaginatedResponse<T>
    {
        public IEnumerable<T> Items { get; set; } = [];
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int? NextPage { get; set; }
        public int? PrevPage { get; set; }
        public int TotalCount { get; set; }
    }

    // ============= PAYMENT OPERATIONS =============

    Task<Result> ProcessUnitPaymentAsync(int bookingId, ProcessPaymentRequest request);
    Task<Result> RefundUnitBookingAsync(int bookingId, decimal refundAmount, string reason);

    // ============= STATISTICS =============

    Task<Result<UnitBookingStatisticsResponse>> GetUnitBookingStatisticsAsync(UnitBookingStatisticsFilter filter);

    // ============= AVAILABILITY CHECK =============

    Task<Result<bool>> IsUnitAvailableForBookingAsync(int unitId, DateTime checkIn, DateTime checkOut);


}
