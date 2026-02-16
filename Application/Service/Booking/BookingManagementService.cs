using Application.Abstraction;
using Application.Contracts.BookingManagement;
using Application.Notifications;
using Domain;
using Domain.Entities;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BookedRoomInfo = Application.Contracts.BookingManagement.BookedRoomInfo;
using BookingTrendsResponse = Application.Contracts.BookingManagement.BookingTrendsResponse;
using CancellationAnalyticsResponse = Application.Contracts.BookingManagement.CancellationAnalyticsResponse;
using OccupancyStatisticsResponse = Application.Contracts.BookingManagement.OccupancyStatisticsResponse;
using PendingPaymentResponse = Application.Contracts.BookingManagement.PendingPaymentResponse;
using UpcomingCheckInResponse = Application.Contracts.BookingManagement.UpcomingCheckInResponse;
using UpcomingCheckOutResponse = Application.Contracts.BookingManagement.UpcomingCheckOutResponse;

namespace Application.Service.Booking;

public class BookingManagementService(
    ApplicationDbcontext context,
    INotinficationService emailNotificationService,
    ILogger<BookingManagementService> logger) : IBookingManagementService
{
    private readonly ApplicationDbcontext _context = context;
    private readonly INotinficationService _emailNotificationService = emailNotificationService;
    private readonly ILogger<BookingManagementService> _logger = logger;

    #region BOOKING QUERIES

    public async Task<Result<IBookingManagementService.PaginatedResponse<UnifiedBookingResponse>>> GetAllBookingsAsync(
        UnifiedBookingFilter filter)
    {
        try
        {
            var query = _context.Bookings
                .Include(b => b.Unit)
                .Include(b => b.User)
                .Include(b => b.BookingRooms)
                    .ThenInclude(br => br.Room)
                .AsQueryable();

            query = ApplyFilters(query, filter);

            var totalCount = await query.CountAsync();

            var bookings = await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var responses = await Task.WhenAll(
                bookings.Select(b => MapToUnifiedResponseAsync(b)));

            var paginatedResult = CreatePaginatedResponse(responses, totalCount, filter.Page, filter.PageSize);
            return Result.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all bookings");
            return Result.Failure<IBookingManagementService.PaginatedResponse<UnifiedBookingResponse>>(
                new Error("GetBookingsFailed", "Failed to retrieve bookings", 500));
        }
    }

    public async Task<Result<IBookingManagementService.PaginatedResponse<UnifiedBookingResponse>>> GetUserBookingsAsync(
        string userId,
        UnifiedBookingFilter filter)
    {
        try
        {
            var query = _context.Bookings
                .Include(b => b.Unit)
                .Include(b => b.User)
                .Include(b => b.BookingRooms)
                    .ThenInclude(br => br.Room)
                .Where(b => b.UserId == userId)
                .AsQueryable();

            query = ApplyFilters(query, filter);

            var totalCount = await query.CountAsync();

            var bookings = await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var responses = await Task.WhenAll(
                bookings.Select(b => MapToUnifiedResponseAsync(b)));

            var paginatedResult = CreatePaginatedResponse(responses, totalCount, filter.Page, filter.PageSize);
            return Result.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user bookings for user {UserId}", userId);
            return Result.Failure<IBookingManagementService.PaginatedResponse<UnifiedBookingResponse>>(
                new Error("GetUserBookingsFailed", "Failed to retrieve user bookings", 500));
        }
    }

    public async Task<Result<IBookingManagementService.PaginatedResponse<UnifiedBookingResponse>>> GetUnitBookingsAsync(
        int unitId,
        UnifiedBookingFilter filter)
    {
        try
        {
            var query = _context.Bookings
                .Include(b => b.Unit)
                .Include(b => b.User)
                .Include(b => b.BookingRooms)
                    .ThenInclude(br => br.Room)
                .Where(b => b.UnitId == unitId)
                .AsQueryable();

            query = ApplyFilters(query, filter);

            var totalCount = await query.CountAsync();

            var bookings = await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var responses = await Task.WhenAll(
                bookings.Select(b => MapToUnifiedResponseAsync(b)));

            var paginatedResult = CreatePaginatedResponse(responses, totalCount, filter.Page, filter.PageSize);
            return Result.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bookings for unit {UnitId}", unitId);
            return Result.Failure<IBookingManagementService.PaginatedResponse<UnifiedBookingResponse>>(
                new Error("GetUnitBookingsFailed", "Failed to retrieve unit bookings", 500));
        }
    }

    public async Task<Result<UnifiedBookingDetailsResponse>> GetBookingByIdAsync(int bookingId)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Unit)
                    .ThenInclude(u => u.City)
                .Include(b => b.Unit)
                    .ThenInclude(u => u.CancellationPolicy)
                .Include(b => b.User)
                .Include(b => b.BookingRooms)
                    .ThenInclude(br => br.Room)
                .Include(b => b.Payments)
                .Include(b => b.Review)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return Result.Failure<UnifiedBookingDetailsResponse>(
                    new Error("NotFound", "Booking not found", 404));

            var response = await MapToUnifiedDetailsResponseAsync(booking);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting booking {BookingId}", bookingId);
            return Result.Failure<UnifiedBookingDetailsResponse>(
                new Error("GetBookingFailed", "Failed to retrieve booking details", 500));
        }
    }

    public async Task<Result<UnifiedBookingDetailsResponse>> GetBookingByNumberAsync(string bookingNumber)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Unit)
                    .ThenInclude(u => u.City)
                .Include(b => b.Unit)
                    .ThenInclude(u => u.CancellationPolicy)
                .Include(b => b.User)
                .Include(b => b.BookingRooms)
                    .ThenInclude(br => br.Room)
                .Include(b => b.Payments)
                .Include(b => b.Review)
                .FirstOrDefaultAsync(b => b.BookingNumber == bookingNumber);

            if (booking == null)
                return Result.Failure<UnifiedBookingDetailsResponse>(
                    new Error("NotFound", "Booking not found", 404));

            var response = await MapToUnifiedDetailsResponseAsync(booking);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting booking by number {BookingNumber}", bookingNumber);
            return Result.Failure<UnifiedBookingDetailsResponse>(
                new Error("GetBookingFailed", "Failed to retrieve booking details", 500));
        }
    }

    public async Task<Result<IBookingManagementService.PaginatedResponse<UnifiedBookingResponse>>> SearchBookingsAsync(
        string searchTerm,
        UnifiedBookingFilter filter)
    {
        try
        {
            var query = _context.Bookings
                .Include(b => b.Unit)
                .Include(b => b.User)
                .Include(b => b.BookingRooms)
                    .ThenInclude(br => br.Room)
                .Where(b =>
                    b.BookingNumber.Contains(searchTerm) ||
                    b.User.FullName.Contains(searchTerm) ||
                    b.User.Email.Contains(searchTerm) ||
                    b.User.PhoneNumber.Contains(searchTerm))
                .AsQueryable();

            query = ApplyFilters(query, filter);

            var totalCount = await query.CountAsync();

            var bookings = await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var responses = await Task.WhenAll(
                bookings.Select(b => MapToUnifiedResponseAsync(b)));

            var paginatedResult = CreatePaginatedResponse(responses, totalCount, filter.Page, filter.PageSize);
            return Result.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching bookings with term {SearchTerm}", searchTerm);
            return Result.Failure<IBookingManagementService.PaginatedResponse<UnifiedBookingResponse>>(
                new Error("SearchFailed", "Failed to search bookings", 500));
        }
    }

    #endregion

    #region BOOKING MANAGEMENT

    public async Task<Result> ConfirmBookingAsync(int bookingId, string adminUserId)
    {
        try
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

            BackgroundJob.Enqueue(() =>
                _emailNotificationService.SendBookingStatusUpdateEmailAsync(bookingId, "Confirmed"));

            _logger.LogInformation(
                "Booking {BookingId} ({BookingType}) confirmed by {AdminUserId}",
                bookingId, booking.BookingType, adminUserId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming booking {BookingId}", bookingId);
            return Result.Failure(new Error("ConfirmFailed", "Failed to confirm booking", 500));
        }
    }

    public async Task<Result> CheckInBookingAsync(int bookingId, CheckInRequest? request = null)
    {
        try
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

            BackgroundJob.Enqueue(() =>
                _emailNotificationService.SendBookingStatusUpdateEmailAsync(bookingId, "CheckedIn"));

            _logger.LogInformation(
                "Booking {BookingId} ({BookingType}) checked in",
                bookingId, booking.BookingType);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking in booking {BookingId}", bookingId);
            return Result.Failure(new Error("CheckInFailed", "Failed to check in booking", 500));
        }
    }

    public async Task<Result> CheckOutBookingAsync(int bookingId, CheckOutRequest? request = null)
    {
        try
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

            BackgroundJob.Enqueue(() =>
                _emailNotificationService.SendBookingCheckoutEmailAsync(bookingId));

            _logger.LogInformation(
                "Booking {BookingId} ({BookingType}) checked out",
                bookingId, booking.BookingType);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking out booking {BookingId}", bookingId);
            return Result.Failure(new Error("CheckOutFailed", "Failed to check out booking", 500));
        }
    }

    public async Task<Result<CancellationResponse>> CancelBookingAsync(
        int bookingId,
        string cancellationReason,
        string userId)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Unit)
                    .ThenInclude(u => u.CancellationPolicy)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return Result.Failure<CancellationResponse>(
                    new Error("NotFound", "Booking not found", 404));

            if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
                return Result.Failure<CancellationResponse>(
                    new Error("InvalidStatus", "Cannot cancel this booking", 400));

            // Calculate refund
            var refundAmount = CalculateRefundAmount(booking);

            booking.Status = BookingStatus.Cancelled;
            booking.CancellationReason = cancellationReason;
            booking.CancelledAt = DateTime.UtcNow.AddHours(3);
            booking.UpdatedAt = DateTime.UtcNow.AddHours(3);

            await _context.SaveChangesAsync();

            // Restore coupon usage if applied
            var bookingCoupon = await _context.Set<BookingCoupon>()
                .Include(bc => bc.Coupon)
                .FirstOrDefaultAsync(bc => bc.BookingId == bookingId);

            if (bookingCoupon != null)
            {
                var coupon = bookingCoupon.Coupon;
                if (coupon.CurrentUsageCount > 0)
                    coupon.CurrentUsageCount--;

                _context.Set<BookingCoupon>().Remove(bookingCoupon);
                await _context.SaveChangesAsync();
            }

            // Process refund if applicable
            if (refundAmount > 0)
            {
                var refundRequest = new RefundRequest
                {
                    Amount = refundAmount,
                    Reason = cancellationReason,
                    RefundMethod = RefundMethod.OriginalPaymentMethod
                };

                await RefundBookingAsync(bookingId, refundRequest);
            }

            BackgroundJob.Enqueue(() =>
                _emailNotificationService.SendBookingCancellationEmailAsync(bookingId, refundAmount));

            _logger.LogInformation(
                "Booking {BookingId} ({BookingType}) cancelled by user {UserId}. Refund: {Refund}",
                bookingId, booking.BookingType, userId, refundAmount);

            var response = new CancellationResponse
            {
                BookingId = bookingId,
                RefundAmount = refundAmount,
                CancellationDate = booking.CancelledAt!.Value,
                Status = "Cancelled"
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling booking {BookingId}", bookingId);
            return Result.Failure<CancellationResponse>(
                new Error("CancelFailed", "Failed to cancel booking", 500));
        }
    }

    public async Task<Result> ModifyBookingDatesAsync(
        int bookingId,
        DateTime newCheckIn,
        DateTime newCheckOut,
        string userId)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Unit)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return Result.Failure(new Error("NotFound", "Booking not found", 404));

            if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.Confirmed)
                return Result.Failure(
                    new Error("InvalidStatus", "Only pending or confirmed bookings can be modified", 400));

            if (newCheckIn >= newCheckOut)
                return Result.Failure(
                    new Error("InvalidDates", "Check-out must be after check-in", 400));

            // Validate availability for new dates
            // (Implementation depends on availability service)

            booking.CheckInDate = newCheckIn;
            booking.CheckOutDate = newCheckOut;
            booking.NumberOfNights = (newCheckOut - newCheckIn).Days;
            booking.UpdatedAt = DateTime.UtcNow.AddHours(3);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Booking {BookingId} dates modified by user {UserId}",
                bookingId, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error modifying booking {BookingId}", bookingId);
            return Result.Failure(new Error("ModifyFailed", "Failed to modify booking", 500));
        }
    }

    #endregion

    #region PAYMENT OPERATIONS

    public async Task<Result> ProcessPaymentAsync(int bookingId, Contracts.BookingManagement.ProcessPaymentRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return Result.Failure(new Error("NotFound", "Booking not found", 404));

            var payment = new Payment
            {
                BookingId = bookingId,
                TransactionId = request.TransactionId,
                Amount = request.Amount,
                PaymentMethod = (PaymentMethod)request.PaymentMethod,
                Status = PaymentStatus.Paid,
                PaymentDate = DateTime.UtcNow.AddHours(3),
                Notes = request.Notes
            };

            await _context.Set<Payment>().AddAsync(payment);

            booking.PaidAmount += request.Amount;

            if (booking.PaidAmount >= booking.TotalPrice)
            {
                booking.PaymentStatus = PaymentStatus.Paid;
                if (booking.Status == BookingStatus.Pending)
                    booking.Status = BookingStatus.Confirmed;
            }
            else
            {
                booking.PaymentStatus = PaymentStatus.PartiallyPaid;
            }

            booking.UpdatedAt = DateTime.UtcNow.AddHours(3);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            BackgroundJob.Enqueue(() =>
                _emailNotificationService.SendPaymentConfirmationEmailAsync(bookingId, request.Amount));

            _logger.LogInformation(
                "Payment processed for booking {BookingId}. Amount: {Amount}",
                bookingId, request.Amount);

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error processing payment for booking {BookingId}", bookingId);
            return Result.Failure(new Error("PaymentFailed", "Failed to process payment", 500));
        }
    }

    public async Task<Result> RefundBookingAsync(int bookingId, RefundRequest request)
    {
        try
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return Result.Failure(new Error("NotFound", "Booking not found", 404));

            var refundPayment = new Payment
            {
                BookingId = bookingId,
                TransactionId = $"REFUND-{Guid.NewGuid()}",
                Amount = -request.Amount,
                PaymentMethod = PaymentMethod.BankTransfer,
                Status = PaymentStatus.Paid,
                PaymentDate = DateTime.UtcNow.AddHours(3),
                Notes = request.Reason
            };

            await _context.Set<Payment>().AddAsync(refundPayment);

            booking.PaidAmount -= request.Amount;
            booking.PaymentStatus = PaymentStatus.Refunded;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Refund processed for booking {BookingId}. Amount: {Amount}",
                bookingId, request.Amount);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding booking {BookingId}", bookingId);
            return Result.Failure(new Error("RefundFailed", "Failed to refund booking", 500));
        }
    }

    public async Task<Result<IEnumerable<PaymentHistoryItem>>> GetBookingPaymentHistoryAsync(int bookingId)
    {
        try
        {
            var payments = await _context.Set<Payment>()
                .Where(p => p.BookingId == bookingId)
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => new PaymentHistoryItem
                {
                    Id = p.Id,
                    TransactionId = p.TransactionId,
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod.ToString(),
                    Status = p.Status.ToString(),
                    PaymentDate = p.PaymentDate,
                    Notes = p.Notes
                })
                .ToListAsync();

            return Result.Success<IEnumerable<PaymentHistoryItem>>(payments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment history for booking {BookingId}", bookingId);
            return Result.Failure<IEnumerable<PaymentHistoryItem>>(
                new Error("GetPaymentsFailed", "Failed to retrieve payment history", 500));
        }
    }

    public async Task<Result<IBookingManagementService.PaginatedResponse<PendingPaymentResponse>>> GetPendingPaymentsAsync(
        PendingPaymentFilter filter)
    {
        try
        {
            var query = _context.Bookings
                .Include(b => b.Unit)
                .Include(b => b.User)
                .Where(b => b.PaymentStatus == PaymentStatus.Pending ||
                           b.PaymentStatus == PaymentStatus.PartiallyPaid)
                .AsQueryable();

            if (filter.UnitId.HasValue)
                query = query.Where(b => b.UnitId == filter.UnitId.Value);

            if (filter.DaysUntilCheckIn.HasValue)
            {
                var targetDate = DateTime.UtcNow.AddDays(filter.DaysUntilCheckIn.Value);
                query = query.Where(b => b.CheckInDate <= targetDate);
            }

            var totalCount = await query.CountAsync();

            var bookings = await query
                .OrderBy(b => b.CheckInDate)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(b => new PendingPaymentResponse
                {
                    BookingId = b.Id,
                    BookingNumber = b.BookingNumber,
                    UnitName = b.Unit.Name,
                    GuestName = b.User.FullName ?? "N/A",
                    GuestEmail = b.User.Email ?? string.Empty,
                    TotalAmount = b.TotalPrice,
                    PaidAmount = b.PaidAmount,
                    RemainingAmount = b.TotalPrice - b.PaidAmount,
                    PaymentStatus = b.PaymentStatus.ToString(),
                    CheckInDate = b.CheckInDate,
                    DaysUntilCheckIn = (int)(b.CheckInDate - DateTime.UtcNow).TotalDays
                })
                .ToListAsync();

            var paginatedResult = CreatePaginatedResponse(bookings, totalCount, filter.Page, filter.PageSize);
            return Result.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending payments");
            return Result.Failure<IBookingManagementService.PaginatedResponse<PendingPaymentResponse>>(
                new Error("GetPendingPaymentsFailed", "Failed to retrieve pending payments", 500));
        }
    }

    #endregion

    #region CALENDAR & SCHEDULING

    public async Task<Result<IEnumerable<UpcomingCheckInResponse>>> GetUpcomingCheckInsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? unitId = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.Date;
            var end = endDate ?? start.AddDays(7);

            var query = _context.Bookings
                .Include(b => b.Unit)
                .Include(b => b.User)
                .Include(b => b.BookingRooms)
                    .ThenInclude(br => br.Room)
                .Where(b => b.CheckInDate >= start &&
                           b.CheckInDate < end &&
                           b.Status == BookingStatus.Confirmed)
                .AsQueryable();

            if (unitId.HasValue)
                query = query.Where(b => b.UnitId == unitId.Value);

            var checkIns = await query
                .OrderBy(b => b.CheckInDate)
                .Select(b => new UpcomingCheckInResponse
                {
                    BookingId = b.Id,
                    BookingNumber = b.BookingNumber,
                    BookingType = b.BookingType.ToString(),
                    UnitName = b.Unit.Name,
                    GuestName = b.User.FullName ?? "N/A",
                    GuestEmail = b.User.Email ?? string.Empty,
                    GuestPhone = b.User.PhoneNumber ?? string.Empty,
                    CheckInDate = b.CheckInDate,
                    CheckInTime = b.CheckInDate.TimeOfDay,
                    NumberOfGuests = b.NumberOfGuests,
                    NumberOfNights = b.NumberOfNights,
                    RoomNumbers = b.BookingRooms.Select(br => br.Room.RoomNumber).ToList(),
                    SpecialRequests = b.SpecialRequests
                })
                .ToListAsync();

            return Result.Success<IEnumerable<UpcomingCheckInResponse>>(checkIns);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting upcoming check-ins");
            return Result.Failure<IEnumerable<UpcomingCheckInResponse>>(
                new Error("GetCheckInsFailed", "Failed to retrieve upcoming check-ins", 500));
        }
    }

    public async Task<Result<IEnumerable<UpcomingCheckOutResponse>>> GetUpcomingCheckOutsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? unitId = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.Date;
            var end = endDate ?? start.AddDays(7);

            var query = _context.Bookings
                .Include(b => b.Unit)
                .Include(b => b.User)
                .Include(b => b.BookingRooms)
                    .ThenInclude(br => br.Room)
                .Where(b => b.CheckOutDate >= start &&
                           b.CheckOutDate < end &&
                           b.Status == BookingStatus.CheckedIn)
                .AsQueryable();

            if (unitId.HasValue)
                query = query.Where(b => b.UnitId == unitId.Value);

            var checkOuts = await query
                .OrderBy(b => b.CheckOutDate)
                .Select(b => new UpcomingCheckOutResponse
                {
                    BookingId = b.Id,
                    BookingNumber = b.BookingNumber,
                    BookingType = b.BookingType.ToString(),
                    UnitId = b.UnitId,
                    UnitName = b.Unit.Name,
                    GuestName = b.User.FullName ?? "N/A",
                    GuestEmail = b.User.Email ?? string.Empty,
                    GuestPhone = b.User.PhoneNumber ?? string.Empty,
                    CheckOutDate = b.CheckOutDate,
                    CheckOutTime = b.CheckOutDate.TimeOfDay,
                    RoomNumbers = b.BookingRooms.Select(br => br.Room.RoomNumber).ToList(),
                    TotalAmount = b.TotalPrice,
                    PaidAmount = b.PaidAmount,
                    BalanceDue = b.TotalPrice - b.PaidAmount
                })
                .ToListAsync();

            return Result.Success<IEnumerable<UpcomingCheckOutResponse>>(checkOuts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting upcoming check-outs");
            return Result.Failure<IEnumerable<UpcomingCheckOutResponse>>(
                new Error("GetCheckOutsFailed", "Failed to retrieve upcoming check-outs", 500));
        }
    }

    public async Task<Result<IEnumerable<CurrentGuestResponse>>> GetCurrentGuestsAsync(int? unitId = null)
    {
        try
        {
            var query = _context.Bookings
                .Include(b => b.Unit)
                .Include(b => b.User)
                .Include(b => b.BookingRooms)
                    .ThenInclude(br => br.Room)
                .Where(b => b.Status == BookingStatus.CheckedIn)
                .AsQueryable();

            if (unitId.HasValue)
                query = query.Where(b => b.UnitId == unitId.Value);

            var guests = await query
                .OrderBy(b => b.CheckOutDate)
                .Select(b => new CurrentGuestResponse
                {
                    BookingId = b.Id,
                    BookingNumber = b.BookingNumber,
                    BookingType = b.BookingType.ToString(),
                    UnitName = b.Unit.Name,
                    GuestName = b.User.FullName ?? "N/A",
                    GuestEmail = b.User.Email ?? string.Empty,
                    GuestPhone = b.User.PhoneNumber ?? string.Empty,
                    CheckInDate = b.CheckInDate,
                    CheckOutDate = b.CheckOutDate,
                    RoomNumbers = b.BookingRooms.Select(br => br.Room.RoomNumber).ToList(),
                    DaysRemaining = (int)(b.CheckOutDate - DateTime.UtcNow).TotalDays
                })
                .ToListAsync();

            return Result.Success<IEnumerable<CurrentGuestResponse>>(guests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current guests");
            return Result.Failure<IEnumerable<CurrentGuestResponse>>(
                new Error("GetGuestsFailed", "Failed to retrieve current guests", 500));
        }
    }

    public async Task<Result<BookingTimelineResponse>> GetBookingTimelineAsync(
        DateTime startDate,
        DateTime endDate,
        int? unitId = null)
    {
        try
        {
            var query = _context.Bookings
                .Include(b => b.Unit)
                .Include(b => b.User)
                .Include(b => b.BookingRooms)
                    .ThenInclude(br => br.Room)
                .Where(b => b.CheckInDate < endDate && b.CheckOutDate > startDate &&
                           b.Status != BookingStatus.Cancelled)
                .AsQueryable();

            if (unitId.HasValue)
                query = query.Where(b => b.UnitId == unitId.Value);

            var bookings = await query.ToListAsync();

            var timeline = new BookingTimelineResponse
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalBookings = bookings.Count,
                BookingsByDate = bookings
                    .GroupBy(b => b.CheckInDate.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new DateBookingSummary
                    {
                        Date = g.Key,
                        CheckIns = g.Count(),
                        CheckOuts = bookings.Count(b => b.CheckOutDate.Date == g.Key)
                    })
                    .ToList(),
                Bookings = await Task.WhenAll(bookings.Select(b => MapToUnifiedResponseAsync(b)))
            };

            return Result.Success(timeline);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting booking timeline");
            return Result.Failure<BookingTimelineResponse>(
                new Error("GetTimelineFailed", "Failed to retrieve booking timeline", 500));
        }
    }

    #endregion

    #region STATISTICS & ANALYTICS

    public async Task<Result<BookingStatisticsResponse>> GetBookingStatisticsAsync(
        BookingStatisticsFilter filter)
    {
        try
        {
            var query = _context.Bookings.AsQueryable();

            if (filter.StartDate.HasValue)
                query = query.Where(b => b.CreatedAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(b => b.CreatedAt <= filter.EndDate.Value);

            if (filter.UnitId.HasValue)
                query = query.Where(b => b.UnitId == filter.UnitId.Value);

            if (filter.BookingType.HasValue)
                query = query.Where(b => b.BookingType == filter.BookingType.Value);

            var bookings = await query.ToListAsync();

            var stats = new BookingStatisticsResponse
            {
                TotalBookings = bookings.Count,
                PendingBookings = bookings.Count(b => b.Status == BookingStatus.Pending),
                ConfirmedBookings = bookings.Count(b => b.Status == BookingStatus.Confirmed),
                CheckedInBookings = bookings.Count(b => b.Status == BookingStatus.CheckedIn),
                CompletedBookings = bookings.Count(b => b.Status == BookingStatus.Completed),
                CancelledBookings = bookings.Count(b => b.Status == BookingStatus.Cancelled),

                UnitBookings = bookings.Count(b => b.BookingType == BookingType.UnitBooking),
                SubUnitBookings = bookings.Count(b => b.BookingType == BookingType.SubUnitBooking),

                TotalRevenue = bookings
                    .Where(b => b.Status == BookingStatus.Completed)
                    .Sum(b => b.TotalPrice),

                PendingRevenue = bookings
                    .Where(b => b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed)
                    .Sum(b => b.TotalPrice),

                AverageBookingValue = bookings.Any() ? bookings.Average(b => b.TotalPrice) : 0,
                AverageNightsPerBooking = bookings.Any() ? bookings.Average(b => b.NumberOfNights) : 0,

                BookingsByStatus = bookings
                    .GroupBy(b => b.Status)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),

                BookingsByType = bookings
                    .GroupBy(b => b.BookingType)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),

                BookingsByMonth = bookings
                    .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                    .OrderByDescending(g => g.Key.Year)
                    .ThenByDescending(g => g.Key.Month)
                    .Take(12)
                    .ToDictionary(
                        g => $"{g.Key.Year}-{g.Key.Month:D2}",
                        g => g.Count())
            };

            return Result.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting booking statistics");
            return Result.Failure<BookingStatisticsResponse>(
                new Error("GetStatsFailed", "Failed to retrieve booking statistics", 500));
        }
    }

    public async Task<Result<RevenueAnalyticsResponse>> GetRevenueAnalyticsAsync(
        RevenueAnalyticsFilter filter)
    {
        try
        {
            var query = _context.Bookings
                .Where(b => b.Status == BookingStatus.Completed)
                .AsQueryable();

            if (filter.StartDate.HasValue)
                query = query.Where(b => b.CreatedAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(b => b.CreatedAt <= filter.EndDate.Value);

            if (filter.UnitId.HasValue)
                query = query.Where(b => b.UnitId == filter.UnitId.Value);

            var bookings = await query
                .Include(b => b.Unit)
                .ToListAsync();

            var analytics = new RevenueAnalyticsResponse
            {
                TotalRevenue = bookings.Sum(b => b.TotalPrice),
                TotalBookings = bookings.Count,
                AverageRevenuePerBooking = bookings.Any() ? bookings.Average(b => b.TotalPrice) : 0,

                RevenueByBookingType = bookings
                    .GroupBy(b => b.BookingType)
                    .ToDictionary(
                        g => g.Key.ToString(),
                        g => g.Sum(b => b.TotalPrice)),

                RevenueByUnit = bookings
                    .GroupBy(b => new { b.UnitId, b.Unit.Name })
                    .OrderByDescending(g => g.Sum(b => b.TotalPrice))
                    .Take(10)
                    .Select(g => new UnitRevenue
                    {
                        UnitId = g.Key.UnitId,
                        UnitName = g.Key.Name,
                        Revenue = g.Sum(b => b.TotalPrice),
                        BookingCount = g.Count()
                    })
                    .ToList(),

                RevenueByMonth = bookings
                    .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                    .OrderBy(g => g.Key.Year)
                    .ThenBy(g => g.Key.Month)
                    .Select(g => new MonthlyRevenue
                    {
                        Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                        Revenue = g.Sum(b => b.TotalPrice),
                        BookingCount = g.Count()
                    })
                    .ToList()
            };

            return Result.Success(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue analytics");
            return Result.Failure<RevenueAnalyticsResponse>(
                new Error("GetAnalyticsFailed", "Failed to retrieve revenue analytics", 500));
        }
    }

    public async Task<Result<OccupancyStatisticsResponse>> GetOccupancyStatisticsAsync(
        Contracts.BookingManagement.OccupancyFilter filter)
    {
        try
        {
            // Implementation for occupancy statistics
            // This would involve calculating room nights, occupancy rates, etc.

            var stats = new OccupancyStatisticsResponse
            {
                Period = $"{filter.StartDate:yyyy-MM-dd} to {filter.EndDate:yyyy-MM-dd}",
                OccupancyRate = 0, // Calculate based on booked nights vs available nights
                TotalRoomNights = 0,
                BookedRoomNights = 0,
                AvailableRoomNights = 0
            };

            return Result.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting occupancy statistics");
            return Result.Failure<OccupancyStatisticsResponse>(
                new Error("GetOccupancyFailed", "Failed to retrieve occupancy statistics", 500));
        }
    }

    public async Task<Result<CancellationAnalyticsResponse>> GetCancellationAnalyticsAsync(
        CancellationAnalyticsFilter filter)
    {
        try
        {
            var query = _context.Bookings
                .Where(b => b.Status == BookingStatus.Cancelled)
                .AsQueryable();

            if (filter.StartDate.HasValue)
                query = query.Where(b => b.CancelledAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(b => b.CancelledAt <= filter.EndDate.Value);

            var cancelledBookings = await query.ToListAsync();
            var totalBookingsInPeriod = await _context.Bookings
                .Where(b => b.CreatedAt >= (filter.StartDate ?? DateTime.MinValue) &&
                           b.CreatedAt <= (filter.EndDate ?? DateTime.MaxValue))
                .CountAsync();

            var analytics = new CancellationAnalyticsResponse
            {
                TotalCancellations = cancelledBookings.Count,
                CancellationRate = totalBookingsInPeriod > 0
                    ? (decimal)cancelledBookings.Count / totalBookingsInPeriod * 100
                    : 0,
                RefundedAmount = cancelledBookings
                    .Where(b => b.PaymentStatus == PaymentStatus.Refunded)
                    .Sum(b => b.PaidAmount),
                AverageCancellationLeadTime = cancelledBookings.Any()
                    ? (decimal)cancelledBookings
                        .Where(b => b.CancelledAt.HasValue)
                        .Average(b => (b.CheckInDate - b.CancelledAt.Value).TotalDays)
                    : 0
            };

            return Result.Success(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cancellation analytics");
            return Result.Failure<CancellationAnalyticsResponse>(
                new Error("GetCancellationFailed", "Failed to retrieve cancellation analytics", 500));
        }
    }

    public async Task<Result<BookingTrendsResponse>> GetBookingTrendsAsync(
        BookingTrendsFilter filter)
    {
        try
        {
            var query = _context.Bookings.AsQueryable();

            if (filter.StartDate.HasValue)
                query = query.Where(b => b.CreatedAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(b => b.CreatedAt <= filter.EndDate.Value);

            var bookings = await query.ToListAsync();

            var monthlyTrends = bookings
                .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g => new Contracts.BookingManagement.MonthlyTrend
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    BookingCount = g.Count(),
                    Revenue = g.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.TotalPrice)
                })
                .ToList();

            var trends = new BookingTrendsResponse
            {
                MonthlyTrends = monthlyTrends,
                TotalBookings = bookings.Count,
                TrendDirection = monthlyTrends.Count > 1
                    ? (monthlyTrends.Last().BookingCount > monthlyTrends.First().BookingCount ? "Up" : "Down")
                    : "Stable"
            };

            return Result.Success(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting booking trends");
            return Result.Failure<BookingTrendsResponse>(
                new Error("GetTrendsFailed", "Failed to retrieve booking trends", 500));
        }
    }

    #endregion

    #region REPORTS

    public async Task<Result<DailyBookingReportResponse>> GetDailyReportAsync(DateTime date)
    {
        try
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            var bookings = await _context.Bookings
                .Include(b => b.Unit)
                .Include(b => b.User)
                .Where(b => b.CreatedAt >= startOfDay && b.CreatedAt < endOfDay)
                .ToListAsync();

            var checkIns = await _context.Bookings
                .Where(b => b.CheckInDate.Date == date.Date && b.Status == BookingStatus.Confirmed)
                .CountAsync();

            var checkOuts = await _context.Bookings
                .Where(b => b.CheckOutDate.Date == date.Date && b.Status == BookingStatus.CheckedIn)
                .CountAsync();

            var report = new DailyBookingReportResponse
            {
                Date = date,
                NewBookings = bookings.Count,
                CheckIns = checkIns,
                CheckOuts = checkOuts,
                Revenue = bookings.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.TotalPrice),
                Cancellations = bookings.Count(b => b.Status == BookingStatus.Cancelled),
                CurrentOccupancy = await _context.Bookings
                    .CountAsync(b => b.Status == BookingStatus.CheckedIn)
            };

            return Result.Success(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting daily report for {Date}", date);
            return Result.Failure<DailyBookingReportResponse>(
                new Error("GetReportFailed", "Failed to generate daily report", 500));
        }
    }

    public async Task<Result<MonthlyBookingReportResponse>> GetMonthlyReportAsync(int year, int month)
    {
        try
        {
            var startOfMonth = new DateTime(year, month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);

            var bookings = await _context.Bookings
                .Include(b => b.Unit)
                .Where(b => b.CreatedAt >= startOfMonth && b.CreatedAt < endOfMonth)
                .ToListAsync();

            var report = new MonthlyBookingReportResponse
            {
                Year = year,
                Month = month,
                TotalBookings = bookings.Count,
                CompletedBookings = bookings.Count(b => b.Status == BookingStatus.Completed),
                CancelledBookings = bookings.Count(b => b.Status == BookingStatus.Cancelled),
                TotalRevenue = bookings.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.TotalPrice),
                AverageBookingValue = bookings.Any() ? bookings.Average(b => b.TotalPrice) : 0,
                BookingsByDay = bookings
                    .GroupBy(b => b.CreatedAt.Day)
                    .OrderBy(g => g.Key)
                    .Select(g => new DailyBookingCount
                    {
                        Day = g.Key,
                        Count = g.Count()
                    })
                    .ToList()
            };

            return Result.Success(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting monthly report for {Year}-{Month}", year, month);
            return Result.Failure<MonthlyBookingReportResponse>(
                new Error("GetReportFailed", "Failed to generate monthly report", 500));
        }
    }

    public async Task<Result<CustomBookingReportResponse>> GetCustomReportAsync(
        DateTime startDate,
        DateTime endDate,
        ReportOptions options)
    {
        try
        {
            var query = _context.Bookings
                .Include(b => b.Unit)
                .Include(b => b.User)
                .Include(b => b.BookingRooms)
                    .ThenInclude(br => br.Room)
                .Where(b => b.CreatedAt >= startDate && b.CreatedAt <= endDate)
                .AsQueryable();

            if (options.UnitId.HasValue)
                query = query.Where(b => b.UnitId == options.UnitId.Value);

            if (options.Status.HasValue)
                query = query.Where(b => b.Status == options.Status.Value);

            if (options.BookingType.HasValue)
                query = query.Where(b => b.BookingType == options.BookingType.Value);

            var bookings = await query.ToListAsync();

            var report = new CustomBookingReportResponse
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalBookings = bookings.Count,
                TotalRevenue = bookings.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.TotalPrice),
                AverageBookingValue = bookings.Any() ? bookings.Average(b => b.TotalPrice) : 0,
                BookingsByStatus = bookings
                    .GroupBy(b => b.Status)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                TopUnits = bookings
                    .GroupBy(b => new { b.UnitId, b.Unit.Name })
                    .OrderByDescending(g => g.Count())
                    .Take(10)
                    .Select(g => new UnitBookingCount
                    {
                        UnitId = g.Key.UnitId,
                        UnitName = g.Key.Name,
                        BookingCount = g.Count()
                    })
                    .ToList()
            };

            return Result.Success(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting custom report");
            return Result.Failure<CustomBookingReportResponse>(
                new Error("GetReportFailed", "Failed to generate custom report", 500));
        }
    }

    #endregion

    #region BULK OPERATIONS

    public async Task<Result<BulkOperationResult>> BulkConfirmBookingsAsync(
        List<int> bookingIds,
        string adminUserId)
    {
        var result = new BulkOperationResult();

        foreach (var bookingId in bookingIds)
        {
            var confirmResult = await ConfirmBookingAsync(bookingId, adminUserId);
            if (confirmResult.IsSuccess)
                result.SuccessCount++;
            else
            {
                result.FailureCount++;
                result.Errors.Add(new BulkOperationError
                {
                    Id = bookingId,
                    Error = confirmResult.Error.Description
                });
            }
        }

        return Result.Success(result);
    }

    public async Task<Result<BulkOperationResult>> BulkCancelBookingsAsync(
        List<int> bookingIds,
        string cancellationReason,
        string userId)
    {
        var result = new BulkOperationResult();

        foreach (var bookingId in bookingIds)
        {
            var cancelResult = await CancelBookingAsync(bookingId, cancellationReason, userId);
            if (cancelResult.IsSuccess)
                result.SuccessCount++;
            else
            {
                result.FailureCount++;
                result.Errors.Add(new BulkOperationError
                {
                    Id = bookingId,
                    Error = cancelResult.Error?.Description ?? "Unknown error"
                });
            }
        }

        return Result.Success(result);
    }

    #endregion

    #region NOTIFICATIONS

    public async Task<Result> SendBookingReminderAsync(int bookingId, ReminderType reminderType)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return Result.Failure(new Error("NotFound", "Booking not found", 404));

            // Send appropriate reminder based on type
            switch (reminderType)
            {
                case ReminderType.CheckInReminder:
                    BackgroundJob.Enqueue(() =>
                        _emailNotificationService.SendBookingStatusUpdateEmailAsync(bookingId, "CheckInReminder"));
                    break;
                case ReminderType.CheckOutReminder:
                    BackgroundJob.Enqueue(() =>
                        _emailNotificationService.SendBookingStatusUpdateEmailAsync(bookingId, "CheckOutReminder"));
                    break;
                case ReminderType.PaymentReminder:
                    BackgroundJob.Enqueue(() =>
                        _emailNotificationService.SendBookingStatusUpdateEmailAsync(bookingId, "PaymentReminder"));
                    break;
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending reminder for booking {BookingId}", bookingId);
            return Result.Failure(new Error("ReminderFailed", "Failed to send reminder", 500));
        }
    }

    public async Task<Result> SendCustomMessageToGuestAsync(
        int bookingId,
        string subject,
        string message)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return Result.Failure(new Error("NotFound", "Booking not found", 404));

            // Send custom message
            BackgroundJob.Enqueue(() =>
                _emailNotificationService.SendBookingStatusUpdateEmailAsync(bookingId, message));

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending custom message for booking {BookingId}", bookingId);
            return Result.Failure(new Error("MessageFailed", "Failed to send message", 500));
        }
    }

    #endregion

    #region VALIDATION & UTILITIES

    public async Task<Result<BookingModificationValidation>> ValidateBookingModificationAsync(
        int bookingId,
        DateTime newCheckIn,
        DateTime newCheckOut)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Unit)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return Result.Failure<BookingModificationValidation>(
                    new Error("NotFound", "Booking not found", 404));

            var validation = new BookingModificationValidation
            {
                CanModify = true,
                Reasons = new List<string>()
            };

            if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.Confirmed)
            {
                validation.CanModify = false;
                validation.Reasons.Add("Booking status does not allow modification");
            }

            if (newCheckIn >= newCheckOut)
            {
                validation.CanModify = false;
                validation.Reasons.Add("Check-out must be after check-in");
            }

            if (newCheckIn < DateTime.UtcNow.Date)
            {
                validation.CanModify = false;
                validation.Reasons.Add("Cannot modify to past dates");
            }

            // Add more validation logic as needed

            return Result.Success(validation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating booking modification for {BookingId}", bookingId);
            return Result.Failure<BookingModificationValidation>(
                new Error("ValidationFailed", "Failed to validate modification", 500));
        }
    }

    public async Task<Result<decimal>> CalculateRefundAmountAsync(
        int bookingId,
        DateTime? cancellationDate = null)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Unit)
                    .ThenInclude(u => u.CancellationPolicy)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return Result.Failure<decimal>(
                    new Error("NotFound", "Booking not found", 404));

            var refundAmount = CalculateRefundAmount(booking, cancellationDate);
            return Result.Success(refundAmount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating refund for booking {BookingId}", bookingId);
            return Result.Failure<decimal>(
                new Error("CalculationFailed", "Failed to calculate refund amount", 500));
        }
    }

    public async Task<Result<BookingDashboardSummary>> GetDashboardSummaryAsync(
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.Date.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow.Date;

            var bookings = await _context.Bookings
                .Where(b => b.CreatedAt >= start && b.CreatedAt <= end)
                .ToListAsync();

            var summary = new BookingDashboardSummary
            {
                TotalBookings = bookings.Count,
                PendingBookings = bookings.Count(b => b.Status == BookingStatus.Pending),
                ConfirmedBookings = bookings.Count(b => b.Status == BookingStatus.Confirmed),
                CheckedInBookings = bookings.Count(b => b.Status == BookingStatus.CheckedIn),
                CompletedBookings = bookings.Count(b => b.Status == BookingStatus.Completed),
                CancelledBookings = bookings.Count(b => b.Status == BookingStatus.Cancelled),

                TodayCheckIns = await _context.Bookings
                    .CountAsync(b => b.CheckInDate.Date == DateTime.UtcNow.Date &&
                                    b.Status == BookingStatus.Confirmed),

                TodayCheckOuts = await _context.Bookings
                    .CountAsync(b => b.CheckOutDate.Date == DateTime.UtcNow.Date &&
                                    b.Status == BookingStatus.CheckedIn),

                CurrentGuests = await _context.Bookings
                    .CountAsync(b => b.Status == BookingStatus.CheckedIn),

                TotalRevenue = bookings
                    .Where(b => b.Status == BookingStatus.Completed)
                    .Sum(b => b.TotalPrice),

                PendingPayments = bookings
                    .Where(b => b.PaymentStatus == PaymentStatus.Pending ||
                               b.PaymentStatus == PaymentStatus.PartiallyPaid)
                    .Sum(b => b.TotalPrice - b.PaidAmount)
            };

            return Result.Success(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard summary");
            return Result.Failure<BookingDashboardSummary>(
                new Error("GetSummaryFailed", "Failed to retrieve dashboard summary", 500));
        }
    }

    #endregion

    #region HELPER METHODS

    private IQueryable<Domain.Entities.Booking> ApplyFilters(IQueryable<Domain.Entities.Booking> query, UnifiedBookingFilter filter)
    {
        if (filter.Status.HasValue)
            query = query.Where(b => b.Status == filter.Status.Value);

        if (filter.PaymentStatus.HasValue)
            query = query.Where(b => b.PaymentStatus == filter.PaymentStatus.Value);

        if (filter.BookingType.HasValue)
            query = query.Where(b => b.BookingType == filter.BookingType.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(b => b.CheckInDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(b => b.CheckOutDate <= filter.EndDate.Value);

        if (filter.UnitId.HasValue)
            query = query.Where(b => b.UnitId == filter.UnitId.Value);

        if (filter.UserId != null)
            query = query.Where(b => b.UserId == filter.UserId);

        return query;
    }

    private async Task<UnifiedBookingResponse> MapToUnifiedResponseAsync(Domain.Entities.Booking booking)
    {
        // Get coupon information if applied
        var bookingCoupon = await _context.Set<BookingCoupon>()
            .Include(bc => bc.Coupon)
            .AsNoTracking()
            .FirstOrDefaultAsync(bc => bc.BookingId == booking.Id);

        return new UnifiedBookingResponse
        {
            Id = booking.Id,
            BookingNumber = booking.BookingNumber,
            BookingType = booking.BookingType.ToString(),
            UnitId = booking.UnitId,
            UnitName = booking.Unit?.Name ?? "",
            UserId = booking.UserId,
            UserName = booking.User?.FullName ?? "",
            UserEmail = booking.User?.Email ?? "",
            CheckInDate = booking.CheckInDate,
            CheckOutDate = booking.CheckOutDate,
            NumberOfGuests = booking.NumberOfGuests,
            NumberOfNights = booking.NumberOfNights,
            TotalPrice = booking.TotalPrice,
            PaidAmount = booking.PaidAmount,
            RemainingBalance = booking.TotalPrice - booking.PaidAmount,
            Status = booking.Status.ToString(),
            PaymentStatus = booking.PaymentStatus.ToString(),
            CreatedAt = booking.CreatedAt,

            // Rooms (for SubUnit bookings)
            Rooms = booking.BookingRooms?.Select(br => new BookedRoomInfo
            {
                RoomId = br.RoomId,
                RoomNumber = br.Room.RoomNumber,
                PricePerNight = br.PricePerNight
            }).ToList() ?? new List<BookedRoomInfo>(),

            // Coupon information
            AppliedCouponCode = bookingCoupon?.Coupon?.Code,
            CouponDiscount = bookingCoupon?.DiscountApplied
        };
    }

    private async Task<UnifiedBookingDetailsResponse> MapToUnifiedDetailsResponseAsync(Domain.Entities.Booking booking)
    {
        // Get coupon information if applied
        var bookingCoupon = await _context.Set<BookingCoupon>()
            .Include(bc => bc.Coupon)
            .AsNoTracking()
            .FirstOrDefaultAsync(bc => bc.BookingId == booking.Id);

        return new UnifiedBookingDetailsResponse
        {
            Id = booking.Id,
            BookingNumber = booking.BookingNumber,
            BookingType = booking.BookingType.ToString(),

            // Unit Information
            UnitId = booking.UnitId,
            UnitName = booking.Unit?.Name ?? "",
            UnitAddress = booking.Unit?.Address ?? "",
            CityName = booking.Unit?.City?.Name ?? "",

            // Guest Information
            UserId = booking.UserId,
            UserName = booking.User?.FullName ?? "",
            UserEmail = booking.User?.Email ?? "",
            UserPhone = booking.User?.PhoneNumber,

            // Booking Details
            CheckInDate = booking.CheckInDate,
            CheckOutDate = booking.CheckOutDate,
            NumberOfGuests = booking.NumberOfGuests,
            NumberOfNights = booking.NumberOfNights,

            // Pricing
            TotalPrice = booking.TotalPrice,
            PaidAmount = booking.PaidAmount,
            RemainingBalance = booking.TotalPrice - booking.PaidAmount,

            // Status
            Status = booking.Status.ToString(),
            PaymentStatus = booking.PaymentStatus.ToString(),

            // Additional Information
            SpecialRequests = booking.SpecialRequests,
            CancellationReason = booking.CancellationReason,
            CancelledAt = booking.CancelledAt,

            // Rooms (for SubUnit bookings)
            Rooms = booking.BookingRooms?.Select(br => new BookedRoomInfo
            {
                RoomId = br.RoomId,
                RoomNumber = br.Room.RoomNumber,
                PricePerNight = br.PricePerNight
            }).ToList() ?? new List<BookedRoomInfo>(),

            // Payments
            Payments = booking.Payments?.Select(p => new PaymentHistoryItem
            {
                Id = p.Id,
                TransactionId = p.TransactionId,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod.ToString(),
                Status = p.Status.ToString(),
                PaymentDate = p.PaymentDate,
                Notes = p.Notes
            }).ToList() ?? new List<PaymentHistoryItem>(),

            // Review
            HasReview = booking.Review != null,
            ReviewRating = booking.Review?.Rating,

            // Coupon information
            AppliedCouponCode = bookingCoupon?.Coupon?.Code,
            CouponDiscount = bookingCoupon?.DiscountApplied,

            // Cancellation Policy
            CancellationPolicyName = booking.Unit?.CancellationPolicy?.Name,

            // Timestamps
            CreatedAt = booking.CreatedAt,
            UpdatedAt = booking.UpdatedAt
        };
    }

    private decimal CalculateRefundAmount(Domain.Entities.Booking booking, DateTime? cancellationDate = null)
    {
        if (booking.Unit?.CancellationPolicy == null)
            return 0;

        var policy = booking.Unit.CancellationPolicy;
        var cancelDate = cancellationDate ?? DateTime.UtcNow;
        var daysUntilCheckIn = (booking.CheckInDate - cancelDate).Days;

        if (daysUntilCheckIn >= policy.FullRefundDays)
            return booking.PaidAmount;

        if (daysUntilCheckIn >= policy.PartialRefundDays)
            return booking.PaidAmount * (policy.PartialRefundPercentage / 100);

        return 0;
    }

    private IBookingManagementService.PaginatedResponse<T> CreatePaginatedResponse<T>(
        IEnumerable<T> items,
        int totalCount,
        int page,
        int pageSize)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new IBookingManagementService.PaginatedResponse<T>
        {
            Items = items,
            TotalCount = totalCount,
            TotalPages = totalPages,
            CurrentPage = page,
            NextPage = page < totalPages ? page + 1 : null,
            PrevPage = page > 1 ? page - 1 : null
        };
    }

    #endregion
}