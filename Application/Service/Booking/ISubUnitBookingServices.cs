using Application.Abstraction;
using Application.Contracts.Bookin;
using Application.Service.Avilabilaties;
using Application.Service.SubUnitType;
using Domain;
using Domain.Entities;
using Hangfire;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Application.Service.Booking;

public interface ISubUnitBookingServices
{
    /// <summary>
    /// Create a booking for one or more subunits
    /// </summary>
    Task<Result<SubUnitBookingResponse>> CreateSubUnitBookingAsync(CreateSubUnitBookingRequest request);

    /// <summary>
    /// Calculate price for subunit booking
    /// </summary>
    Task<Result<decimal>> CalculateSubUnitBookingPriceAsync(CalculateSubUnitBookingPriceRequest request);

    // ============= BOOKING MANAGEMENT =============

    Task<Result> ConfirmSubUnitBookingAsync(int bookingId, string adminUserId);
    Task<Result> CheckInSubUnitAsync(int bookingId);
    Task<Result> CheckOutSubUnitAsync(int bookingId);
    Task<Result> CancelSubUnitBookingAsync(int bookingId, string cancellationReason, string userId);

    // ============= QUERY OPERATIONS =============

    Task<Result<SubUnitBookingDetailsResponse>> GetSubUnitBookingByIdAsync(int bookingId);
    Task<Result<SubUnitBookingDetailsResponse>> GetSubUnitBookingByNumberAsync(string bookingNumber);
    Task<Result<IEnumerable<SubUnitBookingResponse>>> GetUserSubUnitBookingsAsync(string userId, BookingFilter filter);
    Task<Result<IEnumerable<SubUnitBookingResponse>>> GetSubUnitBookingsForRoomAsync(int subUnitId, BookingFilter filter);
    Task<Result<IEnumerable<SubUnitBookingResponse>>> GetSubUnitBookingsForUnitAsync(int unitId, BookingFilter filter);

    // ============= PAYMENT OPERATIONS =============

    Task<Result> ProcessSubUnitPaymentAsync(int bookingId, ProcessPaymentRequest request);
    Task<Result> RefundSubUnitBookingAsync(int bookingId, decimal refundAmount, string reason);

    // ============= STATISTICS =============

    Task<Result<SubUnitBookingStatisticsResponse>> GetSubUnitBookingStatisticsAsync(SubUnitBookingStatisticsFilter filter);

    // ============= AVAILABILITY CHECK =============

    Task<Result<bool>> AreSubUnitsAvailableAsync(List<int> subUnitIds, DateTime checkIn, DateTime checkOut);
    Task<Result<List<AvailableSubUnitInfo>>> GetAvailableSubUnitsAsync(int unitId, DateTime checkIn, DateTime checkOut, int requestedCount);

}
