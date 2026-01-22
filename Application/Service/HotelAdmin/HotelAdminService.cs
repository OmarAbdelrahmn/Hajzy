using Application.Abstraction;
using Application.Contracts.Bookin;
using Application.Contracts.hoteladmincont;
using Application.Contracts.Unit;
using Application.Service.Availability;
using Application.Service.Booking;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Service.HotelAdmin;

public class HotelAdminService(
    ApplicationDbcontext context,
    ILogger<HotelAdminService> logger) : IHotelAdminService
{
    private readonly ApplicationDbcontext _context = context;
    private readonly ILogger<HotelAdminService> _logger = logger;

    #region DASHBOARD & OVERVIEW

    public async Task<Result<HotelAdminDashboardResponse>> GetDashboardAsync(string userId)
    {
        try
        {
            // Verify user is a hotel admin
            var adminUnits = await GetUserAdminUnitsAsync(userId);
            if (!adminUnits.Any())
                return Result.Failure<HotelAdminDashboardResponse>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var unitIds = adminUnits.Select(u => u.Id).ToList();
            var today = DateTime.UtcNow.Date;

            // Get unit summaries
            var unitSummaries = adminUnits.Select(u => new UnitSummary
            {
                Id = u.Id,
                Name = u.Name,
                IsActive = u.IsActive,
                TotalSubUnits = u.Rooms.Count(r => !r.IsDeleted),
                AvailableSubUnits = u.Rooms.Count(r => !r.IsDeleted && r.IsAvailable),
                ActiveBookings = u.Bookings.Count(b =>
                    b.Status == BookingStatus.Confirmed ||
                    b.Status == BookingStatus.CheckedIn),
                MonthlyRevenue = u.Bookings
                    .Where(b => b.CreatedAt.Month == DateTime.UtcNow.Month &&
                               b.Status == BookingStatus.Completed)
                    .Sum(b => b.TotalPrice),
                AverageRating = u.AverageRating
            }).ToList();

            // Get dashboard stats
            var allBookings = await _context.Bookings
                .Where(b => unitIds.Contains(b.UnitId))
                .Include(b => b.BookingRooms)
                .ToListAsync();

            var stats = new DashboardStats
            {
                TodayCheckIns = allBookings.Count(b => b.CheckInDate.Date == today &&
                    b.Status == BookingStatus.Confirmed),
                TodayCheckOuts = allBookings.Count(b => b.CheckOutDate.Date == today &&
                    b.Status == BookingStatus.CheckedIn),
                CurrentGuests = allBookings.Count(b => b.Status == BookingStatus.CheckedIn),
                PendingBookings = allBookings.Count(b => b.Status == BookingStatus.Pending),
                UnreadReviews = await _context.Reviews
                    .CountAsync(r => unitIds.Contains(r.UnitId) && r.OwnerResponse == null),
                TodayRevenue = allBookings
                    .Where(b => b.CreatedAt.Date == today && b.Status == BookingStatus.Completed)
                    .Sum(b => b.TotalPrice),
                WeekRevenue = allBookings
                    .Where(b => b.CreatedAt >= today.AddDays(-7) && b.Status == BookingStatus.Completed)
                    .Sum(b => b.TotalPrice),
                MonthRevenue = allBookings
                    .Where(b => b.CreatedAt.Month == today.Month && b.Status == BookingStatus.Completed)
                    .Sum(b => b.TotalPrice)
            };

            // Get recent bookings
            var recentBookings = await _context.Bookings
                .Where(b => unitIds.Contains(b.UnitId))
                .Include(b => b.Unit)
                .Include(b => b.User)
                .OrderByDescending(b => b.CreatedAt)
                .Take(10)
                .Select(b => new RecentBooking
                {
                    Id = b.Id,
                    BookingNumber = b.BookingNumber,
                    UnitName = b.Unit.Name,
                    GuestName = b.User.FullName ?? "N/A",
                    CheckInDate = b.CheckInDate,
                    CheckOutDate = b.CheckOutDate,
                    Status = b.Status.ToString(),
                    TotalPrice = b.TotalPrice,
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();

            // Get today's check-ins
            var todayCheckIns = await GetUpcomingCheckInsAsync(userId, 0);

            // Get today's check-outs
            var todayCheckOuts = await GetUpcomingCheckOutsAsync(userId, 0);

            // Get revenue overview
            var monthlyRevenue = allBookings
                .Where(b => b.Status == BookingStatus.Completed)
                .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                .OrderByDescending(g => g.Key.Year)
                .ThenByDescending(g => g.Key.Month)
                .Take(12)
                .Select(g => new MonthlyRevenueData
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Revenue = g.Sum(b => b.TotalPrice),
                    BookingCount = g.Count()
                })
                .ToList();

            var revenueOverview = new RevenueOverview
            {
                TodayRevenue = stats.TodayRevenue,
                WeekRevenue = stats.WeekRevenue,
                MonthRevenue = stats.MonthRevenue,
                YearRevenue = allBookings
                    .Where(b => b.CreatedAt.Year == today.Year && b.Status == BookingStatus.Completed)
                    .Sum(b => b.TotalPrice),
                PendingPayments = allBookings
                    .Where(b => b.PaymentStatus == PaymentStatus.Pending ||
                               b.PaymentStatus == PaymentStatus.PartiallyPaid)
                    .Sum(b => b.TotalPrice - b.PaidAmount),
                MonthlyTrend = monthlyRevenue
            };

            var dashboard = new HotelAdminDashboardResponse
            {
                Units = unitSummaries,
                Stats = stats,
                RecentBookings = recentBookings,
                TodayCheckIns = todayCheckIns.Value?.ToList() ?? [],
                TodayCheckOuts = todayCheckOuts.Value?.ToList() ?? [],
                RevenueOverview = revenueOverview
            };

            return Result.Success(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard for user {UserId}", userId);
            return Result.Failure<HotelAdminDashboardResponse>(
                new Error("DashboardFailed", "Failed to load dashboard", 500));
        }
    }

    public async Task<Result<HotelAdminStatsResponse>> GetQuickStatsAsync(string userId)
    {
        try
        {
            var adminUnits = await GetUserAdminUnitsAsync(userId);
            if (!adminUnits.Any())
                return Result.Failure<HotelAdminStatsResponse>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var unitIds = adminUnits.Select(u => u.Id).ToList();

            var stats = new HotelAdminStatsResponse
            {
                TotalUnits = adminUnits.Count,
                ActiveUnits = adminUnits.Count(u => u.IsActive),
                TotalSubUnits = adminUnits.Sum(u => u.Rooms.Count(r => !r.IsDeleted)),
                AvailableSubUnits = adminUnits.Sum(u => u.Rooms.Count(r => !r.IsDeleted && r.IsAvailable)),
                TotalBookings = await _context.Bookings.CountAsync(b => unitIds.Contains(b.UnitId)),
                ActiveBookings = await _context.Bookings
                    .CountAsync(b => unitIds.Contains(b.UnitId) &&
                        (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.CheckedIn)),
                PendingBookings = await _context.Bookings
                    .CountAsync(b => unitIds.Contains(b.UnitId) && b.Status == BookingStatus.Pending),
                TotalRevenue = await _context.Bookings
                    .Where(b => unitIds.Contains(b.UnitId) && b.Status == BookingStatus.Completed)
                    .SumAsync(b => b.TotalPrice),
                PendingRevenue = await _context.Bookings
                    .Where(b => unitIds.Contains(b.UnitId) &&
                        (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed))
                    .SumAsync(b => b.TotalPrice),
                AverageRating = adminUnits.Average(u => u.AverageRating),
                TotalReviews = adminUnits.Sum(u => u.TotalReviews),
                OccupancyRate = await CalculateOccupancyRateAsync(unitIds,
                    DateTime.UtcNow.AddDays(-30), DateTime.UtcNow)
            };

            return Result.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quick stats for user {UserId}", userId);
            return Result.Failure<HotelAdminStatsResponse>(
                new Error("StatsFailed", "Failed to load statistics", 500));
        }
    }

    #endregion

    #region UNIT MANAGEMENT

    public async Task<Result<IEnumerable<UnitComprehensiveResponse>>> GetMyUnitsAsync(
        string userId,
        UnitFilter filter)
    {
        try
        {
            var adminUnits = await GetUserAdminUnitsAsync(userId);
            if (!adminUnits.Any())
                return Result.Failure<IEnumerable<UnitComprehensiveResponse>>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var query = adminUnits.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.Name))
                query = query.Where(u => u.Name.Contains(filter.Name));

            if (filter.IsActive.HasValue)
                query = query.Where(u => u.IsActive == filter.IsActive.Value);

            if (filter.IsVerified.HasValue)
                query = query.Where(u => u.IsVerified == filter.IsVerified.Value);

            var units = query.ToList();

            // Get general policies
            var unitIds = units.Select(u => u.Id).ToList();
            var generalPolicies = await _context.GeneralPolicies
                .Include(p => p.CancellationPolicy)
                .Where(p => unitIds.Contains(p.UnitId.Value) && p.IsActive)
                .AsNoTracking()
                .ToListAsync();

            var responses = units.Select(u => MapToComprehensiveResponse(u,
                generalPolicies.Where(p => p.UnitId == u.Id).ToList())).ToList();

            return Result.Success<IEnumerable<UnitComprehensiveResponse>>(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting units for user {UserId}", userId);
            return Result.Failure<IEnumerable<UnitComprehensiveResponse>>(
                new Error("GetUnitsFailed", "Failed to retrieve units", 500));
        }
    }

    public async Task<Result<UnitComprehensiveResponse>> GetMyUnitDetailsAsync(
        string userId,
        int unitId)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure<UnitComprehensiveResponse>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var unit = await _context.Units
                .Include(u => u.City)
                .Include(u => u.UnitType)
                .Include(u => u.CancellationPolicy)
                .Include(u => u.Admins.Where(a => a.IsActive))
                    .ThenInclude(a => a.User)
                .Include(u => u.Images.Where(i => !i.IsDeleted))
                .Include(u => u.UnitAmenities)
                    .ThenInclude(ua => ua.Amenity)
                .Include(u => u.Rooms.Where(r => !r.IsDeleted))
                    .ThenInclude(r => r.SubUnitImages.Where(i => !i.IsDeleted))
                .Include(u => u.Rooms.Where(r => !r.IsDeleted))
                    .ThenInclude(r => r.SubUnitAmenities)
                    .ThenInclude(sa => sa.Amenity)
                .Include(u => u.Rooms.Where(r => !r.IsDeleted))
                    .ThenInclude(r => r.SubUnitAvailabilities)
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

            if (unit == null)
                return Result.Failure<UnitComprehensiveResponse>(
                    new Error("NotFound", "Unit not found", 404));

            var generalPolicies = await _context.GeneralPolicies
                .Include(p => p.CancellationPolicy)
                .Where(p => p.UnitId == unitId && p.IsActive)
                .AsNoTracking()
                .ToListAsync();

            var response = MapToComprehensiveResponse(unit, generalPolicies);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unit {UnitId} for user {UserId}", unitId, userId);
            return Result.Failure<UnitComprehensiveResponse>(
                new Error("GetUnitFailed", "Failed to retrieve unit details", 500));
        }
    }

    public async Task<Result<UnitResponse>> UpdateMyUnitAsync(
        string userId,
        int unitId,
        UpdateUnitRequest request)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure<UnitResponse>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var unit = await _context.Units
                .Include(u => u.City)
                .Include(u => u.UnitType)
                .Include(u => u.Images.Where(i => !i.IsDeleted))
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

            if (unit == null)
                return Result.Failure<UnitResponse>(
                    new Error("NotFound", "Unit not found", 404));

            // Update allowed fields
            if (request.Name != null) unit.Name = request.Name;
            if (request.Description != null) unit.Description = request.Description;
            if (request.Address != null) unit.Address = request.Address;
            if (request.BasePrice.HasValue) unit.BasePrice = request.BasePrice.Value;
            if (request.MaxGuests.HasValue) unit.MaxGuests = request.MaxGuests.Value;
            if (request.Bedrooms.HasValue) unit.Bedrooms = request.Bedrooms.Value;
            if (request.Bathrooms.HasValue) unit.Bathrooms = request.Bathrooms.Value;

            unit.UpdatedAt = DateTime.UtcNow.AddHours(3);

            await _context.SaveChangesAsync();

            var response = MapToUnitResponse(unit);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating unit {UnitId}", unitId);
            return Result.Failure<UnitResponse>(
                new Error("UpdateFailed", "Failed to update unit", 500));
        }
    }

    #endregion

    #region BOOKING MANAGEMENT

    public async Task<Result<IEnumerable<BookingComprehensiveResponse>>> GetMyUnitBookingsAsync(
        string userId,
        BookingFilter filter)
    {
        try
        {
            var adminUnits = await GetUserAdminUnitsAsync(userId);
            if (!adminUnits.Any())
                return Result.Failure<IEnumerable<BookingComprehensiveResponse>>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var unitIds = adminUnits.Select(u => u.Id).ToList();

            var query = _context.Bookings
                .Include(b => b.Unit)
                .Include(b => b.User)
                .Include(b => b.BookingRooms)
                    .ThenInclude(br => br.Room)
                .Where(b => unitIds.Contains(b.UnitId))
                .AsQueryable();

            // Apply filters
            if (filter.Status.HasValue)
                query = query.Where(b => b.Status == filter.Status.Value);

            if (filter.PaymentStatus.HasValue)
                query = query.Where(b => b.PaymentStatus == filter.PaymentStatus.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(b => b.CheckInDate >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(b => b.CheckOutDate <= filter.EndDate.Value);

            var bookings = await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var responses = await Task.WhenAll(
                bookings.Select(async b => await MapToBookingComprehensiveResponseAsync(b)));

            return Result.Success<IEnumerable<BookingComprehensiveResponse>>(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bookings for user {UserId}", userId);
            return Result.Failure<IEnumerable<BookingComprehensiveResponse>>(
                new Error("GetBookingsFailed", "Failed to retrieve bookings", 500));
        }
    }

    public async Task<Result<BookingDetailsResponse>> GetMyBookingDetailsAsync(
        string userId,
        int bookingId)
    {
        try
        {
            var hasAccess = await IsBookingForMyUnitAsync(userId, bookingId);
            if (!hasAccess.Value)
                return Result.Failure<BookingDetailsResponse>(
                    new Error("NoAccess", "You do not have access to this booking", 403));

            var booking = await _context.Bookings
                .Include(b => b.Unit)
                .Include(b => b.User)
                .Include(b => b.BookingRooms)
                    .ThenInclude(br => br.Room)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return Result.Failure<BookingDetailsResponse>(
                    new Error("NotFound", "Booking not found", 404));

            var response = await MapToBookingDetailsResponseAsync(booking);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting booking {BookingId}", bookingId);
            return Result.Failure<BookingDetailsResponse>(
                new Error("GetBookingFailed", "Failed to retrieve booking details", 500));
        }
    }

    public async Task<Result<BookingDetailsResponse>> GetMyBookingByNumberAsync(
        string userId,
        string bookingNumber)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Unit)
                .Include(b => b.User)
                .Include(b => b.BookingRooms)
                    .ThenInclude(br => br.Room)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.BookingNumber == bookingNumber);

            if (booking == null)
                return Result.Failure<BookingDetailsResponse>(
                    new Error("NotFound", "Booking not found", 404));

            var hasAccess = await IsBookingForMyUnitAsync(userId, booking.Id);
            if (!hasAccess.Value)
                return Result.Failure<BookingDetailsResponse>(
                    new Error("NoAccess", "You do not have access to this booking", 403));

            var response = await MapToBookingDetailsResponseAsync(booking);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting booking by number {BookingNumber}", bookingNumber);
            return Result.Failure<BookingDetailsResponse>(
                new Error("GetBookingFailed", "Failed to retrieve booking details", 500));
        }
    }

    public async Task<Result> ConfirmBookingAsync(string userId, int bookingId)
    {
        try
        {
            var hasAccess = await IsBookingForMyUnitAsync(userId, bookingId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this booking", 403));

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

            _logger.LogInformation(
                "Booking {BookingId} confirmed by hotel admin {UserId}",
                bookingId, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming booking {BookingId}", bookingId);
            return Result.Failure(new Error("ConfirmFailed", "Failed to confirm booking", 500));
        }
    }

    public async Task<Result> CheckInBookingAsync(string userId, int bookingId)
    {
        try
        {
            var hasAccess = await IsBookingForMyUnitAsync(userId, bookingId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this booking", 403));

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return Result.Failure(new Error("NotFound", "Booking not found", 404));

            if (booking.Status != BookingStatus.Confirmed)
                return Result.Failure(
                    new Error("InvalidStatus", "Only confirmed bookings can be checked in", 400));

            booking.Status = BookingStatus.CheckedIn;
            booking.UpdatedAt = DateTime.UtcNow.AddHours(3);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Booking {BookingId} checked in by hotel admin {UserId}",
                bookingId, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking in booking {BookingId}", bookingId);
            return Result.Failure(new Error("CheckInFailed", "Failed to check in booking", 500));
        }
    }

    public async Task<Result> CheckOutBookingAsync(string userId, int bookingId)
    {
        try
        {
            var hasAccess = await IsBookingForMyUnitAsync(userId, bookingId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this booking", 403));

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

            _logger.LogInformation(
                "Booking {BookingId} checked out by hotel admin {UserId}",
                bookingId, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking out booking {BookingId}", bookingId);
            return Result.Failure(new Error("CheckOutFailed", "Failed to check out booking", 500));
        }
    }

    public async Task<Result> CancelBookingAsync(
        string userId,
        int bookingId,
        string cancellationReason)
    {
        try
        {
            var hasAccess = await IsBookingForMyUnitAsync(userId, bookingId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this booking", 403));

            var booking = await _context.Bookings
                .Include(b => b.Unit)
                    .ThenInclude(u => u.CancellationPolicy)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return Result.Failure(new Error("NotFound", "Booking not found", 404));

            if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
                return Result.Failure(
                    new Error("InvalidStatus", "Cannot cancel this booking", 400));

            booking.Status = BookingStatus.Cancelled;
            booking.CancellationReason = cancellationReason;
            booking.CancelledAt = DateTime.UtcNow.AddHours(3);
            booking.UpdatedAt = DateTime.UtcNow.AddHours(3);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Booking {BookingId} cancelled by hotel admin {UserId}",
                bookingId, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling booking {BookingId}", bookingId);
            return Result.Failure(new Error("CancelFailed", "Failed to cancel booking", 500));
        }
    }

    #endregion

    // Continued in next artifact...


    private async Task<List<Domain.Entities.Unit>> GetUserAdminUnitsAsync(string userId)
    {
        return await _context.Units
            .Include(u => u.City)
            .Include(u => u.UnitType)
            .Include(u => u.CancellationPolicy)
            .Include(u => u.Admins.Where(a => a.IsActive))
                .ThenInclude(a => a.User)
            .Include(u => u.Images.Where(i => !i.IsDeleted))
            .Include(u => u.UnitAmenities)
                .ThenInclude(ua => ua.Amenity)
            .Include(u => u.Rooms.Where(r => !r.IsDeleted))
                .ThenInclude(r => r.SubUnitImages.Where(i => !i.IsDeleted))
            .Include(u => u.Rooms.Where(r => !r.IsDeleted))
                .ThenInclude(r => r.SubUnitAmenities)
                .ThenInclude(sa => sa.Amenity)
            .Include(u => u.Rooms.Where(r => !r.IsDeleted))
                .ThenInclude(r => r.SubUnitAvailabilities)
            .Include(u => u.Bookings)
            .Where(u => !u.IsDeleted && u.Admins.Any(a => a.UserId == userId && a.IsActive))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Result<bool>> IsAdminOfUnitAsync(string userId, int unitId)
    {
        var isAdmin = await _context.Set<UniteAdmin>()
            .AnyAsync(a => a.UnitId == unitId && a.UserId == userId && a.IsActive);

        return Result.Success(isAdmin);
    }

    public async Task<Result<bool>> IsBookingForMyUnitAsync(string userId, int bookingId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Unit)
                .ThenInclude(u => u.Admins)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            return Result.Success(false);

        var isAdmin = booking.Unit.Admins.Any(a => a.UserId == userId && a.IsActive);
        return Result.Success(isAdmin);
    }

    private async Task<decimal> CalculateOccupancyRateAsync(
        List<int> unitIds,
        DateTime startDate,
        DateTime endDate)
    {
        var totalRoomNights = await _context.SubUnits
            .Where(s => unitIds.Contains(s.UnitId) && !s.IsDeleted)
            .CountAsync() * (endDate - startDate).Days;

        if (totalRoomNights == 0)
            return 0;

        var occupiedNights = await _context.BookingRooms
            .Include(br => br.Booking)
            .Where(br => unitIds.Contains(br.Room.UnitId) &&
                        br.Booking.CheckInDate < endDate &&
                        br.Booking.CheckOutDate > startDate &&
                        br.Booking.Status != BookingStatus.Cancelled)
            .SumAsync(br => br.NumberOfNights);

        return (decimal)occupiedNights / totalRoomNights * 100;
    }


public Task<Result<IEnumerable<SubUnitComprehensiveDetail>>> GetMySubUnitsAsync(string userId, int? unitId = null)
    {
        throw new NotImplementedException();
    }

    public Task<Result<SubUnitComprehensiveDetail>> GetMySubUnitDetailsAsync(string userId, int subUnitId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<AvailabilityCalendarResponse>> GetMyUnitsAvailabilityAsync(string userId, int unitId, DateTime startDate, DateTime endDate)
    {
        throw new NotImplementedException();
    }

    public Task<Result> UpdateSubUnitAvailabilityAsync(string userId, int subUnitId, UpdateAvailabilityRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<Result<RevenueReportResponse>> GetRevenueReportAsync(string userId, RevenueReportFilter filter)
    {
        throw new NotImplementedException();
    }

    public Task<Result<BookingAnalyticsResponse>> GetBookingAnalyticsAsync(string userId, AnalyticsFilter filter)
    {
        throw new NotImplementedException();
    }

    public Task<Result<OccupancyReportResponse>> GetOccupancyReportAsync(string userId, int? unitId, DateTime startDate, DateTime endDate)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IEnumerable<ReviewResponse>>> GetMyUnitsReviewsAsync(string userId, ReviewFilter filter)
    {
        throw new NotImplementedException();
    }

    public Task<Result> RespondToReviewAsync(string userId, int reviewId, string response)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IEnumerable<UpcomingCheckInResponse>>> GetUpcomingCheckInsAsync(string userId, int days = 1)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IEnumerable<UpcomingCheckOutResponse>>> GetUpcomingCheckOutsAsync(string userId, int days = 1)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IEnumerable<PaymentHistoryResponse>>> GetPaymentHistoryAsync(string userId, PaymentFilter filter)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IEnumerable<PendingPaymentResponse>>> GetPendingPaymentsAsync(string userId)
    {
        throw new NotImplementedException();
    }
}
