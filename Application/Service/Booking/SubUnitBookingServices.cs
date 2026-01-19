using Application.Abstraction;
using Application.Contracts;
using Application.Contracts.Bookin;
using Application.Helpers;
using Application.Service.Availability;
using Application.Service.PromoCode;
using Domain;
using Domain.Entities;
using Hangfire;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Service.Booking;

public class SubUnitBookingService(
    ApplicationDbcontext context,
    IAvailabilityService availabilityService,
    ICouponService couponService,
    IEmailSender emailSender,
    ILogger<SubUnitBookingService> logger) : ISubUnitBookingServices
{
    private readonly ApplicationDbcontext _context = context;
    private readonly IAvailabilityService _availabilityService = availabilityService;
    private readonly ICouponService _couponService = couponService;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly ILogger<SubUnitBookingService> _logger = logger;

    #region CREATE BOOKING

    public async Task<Result<SubUnitBookingResponse>> CreateSubUnitBookingAsync(CreateSubUnitBookingRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Validate Unit exists
            var unit = await _context.Units
                .Include(u => u.Rooms)
                .Include(u => u.CancellationPolicy)
                .FirstOrDefaultAsync(u => u.Id == request.UnitId && !u.IsDeleted);

            if (unit == null)
                return Result.Failure<SubUnitBookingResponse>(
                    new Abstraction.Error("UnitNotFound", "Unit not found", 404));

            // 2. Validate dates
            if (request.CheckInDate >= request.CheckOutDate)
                return Result.Failure<SubUnitBookingResponse>(
                    new Abstraction.Error("InvalidDates", "Check-out must be after check-in", 400));

            if (request.CheckInDate < DateTime.UtcNow.Date)
                return Result.Failure<SubUnitBookingResponse>(
                    new Abstraction.Error("InvalidDates", "Cannot book past dates", 400));

            // 3. Validate subunits exist and belong to this unit
            var subUnits = await _context.SubUnits
                .Where(s => request.SubUnitIds.Contains(s.Id) &&
                           s.UnitId == request.UnitId &&
                           !s.IsDeleted)
                .ToListAsync();

            if (subUnits.Count != request.SubUnitIds.Count)
                return Result.Failure<SubUnitBookingResponse>(
                    new Abstraction.Error("InvalidSubUnits", "One or more subunits not found or don't belong to this unit", 400));

            // 4. Check availability for all requested subunits
            var availabilityCheck = await _availabilityService.CheckMultipleSubUnitsAvailabilityAsync(
                request.SubUnitIds,
                request.CheckInDate,
                request.CheckOutDate);

            if (!availabilityCheck.IsSuccess)
                return Result.Failure<SubUnitBookingResponse>(
                    new Abstraction.Error("NotAvailable", "One or more subunits are not available for the requested dates", 400));

            // 5. Calculate pricing
            var nights = (request.CheckOutDate - request.CheckInDate).Days;
            var originalPrice = await CalculateTotalPriceAsync(
                subUnits,
                request.CheckInDate,
                request.CheckOutDate);

            decimal finalPrice = originalPrice;
            decimal discountAmount = 0;
            Coupon? appliedCoupon = null;

            // 6. Validate and apply coupon if provided
            if (!string.IsNullOrWhiteSpace(request.CouponCode))
            {
                var couponValidation = await _couponService.ValidateCouponAsync(new ValidateCouponRequest(
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
                    return Result.Failure<SubUnitBookingResponse>(
                        new Abstraction.Error("InvalidCoupon",
                            couponValidation.Value?.ErrorMessage ?? "Coupon is not valid", 400));
                }

                discountAmount = couponValidation.Value.DiscountAmount;
                finalPrice = couponValidation.Value.FinalPrice;

                // Get coupon entity for later use
                appliedCoupon = await _context.Set<Coupon>()
                    .FirstOrDefaultAsync(c => c.Code.ToUpper() == request.CouponCode.ToUpper());
            }

            // 7. Generate unique booking number
            var bookingNumber = await GenerateBookingNumberAsync("ROOM");

            // 8. Create SubUnit booking
            var booking = new Domain.Entities.Booking
            {
                BookingNumber = bookingNumber,
                BookingType = BookingType.SubUnitBooking,
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

            // 9. Assign subunits to booking
            foreach (var subUnit in subUnits)
            {
                var bookingRoom = new BookingRoom
                {
                    BookingId = booking.Id,
                    RoomId = subUnit.Id,
                    PricePerNight = subUnit.PricePerNight,
                    NumberOfNights = nights
                };
                await _context.Set<BookingRoom>().AddAsync(bookingRoom);
            }

            await _context.SaveChangesAsync();

            // 10. Apply coupon to booking if validated
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

                // Update coupon usage count
                appliedCoupon.CurrentUsageCount++;

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Coupon {CouponCode} applied to booking {BookingNumber}. Discount: {Discount}",
                    appliedCoupon.Code, bookingNumber, discountAmount);
            }

            // 11. Mark dates as booked in availability service
            var markBooked = await _availabilityService.MarkDatesAsBookedAsync(new MarkDatesAsBookedRequest(
                BookingType.SubUnitBooking,
                SubUnitIds: request.SubUnitIds,
                BookingId: booking.Id,
                CheckInDate: request.CheckInDate,
                CheckOutDate: request.CheckOutDate
            ));

            if (!markBooked.IsSuccess)
            {
                await transaction.RollbackAsync();
                return Result.Failure<SubUnitBookingResponse>(
                    new Abstraction.Error("AvailabilityUpdateFailed", "Failed to update availability", 500));
            }

            await transaction.CommitAsync();

            // 12. Send confirmation email
            BackgroundJob.Enqueue(() => SendSubUnitBookingConfirmationEmailAsync(booking.Id));

            _logger.LogInformation(
                "SubUnit booking {BookingNumber} created for user {UserId} with {RoomCount} rooms. Original price: {OriginalPrice}, Final price: {FinalPrice}",
                bookingNumber, request.UserId, request.SubUnitIds.Count, originalPrice, finalPrice);

            // 13. Return response
            var response = await MapToResponseAsync(booking);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating subunit booking");
            return Result.Failure<SubUnitBookingResponse>(
                new Abstraction.Error("BookingFailed", "Failed to create subunit booking", 500));
        }
    }

    public async Task<Result<decimal>> CalculateSubUnitBookingPriceAsync(CalculateSubUnitBookingPriceRequest request)
    {
        var subUnits = await _context.SubUnits
            .Where(s => request.SubUnitIds.Contains(s.Id) && !s.IsDeleted)
            .ToListAsync();

        if (subUnits.Count != request.SubUnitIds.Count)
            return Result.Failure<decimal>(
                new Abstraction.Error("InvalidSubUnits", "One or more subunits not found", 404));

        var totalPrice = await CalculateTotalPriceAsync(
            subUnits,
            request.CheckInDate,
            request.CheckOutDate);

        return Result.Success(totalPrice);
    }

    #endregion

    #region BOOKING MANAGEMENT

    public async Task<Result> ConfirmSubUnitBookingAsync(int bookingId, string adminUserId)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.BookingType == BookingType.SubUnitBooking);

        if (booking == null)
            return Result.Failure(new Abstraction.Error("NotFound", "Booking not found", 404));

        if (booking.Status != BookingStatus.Pending)
            return Result.Failure(
                new Abstraction.Error("InvalidStatus", $"Cannot confirm booking with status {booking.Status}", 400));

        booking.Status = BookingStatus.Confirmed;
        booking.UpdatedAt = DateTime.UtcNow.AddHours(3);

        await _context.SaveChangesAsync();

        BackgroundJob.Enqueue(() => SendSubUnitBookingStatusEmailAsync(bookingId, "Confirmed"));

        _logger.LogInformation(
            "SubUnit booking {BookingId} confirmed by admin {AdminId}",
            bookingId, adminUserId);

        return Result.Success();
    }

    public async Task<Result> CheckInSubUnitAsync(int bookingId)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.BookingType == BookingType.SubUnitBooking);

        if (booking == null)
            return Result.Failure(new Abstraction.Error("NotFound", "Booking not found", 404));

        if (booking.Status != BookingStatus.Confirmed)
            return Result.Failure(
                new Abstraction.Error("InvalidStatus", "Only confirmed bookings can be checked in", 400));

        if (DateTime.UtcNow.Date < booking.CheckInDate.Date)
            return Result.Failure(
                new Abstraction.Error("TooEarly", "Check-in date has not arrived yet", 400));

        booking.Status = BookingStatus.CheckedIn;
        booking.UpdatedAt = DateTime.UtcNow.AddHours(3);

        await _context.SaveChangesAsync();

        BackgroundJob.Enqueue(() => SendSubUnitBookingStatusEmailAsync(bookingId, "CheckedIn"));

        return Result.Success();
    }

    public async Task<Result> CheckOutSubUnitAsync(int bookingId)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.BookingType == BookingType.SubUnitBooking);

        if (booking == null)
            return Result.Failure(new Abstraction.Error("NotFound", "Booking not found", 404));

        if (booking.Status != BookingStatus.CheckedIn)
            return Result.Failure(
                new Abstraction.Error("InvalidStatus", "Only checked-in bookings can be checked out", 400));

        booking.Status = BookingStatus.Completed;
        booking.UpdatedAt = DateTime.UtcNow.AddHours(3);

        await _context.SaveChangesAsync();

        BackgroundJob.Enqueue(() => SendSubUnitCheckoutEmailAsync(bookingId));

        return Result.Success();
    }

    public async Task<Result> CancelSubUnitBookingAsync(
        int bookingId,
        string cancellationReason,
        string userId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Unit)
                .ThenInclude(u => u.CancellationPolicy)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.BookingType == BookingType.SubUnitBooking);

        if (booking == null)
            return Result.Failure(new Abstraction.Error("NotFound", "Booking not found", 404));

        if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
            return Result.Failure(
                new Abstraction.Error("InvalidStatus", "Cannot cancel this booking", 400));

        // Calculate refund
        var refundAmount = CalculateRefundAmount(booking);

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationReason = cancellationReason;
        booking.CancelledAt = DateTime.UtcNow.AddHours(3);
        booking.UpdatedAt = DateTime.UtcNow.AddHours(3);

        await _context.SaveChangesAsync();

        // Remove coupon and restore usage count if coupon was applied
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
                RefundSubUnitBookingAsync(bookingId, refundAmount, "Booking cancelled"));
        }

        BackgroundJob.Enqueue(() => SendSubUnitCancellationEmailAsync(bookingId, refundAmount));

        _logger.LogInformation(
            "SubUnit booking {BookingId} cancelled by user {UserId}. Refund: {Refund}",
            bookingId, userId, refundAmount);

        return Result.Success();
    }

    #endregion

    #region PAYMENT OPERATIONS

    public async Task<Result> ProcessSubUnitPaymentAsync(int bookingId, ProcessPaymentRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.BookingType == BookingType.SubUnitBooking);

            if (booking == null)
                return Result.Failure(new Abstraction.Error("NotFound", "Booking not found", 404));

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

            BackgroundJob.Enqueue(() =>
                SendSubUnitPaymentConfirmationEmailAsync(bookingId, request.Amount));

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error processing payment for subunit booking {BookingId}", bookingId);
            return Result.Failure(new Abstraction.Error("PaymentFailed", "Failed to process payment", 500));
        }
    }

    public async Task<Result> RefundSubUnitBookingAsync(int bookingId, decimal refundAmount, string reason)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.BookingType == BookingType.SubUnitBooking);

        if (booking == null)
            return Result.Failure(new Abstraction.Error("NotFound", "Booking not found", 404));

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

    public async Task<Result<SubUnitBookingDetailsResponse>> GetSubUnitBookingByIdAsync(int bookingId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Unit)
            .Include(b => b.User)
            .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
            .Include(b => b.Payments)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.BookingType == BookingType.SubUnitBooking);

        if (booking == null)
            return Result.Failure<SubUnitBookingDetailsResponse>(
                new Abstraction.Error("NotFound", "Booking not found", 404));

        var response = MapToDetailsResponseAsync(booking);
        return Result.Success(await response.ConfigureAwait(false));
    }

    public async Task<Result<SubUnitBookingDetailsResponse>> GetSubUnitBookingByNumberAsync(string bookingNumber)
    {
        var booking = await _context.Bookings
            .Include(b => b.Unit)
            .Include(b => b.User)
            .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
            .Include(b => b.Payments)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.BookingNumber == bookingNumber && b.BookingType == BookingType.SubUnitBooking);

        if (booking == null)
            return Result.Failure<SubUnitBookingDetailsResponse>(
                new Abstraction.Error("NotFound", "Booking not found", 404));

        var response = MapToDetailsResponseAsync(booking);
        return Result.Success(await response.ConfigureAwait(false));
    }

    public async Task<Result<IEnumerable<SubUnitBookingResponse>>> GetUserSubUnitBookingsAsync(
        string userId,
        BookingFilter filter)
    {
        var query = _context.Bookings
            .Include(b => b.Unit)
            .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
            .Where(b => b.UserId == userId && b.BookingType == BookingType.SubUnitBooking)
            .AsQueryable();

        query = ApplyFilters(query, filter);

        var bookings = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var responses = new List<SubUnitBookingResponse>();
        foreach (var booking in bookings)
        {
            responses.Add(await MapToResponseAsync(booking));
        }

        return Result.Success<IEnumerable<SubUnitBookingResponse>>(responses);
    }

    public async Task<Result<IEnumerable<SubUnitBookingResponse>>> GetSubUnitBookingsForRoomAsync(
        int subUnitId,
        BookingFilter filter)
    {
        var query = _context.Bookings
            .Include(b => b.Unit)
            .Include(b => b.User)
            .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
            .Where(b => b.BookingType == BookingType.SubUnitBooking &&
                       b.BookingRooms.Any(br => br.RoomId == subUnitId))
            .AsQueryable();

        query = ApplyFilters(query, filter);

        var bookings = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var responses = new List<SubUnitBookingResponse>();
        foreach (var booking in bookings)
        {
            responses.Add(await MapToResponseAsync(booking));
        }

        return Result.Success<IEnumerable<SubUnitBookingResponse>>(responses);
    }

    public async Task<Result<IEnumerable<SubUnitBookingResponse>>> GetSubUnitBookingsForUnitAsync(
        int unitId,
        BookingFilter filter)
    {
        var query = _context.Bookings
            .Include(b => b.Unit)
            .Include(b => b.User)
            .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
            .Where(b => b.UnitId == unitId && b.BookingType == BookingType.SubUnitBooking)
            .AsQueryable();

        query = ApplyFilters(query, filter);

        var bookings = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var responses = new List<SubUnitBookingResponse>();
        foreach (var booking in bookings)
        {
            responses.Add(await MapToResponseAsync(booking));
        }

        return Result.Success<IEnumerable<SubUnitBookingResponse>>(responses);
    }

    #endregion

    #region STATISTICS

    public async Task<Result<SubUnitBookingStatisticsResponse>> GetSubUnitBookingStatisticsAsync(
        SubUnitBookingStatisticsFilter filter)
    {
        var query = _context.Bookings
            .Include(b => b.Unit)
            .Include(b => b.User)
            .Include(b => b.BookingRooms)
            .Where(b => b.BookingType == BookingType.SubUnitBooking)
            .AsQueryable();

        if (filter.StartDate.HasValue)
            query = query.Where(b => b.CreatedAt >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(b => b.CreatedAt <= filter.EndDate.Value);

        if (filter.UnitId.HasValue)
            query = query.Where(b => b.UnitId == filter.UnitId.Value);

        if (filter.SubUnitId.HasValue)
            query = query.Where(b => b.BookingRooms.Any(br => br.RoomId == filter.SubUnitId.Value));

        if (filter.Status.HasValue)
            query = query.Where(b => b.Status == filter.Status.Value);

        var bookings = await query.ToListAsync();

        var statistics = new SubUnitBookingStatisticsResponse
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
            TotalRoomsBooked = bookings.Sum(b => b.BookingRooms.Count),
            AverageRoomsPerBooking = bookings.Any()
                ? (double)bookings.Sum(b => b.BookingRooms.Count) / bookings.Count
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
                        Revenue = g.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.TotalPrice)
                    })
        };

        return Result.Success(statistics);
    }

    #endregion

    #region AVAILABILITY

    public async Task<Result<bool>> AreSubUnitsAvailableAsync(
        List<int> subUnitIds,
        DateTime checkIn,
        DateTime checkOut)
    {
        var availabilityCheck = await _availabilityService.CheckMultipleSubUnitsAvailabilityAsync(
            subUnitIds, checkIn, checkOut);

        if (!availabilityCheck.IsSuccess)
            return Result.Failure<bool>(availabilityCheck.Error);

        return Result.Success(availabilityCheck.Value.All(kvp => kvp.Value));
    }

    public async Task<Result<List<AvailableSubUnitInfo>>> GetAvailableSubUnitsAsync(
        int unitId,
        DateTime checkIn,
        DateTime checkOut,
        int requestedCount)
    {
        var availableIds = await _availabilityService.GetAvailableSubUnitIdsAsync(
            unitId, checkIn, checkOut);

        if (!availableIds.IsSuccess)
            return Result.Failure<List<AvailableSubUnitInfo>>(availableIds.Error);

        var subUnits = await _context.SubUnits
            .Where(s => availableIds.Value.Contains(s.Id))
            .Take(requestedCount)
            .ToListAsync();

        var result = subUnits.Select(s => new AvailableSubUnitInfo
        {
            Id = s.Id,
            RoomNumber = s.RoomNumber,
            TypeId = s.SubUnitTypeId,
            PricePerNight = s.PricePerNight,
            MaxOccupancy = s.MaxOccupancy,
            IsAvailable = true,
            SpecialPrice = null
        }).ToList();

        return Result.Success(result);
    }

    #endregion

    #region HELPER METHODS

    private async Task<string> GenerateBookingNumberAsync(string prefix)
    {
        var date = DateTime.UtcNow;
        var bookingPrefix = $"BK-{prefix}-{date:yyyyMMdd}";

        var lastBooking = await _context.Bookings
            .Where(b => b.BookingNumber.StartsWith(bookingPrefix) &&
                       b.BookingType == BookingType.SubUnitBooking)
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

    private async Task<decimal> CalculateTotalPriceAsync(
        List<Domain.Entities.SubUnit> subUnits,
        DateTime checkIn,
        DateTime checkOut)
    {
        decimal total = 0;
        var nights = (checkOut - checkIn).Days;

        foreach (var subUnit in subUnits)
        {
            var availability = await _context.Set<SubUnitAvailability>()
                .FirstOrDefaultAsync(a => a.SubUnitId == subUnit.Id &&
                                         checkIn >= a.StartDate &&
                                         checkOut <= a.EndDate);

            var calculator = new PricingCalculator();
            total += calculator.CalculatePrice(subUnit, checkIn, checkOut, availability);
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

    private IQueryable<Domain.Entities.Booking> ApplyFilters(IQueryable<Domain.Entities.Booking> query, BookingFilter filter)
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

    private async Task<SubUnitBookingResponse> MapToResponseAsync(Domain.Entities.Booking booking)
    {
        var subUnits = booking.BookingRooms.Select(br => new BookedSubUnitInfo
        {
            SubUnitId = br.RoomId,
            RoomNumber = br.Room.RoomNumber,
            PricePerNight = br.PricePerNight,
            NumberOfNights = br.NumberOfNights,
            SubTotal = br.PricePerNight * br.NumberOfNights
        }).ToList();

        // Get coupon information if applied
        var bookingCoupon = await _context.Set<BookingCoupon>()
            .Include(bc => bc.Coupon)
            .AsNoTracking()
            .FirstOrDefaultAsync(bc => bc.BookingId == booking.Id);

        return new SubUnitBookingResponse
        {
            Id = booking.Id,
            BookingNumber = booking.BookingNumber,
            UnitId = booking.UnitId,
            UnitName = booking.Unit?.Name ?? "",
            SubUnits = subUnits,
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

            // Coupon information
            AppliedCouponCode = bookingCoupon?.Coupon?.Code ?? "non",
            CouponDiscount = bookingCoupon?.DiscountApplied.ToString() ?? "non"
        };
    }

    private async Task<SubUnitBookingDetailsResponse> MapToDetailsResponseAsync(Domain.Entities.Booking booking)
    {
        var subUnits = booking.BookingRooms.Select(br => new BookedSubUnitInfo
        {
            SubUnitId = br.RoomId,
            RoomNumber = br.Room.RoomNumber,
            PricePerNight = br.PricePerNight,
            NumberOfNights = br.NumberOfNights,
            SubTotal = br.PricePerNight * br.NumberOfNights
        }).ToList();

        // Get coupon information if applied
        var bookingCoupon = await _context.Set<BookingCoupon>()
            .Include(bc => bc.Coupon)
            .AsNoTracking()
            .FirstOrDefaultAsync(bc => bc.BookingId == booking.Id);

        return new SubUnitBookingDetailsResponse
        {
            Id = booking.Id,
            BookingNumber = booking.BookingNumber,
            UnitId = booking.UnitId,
            UnitName = booking.Unit?.Name ?? "",
            UnitAddress = booking.Unit?.Address ?? "",
            SubUnits = subUnits,
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

            // Coupon information
            AppliedCouponCode = bookingCoupon?.Coupon?.Code,
            CouponDiscount = bookingCoupon?.DiscountApplied.ToString()
        };
    }

    //private async Task<SubUnitBookingResponse> MapToResponseAsync(Domain.Entities.Booking booking)
    //{
    //    var subUnits = booking.BookingRooms.Select(br => new BookedSubUnitInfo
    //    {
    //        SubUnitId = br.RoomId,
    //        RoomNumber = br.Room.RoomNumber,
    //        PricePerNight = br.PricePerNight,
    //        NumberOfNights = br.NumberOfNights,
    //        SubTotal = br.PricePerNight * br.NumberOfNights
    //    }).ToList();

    //    return new SubUnitBookingResponse
    //    {
    //        Id = booking.Id,
    //        BookingNumber = booking.BookingNumber,
    //        UnitId = booking.UnitId,
    //        UnitName = booking.Unit?.Name ?? "",
    //        SubUnits = subUnits,
    //        UserId = booking.UserId,
    //        UserName = booking.User?.FullName ?? "",
    //        CheckInDate = booking.CheckInDate,
    //        CheckOutDate = booking.CheckOutDate,
    //        NumberOfGuests = booking.NumberOfGuests,
    //        NumberOfNights = booking.NumberOfNights,
    //        TotalPrice = booking.TotalPrice,
    //        PaidAmount = booking.PaidAmount,
    //        Status = booking.Status,
    //        PaymentStatus = booking.PaymentStatus,
    //        CreatedAt = booking.CreatedAt
    //    };
    //}

    //private SubUnitBookingDetailsResponse MapToDetailsResponse(Domain.Entities.Booking booking)
    //{
    //    var subUnits = booking.BookingRooms.Select(br => new BookedSubUnitInfo
    //    {
    //        SubUnitId = br.RoomId,
    //        RoomNumber = br.Room.RoomNumber,
    //        PricePerNight = br.PricePerNight,
    //        NumberOfNights = br.NumberOfNights,
    //        SubTotal = br.PricePerNight * br.NumberOfNights
    //    }).ToList();

    //    return new SubUnitBookingDetailsResponse
    //    {
    //        Id = booking.Id,
    //        BookingNumber = booking.BookingNumber,
    //        UnitId = booking.UnitId,
    //        UnitName = booking.Unit?.Name ?? "",
    //        UnitAddress = booking.Unit?.Address ?? "",
    //        SubUnits = subUnits,
    //        UserId = booking.UserId,
    //        UserName = booking.User?.FullName ?? "",
    //        UserEmail = booking.User?.Email ?? "",
    //        UserPhone = booking.User?.PhoneNumber,
    //        CheckInDate = booking.CheckInDate,
    //        CheckOutDate = booking.CheckOutDate,
    //        NumberOfGuests = booking.NumberOfGuests,
    //        NumberOfNights = booking.NumberOfNights,
    //        TotalPrice = booking.TotalPrice,
    //        PaidAmount = booking.PaidAmount,
    //        Status = booking.Status,
    //        PaymentStatus = booking.PaymentStatus,
    //        SpecialRequests = booking.SpecialRequests,
    //        CancellationReason = booking.CancellationReason,
    //        CancelledAt = booking.CancelledAt,
    //        Payments = booking.Payments?.Select(p => new PaymentInfo
    //        {
    //            Id = p.Id,
    //            Amount = p.Amount,
    //            PaymentMethod = p.PaymentMethod.ToString(),
    //            Status = p.Status.ToString(),
    //            PaymentDate = p.PaymentDate
    //        }).ToList() ?? new List<PaymentInfo>(),
    //        UpdatedAt = booking.UpdatedAt
    //    };
    //}

    #endregion

    #region EMAIL NOTIFICATIONS

    public async Task SendSubUnitBookingConfirmationEmailAsync(int bookingId)
    {
        _logger.LogInformation("Sending subunit booking confirmation email for {BookingId}", bookingId);
    }

    public async Task SendSubUnitBookingStatusEmailAsync(int bookingId, string status)
    {
        _logger.LogInformation("Sending subunit booking status email for {BookingId}: {Status}", bookingId, status);
    }

    public async Task SendSubUnitCheckoutEmailAsync(int bookingId)
    {
        _logger.LogInformation("Sending subunit checkout email for {BookingId}", bookingId);
    }

    public async Task SendSubUnitCancellationEmailAsync(int bookingId, decimal refundAmount)
    {
        _logger.LogInformation("Sending subunit cancellation email for {BookingId}", bookingId);
    }

    public async Task SendSubUnitPaymentConfirmationEmailAsync(int bookingId, decimal amount)
    {
        _logger.LogInformation("Sending subunit payment confirmation email for {BookingId}", bookingId);
    }

    #endregion
}