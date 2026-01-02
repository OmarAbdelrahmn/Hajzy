using Application.Abstraction;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity.UI.Services;
using Hangfire;
using System.Text;

namespace Application.Service.Booking;

public interface IBookingService
{
    // Core Booking Operations
    Task<Result<BookingResponse>> CreateBookingAsync(CreateBookingRequest request);
    Task<Result<BookingDetailsResponse>> GetBookingByIdAsync(int bookingId);
    Task<Result<BookingDetailsResponse>> GetBookingByNumberAsync(string bookingNumber);
    Task<Result<IEnumerable<BookingResponse>>> GetUserBookingsAsync(string userId, BookingFilter filter);
    Task<Result<IEnumerable<BookingResponse>>> GetUnitBookingsAsync(int unitId, BookingFilter filter);

    // Booking Management
    Task<Result> ConfirmBookingAsync(int bookingId, string adminUserId);
    Task<Result> CheckInAsync(int bookingId);
    Task<Result> CheckOutAsync(int bookingId);
    Task<Result> CancelBookingAsync(int bookingId, string cancellationReason, string userId);

    // Payment Integration
    Task<Result> ProcessPaymentAsync(int bookingId, ProcessPaymentRequest request);
    Task<Result> RefundBookingAsync(int bookingId, decimal refundAmount, string reason);

    // Statistics & Reports
    Task<Result<BookingStatisticsResponse>> GetBookingStatisticsAsync(BookingStatisticsFilter filter);
    Task<Result<decimal>> CalculateBookingPriceAsync(CalculateBookingPriceRequest request);
}
