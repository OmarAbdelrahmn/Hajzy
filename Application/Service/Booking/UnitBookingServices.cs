using Application.Abstraction;
using Application.Contracts;
using Application.Contracts.Availability;
using Application.Contracts.Bookin;
using Application.Notifications;
using Application.Service.Avilabilaties;
using Application.Service.PromoCode;
using Domain;
using Domain.Entities;
using Hangfire;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Service.Booking;

/// <summary>
/// UPDATED: Unit Booking Service now handles BOTH:
/// 1. Standalone Units (gardens, pools, event halls) - no subunits, rent the entire unit
/// 2. Units with Subunits (hotels, apartments) - rent all rooms together
/// </summary>
public class UnitBookingService(
    ApplicationDbcontext context,
    IAvailabilityService availabilityService,
    ICouponService couponService,
    IEmailSender emailSender,
    ILogger<UnitBookingService> logger,
    INotinficationService emailNotificationService) : IUnitBookingServices
{
    private readonly ApplicationDbcontext _context = context;
    private readonly IAvailabilityService _availabilityService = availabilityService;
    private readonly ICouponService _couponService = couponService;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly ILogger<UnitBookingService> _logger = logger;
    private readonly INotinficationService _emailNotificationService = emailNotificationService;

    #region CREATE BOOKING

    public async Task<Result<UnitBookingResponse>> CreateUnitBookingAsync(CreateUnitBookingRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Validate Unit exists and load related data
            var unit = await _context.Units
                .Include(u => u.Rooms.Where(r => !r.IsDeleted)) // Load subunits
                .Include(u => u.CancellationPolicy)
                .FirstOrDefaultAsync(u => u.Id == request.UnitId && !u.IsDeleted);

            if (unit == null)
                return Result.Failure<UnitBookingResponse>(
                    new Error("UnitNotFound", "Unit not found", 404));

            // NEW: Determine if this is a standalone unit
            var isStandaloneUnit = !unit.Rooms.Any();

            _logger.LogInformation(
                "Creating booking for {UnitType} unit {UnitId}",
                isStandaloneUnit ? "standalone" : "multi-subunit", request.UnitId);

            // 2. Validate dates
            if (request.CheckInDate >= request.CheckOutDate)
                return Result.Failure<UnitBookingResponse>(
                    new Error("InvalidDates", "Check-out must be after check-in", 400));

            if (request.CheckInDate < DateTime.UtcNow.Date)
                return Result.Failure<UnitBookingResponse>(
                    new Error("InvalidDates", "Cannot book past dates", 400));

            // 3. Check unit availability
            // This works for BOTH standalone and regular units
            var availabilityCheck = await _availabilityService.CheckUnitAvailabilityAsync(
                request.UnitId,
                request.CheckInDate,
                request.CheckOutDate);

            if (!availabilityCheck.IsSuccess || !availabilityCheck.Value.IsAvailable)
            {
                var reason = availabilityCheck.Value?.Reason ?? "Unit not available";
                _logger.LogWarning(
                    "Unit {UnitId} is not available: {Reason}",
                    request.UnitId, reason);

                return Result.Failure<UnitBookingResponse>(
                    new Error("NotAvailable", $"Unit not available: {reason}", 400));
            }

            // 4. Calculate pricing
            var nights = (request.CheckOutDate - request.CheckInDate).Days;

            // NEW: Use BasePrice for standalone units, calculate from rooms for others
            decimal originalPrice = isStandaloneUnit
                ? unit.BasePrice * nights
                : await CalculateUnitWithRoomsPriceAsync(unit, request.CheckInDate, request.CheckOutDate);

            decimal finalPrice = originalPrice;
            decimal discountAmount = 0;
            Coupon? appliedCoupon = null;

            // 5. Validate and apply coupon if provided
            if (!string.IsNullOrWhiteSpace(request.CouponCode))
            {
                var couponValidation = await _couponService.ValidateCouponAsync(new Contracts.couponcontract.ValidateCouponRequest(
                    CouponCode: request.CouponCode,
                    BookingAmount: originalPrice,
                    UnitId: request.UnitId,
                    CityId: unit.CityId,
                    UnitTypeId: unit.UnitTypeId,
                    UserId: request.UserId,
                    CheckInDate: request.CheckInDate,
                    CheckOutDate: request.CheckOutDate
                ));

                if (!couponValidation.IsSuccess || !couponValidation.Value.IsValid)
                {
                    return Result.Failure<UnitBookingResponse>(
                        new Error("InvalidCoupon",
                            couponValidation.Value?.ErrorMessage ?? "Coupon is not valid", 400));
                }

                discountAmount = couponValidation.Value.DiscountAmount;
                finalPrice = couponValidation.Value.FinalPrice;

                appliedCoupon = await _context.Set<Coupon>()
                    .FirstOrDefaultAsync(c => c.Code.ToUpper() == request.CouponCode.ToUpper());
            }

            // 6. Generate unique booking number
            var bookingPrefix = isStandaloneUnit ? "STAND" : "UNIT";
            var bookingNumber = await GenerateBookingNumberAsync(bookingPrefix);

            // 7. Create booking record
            // NOTE: BookingRooms will be EMPTY for both standalone units AND full unit bookings
            var booking = new Domain.Entities.Booking
            {
                BookingNumber = bookingNumber,
                BookingType = BookingType.UnitBooking, // Always UnitBooking type
                UnitId = request.UnitId,
                UserId = request.UserId,
                CheckInDate = request.CheckInDate,
                CheckOutDate = request.CheckOutDate,
                NumberOfGuests = request.NumberOfGuests,
                NumberOfNights = nights,
                TotalPrice = finalPrice,
                PaidAmount = 0,
                Status = BookingStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                SpecialRequests = request.SpecialRequests,
                CreatedAt = DateTime.UtcNow.AddHours(3)
            };

            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();

            // 8. Apply coupon if validated
            if (appliedCoupon != null && discountAmount > 0)
            {
                var bookingCoupon = new BookingCoupon
                {
                    BookingId = booking.Id,
                    CouponId = appliedCoupon.Id,
                    DiscountApplied = discountAmount,
                    AppliedAt = DateTime.UtcNow.AddHours(3)
                };

                await _context.Set<BookingCoupon>().AddAsync(bookingCoupon);
                appliedCoupon.CurrentUsageCount++;
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Coupon {CouponCode} applied to booking {BookingNumber}. Discount: {Discount}",
                    appliedCoupon.Code, bookingNumber, discountAmount);
            }

            // 9. Mark dates as booked
            var markBooked = await _availabilityService.MarkDatesAsBookedAsync(new MarkDatesAsBookedRequest(
                BookingType.UnitBooking,
                UnitId: request.UnitId,
                BookingId: booking.Id,
                CheckInDate: request.CheckInDate,
                CheckOutDate: request.CheckOutDate
            ));

            if (!markBooked.IsSuccess)
            {
                await transaction.RollbackAsync();
                return Result.Failure<UnitBookingResponse>(
                    new Error("AvailabilityUpdateFailed", "Failed to update availability", 500));
            }

            await transaction.CommitAsync();

            // 10. Send confirmation email
            BackgroundJob.Enqueue(() => _emailNotificationService.SendBookingConfirmationEmailAsync(booking.Id));

            _logger.LogInformation(
                "{UnitType} booking {BookingNumber} created for user {UserId}. Original: {Original}, Final: {Final}",
                isStandaloneUnit ? "Standalone unit" : "Full unit",
                bookingNumber, request.UserId, originalPrice, finalPrice);

            // 11. Return response
            var response = await MapToResponseAsync(booking);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating unit booking");
            return Result.Failure<UnitBookingResponse>(
                new Error("BookingFailed", "Failed to create unit booking", 500));
        }
    }

    public async Task<Result<decimal>> CalculateUnitBookingPriceAsync(CalculateUnitBookingPriceRequest request)
    {
        var unit = await _context.Units
            .Include(u => u.Rooms.Where(r => !r.IsDeleted && r.IsAvailable))
            .FirstOrDefaultAsync(u => u.Id == request.UnitId && !u.IsDeleted);

        if (unit == null)
            return Result.Failure<decimal>(
                new Error("UnitNotFound", "Unit not found", 404));

        var nights = (request.CheckOutDate - request.CheckInDate).Days;

        // NEW: Check if standalone unit
        var isStandaloneUnit = !unit.Rooms.Any();

        decimal totalPrice;
        if (isStandaloneUnit)
        {
            // Standalone: use base price
            totalPrice = unit.BasePrice * nights;
            _logger.LogInformation(
                "Calculating price for standalone unit {UnitId}: {BasePrice} × {Nights} nights = {Total}",
                request.UnitId, unit.BasePrice, nights, totalPrice);
        }
        else
        {
            // Units with rooms: sum all room prices
            totalPrice = await CalculateUnitWithRoomsPriceAsync(unit, request.CheckInDate, request.CheckOutDate);
            _logger.LogInformation(
                "Calculating price for unit {UnitId} with {RoomCount} rooms: {Total}",
                request.UnitId, unit.Rooms.Count, totalPrice);
        }

        return Result.Success(totalPrice);
    }

    #endregion

    #region BOOKING MANAGEMENT

    public async Task<Result> ConfirmUnitBookingAsync(int bookingId, string adminUserId)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.BookingType == BookingType.UnitBooking);

        if (booking == null)
            return Result.Failure(new Error("NotFound", "Booking not found", 404));

        if (booking.Status != BookingStatus.Pending)
            return Result.Failure(
                new Error("InvalidStatus", $"Cannot confirm booking with status {booking.Status}", 400));

        booking.Status = BookingStatus.Confirmed;
        booking.UpdatedAt = DateTime.UtcNow.AddHours(3);

        await _context.SaveChangesAsync();

        BackgroundJob.Enqueue(() => _emailNotificationService.SendBookingStatusUpdateEmailAsync(bookingId, "Confirmed"));

        _logger.LogInformation(
            "Unit booking {BookingId} confirmed by admin {AdminId}",
            bookingId, adminUserId);

        return Result.Success();
    }

    public async Task<Result> CheckInUnitAsync(int bookingId)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.BookingType == BookingType.UnitBooking);

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

        BackgroundJob.Enqueue(() => _emailNotificationService.SendBookingStatusUpdateEmailAsync(bookingId, "CheckedIn"));

        return Result.Success();
    }

    public async Task<Result> CheckOutUnitAsync(int bookingId)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.BookingType == BookingType.UnitBooking);

        if (booking == null)
            return Result.Failure(new Error("NotFound", "Booking not found", 404));

        if (booking.Status != BookingStatus.CheckedIn)
            return Result.Failure(
                new Error("InvalidStatus", "Only checked-in bookings can be checked out", 400));

        booking.Status = BookingStatus.Completed;
        booking.UpdatedAt = DateTime.UtcNow.AddHours(3);

        await _context.SaveChangesAsync();

        BackgroundJob.Enqueue(() => _emailNotificationService.SendBookingCheckoutEmailAsync(bookingId));

        return Result.Success();
    }

    public async Task<Result> CancelUnitBookingAsync(
        int bookingId,
        string cancellationReason,
        string userId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Unit)
                .ThenInclude(u => u.CancellationPolicy)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.BookingType == BookingType.UnitBooking);

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

        // Remove coupon and restore usage count
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

            _logger.LogInformation(
                "Coupon usage restored for booking {BookingId} cancellation",
                bookingId);
        }

        // Process refund if applicable
        if (refundAmount > 0)
        {
            BackgroundJob.Enqueue(() =>
                RefundUnitBookingAsync(bookingId, refundAmount, "Booking cancelled"));
        }

        BackgroundJob.Enqueue(() => _emailNotificationService.SendBookingCancellationEmailAsync(bookingId, refundAmount));

        _logger.LogInformation(
            "Unit booking {BookingId} cancelled by user {UserId}. Refund: {Refund}",
            bookingId, userId, refundAmount);

        return Result.Success();
    }

    #endregion

    #region PAYMENT OPERATIONS

    public async Task<Result> ProcessUnitPaymentAsync(int bookingId, ProcessPaymentRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.BookingType == BookingType.UnitBooking);

            if (booking == null)
                return Result.Failure(new Error("NotFound", "Booking not found", 404));

            var payment = new Payment
            {
                BookingId = bookingId,
                TransactionId = request.TransactionId,
                Amount = request.Amount,
                PaymentMethod = (Domain.PaymentMethod)request.PaymentMethod,
                Status = PaymentStatus.Paid,
                PaymentDate = DateTime.UtcNow.AddHours(3),
                Notes = request.Notes
            };

            await _context.Set<Payment>().AddAsync(payment);

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

            BackgroundJob.Enqueue(() => _emailNotificationService.SendPaymentConfirmationEmailAsync(bookingId, request.Amount));


            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error processing payment for unit booking {BookingId}", bookingId);
            return Result.Failure(new Error("PaymentFailed", "Failed to process payment", 500));
        }
    }

    public async Task<Result> RefundUnitBookingAsync(int bookingId, decimal refundAmount, string reason)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.BookingType == BookingType.UnitBooking);

        if (booking == null)
            return Result.Failure(new Error("NotFound", "Booking not found", 404));

        var refundPayment = new Payment
        {
            BookingId = bookingId,
            TransactionId = $"REFUND-{Guid.NewGuid()}",
            Amount = -refundAmount,
            PaymentMethod = PaymentMethod.BankTransfer,
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

    #region QUERY OPERATIONS

    public async Task<Result<UnitBookingDetailsResponse>> GetUnitBookingByIdAsync(int bookingId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Unit)
            .Include(b => b.User)
            .Include(b => b.Payments)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.BookingType == BookingType.UnitBooking);

        if (booking == null)
            return Result.Failure<UnitBookingDetailsResponse>(
                new Error("NotFound", "Booking not found", 404));

        var response = MapToDetailsResponseAsync(booking);
        return Result.Success(await response.ConfigureAwait(false));
    }

    public async Task<Result<UnitBookingDetailsResponse>> GetUnitBookingByNumberAsync(string bookingNumber)
    {
        var booking = await _context.Bookings
            .Include(b => b.Unit)
            .Include(b => b.User)
            .Include(b => b.Payments)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.BookingNumber == bookingNumber && b.BookingType == BookingType.UnitBooking);

        if (booking == null)
            return Result.Failure<UnitBookingDetailsResponse>(
                new Error("NotFound", "Booking not found", 404));

        var response = MapToDetailsResponseAsync(booking);
        return Result.Success(await response.ConfigureAwait(false));
    }

    public async Task<Result<IEnumerable<UnitBookingResponse>>> GetUserUnitBookingsAsync(
        string userId,
        BookingFilter filter)
    {
        var query = _context.Bookings
            .Include(b => b.Unit)
            .Where(b => b.UserId == userId && b.BookingType == BookingType.UnitBooking)
            .AsQueryable();

        query = ApplyFilters(query, filter);

        var bookings = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var responses = new List<UnitBookingResponse>();
        foreach (var booking in bookings)
        {
            responses.Add(await MapToResponseAsync(booking));
        }

        return Result.Success<IEnumerable<UnitBookingResponse>>(responses);
    }

    public async Task<Result<IEnumerable<UnitBookingResponse>>> GetUnitBookingsForPropertyAsync(
        int unitId,
        BookingFilter filter)
    {
        var query = _context.Bookings
            .Include(b => b.Unit)
            .Include(b => b.User)
            .Where(b => b.UnitId == unitId && b.BookingType == BookingType.UnitBooking)
            .AsQueryable();

        query = ApplyFilters(query, filter);

        var bookings = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var responses = new List<UnitBookingResponse>();
        foreach (var booking in bookings)
        {
            responses.Add(await MapToResponseAsync(booking));
        }

        return Result.Success<IEnumerable<UnitBookingResponse>>(responses);
    }

    #endregion

    #region STATISTICS

    public async Task<Result<UnitBookingStatisticsResponse>> GetUnitBookingStatisticsAsync(
        UnitBookingStatisticsFilter filter)
    {
        var query = _context.Bookings
            .Include(b => b.Unit)
            .Include(b => b.User)
            .Where(b => b.BookingType == BookingType.UnitBooking)
            .AsQueryable();

        if (filter.StartDate.HasValue)
            query = query.Where(b => b.CreatedAt >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(b => b.CreatedAt <= filter.EndDate.Value);

        if (filter.UnitId.HasValue)
            query = query.Where(b => b.UnitId == filter.UnitId.Value);

        if (filter.Status.HasValue)
            query = query.Where(b => b.Status == filter.Status.Value);

        var bookings = await query.ToListAsync();

        var statistics = new UnitBookingStatisticsResponse
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
                .Where(b => b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed)
                .Sum(b => b.TotalPrice),

            AverageBookingValue = bookings.Any() ? bookings.Average(b => b.TotalPrice) : 0,
            AverageNightsPerBooking = bookings.Any() ? bookings.Average(b => b.NumberOfNights) : 0,

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
                        Revenue = g.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.TotalPrice)
                    })
        };

        return Result.Success(statistics);
    }

    #endregion

    #region AVAILABILITY CHECK

    public async Task<Result<bool>> IsUnitAvailableForBookingAsync(
        int unitId,
        DateTime checkIn,
        DateTime checkOut)
    {
        var availabilityCheck = await _availabilityService.CheckUnitAvailabilityAsync(
            unitId, checkIn, checkOut);

        if (!availabilityCheck.IsSuccess)
            return Result.Failure<bool>(availabilityCheck.Error);

        return Result.Success(availabilityCheck.Value.IsAvailable);
    }

    #endregion

    #region HELPER METHODS

    private async Task<string> GenerateBookingNumberAsync(string prefix)
    {
        var date = DateTime.UtcNow;
        var bookingPrefix = $"BK-{prefix}-{date:yyyyMMdd}";

        var lastBooking = await _context.Bookings
            .Where(b => b.BookingNumber.StartsWith(bookingPrefix) &&
                       b.BookingType == BookingType.UnitBooking)
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

        return $"{bookingPrefix}-{sequence:D4}";
    }

    /// <summary>
    /// NEW: Calculate price for units with rooms (sum all room prices)
    /// </summary>
    private async Task<decimal> CalculateUnitWithRoomsPriceAsync(
       Domain.Entities.Unit unit,
        DateTime checkIn,
        DateTime checkOut)
    {
        if (!unit.Rooms.Any())
            return 0;

        decimal total = 0;
        var nights = (checkOut - checkIn).Days;

        foreach (var room in unit.Rooms)
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

    private async Task<UnitBookingResponse> MapToResponseAsync(Domain.Entities.Booking booking)
    {
        var bookingCoupon = await _context.Set<BookingCoupon>()
            .Include(bc => bc.Coupon)
            .AsNoTracking()
            .FirstOrDefaultAsync(bc => bc.BookingId == booking.Id);

        return new UnitBookingResponse
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
            CreatedAt = booking.CreatedAt,
            AppliedCouponCode = bookingCoupon?.Coupon?.Code ?? "non",
            CouponDiscount = bookingCoupon?.DiscountApplied.ToString() ?? "non"
        };
    }

    private async Task<UnitBookingDetailsResponse> MapToDetailsResponseAsync(Domain.Entities.Booking booking)
    {
        var bookingCoupon = await _context.Set<BookingCoupon>()
            .Include(bc => bc.Coupon)
            .AsNoTracking()
            .FirstOrDefaultAsync(bc => bc.BookingId == booking.Id);

        return new UnitBookingDetailsResponse
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
            Payments = booking.Payments?.Select(p => new PaymentInfo
            {
                Id = p.Id,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod.ToString(),
                Status = p.Status.ToString(),
                PaymentDate = p.PaymentDate
            }).ToList() ?? new List<PaymentInfo>(),
            UpdatedAt = booking.UpdatedAt,
            AppliedCouponCode = bookingCoupon?.Coupon?.Code,
            CouponDiscount = bookingCoupon?.DiscountApplied.ToString()
        };
    }

    #endregion

    #region EMAIL NOTIFICATIONS

    public async Task SendUnitBookingConfirmationEmailAsync(int bookingId)
    {
        _logger.LogInformation("Sending unit booking confirmation email for {BookingId}", bookingId);
    }

    public async Task SendUnitBookingStatusEmailAsync(int bookingId, string status)
    {
        _logger.LogInformation("Sending unit booking status email for {BookingId}: {Status}", bookingId, status);
    }

    public async Task SendUnitCheckoutEmailAsync(int bookingId)
    {
        _logger.LogInformation("Sending unit checkout email for {BookingId}", bookingId);
    }

    public async Task SendUnitCancellationEmailAsync(int bookingId, decimal refundAmount)
    {
        _logger.LogInformation("Sending unit cancellation email for {BookingId}", bookingId);
    }

    public async Task SendUnitPaymentConfirmationEmailAsync(int bookingId, decimal amount)
    {
        _logger.LogInformation("Sending unit payment confirmation email for {BookingId}", bookingId);
    }

    #endregion
}