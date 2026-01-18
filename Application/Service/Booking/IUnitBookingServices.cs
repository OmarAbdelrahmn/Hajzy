using Application.Abstraction;
using Application.Contracts.Bookin;
using Application.Service.Availability;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Service.Booking;

public interface IUnitBookingServices
{
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
    Task<Result<IEnumerable<UnitBookingResponse>>> GetUserUnitBookingsAsync(string userId, BookingFilter filter);
    Task<Result<IEnumerable<UnitBookingResponse>>> GetUnitBookingsForPropertyAsync(int unitId, BookingFilter filter);

    // ============= PAYMENT OPERATIONS =============

    Task<Result> ProcessUnitPaymentAsync(int bookingId, ProcessPaymentRequest request);
    Task<Result> RefundUnitBookingAsync(int bookingId, decimal refundAmount, string reason);

    // ============= STATISTICS =============

    Task<Result<UnitBookingStatisticsResponse>> GetUnitBookingStatisticsAsync(UnitBookingStatisticsFilter filter);

    // ============= AVAILABILITY CHECK =============

    Task<Result<bool>> IsUnitAvailableForBookingAsync(int unitId, DateTime checkIn, DateTime checkOut);


}
