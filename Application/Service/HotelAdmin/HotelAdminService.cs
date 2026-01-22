using Application.Abstraction;
using Application.Contracts.hoteladmincont;
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
            var today = DateTime.UtcNow.AddHours(3).Date;

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
                    DateTime.UtcNow.AddHours(3).AddDays(-30), DateTime.UtcNow.AddHours(3))
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


    // Add these implementations to HotelAdminService.cs

    #region SUBUNIT MANAGEMENT

    public async Task<Result<IEnumerable<SubUnitComprehensiveDetail>>> GetMySubUnitsAsync(
        string userId,
        int? unitId = null)
    {
        try
        {
            var adminUnits = await GetUserAdminUnitsAsync(userId);
            if (!adminUnits.Any())
                return Result.Failure<IEnumerable<SubUnitComprehensiveDetail>>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var unitIds = unitId.HasValue
                ? new List<int> { unitId.Value }
                : adminUnits.Select(u => u.Id).ToList();

            // Verify access if specific unit requested
            if (unitId.HasValue && !unitIds.Contains(unitId.Value))
                return Result.Failure<IEnumerable<SubUnitComprehensiveDetail>>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var subUnits = await _context.SubUnits
                .Include(s => s.Unit)
                .Include(s => s.SubUnitType)
                .Include(s => s.SubUnitImages.Where(i => !i.IsDeleted))
                .Include(s => s.SubUnitAmenities)
                    .ThenInclude(sa => sa.Amenity)
                .Include(s => s.SubUnitAvailabilities)
                .Where(s => unitIds.Contains(s.UnitId) && !s.IsDeleted)
                .ToListAsync();

            var responses = subUnits.Select(MapToSubUnitComprehensiveDetail).ToList();
            return Result.Success<IEnumerable<SubUnitComprehensiveDetail>>(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subunits for user {UserId}", userId);
            return Result.Failure<IEnumerable<SubUnitComprehensiveDetail>>(
                new Error("GetSubUnitsFailed", "Failed to retrieve subunits", 500));
        }
    }

    public async Task<Result<SubUnitComprehensiveDetail>> GetMySubUnitDetailsAsync(
        string userId,
        int subUnitId)
    {
        try
        {
            var subUnit = await _context.SubUnits
                .Include(s => s.Unit)
                .Include(s => s.SubUnitType)
                .Include(s => s.SubUnitImages.Where(i => !i.IsDeleted))
                .Include(s => s.SubUnitAmenities)
                    .ThenInclude(sa => sa.Amenity)
                .Include(s => s.SubUnitAvailabilities)
                .Include(s => s.BookingRooms)
                    .ThenInclude(br => br.Booking)
                .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

            if (subUnit == null)
                return Result.Failure<SubUnitComprehensiveDetail>(
                    new Error("NotFound", "SubUnit not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(userId, subUnit.UnitId);
            if (!hasAccess.Value)
                return Result.Failure<SubUnitComprehensiveDetail>(
                    new Error("NoAccess", "You do not have access to this subunit", 403));

            var response = MapToSubUnitComprehensiveDetail(subUnit);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subunit {SubUnitId}", subUnitId);
            return Result.Failure<SubUnitComprehensiveDetail>(
                new Error("GetSubUnitFailed", "Failed to retrieve subunit details", 500));
        }
    }

    #endregion

    #region AVAILABILITY MANAGEMENT

    public async Task<Result<AvailabilityCalendarResponse>> GetMyUnitsAvailabilityAsync(
        string userId,
        int unitId,
        DateTime startDate,
        DateTime endDate)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure<AvailabilityCalendarResponse>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var subUnits = await _context.SubUnits
                .Include(s => s.SubUnitAvailabilities.Where(a =>
                    a.StartDate <= endDate && a.EndDate >= startDate))
                .Include(s => s.BookingRooms)
                    .ThenInclude(br => br.Booking)
                .Where(s => s.UnitId == unitId && !s.IsDeleted)
                .ToListAsync();

            var calendar = new AvailabilityCalendarResponse
            {
                UnitId = unitId,
                StartDate = startDate,
                EndDate = endDate,
                SubUnits = subUnits.Select(s => new SubUnitAvailabilityInfo
                {
                    SubUnitId = s.Id,
                    RoomNumber = s.RoomNumber,
                    IsAvailable = s.IsAvailable,
                    AvailabilityPeriods = s.SubUnitAvailabilities
                        .Select(a => new AvailabilityPeriod
                        {
                            StartDate = a.StartDate,
                            EndDate = a.EndDate,
                            IsAvailable = a.IsAvailable,
                            Reason = a.Reason?.ToString(),
                            SpecialPrice = a.SpecialPrice,
                            WeekendPrice = a.WeekendPrice
                        }).ToList(),
                    Bookings = s.BookingRooms
                        .Where(br => br.Booking.CheckInDate <= endDate &&
                                    br.Booking.CheckOutDate >= startDate &&
                                    br.Booking.Status != BookingStatus.Cancelled)
                        .Select(br => new BookingPeriod
                        {
                            BookingId = br.BookingId,
                            CheckInDate = br.Booking.CheckInDate,
                            CheckOutDate = br.Booking.CheckOutDate,
                            Status = br.Booking.Status.ToString()
                        }).ToList()
                }).ToList()
            };

            return Result.Success(calendar);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting availability for unit {UnitId}", unitId);
            return Result.Failure<AvailabilityCalendarResponse>(
                new Error("GetAvailabilityFailed", "Failed to retrieve availability", 500));
        }
    }

    public async Task<Result> UpdateSubUnitAvailabilityAsync(
        string userId,
        int subUnitId,
        UpdateAvailabilityRequest request)
    {
        try
        {
            var subUnit = await _context.SubUnits
                .Include(s => s.Unit)
                .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

            if (subUnit == null)
                return Result.Failure(new Error("NotFound", "SubUnit not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(userId, subUnit.UnitId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this subunit", 403));

            // Check for overlapping bookings
            var hasBookings = await _context.BookingRooms
                .Include(br => br.Booking)
                .AnyAsync(br => br.RoomId == subUnitId &&
                               br.Booking.CheckInDate < request.EndDate &&
                               br.Booking.CheckOutDate > request.StartDate &&
                               br.Booking.Status != BookingStatus.Cancelled);

            if (hasBookings && !request.IsAvailable)
                return Result.Failure(
                    new Error("HasBookings", "Cannot mark as unavailable - active bookings exist", 400));

            var availability = new SubUnitAvailability
            {
                SubUnitId = subUnitId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsAvailable = request.IsAvailable,
                Reason = request.Reason,
                SpecialPrice = request.SpecialPrice,
                WeekendPrice = request.WeekendPrice,
                UpdatedByUserId = userId,
                CreatedAt = DateTime.UtcNow.AddHours(3)
            };

            _context.Set<SubUnitAvailability>().Add(availability);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Availability updated for subunit {SubUnitId} by user {UserId}",
                subUnitId, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating availability for subunit {SubUnitId}", subUnitId);
            return Result.Failure(
                new Error("UpdateAvailabilityFailed", "Failed to update availability", 500));
        }
    }

    #endregion

    #region REVENUE & ANALYTICS

    public async Task<Result<RevenueReportResponse>> GetRevenueReportAsync(
        string userId,
        RevenueReportFilter filter)
    {
        try
        {
            var adminUnits = await GetUserAdminUnitsAsync(userId);
            if (!adminUnits.Any())
                return Result.Failure<RevenueReportResponse>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var unitIds = filter.UnitId.HasValue
                ? new List<int> { filter.UnitId.Value }
                : adminUnits.Select(u => u.Id).ToList();

            var bookings = await _context.Bookings
                .Where(b => unitIds.Contains(b.UnitId) &&
                           b.CreatedAt >= filter.StartDate &&
                           b.CreatedAt <= filter.EndDate &&
                           b.Status == BookingStatus.Completed)
                .Include(b => b.Unit)
                .Include(b => b.Payments)
                .ToListAsync();

            var totalRevenue = bookings.Sum(b => b.TotalPrice);
            var totalBookings = bookings.Count;

            var revenueByUnit = bookings
                .GroupBy(b => new { b.UnitId, b.Unit.Name })
                .Select(g => new UnitRevenueData
                {
                    UnitId = g.Key.UnitId,
                    UnitName = g.Key.Name,
                    Revenue = g.Sum(b => b.TotalPrice),
                    BookingCount = g.Count(),
                    AverageBookingValue = g.Average(b => b.TotalPrice)
                }).ToList();

            var revenueByMonth = bookings
                .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g => new MonthlyRevenueData
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Revenue = g.Sum(b => b.TotalPrice),
                    BookingCount = g.Count()
                }).ToList();

            var report = new RevenueReportResponse
            {
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                TotalRevenue = totalRevenue,
                TotalBookings = totalBookings,
                AverageBookingValue = totalBookings > 0 ? totalRevenue / totalBookings : 0,
                RevenueByUnit = revenueByUnit,
                RevenueByMonth = revenueByMonth
            };

            return Result.Success(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating revenue report for user {UserId}", userId);
            return Result.Failure<RevenueReportResponse>(
                new Error("ReportFailed", "Failed to generate revenue report", 500));
        }
    }

    public async Task<Result<BookingAnalyticsResponse>> GetBookingAnalyticsAsync(
        string userId,
        AnalyticsFilter filter)
    {
        try
        {
            var adminUnits = await GetUserAdminUnitsAsync(userId);
            if (!adminUnits.Any())
                return Result.Failure<BookingAnalyticsResponse>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var unitIds = filter.UnitId.HasValue
                ? new List<int> { filter.UnitId.Value }
                : adminUnits.Select(u => u.Id).ToList();

            var bookings = await _context.Bookings
                .Where(b => unitIds.Contains(b.UnitId) &&
                           b.CreatedAt >= filter.StartDate &&
                           b.CreatedAt <= filter.EndDate)
                .ToListAsync();

            var statusDistribution = bookings
                .GroupBy(b => b.Status)
                .Select(g => new StatusCount
                {
                    Status = g.Key.ToString(),
                    Count = g.Count(),
                    Percentage = (decimal)g.Count() / bookings.Count * 100
                }).ToList();

            var bookingsByDay = bookings
                .GroupBy(b => b.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new DailyBookingData
                {
                    Date = g.Key,
                    BookingCount = g.Count(),
                    Revenue = g.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.TotalPrice)
                }).ToList();

            var avgLeadTime = bookings
                .Where(b => b.Status != BookingStatus.Cancelled)
                .Average(b => (b.CheckInDate - b.CreatedAt).Days);

            var analytics = new BookingAnalyticsResponse
            {
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                TotalBookings = bookings.Count,
                CompletedBookings = bookings.Count(b => b.Status == BookingStatus.Completed),
                CancelledBookings = bookings.Count(b => b.Status == BookingStatus.Cancelled),
                CancellationRate = bookings.Any()
                    ? (decimal)bookings.Count(b => b.Status == BookingStatus.Cancelled) / bookings.Count * 100
                    : 0,
                AverageLeadTime = avgLeadTime,
                AverageStayDuration = bookings.Any() ? bookings.Average(b => b.NumberOfNights) : 0,
                StatusDistribution = statusDistribution,
                BookingsByDay = bookingsByDay
            };

            return Result.Success(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating analytics for user {UserId}", userId);
            return Result.Failure<BookingAnalyticsResponse>(
                new Error("AnalyticsFailed", "Failed to generate analytics", 500));
        }
    }

    public async Task<Result<OccupancyReportResponse>> GetOccupancyReportAsync(
        string userId,
        int? unitId,
        DateTime startDate,
        DateTime endDate)
    {
        try
        {
            var adminUnits = await GetUserAdminUnitsAsync(userId);
            if (!adminUnits.Any())
                return Result.Failure<OccupancyReportResponse>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var unitIds = unitId.HasValue
                ? new List<int> { unitId.Value }
                : adminUnits.Select(u => u.Id).ToList();

            var overallOccupancy = await CalculateOccupancyRateAsync(unitIds, startDate, endDate);

            var occupancyByUnit = new List<UnitOccupancyData>();
            foreach (var unit in adminUnits.Where(u => unitIds.Contains(u.Id)))
            {
                var unitOccupancy = await CalculateOccupancyRateAsync(
                    new List<int> { unit.Id }, startDate, endDate);

                occupancyByUnit.Add(new UnitOccupancyData
                {
                    UnitId = unit.Id,
                    UnitName = unit.Name,
                    OccupancyRate = unitOccupancy,
                    TotalRooms = unit.Rooms.Count(r => !r.IsDeleted),
                    AvailableRooms = unit.Rooms.Count(r => !r.IsDeleted && r.IsAvailable)
                });
            }

            var report = new OccupancyReportResponse
            {
                StartDate = startDate,
                EndDate = endDate,
                OverallOccupancyRate = overallOccupancy,
                OccupancyByUnit = occupancyByUnit
            };

            return Result.Success(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating occupancy report for user {UserId}", userId);
            return Result.Failure<OccupancyReportResponse>(
                new Error("OccupancyReportFailed", "Failed to generate occupancy report", 500));
        }
    }

    #endregion

    #region REVIEWS MANAGEMENT

    public async Task<Result<IEnumerable<ReviewResponse>>> GetMyUnitsReviewsAsync(
        string userId,
        ReviewFilter filter)
    {
        try
        {
            var adminUnits = await GetUserAdminUnitsAsync(userId);
            if (!adminUnits.Any())
                return Result.Failure<IEnumerable<ReviewResponse>>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var unitIds = adminUnits.Select(u => u.Id).ToList();

            var query = _context.Reviews
                .Include(r => r.Unit)
                .Include(r => r.User)
                .Include(r => r.Booking)
                .Include(r => r.Images.Where(i => !i.IsDeleted))
                .Where(r => unitIds.Contains(r.UnitId))
                .AsQueryable();

            if (filter.UnitId.HasValue)
                query = query.Where(r => r.UnitId == filter.UnitId.Value);

            if (filter.MinRating.HasValue)
                query = query.Where(r => r.Rating >= filter.MinRating.Value);

            if (filter.HasResponse.HasValue)
                query = query.Where(r => filter.HasResponse.Value
                    ? r.OwnerResponse != null
                    : r.OwnerResponse == null);

            var reviews = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var responses = reviews.Select(MapToReviewResponse).ToList();
            return Result.Success<IEnumerable<ReviewResponse>>(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reviews for user {UserId}", userId);
            return Result.Failure<IEnumerable<ReviewResponse>>(
                new Error("GetReviewsFailed", "Failed to retrieve reviews", 500));
        }
    }

    public async Task<Result> RespondToReviewAsync(
        string userId,
        int reviewId,
        string response)
    {
        try
        {
            var review = await _context.Reviews
                .Include(r => r.Unit)
                    .ThenInclude(u => u.Admins)
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review == null)
                return Result.Failure(new Error("NotFound", "Review not found", 404));

            var isAdmin = review.Unit.Admins.Any(a => a.UserId == userId && a.IsActive);
            if (!isAdmin)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to respond to this review", 403));

            if (review.OwnerResponse != null)
                return Result.Failure(
                    new Error("AlreadyResponded", "This review already has a response", 400));

            review.OwnerResponse = response;
            review.OwnerResponseDate = DateTime.UtcNow.AddHours(3);
            review.UpdatedAt = DateTime.UtcNow.AddHours(3);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Review {ReviewId} responded to by admin {UserId}",
                reviewId, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error responding to review {ReviewId}", reviewId);
            return Result.Failure(
                new Error("RespondFailed", "Failed to respond to review", 500));
        }
    }

    #endregion

    #region CALENDAR & SCHEDULING

    public async Task<Result<IEnumerable<UpcomingCheckInResponse>>> GetUpcomingCheckInsAsync(
        string userId,
        int days = 1)
    {
        try
        {
            var adminUnits = await GetUserAdminUnitsAsync(userId);
            if (!adminUnits.Any())
                return Result.Failure<IEnumerable<UpcomingCheckInResponse>>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var unitIds = adminUnits.Select(u => u.Id).ToList();
            var targetDate = DateTime.UtcNow.Date.AddDays(days);

            var checkIns = await _context.Bookings
                .Include(b => b.Unit)
                .Include(b => b.User)
                .Include(b => b.BookingRooms)
                    .ThenInclude(br => br.Room)
                .Where(b => unitIds.Contains(b.UnitId) &&
                           b.CheckInDate.Date == targetDate &&
                           b.Status == BookingStatus.Confirmed)
                .OrderBy(b => b.CheckInDate)
                .ToListAsync();

            var responses = checkIns.Select(b => new UpcomingCheckInResponse
            {
                BookingId = b.Id,
                BookingNumber = b.BookingNumber,
                UnitName = b.Unit.Name,
                GuestName = b.User.FullName ?? "N/A",
                GuestEmail = b.User.Email ?? string.Empty,
                GuestPhone = b.User.PhoneNumber ?? string.Empty,
                CheckInDate = b.CheckInDate,
                CheckOutDate = b.CheckOutDate,
                NumberOfGuests = b.NumberOfGuests,
                NumberOfNights = b.NumberOfNights,
                Rooms = b.BookingRooms.Select(br => new RoomInfo
                {
                    RoomId = br.RoomId,
                    RoomNumber = br.Room.RoomNumber
                }).ToList(),
                SpecialRequests = b.SpecialRequests
            }).ToList();

            return Result.Success<IEnumerable<UpcomingCheckInResponse>>(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting upcoming check-ins for user {UserId}", userId);
            return Result.Failure<IEnumerable<UpcomingCheckInResponse>>(
                new Error("GetCheckInsFailed", "Failed to retrieve upcoming check-ins", 500));
        }
    }

    public async Task<Result<IEnumerable<UpcomingCheckOutResponse>>> GetUpcomingCheckOutsAsync(
        string userId,
        int days = 1)
    {
        try
        {
            var adminUnits = await GetUserAdminUnitsAsync(userId);
            if (!adminUnits.Any())
                return Result.Failure<IEnumerable<UpcomingCheckOutResponse>>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var unitIds = adminUnits.Select(u => u.Id).ToList();
            var targetDate = DateTime.UtcNow.Date.AddDays(days);

            var checkOuts = await _context.Bookings
                .Include(b => b.Unit)
                .Include(b => b.User)
                .Include(b => b.BookingRooms)
                    .ThenInclude(br => br.Room)
                .Where(b => unitIds.Contains(b.UnitId) &&
                           b.CheckOutDate.Date == targetDate &&
                           b.Status == BookingStatus.CheckedIn)
                .OrderBy(b => b.CheckOutDate)
                .ToListAsync();

            var responses = checkOuts.Select(b => new UpcomingCheckOutResponse
            {
                BookingId = b.Id,
                BookingNumber = b.BookingNumber,
                UnitName = b.Unit.Name,
                GuestName = b.User.FullName ?? "N/A",
                GuestEmail = b.User.Email ?? string.Empty,
                GuestPhone = b.User.PhoneNumber ?? string.Empty,
                CheckOutDate = b.CheckOutDate,
                NumberOfNights = b.NumberOfNights,
                Rooms = b.BookingRooms.Select(br => new RoomInfo
                {
                    RoomId = br.RoomId,
                    RoomNumber = br.Room.RoomNumber
                }).ToList(),
                TotalAmount = b.TotalPrice,
                PaidAmount = b.PaidAmount,
                RemainingBalance = b.TotalPrice - b.PaidAmount
            }).ToList();

            return Result.Success<IEnumerable<UpcomingCheckOutResponse>>(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting upcoming check-outs for user {UserId}", userId);
            return Result.Failure<IEnumerable<UpcomingCheckOutResponse>>(
                new Error("GetCheckOutsFailed", "Failed to retrieve upcoming check-outs", 500));
        }
    }

    #endregion

    #region PAYMENT MANAGEMENT

    public async Task<Result<IEnumerable<PaymentHistoryResponse>>> GetPaymentHistoryAsync(
        string userId,
        PaymentFilter filter)
    {
        try
        {
            var adminUnits = await GetUserAdminUnitsAsync(userId);
            if (!adminUnits.Any())
                return Result.Failure<IEnumerable<PaymentHistoryResponse>>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var unitIds = adminUnits.Select(u => u.Id).ToList();

            var query = _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Unit)
                .Include(p => p.Booking.User)
                .Where(p => unitIds.Contains(p.Booking.UnitId))
                .AsQueryable();

            if (filter.UnitId.HasValue)
                query = query.Where(p => p.Booking.UnitId == filter.UnitId.Value);

            if (filter.PaymentStatus.HasValue)
                query = query.Where(p => p.Status == filter.PaymentStatus.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(p => p.PaymentDate >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(p => p.PaymentDate <= filter.EndDate.Value);

            var payments = await query
                .OrderByDescending(p => p.PaymentDate)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var responses = payments.Select(p => new PaymentHistoryResponse
            {
                PaymentId = p.Id,
                BookingId = p.BookingId,
                BookingNumber = p.Booking.BookingNumber,
                UnitName = p.Booking.Unit.Name,
                GuestName = p.Booking.User.FullName ?? "N/A",
                TransactionId = p.TransactionId,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod.ToString(),
                Status = p.Status.ToString(),
                PaymentDate = p.PaymentDate,
                Notes = p.Notes
            }).ToList();

            return Result.Success<IEnumerable<PaymentHistoryResponse>>(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment history for user {UserId}", userId);
            return Result.Failure<IEnumerable<PaymentHistoryResponse>>(
                new Error("GetPaymentsFailed", "Failed to retrieve payment history", 500));
        }
    }

    public async Task<Result<IEnumerable<PendingPaymentResponse>>> GetPendingPaymentsAsync(
        string userId)
    {
        try
        {
            var adminUnits = await GetUserAdminUnitsAsync(userId);
            if (!adminUnits.Any())
                return Result.Failure<IEnumerable<PendingPaymentResponse>>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var unitIds = adminUnits.Select(u => u.Id).ToList();

            var pendingBookings = await _context.Bookings
                .Include(b => b.Unit)
                .Include(b => b.User)
                .Where(b => unitIds.Contains(b.UnitId) &&
                           (b.PaymentStatus == PaymentStatus.Pending ||
                            b.PaymentStatus == PaymentStatus.PartiallyPaid) &&
                           b.Status != BookingStatus.Cancelled)
                .OrderBy(b => b.CheckInDate)
                .ToListAsync();

            var responses = pendingBookings.Select(b => new PendingPaymentResponse
            {
                BookingId = b.Id,
                BookingNumber = b.BookingNumber,
                UnitName = b.Unit.Name,
                GuestName = b.User.FullName ?? "N/A",
                GuestEmail = b.User.Email ?? string.Empty,
                GuestPhone = b.User.PhoneNumber ?? string.Empty,
                CheckInDate = b.CheckInDate,
                CheckOutDate = b.CheckOutDate,
                TotalAmount = b.TotalPrice,
                PaidAmount = b.PaidAmount,
                RemainingBalance = b.TotalPrice - b.PaidAmount,
                PaymentStatus = b.PaymentStatus.ToString(),
                DaysUntilCheckIn = (b.CheckInDate.Date - DateTime.UtcNow.Date).Days
            }).ToList();

            return Result.Success<IEnumerable<PendingPaymentResponse>>(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending payments for user {UserId}", userId);
            return Result.Failure<IEnumerable<PendingPaymentResponse>>(
                new Error("GetPendingPaymentsFailed", "Failed to retrieve pending payments", 500));
        }
    }

    #endregion

    #region MAPPING METHODS

    private UnitComprehensiveResponse MapToComprehensiveResponse(
        Domain.Entities.Unit unit,
        List<GeneralPolicy> policies)
    {
        return new UnitComprehensiveResponse
        {
            Id = unit.Id,
            Name = unit.Name,
            Description = unit.Description,
            Address = unit.Address,
            Latitude = unit.Latitude,
            Longitude = unit.Longitude,
            CityId = unit.CityId,
            CityName = unit.City.Name,
            UnitTypeId = unit.UnitTypeId,
            UnitTypeName = unit.UnitType.Name,
            BasePrice = unit.BasePrice,
            MaxGuests = unit.MaxGuests,
            Bedrooms = unit.Bedrooms,
            Bathrooms = unit.Bathrooms,
            IsActive = unit.IsActive,
            IsVerified = unit.IsVerified,
            IsFeatured = unit.IsFeatured,
            AverageRating = unit.AverageRating,
            TotalReviews = unit.TotalReviews,
            TotalRooms = unit.Rooms.Count(r => !r.IsDeleted),
            AvailableRooms = unit.Rooms.Count(r => !r.IsDeleted && r.IsAvailable),
            Images = unit.Images.OrderBy(i => i.DisplayOrder).Select(i => new ImageResponse
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                IsPrimary = i.IsPrimary,
                Caption = i.Caption
            }).ToList(),
            Amenities = unit.UnitAmenities.Select(ua => new AmenityResponse
            {
                Id = ua.Amenity.Id,
                Name = ua.Amenity.Name,
                Category = ua.Amenity.Category,
                IsAvailable = ua.IsAvailable
            }).ToList(),
            Policies = policies.Select(p => new PolicyResponse
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                PolicyType = p.PolicyType.ToString()
            }).ToList(),
            CreatedAt = unit.CreatedAt,
            UpdatedAt = unit.UpdatedAt
        };
    }

    private UnitResponse MapToUnitResponse(Domain.Entities.Unit unit)
    {
        return new UnitResponse
        {
            Id = unit.Id,
            Name = unit.Name,
            Description = unit.Description,
            Address = unit.Address,
            BasePrice = unit.BasePrice,
            MaxGuests = unit.MaxGuests,
            Bedrooms = unit.Bedrooms,
            Bathrooms = unit.Bathrooms,
            IsActive = unit.IsActive,
            IsVerified = unit.IsVerified,
            UpdatedAt = unit.UpdatedAt
        };
    }

    private async Task<BookingComprehensiveResponse> MapToBookingComprehensiveResponseAsync(Domain.Entities.Booking booking)
    {
        return new BookingComprehensiveResponse
        {
            Id = booking.Id,
            BookingNumber = booking.BookingNumber,
            UnitId = booking.UnitId,
            UnitName = booking.Unit.Name,
            UserId = booking.UserId,
            GuestName = booking.User.FullName ?? "N/A",
            GuestEmail = booking.User.Email ?? string.Empty,
            CheckInDate = booking.CheckInDate,
            CheckOutDate = booking.CheckOutDate,
            NumberOfGuests = booking.NumberOfGuests,
            NumberOfNights = booking.NumberOfNights,
            TotalPrice = booking.TotalPrice,
            PaidAmount = booking.PaidAmount,
            Status = booking.Status.ToString(),
            PaymentStatus = booking.PaymentStatus.ToString(),
            Rooms = booking.BookingRooms.Select(br => new BookedRoomInfo
            {
                RoomId = br.RoomId,
                RoomNumber = br.Room.RoomNumber,
                PricePerNight = br.PricePerNight
            }).ToList(),
            CreatedAt = booking.CreatedAt
        };
    }

    private async Task<BookingDetailsResponse> MapToBookingDetailsResponseAsync(Domain.Entities.Booking booking)
    {
        return new BookingDetailsResponse
        {
            Id = booking.Id,
            BookingNumber = booking.BookingNumber,
            UnitId = booking.UnitId,
            UnitName = booking.Unit.Name,
            UserId = booking.UserId,
            GuestName = booking.User.FullName ?? "N/A",
            GuestEmail = booking.User.Email ?? string.Empty,
            GuestPhone = booking.User.PhoneNumber ?? string.Empty,
            CheckInDate = booking.CheckInDate,
            CheckOutDate = booking.CheckOutDate,
            NumberOfGuests = booking.NumberOfGuests,
            NumberOfNights = booking.NumberOfNights,
            TotalPrice = booking.TotalPrice,
            PaidAmount = booking.PaidAmount,
            RemainingBalance = booking.TotalPrice - booking.PaidAmount,
            Status = booking.Status.ToString(),
            PaymentStatus = booking.PaymentStatus.ToString(),
            SpecialRequests = booking.SpecialRequests,
            CancellationReason = booking.CancellationReason,
            CancelledAt = booking.CancelledAt,
            Rooms = booking.BookingRooms.Select(br => new BookedRoomInfo
            {
                RoomId = br.RoomId,
                RoomNumber = br.Room.RoomNumber,
                PricePerNight = br.PricePerNight
            }).ToList(),
            Payments = booking.Payments.Select(p => new PaymentInfo
            {
                Id = p.Id,
                TransactionId = p.TransactionId,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod.ToString(),
                Status = p.Status.ToString(),
                PaymentDate = p.PaymentDate
            }).ToList(),
            CreatedAt = booking.CreatedAt,
            UpdatedAt = booking.UpdatedAt
        };
    }

    private SubUnitComprehensiveDetail MapToSubUnitComprehensiveDetail(Domain.Entities.SubUnit subUnit)
    {
        return new SubUnitComprehensiveDetail
        {
            Id = subUnit.Id,
            UnitId = subUnit.UnitId,
            UnitName = subUnit.Unit.Name,
            RoomNumber = subUnit.RoomNumber,
            SubUnitTypeName = subUnit.SubUnitType.Name,
            PricePerNight = subUnit.PricePerNight,
            MaxOccupancy = subUnit.MaxOccupancy,
            Bedrooms = subUnit.Bedrooms,
            Bathrooms = subUnit.Bathrooms,
            Size = subUnit.Size,
            IsAvailable = subUnit.IsAvailable,
            Description = subUnit.Description,
            Images = subUnit.SubUnitImages.OrderBy(i => i.DisplayOrder).Select(i => new ImageResponse
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                IsPrimary = i.IsPrimary,
                Caption = i.Caption
            }).ToList(),
            Amenities = subUnit.SubUnitAmenities.Select(sa => new AmenityResponse
            {
                Id = sa.Amenity.Id,
                Name = sa.Amenity.Name,
                Category = sa.Amenity.Category,
                IsAvailable = sa.IsAvailable
            }).ToList()
        };
    }

    private ReviewResponse MapToReviewResponse(Domain.Entities.Review review)
    {
        return new ReviewResponse
        {
            Id = review.Id,
            UnitId = review.UnitId,
            UnitName = review.Unit.Name,
            BookingId = review.BookingId,
            UserId = review.UserId,
            GuestName = review.User.FullName ?? "Anonymous",
            Rating = review.Rating,
            CleanlinessRating = review.CleanlinessRating,
            LocationRating = review.LocationRating,
            ServiceRating = review.ServiceRating,
            ValueRating = review.ValueRating,
            Comment = review.Comment,
            OwnerResponse = review.OwnerResponse,
            OwnerResponseDate = review.OwnerResponseDate,
            Images = review.Images.Select(i => new ImageResponse
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                Caption = i.Caption
            }).ToList(),
            CreatedAt = review.CreatedAt
        };
    }

    #endregion
}
