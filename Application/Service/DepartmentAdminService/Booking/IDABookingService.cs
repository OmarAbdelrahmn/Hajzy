using Application.Abstraction;
using Application.Contracts.Bookin;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Service.DepartmentAdminService.Booking
{
    public interface IDABookingService
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
}
