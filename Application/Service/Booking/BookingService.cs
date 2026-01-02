using Application.Abstraction;
using Domain;
using Domain.Entities;
using Hangfire;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Service.Booking;


public class BookingService(
    ApplicationDbcontext context,
    IEmailSender emailSender,
    ILogger<BookingService> logger) : IBookingService
{
    private readonly ApplicationDbcontext _context = context;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly ILogger<BookingService> _logger = logger;

    #region CREATE BOOKING

    public async Task<Result<BookingResponse>> CreateBookingAsync(CreateBookingRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Validate Unit exists
            var unit = await _context.Units
                .Include(u => u.Rooms)
                .FirstOrDefaultAsync(u => u.Id == request.UnitId && !u.IsDeleted);

            if (unit == null)
                return Result.Failure<BookingResponse>(
                    new Error("UnitNotFound", "Unit not found", 404));

            // 2. Validate dates
            if (request.CheckInDate >= request.CheckOutDate)
                return Result.Failure<BookingResponse>(
                    new Error("InvalidDates", "Check-out must be after check-in", 400));

            if (request.CheckInDate < DateTime.UtcNow.Date)
                return Result.Failure<BookingResponse>(
                    new Error("InvalidDates", "Cannot book past dates", 400));

            // 3. Check availability
            var availableRooms = await GetAvailableRoomsAsync(
                request.UnitId,
                request.CheckInDate,
                request.CheckOutDate);

            if (availableRooms.Count < request.NumberOfRooms)
                return Result.Failure<BookingResponse>(
                    new Error("NotAvailable",
                        $"Only {availableRooms.Count} rooms available", 400));

            // 4. Calculate pricing
            var nights = (request.CheckOutDate - request.CheckInDate).Days;
            var totalPrice = await CalculateTotalPriceAsync(
                availableRooms.Take(request.NumberOfRooms).ToList(),
                request.CheckInDate,
                request.CheckOutDate);

            // 5. Generate unique booking number
            var bookingNumber = await GenerateBookingNumberAsync();

            // 6. Create booking
            var booking = new Domain.Entities.Booking
            {
                BookingNumber = bookingNumber,
                UnitId = request.UnitId,
                UserId = request.UserId,
                CheckInDate = request.CheckInDate,
                CheckOutDate = request.CheckOutDate,
                NumberOfGuests = request.NumberOfGuests,
                NumberOfNights = nights,
                TotalPrice = totalPrice,
                PaidAmount = 0,
                Status = BookingStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                SpecialRequests = request.SpecialRequests,
                CreatedAt = DateTime.UtcNow.AddHours(3)
            };

            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();

            // 7. Assign rooms to booking
            var roomsToBook = availableRooms.Take(request.NumberOfRooms).ToList();
            foreach (var room in roomsToBook)
            {
                var bookingRoom = new BookingRoom
                {
                    BookingId = booking.Id,
                    RoomId = room.Id,
                    PricePerNight = room.PricePerNight,
                    NumberOfNights = nights
                };
                await _context.Set<BookingRoom>().AddAsync(bookingRoom);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // 8. Send confirmation email
            BackgroundJob.Enqueue(() =>
                SendBookingConfirmationEmailAsync(booking.Id));

            _logger.LogInformation(
                "Booking {BookingNumber} created for user {UserId}",
                bookingNumber, request.UserId);

            // 9. Return response
            var response = await MapToResponseAsync(booking);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating booking");
            return Result.Failure<BookingResponse>(
                new Error("BookingFailed", "Failed to create booking", 500));
        }
    }

    #endregion

    #region BOOKING MANAGEMENT

    public async Task<Result> ConfirmBookingAsync(int bookingId, string adminUserId)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            return Result.Failure(new Error("NotFound", "Booking not found", 404));

        if (booking.Status != BookingStatus.Pending)
            return Result.Failure(
                new Error("InvalidStatus", $"Cannot confirm booking with status {booking.Status}", 400));

        booking.Status = BookingStatus.Confirmed;
        booking.UpdatedAt = DateTime.UtcNow.AddHours(3);

        await _context.SaveChangesAsync();

        // Send confirmation email
        BackgroundJob.Enqueue(() =>
            SendBookingStatusEmailAsync(bookingId, "Confirmed"));

        _logger.LogInformation(
            "Booking {BookingId} confirmed by admin {AdminId}",
            bookingId, adminUserId);

        return Result.Success();
    }

    public async Task<Result> CheckInAsync(int bookingId)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            return Result.Failure(new Error("NotFound", "Booking not found", 404));

        if (booking.Status != BookingStatus.Confirmed)
            return Result.Failure(
                new Error("InvalidStatus", "Only confirmed bookings can be checked in", 400));

        if (DateTime.UtcNow.Date < booking.CheckInDate.Date)
            return Result.Failure(
                new Error("TooEarly", "Check-in date has not arrived yet", 400));

        booking.Status = BookingStatus.CheckedIn;
        booking.UpdatedAt = DateTime.UtcNow.AddHours(3);

        await _context.SaveChangesAsync();

        // Send check-in email
        BackgroundJob.Enqueue(() =>
            SendBookingStatusEmailAsync(bookingId, "CheckedIn"));

        return Result.Success();
    }

    public async Task<Result> CheckOutAsync(int bookingId)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            return Result.Failure(new Error("NotFound", "Booking not found", 404));

        if (booking.Status != BookingStatus.CheckedIn)
            return Result.Failure(
                new Error("InvalidStatus", "Only checked-in bookings can be checked out", 400));

        booking.Status = BookingStatus.Completed;
        booking.UpdatedAt = DateTime.UtcNow.AddHours(3);

        await _context.SaveChangesAsync();

        // Send check-out email with review request
        BackgroundJob.Enqueue(() =>
            SendCheckoutEmailAsync(bookingId));

        return Result.Success();
    }

    public async Task<Result> CancelBookingAsync(
        int bookingId,
        string cancellationReason,
        string userId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Unit)
                .ThenInclude(u => u.CancellationPolicy)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            return Result.Failure(new Error("NotFound", "Booking not found", 404));

        if (booking.Status == BookingStatus.Completed ||
            booking.Status == BookingStatus.Cancelled)
            return Result.Failure(
                new Error("InvalidStatus", "Cannot cancel this booking", 400));

        // Calculate refund based on cancellation policy
        var refundAmount = CalculateRefundAmount(booking);

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationReason = cancellationReason;
        booking.CancelledAt = DateTime.UtcNow.AddHours(3);
        booking.UpdatedAt = DateTime.UtcNow.AddHours(3);

        await _context.SaveChangesAsync();

        // Process refund if applicable
        if (refundAmount > 0)
        {
            BackgroundJob.Enqueue(() =>
                RefundBookingAsync(bookingId, refundAmount, "Booking cancelled"));
        }

        // Send cancellation email
        BackgroundJob.Enqueue(() =>
            SendCancellationEmailAsync(bookingId, refundAmount));

        _logger.LogInformation(
            "Booking {BookingId} cancelled by user {UserId}. Refund: {Refund}",
            bookingId, userId, refundAmount);

        return Result.Success();
    }

    #endregion

    #region PAYMENTS

    public async Task<Result> ProcessPaymentAsync(
        int bookingId,
        ProcessPaymentRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return Result.Failure(new Error("NotFound", "Booking not found", 404));

            // Create payment record
            var payment = new Payment
            {
                BookingId = bookingId,
                TransactionId = request.TransactionId,
                Amount = request.Amount,
                PaymentMethod = request.PaymentMethod,
                Status = PaymentStatus.Paid,
                PaymentDate = DateTime.UtcNow.AddHours(3),
                Notes = request.Notes
            };

            await _context.Set<Payment>().AddAsync(payment);

            // Update booking payment status
            booking.PaidAmount += request.Amount;

            if (booking.PaidAmount >= booking.TotalPrice)
            {
                booking.PaymentStatus = PaymentStatus.Paid;
                booking.Status = BookingStatus.Confirmed;
            }
            else
            {
                booking.PaymentStatus = PaymentStatus.PartiallyPaid;
            }

            booking.UpdatedAt = DateTime.UtcNow.AddHours(3);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Send payment confirmation email
            BackgroundJob.Enqueue(() =>
                SendPaymentConfirmationEmailAsync(bookingId, request.Amount));

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error processing payment for booking {BookingId}", bookingId);
            return Result.Failure(
                new Error("PaymentFailed", "Failed to process payment", 500));
        }
    }

    public async Task<Result> RefundBookingAsync(
        int bookingId,
        decimal refundAmount,
        string reason)
    {
        // Implementation for refund processing
        // This would integrate with your payment gateway

        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            return Result.Failure(new Error("NotFound", "Booking not found", 404));

        // Create refund payment record
        var refundPayment = new Payment
        {
            BookingId = bookingId,
            TransactionId = $"REFUND-{Guid.NewGuid()}",
            Amount = -refundAmount,
            PaymentMethod = PaymentMethod.DebitCard,
            Status = PaymentStatus.Paid,
            PaymentDate = DateTime.UtcNow.AddHours(3),
            Notes = reason
        };

        await _context.Set<Payment>().AddAsync(refundPayment);

        booking.PaidAmount -= refundAmount;
        booking.PaymentStatus = PaymentStatus.Refunded;

        await _context.SaveChangesAsync();

        return Result.Success();
    }

    #endregion

    #region STATISTICS

    public async Task<Result<BookingStatisticsResponse>> GetBookingStatisticsAsync(
        BookingStatisticsFilter filter)
    {
        var query = _context.Bookings
            .Include(b => b.Unit)
            .Include(b => b.User)
            .AsQueryable();

        // Apply filters
        if (filter.StartDate.HasValue)
            query = query.Where(b => b.CreatedAt >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(b => b.CreatedAt <= filter.EndDate.Value);

        if (filter.UnitId.HasValue)
            query = query.Where(b => b.UnitId == filter.UnitId.Value);

        if (filter.Status.HasValue)
            query = query.Where(b => b.Status == filter.Status.Value);

        var bookings = await query.ToListAsync();

        var statistics = new BookingStatisticsResponse
        {
            TotalBookings = bookings.Count,
            PendingBookings = bookings.Count(b => b.Status == BookingStatus.Pending),
            ConfirmedBookings = bookings.Count(b => b.Status == BookingStatus.Confirmed),
            CheckedInBookings = bookings.Count(b => b.Status == BookingStatus.CheckedIn),
            CompletedBookings = bookings.Count(b => b.Status == BookingStatus.Completed),
            CancelledBookings = bookings.Count(b => b.Status == BookingStatus.Cancelled),

            TotalRevenue = bookings
                .Where(b => b.Status == BookingStatus.Completed)
                .Sum(b => b.TotalPrice),

            PendingRevenue = bookings
                .Where(b => b.Status == BookingStatus.Pending ||
                           b.Status == BookingStatus.Confirmed)
                .Sum(b => b.TotalPrice),

            AverageBookingValue = bookings.Any()
                ? bookings.Average(b => b.TotalPrice)
                : 0,

            AverageNightsPerBooking = bookings.Any()
                ? bookings.Average(b => b.NumberOfNights)
                : 0,

            BookingsByStatus = bookings
                .GroupBy(b => b.Status)
                .ToDictionary(g => g.Key.ToString(), g => g.Count()),

            BookingsByMonth = bookings
                .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                .OrderByDescending(g => g.Key.Year)
                .ThenByDescending(g => g.Key.Month)
                .Take(12)
                .ToDictionary(
                    g => $"{g.Key.Year}-{g.Key.Month:D2}",
                    g => new MonthlyBookingStats
                    {
                        Count = g.Count(),
                        Revenue = g.Where(b => b.Status == BookingStatus.Completed)
                                   .Sum(b => b.TotalPrice)
                    })
        };

        return Result.Success(statistics);
    }

    #endregion

    #region QUERY METHODS

    public async Task<Result<BookingDetailsResponse>> GetBookingByIdAsync(int bookingId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Unit)
            .Include(b => b.User)
            .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
            .Include(b => b.Payments)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            return Result.Failure<BookingDetailsResponse>(
                new Error("NotFound", "Booking not found", 404));

        var response = MapToDetailsResponse(booking);
        return Result.Success(response);
    }

    public async Task<Result<BookingDetailsResponse>> GetBookingByNumberAsync(string bookingNumber)
    {
        var booking = await _context.Bookings
            .Include(b => b.Unit)
            .Include(b => b.User)
            .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
            .Include(b => b.Payments)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.BookingNumber == bookingNumber);

        if (booking == null)
            return Result.Failure<BookingDetailsResponse>(
                new Error("NotFound", "Booking not found", 404));

        var response = MapToDetailsResponse(booking);
        return Result.Success(response);
    }

    public async Task<Result<IEnumerable<BookingResponse>>> GetUserBookingsAsync(
        string userId,
        BookingFilter filter)
    {
        var query = _context.Bookings
            .Include(b => b.Unit)
            .Where(b => b.UserId == userId)
            .AsQueryable();

        query = ApplyFilters(query, filter);

        var bookings = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var responses = new List<BookingResponse>();
        foreach (var booking in bookings)
        {
            responses.Add(await MapToResponseAsync(booking));
        }

        return Result.Success<IEnumerable<BookingResponse>>(responses);
    }

    public async Task<Result<IEnumerable<BookingResponse>>> GetUnitBookingsAsync(
        int unitId,
        BookingFilter filter)
    {
        var query = _context.Bookings
            .Include(b => b.Unit)
            .Include(b => b.User)
            .Where(b => b.UnitId == unitId)
            .AsQueryable();

        query = ApplyFilters(query, filter);

        var bookings = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var responses = new List<BookingResponse>();
        foreach (var booking in bookings)
        {
            responses.Add(await MapToResponseAsync(booking));
        }

        return Result.Success<IEnumerable<BookingResponse>>(responses);
    }

    #endregion

    #region HELPER METHODS

    private async Task<List<Domain.Entities.SubUnit>> GetAvailableRoomsAsync(
        int unitId,
        DateTime checkIn,
        DateTime checkOut)
    {
        var allRooms = await _context.SubUnits
            .Where(r => r.UnitId == unitId && !r.IsDeleted && r.IsAvailable)
            .ToListAsync();

        var bookedRoomIds = await _context.BookingRooms
            .Include(br => br.Booking)
            .Where(br => br.Room.UnitId == unitId &&
                        br.Booking.CheckInDate < checkOut &&
                        br.Booking.CheckOutDate > checkIn &&
                        br.Booking.Status != BookingStatus.Cancelled)
            .Select(br => br.RoomId)
            .ToListAsync();

        return allRooms
            .Where(r => !bookedRoomIds.Contains(r.Id))
            .ToList();
    }

    private async Task<decimal> CalculateTotalPriceAsync(
        List<Domain.Entities.SubUnit> rooms,
        DateTime checkIn,
        DateTime checkOut)
    {
        decimal total = 0;
        var nights = (checkOut - checkIn).Days;

        foreach (var room in rooms)
        {
            // Check for special pricing
            var availability = await _context.Set<SubUnitAvailability>()
                .FirstOrDefaultAsync(a => a.SubUnitId == room.Id &&
                                         checkIn >= a.StartDate &&
                                         checkOut <= a.EndDate);

            var calculator = new PricingCalculator();
            total += calculator.CalculatePrice(room, checkIn, checkOut, availability);
        }

        return total;
    }

    private async Task<string> GenerateBookingNumberAsync()
    {
        var date = DateTime.UtcNow;
        var prefix = $"BK-{date:yyyyMMdd}";

        var lastBooking = await _context.Bookings
            .Where(b => b.BookingNumber.StartsWith(prefix))
            .OrderByDescending(b => b.Id)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastBooking != null)
        {
            var parts = lastBooking.BookingNumber.Split('-');
            if (parts.Length > 0 && int.TryParse(parts[^1], out int lastSeq))
            {
                sequence = lastSeq + 1;
            }
        }

        return $"{prefix}-{sequence:D4}";
    }

    private decimal CalculateRefundAmount(Domain.Entities.Booking booking)
    {
        if (booking.Unit?.CancellationPolicy == null)
            return 0;

        var policy = booking.Unit.CancellationPolicy;
        var daysUntilCheckIn = (booking.CheckInDate - DateTime.UtcNow).Days;

        if (daysUntilCheckIn >= policy.FullRefundDays)
            return booking.PaidAmount;

        if (daysUntilCheckIn >= policy.PartialRefundDays)
            return booking.PaidAmount * (policy.PartialRefundPercentage / 100);

        return 0;
    }

    private IQueryable<Domain.Entities.Booking> ApplyFilters(
        IQueryable<Domain.Entities.Booking> query,
        BookingFilter filter)
    {
        if (filter.Status.HasValue)
            query = query.Where(b => b.Status == filter.Status.Value);

        if (filter.PaymentStatus.HasValue)
            query = query.Where(b => b.PaymentStatus == filter.PaymentStatus.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(b => b.CheckInDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(b => b.CheckOutDate <= filter.EndDate.Value);

        return query;
    }

    private async Task<BookingResponse> MapToResponseAsync(Domain.Entities.Booking booking)
    {
        return new BookingResponse
        {
            Id = booking.Id,
            BookingNumber = booking.BookingNumber,
            UnitId = booking.UnitId,
            UnitName = booking.Unit?.Name ?? "",
            UserId = booking.UserId,
            UserName = booking.User?.FullName ?? "",
            CheckInDate = booking.CheckInDate,
            CheckOutDate = booking.CheckOutDate,
            NumberOfGuests = booking.NumberOfGuests,
            NumberOfNights = booking.NumberOfNights,
            TotalPrice = booking.TotalPrice,
            PaidAmount = booking.PaidAmount,
            Status = booking.Status,
            PaymentStatus = booking.PaymentStatus,
            CreatedAt = booking.CreatedAt
        };
    }

    private BookingDetailsResponse MapToDetailsResponse(Domain.Entities.Booking booking)
    {
        return new BookingDetailsResponse
        {
            Id = booking.Id,
            BookingNumber = booking.BookingNumber,
            UnitId = booking.UnitId,
            UnitName = booking.Unit?.Name ?? "",
            UnitAddress = booking.Unit?.Address ?? "",
            UserId = booking.UserId,
            UserName = booking.User?.FullName ?? "",
            UserEmail = booking.User?.Email ?? "",
            UserPhone = booking.User?.PhoneNumber,
            CheckInDate = booking.CheckInDate,
            CheckOutDate = booking.CheckOutDate,
            NumberOfGuests = booking.NumberOfGuests,
            NumberOfNights = booking.NumberOfNights,
            TotalPrice = booking.TotalPrice,
            PaidAmount = booking.PaidAmount,
            Status = booking.Status,
            PaymentStatus = booking.PaymentStatus,
            SpecialRequests = booking.SpecialRequests,
            CancellationReason = booking.CancellationReason,
            CancelledAt = booking.CancelledAt,
            Rooms = booking.BookingRooms?.Select(br => new BookingRoomInfo
            {
                RoomId = br.RoomId,
                RoomNumber = br.Room?.RoomNumber ?? "",
                PricePerNight = br.PricePerNight,
                NumberOfNights = br.NumberOfNights
            }).ToList() ?? new List<BookingRoomInfo>(),
            Payments = booking.Payments?.Select(p => new PaymentInfo
            {
                Id = p.Id,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod.ToString(),
                Status = p.Status.ToString(),
                PaymentDate = p.PaymentDate
            }).ToList() ?? new List<PaymentInfo>(),
            CreatedAt = booking.CreatedAt,
            UpdatedAt = booking.UpdatedAt
        };
    }

    #endregion

    #region EMAIL NOTIFICATIONS

    public async Task SendBookingConfirmationEmailAsync(int bookingId)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Unit)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null) return;

            var emailBody = $@"
                <h2>Booking Confirmation - {booking.BookingNumber}</h2>
                <p>Dear {booking.User?.FullName},</p>
                <p>Your booking has been received successfully!</p>
                
                <h3>Booking Details:</h3>
                <ul>
                    <li>Booking Number: <strong>{booking.BookingNumber}</strong></li>
                    <li>Unit: {booking.Unit?.Name}</li>
                    <li>Check-in: {booking.CheckInDate:MMM dd, yyyy}</li>
                    <li>Check-out: {booking.CheckOutDate:MMM dd, yyyy}</li>
                    <li>Nights: {booking.NumberOfNights}</li>
                    <li>Guests: {booking.NumberOfGuests}</li>
                    <li>Total Amount: ${booking.TotalPrice:F2}</li>
                </ul>
                
                <p>Status: <strong>{booking.Status}</strong></p>
                
                <p>We'll notify you once your booking is confirmed.</p>
                
                <p>Best regards,<br>The Hajzzy Team</p>
            ";

            await _emailSender.SendEmailAsync(
                booking.User!.Email!,
                $"Booking Confirmation - {booking.BookingNumber}",
                emailBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send booking confirmation email for booking {BookingId}", bookingId);
        }
    }

    public async Task SendBookingStatusEmailAsync(int bookingId, string newStatus)
    {
        // Similar implementation for status updates
        _logger.LogInformation("Sending booking status email for {BookingId}: {Status}", bookingId, newStatus);
    }

    public async Task SendCheckoutEmailAsync(int bookingId)
    {
        // Implementation for checkout email with review request
        _logger.LogInformation("Sending checkout email for booking {BookingId}", bookingId);
    }

    public async Task SendCancellationEmailAsync(int bookingId, decimal refundAmount)
    {
        // Implementation for cancellation email
        _logger.LogInformation("Sending cancellation email for booking {BookingId}", bookingId);
    }

    public async Task SendPaymentConfirmationEmailAsync(int bookingId, decimal amount)
    {
        // Implementation for payment confirmation
        _logger.LogInformation("Sending payment confirmation for booking {BookingId}", bookingId);
    }

    public async Task<Result<decimal>> CalculateBookingPriceAsync(CalculateBookingPriceRequest request)
    {
        var availableRooms = await GetAvailableRoomsAsync(
            request.UnitId,
            request.CheckInDate,
            request.CheckOutDate);

        if (availableRooms.Count < request.NumberOfRooms)
            return Result.Failure<decimal>(
                new Error("NotAvailable", "Not enough rooms available", 400));

        var totalPrice = await CalculateTotalPriceAsync(
            availableRooms.Take(request.NumberOfRooms).ToList(),
            request.CheckInDate,
            request.CheckOutDate);

        return Result.Success(totalPrice);
    }

    #endregion
}

// ============= REQUEST & RESPONSE MODELS =============

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
