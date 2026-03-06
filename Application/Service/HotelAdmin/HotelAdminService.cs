using Amazon.S3;
using Amazon.S3.Transfer;
using Application.Abstraction;
using Application.Contracts.Bookin;
using Application.Contracts.Currency;
using Application.Contracts.hoteladmincont;
using Application.Contracts.Options;
using Application.Service.Avilabilaties;
using Application.Service.Currency;
using Application.Service.S3Image;
using Domain;
using Domain.Entities;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using System.Text.Json;

namespace Application.Service.HotelAdmin;

public class HotelAdminService(
    ApplicationDbcontext context,
    ILogger<HotelAdminService> logger,
    IAvailabilityService availabilityService,
    IS3ImageService service,
    IAmazonS3 _s3Client,
    ICurrencyService currencyService,
    IBackgroundJobClient backgroundJobClient   // ← add this
    ) : IHotelAdminService
{
    private readonly ApplicationDbcontext _context = context;
    private readonly ILogger<HotelAdminService> _logger = logger;
    private readonly IAvailabilityService _availabilityService = availabilityService;
    private readonly IS3ImageService service = service;
    private readonly IAmazonS3 s3Client = _s3Client;
    private readonly ICurrencyService currencyService = currencyService;
    private readonly IBackgroundJobClient backgroundJobClient = backgroundJobClient;
    private const string CloudFrontUrl = "";
    private const string BucketName = "hujjzy-bucket";

    // =========================================================================
    // LIGHTWEIGHT HELPER — replaces the bloated GetUserAdminUnitsAsync for
    // every operation that only needs to know which unit IDs the admin owns.
    // =========================================================================

    private async Task<List<int>> GetUserAdminUnitIdsAsync(string userId)
    {
        return await _context.Set<UniteAdmin>()
            .Where(a => a.UserId == userId && a.IsActive)
            .Select(a => a.UnitId)
            .ToListAsync();
    }

    // Kept for the rare places (GetMyUnitsAsync, MapToComprehensiveResponse)
    // that genuinely need the full navigation properties.
    private async Task<List<Domain.Entities.Unit>> GetUserAdminUnitsFullAsync(string userId)
    {
        return await _context.Units
            .Include(u => u.CustomPolicies.Where(p => p.IsActive))
            .Include(u => u.City)
            .Include(u => u.UnitType)
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
            .Where(u => !u.IsDeleted && u.Admins.Any(a => a.UserId == userId && a.IsActive))
            .AsNoTracking()
            .ToListAsync();
    }

    // =========================================================================
    // DASHBOARD & OVERVIEW
    // =========================================================================

    public async Task<Result<HotelAdminDashboardResponse>> GetDashboardAsync(string userId)
    {
        try
        {
            var unitIds = await GetUserAdminUnitIdsAsync(userId);
            if (!unitIds.Any())
                return Result.Failure<HotelAdminDashboardResponse>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var today = DateTime.UtcNow.AddHours(3).Date;
            var weekAgo = today.AddDays(-7);
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var yearStart = new DateTime(today.Year, 1, 1);

            // ── Single aggregated stats query ──────────────────────────────
            var stats = await _context.Bookings
                .Where(b => unitIds.Contains(b.UnitId))
                .GroupBy(_ => 1)
                .Select(g => new DashboardStats
                {
                    TodayCheckIns = g.Count(b =>
                        b.CheckInDate.Date == today && b.Status == BookingStatus.Confirmed),
                    TodayCheckOuts = g.Count(b =>
                        b.CheckOutDate.Date == today && b.Status == BookingStatus.CheckedIn),
                    CurrentGuests = g.Count(b => b.Status == BookingStatus.CheckedIn),
                    PendingBookings = g.Count(b => b.Status == BookingStatus.Pending),
                    TodayRevenue = g
                        .Where(b => b.CreatedAt.Date == today && b.Status == BookingStatus.Completed)
                        .Sum(b => (decimal?)b.TotalPrice) ?? 0,
                    WeekRevenue = g
                        .Where(b => b.CreatedAt >= weekAgo && b.Status == BookingStatus.Completed)
                        .Sum(b => (decimal?)b.TotalPrice) ?? 0,
                    MonthRevenue = g
                        .Where(b => b.CreatedAt >= monthStart && b.Status == BookingStatus.Completed)
                        .Sum(b => (decimal?)b.TotalPrice) ?? 0
                })
                .FirstOrDefaultAsync() ?? new DashboardStats();

            stats.UnreadReviews = await _context.Reviews
                .CountAsync(r => unitIds.Contains(r.UnitId) && r.OwnerResponse == null);

            // ── Unit summaries via projection (no in-memory navigation) ───
            var unitSummaries = await _context.Units
                .Where(u => unitIds.Contains(u.Id) && !u.IsDeleted)
                .Select(u => new UnitSummary
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
                        .Where(b => b.CreatedAt >= monthStart && b.Status == BookingStatus.Completed)
                        .Sum(b => (decimal?)b.TotalPrice) ?? 0,
                    AverageRating = u.AverageRating
                })
                .AsNoTracking()
                .ToListAsync();

            // ── Recent bookings ───────────────────────────────────────────
            var recentBookings = await _context.Bookings
                .Where(b => unitIds.Contains(b.UnitId))
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
                .AsNoTracking()
                .ToListAsync();

            // ── Revenue overview (single query) ───────────────────────────
            var yearRevenue = await _context.Bookings
                .Where(b => unitIds.Contains(b.UnitId)
                    && b.CreatedAt >= yearStart
                    && b.Status == BookingStatus.Completed)
                .SumAsync(b => (decimal?)b.TotalPrice) ?? 0;

            var pendingPayments = await _context.Bookings
                .Where(b => unitIds.Contains(b.UnitId)
                    && (b.PaymentStatus == PaymentStatus.Pending
                        || b.PaymentStatus == PaymentStatus.PartiallyPaid))
                .SumAsync(b => (decimal?)(b.TotalPrice - b.PaidAmount)) ?? 0;

            var monthlyTrend = await _context.Bookings
                .Where(b => unitIds.Contains(b.UnitId) && b.Status == BookingStatus.Completed)
                .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                .OrderByDescending(g => g.Key.Year).ThenByDescending(g => g.Key.Month)
                .Take(12)
                .Select(g => new MonthlyRevenueData
                {
                    Month = g.Key.Year + "-" + g.Key.Month.ToString("D2"),
                    Revenue = g.Sum(b => b.TotalPrice),
                    BookingCount = g.Count()
                })
                .AsNoTracking()
                .ToListAsync();

            var revenueOverview = new RevenueOverview
            {
                TodayRevenue = stats.TodayRevenue,
                WeekRevenue = stats.WeekRevenue,
                MonthRevenue = stats.MonthRevenue,
                YearRevenue = yearRevenue,
                PendingPayments = pendingPayments,
                MonthlyTrend = monthlyTrend
            };

            // ── Today check-ins / check-outs ──────────────────────────────
            var todayCheckIns = await GetUpcomingCheckInsAsync(userId, 0);
            var todayCheckOuts = await GetUpcomingCheckOutsAsync(userId, 0);

            return Result.Success(new HotelAdminDashboardResponse
            {
                Units = unitSummaries,
                Stats = stats,
                RecentBookings = recentBookings,
                TodayCheckIns = todayCheckIns.Value?.ToList() ?? [],
                TodayCheckOuts = todayCheckOuts.Value?.ToList() ?? [],
                RevenueOverview = revenueOverview
            });
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
            var unitIds = await GetUserAdminUnitIdsAsync(userId);
            if (!unitIds.Any())
                return Result.Failure<HotelAdminStatsResponse>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            // All aggregates pushed to the DB
            var unitStats = await _context.Units
                .Where(u => unitIds.Contains(u.Id) && !u.IsDeleted)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    TotalUnits = g.Count(),
                    ActiveUnits = g.Count(u => u.IsActive),
                    TotalSubUnits = g.Sum(u => u.Rooms.Count(r => !r.IsDeleted)),
                    AvailableSubUnits = g.Sum(u => u.Rooms.Count(r => !r.IsDeleted && r.IsAvailable)),
                    AverageRating = g.Average(u => (decimal?)u.AverageRating) ?? 0,
                    TotalReviews = g.Sum(u => u.TotalReviews)
                })
                .FirstOrDefaultAsync();

            var bookingStats = await _context.Bookings
                .Where(b => unitIds.Contains(b.UnitId))
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    TotalBookings = g.Count(),
                    ActiveBookings = g.Count(b =>
                        b.Status == BookingStatus.Confirmed ||
                        b.Status == BookingStatus.CheckedIn),
                    PendingBookings = g.Count(b => b.Status == BookingStatus.Pending),
                    TotalRevenue = g
                        .Where(b => b.Status == BookingStatus.Completed)
                        .Sum(b => (decimal?)b.TotalPrice) ?? 0,
                    PendingRevenue = g
                        .Where(b => b.Status == BookingStatus.Pending ||
                                    b.Status == BookingStatus.Confirmed)
                        .Sum(b => (decimal?)b.TotalPrice) ?? 0
                })
                .FirstOrDefaultAsync();

            var occupancyRate = await CalculateOccupancyRateAsync(
                unitIds,
                DateTime.UtcNow.AddHours(3).AddDays(-30),
                DateTime.UtcNow.AddHours(3));

            var stats = new HotelAdminStatsResponse
            {
                TotalUnits = unitStats?.TotalUnits ?? 0,
                ActiveUnits = unitStats?.ActiveUnits ?? 0,
                TotalSubUnits = unitStats?.TotalSubUnits ?? 0,
                AvailableSubUnits = unitStats?.AvailableSubUnits ?? 0,
                AverageRating = unitStats?.AverageRating ?? 0,
                TotalReviews = unitStats?.TotalReviews ?? 0,
                TotalBookings = bookingStats?.TotalBookings ?? 0,
                ActiveBookings = bookingStats?.ActiveBookings ?? 0,
                PendingBookings = bookingStats?.PendingBookings ?? 0,
                TotalRevenue = bookingStats?.TotalRevenue ?? 0,
                PendingRevenue = bookingStats?.PendingRevenue ?? 0,
                OccupancyRate = occupancyRate
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

    // =========================================================================
    // UNIT MANAGEMENT
    // =========================================================================

    public async Task<Result<IEnumerable<UnitComprehensiveResponse>>> GetMyUnitsAsync(
        string userId,
        UnitFilter filter)
    {
        try
        {
            var unitIds = await GetUserAdminUnitIdsAsync(userId);
            if (!unitIds.Any())
                return Result.Failure<IEnumerable<UnitComprehensiveResponse>>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            // Build the query directly on the DB — no in-memory filtering
            var query = _context.Units
                .Include(a=>a.Currency)
                .Include(u => u.CustomPolicies.Where(p => p.IsActive))
                .Include(u => u.City)
                .Include(u => u.UnitType)
                .Include(u => u.Images.Where(i => !i.IsDeleted))
                .Include(u => u.UnitAmenities).ThenInclude(ua => ua.Amenity)
                .Include(u => u.Rooms.Where(r => !r.IsDeleted))
                .Include(u => u.OptionValues).ThenInclude(ov => ov.UnitTypeOption)
                .Include(u => u.Rooms.Where(r => !r.IsDeleted))
                    .ThenInclude(r => r.OptionValues).ThenInclude(ov => ov.SubUnitTypeOption)

                .Where(u => unitIds.Contains(u.Id) && !u.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Name))
                query = query.Where(u => u.Name.Contains(filter.Name));

            if (filter.IsActive.HasValue)
                query = query.Where(u => u.IsActive == filter.IsActive.Value);

            if (filter.IsVerified.HasValue)
                query = query.Where(u => u.IsVerified == filter.IsVerified.Value);

            if (!string.IsNullOrWhiteSpace(filter.SearchKeyword))
            {
                var kw = filter.SearchKeyword.ToLower().Trim();
                query = query.Where(u =>
                    u.Name.ToLower().Contains(kw) ||
                    u.Description.ToLower().Contains(kw) ||
                    u.Address.ToLower().Contains(kw) ||
                    u.UnitType.Name.ToLower().Contains(kw) ||
                    u.City.Name.ToLower().Contains(kw));
            }
            var defaultCurrencyCode = await GetDefaultCurrencyCodeAsync();
            var units = await query.AsNoTracking().ToListAsync();
            var fetchedUnitIds = units.Select(u => u.Id).ToList();

            var generalPolicies = await _context.GeneralPolicies
                .Include(p => p.CancellationPolicy)
                .Where(p => p.UnitId.HasValue && fetchedUnitIds.Contains(p.UnitId.Value) && p.IsActive)
                .AsNoTracking()
                .ToListAsync();

            var responses = units.Select(u =>
                MapToComprehensiveResponse(u, generalPolicies.Where(p => p.UnitId == u.Id).ToList(), defaultCurrencyCode));

            return Result.Success(responses);
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
                .Include(a=>a.Currency)
                .Include(u => u.City)
                .Include(u => u.UnitType)
                .Include(u => u.CustomPolicies.Where(p => p.IsActive))
                .Include(u => u.Admins.Where(a => a.IsActive)).ThenInclude(a => a.User)
                .Include(u => u.Images.Where(i => !i.IsDeleted))
                .Include(u => u.UnitAmenities).ThenInclude(ua => ua.Amenity)
                .Include(u => u.Rooms.Where(r => !r.IsDeleted))
                    .ThenInclude(r => r.SubUnitImages.Where(i => !i.IsDeleted))
                .Include(u => u.Rooms.Where(r => !r.IsDeleted))
                    .ThenInclude(r => r.SubUnitAmenities).ThenInclude(sa => sa.Amenity)
                .Include(u => u.Rooms.Where(r => !r.IsDeleted))
                    .ThenInclude(r => r.SubUnitAvailabilities)
                    .Include(u => u.OptionValues).ThenInclude(ov => ov.UnitTypeOption)
                .Include(u => u.Rooms.Where(r => !r.IsDeleted))
                    .ThenInclude(r => r.OptionValues).ThenInclude(ov => ov.SubUnitTypeOption)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

            if (unit == null)
                return Result.Failure<UnitComprehensiveResponse>(
                    new Error("NotFound", "Unit not found", 404));

            var generalPolicies = await _context.GeneralPolicies
                .Include(p => p.CancellationPolicy)
                .Where(p => p.UnitId == unitId && p.IsActive)
                .AsNoTracking()
                .ToListAsync();

            return Result.Success(MapToComprehensiveResponse(unit, generalPolicies));
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
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

            if (unit == null)
                return Result.Failure<UnitResponse>(
                    new Error("NotFound", "Unit not found", 404));

            if (request.Name != null) unit.Name = request.Name;
            if (request.Description != null) unit.Description = request.Description;
            if (request.Address != null) unit.Address = request.Address;
            if (request.BasePrice.HasValue) unit.BasePrice = request.BasePrice.Value;
            if (request.MaxGuests.HasValue) unit.MaxGuests = request.MaxGuests.Value;
            if (request.Bedrooms.HasValue) unit.Bedrooms = request.Bedrooms.Value;
            if (request.Bathrooms.HasValue) unit.Bathrooms = request.Bathrooms.Value;
            if (request.Rank.HasValue)
            {
                if (request.Rank.Value < 1 || request.Rank.Value > 5)
                    return Result.Failure<UnitResponse>(
                        new Error("InvalidRank", "Rank must be between 1 and 5 stars", 400));
                unit.Rank = request.Rank.Value;
            }

            unit.UpdatedAt = DateTime.UtcNow.AddHours(3);
            await _context.SaveChangesAsync();

            return Result.Success(MapToUnitResponse(unit));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating unit {UnitId}", unitId);
            return Result.Failure<UnitResponse>(
                new Error("UpdateFailed", "Failed to update unit", 500));
        }
    }

    // =========================================================================
    // BOOKING MANAGEMENT
    // =========================================================================

    public async Task<Result<IHotelAdminService.PaginatedResponse<BookingComprehensiveResponse>>> GetMyUnitBookingsAsync(
        string userId,
        Contracts.hoteladmincont.BookingFilter filter)
    {
        try
        {
            var unitIds = await GetUserAdminUnitIdsAsync(userId);
            if (!unitIds.Any())
                return Result.Failure<IHotelAdminService.PaginatedResponse<BookingComprehensiveResponse>>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var query = _context.Bookings
                .Include(b => b.Unit)
                .ThenInclude(b => b.Currency)
                .AsNoTracking()
                .Where(b => unitIds.Contains(b.UnitId));

            if (filter.Status.HasValue)
                query = query.Where(b => b.Status == filter.Status.Value);

            if (filter.PaymentStatus.HasValue)
                query = query.Where(b => b.PaymentStatus == filter.PaymentStatus.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(b => b.CheckInDate >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(b => b.CheckOutDate <= filter.EndDate.Value);

            if (!string.IsNullOrWhiteSpace(filter.SearchKeyword))
            {
                var kw = filter.SearchKeyword.ToLower().Trim();
                query = query.Where(b =>
                    b.BookingNumber.ToLower().Contains(kw) ||
                    b.Unit.Name.ToLower().Contains(kw) ||
                    (b.User.FullName != null && b.User.FullName.ToLower().Contains(kw)) ||
                    (b.User.Email != null && b.User.Email.ToLower().Contains(kw)) ||
                    (b.User.PhoneNumber != null && b.User.PhoneNumber.Contains(kw)));
            }
            var defaultCurrencyCode = await GetDefaultCurrencyCodeAsync();

            var totalCount = await query.CountAsync();

            var responses = await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(b => new BookingComprehensiveResponse
                {
                    Id = b.Id,
                    BookingNumber = b.BookingNumber,
                    UnitId = b.UnitId,
                    UnitName = b.Unit.Name,
                    UserId = b.UserId,
                    GuestName = b.User.FullName ?? "N/A",
                    GuestEmail = b.User.Email ?? string.Empty,
                    CheckInDate = b.CheckInDate,
                    CheckOutDate = b.CheckOutDate,
                    NumberOfGuests = b.NumberOfGuests,
                    NumberOfNights = b.NumberOfNights,
                    TotalPrice = b.TotalPrice,
                    PaidAmount = b.PaidAmount,
                    Status = b.Status.ToString(),
                    PaymentStatus = b.PaymentStatus.ToString(),
                    Rooms = b.BookingRooms.Select(br => new BookedRoomInfo
                    {
                        RoomId = br.RoomId,
                        RoomNumber = br.Room.RoomNumber,
                        PricePerNight = br.PricePerNight
                    }).ToList(),
                    CreatedAt = b.CreatedAt,
                    UserCity = b.User.City,
                    UserCountry = b.User.Country,
                    GuestFirstName = b.GuestFirstName,
                    GuestLastName = b.GuestLastName,
                    GuestEmail2 = b.GuestEmail,
                    GuestPhone = b.GuestPhone,
                    GuestSpecialRequirements = b.SpecialRequests,
                    Currency = b.Unit.Currency != null ? b.Unit.Currency.Code : defaultCurrencyCode 
                })
                .ToListAsync();

            return Result.Success(CreatePaginatedResponse(responses, totalCount, filter.Page, filter.PageSize));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bookings for user {UserId}. Message: {Message}", userId, ex.Message);
            return Result.Failure<IHotelAdminService.PaginatedResponse<BookingComprehensiveResponse>>(
                new Error("GetBookingsFailed", "Failed to retrieve bookings", 500));
        }
    }
    private async Task<string?> GetDefaultCurrencyCodeAsync()
    {
        return await _context.Currencies
            .AsNoTracking()
            .Where(c => c.IsDefault && c.IsActive)
            .Select(c => c.Code)
            .FirstOrDefaultAsync();
    }

    private IHotelAdminService.PaginatedResponse<T> CreatePaginatedResponse<T>(
        IEnumerable<T> items, int totalCount, int page, int pageSize)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new IHotelAdminService.PaginatedResponse<T>
        {
            Items = items,
            TotalCount = totalCount,
            TotalPages = totalPages,
            CurrentPage = page,
            NextPage = page < totalPages ? page + 1 : null,
            PrevPage = page > 1 ? page - 1 : null
        };
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
                .Include(b => b.BookingRooms).ThenInclude(br => br.Room)
                .Include(b => b.Payments)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return Result.Failure<BookingDetailsResponse>(
                    new Error("NotFound", "Booking not found", 404));

            return Result.Success(MapToBookingDetailsResponse(booking));
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
                .Include(b => b.Unit).ThenInclude(u => u.Admins)
                .Include(b => b.User)
                .Include(b => b.BookingRooms).ThenInclude(br => br.Room)
                .Include(b => b.Payments)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BookingNumber == bookingNumber);

            if (booking == null)
                return Result.Failure<BookingDetailsResponse>(
                    new Error("NotFound", "Booking not found", 404));

            if (!booking.Unit.Admins.Any(a => a.UserId == userId && a.IsActive))
                return Result.Failure<BookingDetailsResponse>(
                    new Error("NoAccess", "You do not have access to this booking", 403));

            return Result.Success(MapToBookingDetailsResponse(booking));
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
                return Result.Failure(new Error("NoAccess", "You do not have access to this booking", 403));

            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
            if (booking == null)
                return Result.Failure(new Error("NotFound", "Booking not found", 404));

            if (booking.Status != BookingStatus.Pending)
                return Result.Failure(new Error("InvalidStatus", $"Cannot confirm booking with status {booking.Status}", 400));

            booking.Status = BookingStatus.Confirmed;
            booking.UpdatedAt = DateTime.UtcNow.AddHours(3);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Booking {BookingId} confirmed by hotel admin {UserId}", bookingId, userId);
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
                return Result.Failure(new Error("NoAccess", "You do not have access to this booking", 403));

            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
            if (booking == null)
                return Result.Failure(new Error("NotFound", "Booking not found", 404));

            if (booking.Status != BookingStatus.Confirmed)
                return Result.Failure(new Error("InvalidStatus", "Only confirmed bookings can be checked in", 400));

            booking.Status = BookingStatus.CheckedIn;
            booking.UpdatedAt = DateTime.UtcNow.AddHours(3);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Booking {BookingId} checked in by hotel admin {UserId}", bookingId, userId);
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
                return Result.Failure(new Error("NoAccess", "You do not have access to this booking", 403));

            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
            if (booking == null)
                return Result.Failure(new Error("NotFound", "Booking not found", 404));

            if (booking.Status != BookingStatus.CheckedIn)
                return Result.Failure(new Error("InvalidStatus", "Only checked-in bookings can be checked out", 400));

            booking.Status = BookingStatus.Completed;
            booking.UpdatedAt = DateTime.UtcNow.AddHours(3);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Booking {BookingId} checked out by hotel admin {UserId}", bookingId, userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking out booking {BookingId}", bookingId);
            return Result.Failure(new Error("CheckOutFailed", "Failed to check out booking", 500));
        }
    }

    public async Task<Result> CancelBookingAsync(string userId, int bookingId, string cancellationReason)
    {
        try
        {
            var hasAccess = await IsBookingForMyUnitAsync(userId, bookingId);
            if (!hasAccess.Value)
                return Result.Failure(new Error("NoAccess", "You do not have access to this booking", 403));

            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
            if (booking == null)
                return Result.Failure(new Error("NotFound", "Booking not found", 404));

            if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
                return Result.Failure(new Error("InvalidStatus", "Cannot cancel this booking", 400));

            booking.Status = BookingStatus.Cancelled;
            booking.CancellationReason = cancellationReason;
            booking.CancelledAt = DateTime.UtcNow.AddHours(3);
            booking.UpdatedAt = DateTime.UtcNow.AddHours(3);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Booking {BookingId} cancelled by hotel admin {UserId}", bookingId, userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling booking {BookingId}", bookingId);
            return Result.Failure(new Error("CancelFailed", "Failed to cancel booking", 500));
        }
    }

    // =========================================================================
    // SUBUNIT MANAGEMENT
    // =========================================================================

    public async Task<Result<IEnumerable<SubUnitComprehensiveDetail>>> GetMySubUnitsAsync(
        string userId,
        int? unitId = null)
    {
        try
        {
            var adminUnitIds = await GetUserAdminUnitIdsAsync(userId);
            if (!adminUnitIds.Any())
                return Result.Failure<IEnumerable<SubUnitComprehensiveDetail>>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var targetIds = unitId.HasValue
                ? new List<int> { unitId.Value }
                : adminUnitIds;

            if (unitId.HasValue && !adminUnitIds.Contains(unitId.Value))
                return Result.Failure<IEnumerable<SubUnitComprehensiveDetail>>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var subUnits = await _context.SubUnits
                .Include(s => s.Unit)
                .ThenInclude(a=>a.Currency)
                .Include(s => s.SubUnitType)
                .Include(s => s.SubUnitImages.Where(i => !i.IsDeleted))
                .Include(s => s.SubUnitAmenities).ThenInclude(sa => sa.Amenity)
                .Include(s => s.SubUnitAvailabilities)
                .Include(s => s.OptionValues).ThenInclude(ov => ov.SubUnitTypeOption)
                .Where(s => targetIds.Contains(s.UnitId) && !s.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
            var defaultCurrencyCode = await GetDefaultCurrencyCodeAsync();

            return Result.Success<IEnumerable<SubUnitComprehensiveDetail>>(
                subUnits.Select(s => MapToSubUnitComprehensiveDetail(s,defaultCurrencyCode)));
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
                .ThenInclude(a => a.Currency)
                .Include(s => s.SubUnitType)
                .Include(s => s.SubUnitImages.Where(i => !i.IsDeleted))
                .Include(s => s.SubUnitAmenities).ThenInclude(sa => sa.Amenity)
                .Include(s => s.SubUnitAvailabilities)
                .Include(s => s.BookingRooms).ThenInclude(br => br.Booking)
                .Include(s => s.OptionValues).ThenInclude(ov => ov.SubUnitTypeOption)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

            if (subUnit == null)
                return Result.Failure<SubUnitComprehensiveDetail>(
                    new Error("NotFound", "SubUnit not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(userId, subUnit.UnitId);
            if (!hasAccess.Value)
                return Result.Failure<SubUnitComprehensiveDetail>(
                    new Error("NoAccess", "You do not have access to this subunit", 403));

            var defaultCurrencyCode = await GetDefaultCurrencyCodeAsync();


            return Result.Success(MapToSubUnitComprehensiveDetail(subUnit , defaultCurrencyCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subunit {SubUnitId}", subUnitId);
            return Result.Failure<SubUnitComprehensiveDetail>(
                new Error("GetSubUnitFailed", "Failed to retrieve subunit details", 500));
        }
    }

    // =========================================================================
    // AVAILABILITY MANAGEMENT
    // =========================================================================

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
                .Include(s => s.BookingRooms).ThenInclude(br => br.Booking)
                .Where(s => s.UnitId == unitId && !s.IsDeleted)
                .AsNoTracking()
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
                .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

            if (subUnit == null)
                return Result.Failure(new Error("NotFound", "SubUnit not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(userId, subUnit.UnitId);
            if (!hasAccess.Value)
                return Result.Failure(new Error("NoAccess", "You do not have access to this subunit", 403));

            var hasBookings = await _context.BookingRooms
                .AnyAsync(br => br.RoomId == subUnitId &&
                               br.Booking.CheckInDate < request.EndDate &&
                               br.Booking.CheckOutDate > request.StartDate &&
                               br.Booking.Status != BookingStatus.Cancelled);

            if (hasBookings && !request.IsAvailable)
                return Result.Failure(new Error("HasBookings", "Cannot mark as unavailable - active bookings exist", 400));

            _context.Set<SubUnitAvailability>().Add(new SubUnitAvailability
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
            });

            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating availability for subunit {SubUnitId}", subUnitId);
            return Result.Failure(new Error("UpdateAvailabilityFailed", "Failed to update availability", 500));
        }
    }

    // =========================================================================
    // REVENUE & ANALYTICS
    // =========================================================================

    public async Task<Result<RevenueReportResponse>> GetRevenueReportAsync(
        string userId,
        RevenueReportFilter filter)
    {
        try
        {
            var unitIds = await GetUserAdminUnitIdsAsync(userId);
            if (!unitIds.Any())
                return Result.Failure<RevenueReportResponse>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            if (filter.UnitId.HasValue) unitIds = [filter.UnitId.Value];

            var bookings = await _context.Bookings
                .Where(b => unitIds.Contains(b.UnitId) &&
                            b.CreatedAt >= filter.StartDate &&
                            b.CreatedAt <= filter.EndDate &&
                            b.Status == BookingStatus.Completed)
                .Include(b => b.Unit)
                .Include(b => b.Payments)
                .AsNoTracking()
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
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new MonthlyRevenueData
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Revenue = g.Sum(b => b.TotalPrice),
                    BookingCount = g.Count()
                }).ToList();

            return Result.Success(new RevenueReportResponse
            {
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                TotalRevenue = totalRevenue,
                TotalBookings = totalBookings,
                AverageBookingValue = totalBookings > 0 ? totalRevenue / totalBookings : 0,
                RevenueByUnit = revenueByUnit,
                RevenueByMonth = revenueByMonth
            });
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
            var unitIds = await GetUserAdminUnitIdsAsync(userId);
            if (!unitIds.Any())
                return Result.Failure<BookingAnalyticsResponse>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            if (filter.UnitId.HasValue) unitIds = [filter.UnitId.Value];

            var bookings = await _context.Bookings
                .Where(b => unitIds.Contains(b.UnitId) &&
                            b.CreatedAt >= filter.StartDate &&
                            b.CreatedAt <= filter.EndDate)
                .AsNoTracking()
                .ToListAsync();

            var statusDistribution = bookings
                .GroupBy(b => b.Status)
                .Select(g => new StatusCount
                {
                    Status = g.Key.ToString(),
                    Count = g.Count(),
                    Percentage = bookings.Count > 0 ? (decimal)g.Count() / bookings.Count * 100 : 0
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

            var nonCancelledBookings = bookings.Where(b => b.Status != BookingStatus.Cancelled).ToList();
            var avgLeadTime = nonCancelledBookings.Any()
                ? nonCancelledBookings.Average(b => (b.CheckInDate - b.CreatedAt).Days)
                : 0;

            return Result.Success(new BookingAnalyticsResponse
            {
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                TotalBookings = bookings.Count,
                CompletedBookings = bookings.Count(b => b.Status == BookingStatus.Completed),
                CancelledBookings = bookings.Count(b => b.Status == BookingStatus.Cancelled),
                CancellationRate = bookings.Any()
                    ? (decimal)bookings.Count(b => b.Status == BookingStatus.Cancelled) / bookings.Count * 100 : 0,
                AverageLeadTime = avgLeadTime,
                AverageStayDuration = bookings.Any() ? bookings.Average(b => b.NumberOfNights) : 0,
                StatusDistribution = statusDistribution,
                BookingsByDay = bookingsByDay
            });
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
            var unitIds = await GetUserAdminUnitIdsAsync(userId);
            if (!unitIds.Any())
                return Result.Failure<OccupancyReportResponse>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            if (unitId.HasValue) unitIds = [unitId.Value];

            var totalDays = (endDate - startDate).Days;

            // Single query — no N+1
            var occupancyData = await _context.Units
                .Where(u => unitIds.Contains(u.Id) && !u.IsDeleted)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    TotalRooms = u.Rooms.Count(r => !r.IsDeleted),
                    AvailableRooms = u.Rooms.Count(r => !r.IsDeleted && r.IsAvailable),
                    OccupiedNights = u.Rooms
                        .Where(r => !r.IsDeleted)
                        .SelectMany(r => r.BookingRooms)
                        .Where(br => br.Booking.CheckInDate < endDate &&
                                     br.Booking.CheckOutDate > startDate &&
                                     br.Booking.Status != BookingStatus.Cancelled)
                        .Sum(br => (int?)br.NumberOfNights) ?? 0
                })
                .AsNoTracking()
                .ToListAsync();

            var occupancyByUnit = occupancyData.Select(u => new UnitOccupancyData
            {
                UnitId = u.Id,
                UnitName = u.Name,
                TotalRooms = u.TotalRooms,
                AvailableRooms = u.AvailableRooms,
                OccupancyRate = u.TotalRooms * totalDays > 0
                    ? (decimal)u.OccupiedNights / (u.TotalRooms * totalDays) * 100
                    : 0
            }).ToList();

            var overall = occupancyByUnit.Any() ? occupancyByUnit.Average(u => u.OccupancyRate) : 0;

            return Result.Success(new OccupancyReportResponse
            {
                StartDate = startDate,
                EndDate = endDate,
                OverallOccupancyRate = overall,
                OccupancyByUnit = occupancyByUnit
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating occupancy report for user {UserId}", userId);
            return Result.Failure<OccupancyReportResponse>(
                new Error("OccupancyReportFailed", "Failed to generate occupancy report", 500));
        }
    }

    // =========================================================================
    // REVIEWS MANAGEMENT
    // =========================================================================

    public async Task<Result<IEnumerable<ReviewResponse>>> GetMyUnitsReviewsAsync(
        string userId,
        ReviewFilter filter)
    {
        try
        {
            var unitIds = await GetUserAdminUnitIdsAsync(userId);
            if (!unitIds.Any())
                return Result.Failure<IEnumerable<ReviewResponse>>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

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

            if (!string.IsNullOrWhiteSpace(filter.SearchKeyword))
            {
                var kw = filter.SearchKeyword.ToLower().Trim();
                query = query.Where(r =>
                    r.Unit.Name.ToLower().Contains(kw) ||
                    (r.User.FullName != null && r.User.FullName.ToLower().Contains(kw)) ||
                    (r.Comment != null && r.Comment.ToLower().Contains(kw)) ||
                    (r.OwnerResponse != null && r.OwnerResponse.ToLower().Contains(kw)));
            }

            var reviews = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .AsNoTracking()
                .ToListAsync();

            return Result.Success<IEnumerable<ReviewResponse>>(reviews.Select(MapToReviewResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reviews for user {UserId}", userId);
            return Result.Failure<IEnumerable<ReviewResponse>>(
                new Error("GetReviewsFailed", "Failed to retrieve reviews", 500));
        }
    }

    public async Task<Result> RespondToReviewAsync(string userId, int reviewId, string response)
    {
        try
        {
            var review = await _context.Reviews
                .Include(r => r.Unit).ThenInclude(u => u.Admins)
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review == null)
                return Result.Failure(new Error("NotFound", "Review not found", 404));

            if (!review.Unit.Admins.Any(a => a.UserId == userId && a.IsActive))
                return Result.Failure(new Error("NoAccess", "You do not have access to respond to this review", 403));

            if (review.OwnerResponse != null)
                return Result.Failure(new Error("AlreadyResponded", "This review already has a response", 400));

            review.OwnerResponse = response;
            review.OwnerResponseDate = DateTime.UtcNow.AddHours(3);
            review.UpdatedAt = DateTime.UtcNow.AddHours(3);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Review {ReviewId} responded to by admin {UserId}", reviewId, userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error responding to review {ReviewId}", reviewId);
            return Result.Failure(new Error("RespondFailed", "Failed to respond to review", 500));
        }
    }

    // =========================================================================
    // CALENDAR & SCHEDULING
    // =========================================================================

    public async Task<Result<IEnumerable<UpcomingCheckInResponse>>> GetUpcomingCheckInsAsync(
        string userId,
        int days = 1)
    {
        try
        {
            var unitIds = await GetUserAdminUnitIdsAsync(userId);
            if (!unitIds.Any())
                return Result.Failure<IEnumerable<UpcomingCheckInResponse>>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var targetDate = DateTime.UtcNow.Date.AddDays(days);

            var checkIns = await _context.Bookings
                .Include(b => b.Unit)
                .Include(b => b.User)
                .Include(b => b.BookingRooms).ThenInclude(br => br.Room)
                .Where(b => unitIds.Contains(b.UnitId) &&
                            b.CheckInDate.Date == targetDate &&
                            b.Status == BookingStatus.Confirmed)
                .OrderBy(b => b.CheckInDate)
                .AsNoTracking()
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
            });

            return Result.Success(responses);
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
            var unitIds = await GetUserAdminUnitIdsAsync(userId);
            if (!unitIds.Any())
                return Result.Failure<IEnumerable<UpcomingCheckOutResponse>>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var targetDate = DateTime.UtcNow.Date.AddDays(days);

            var checkOuts = await _context.Bookings
                .Include(b => b.Unit)
                .Include(b => b.User)
                .Include(b => b.BookingRooms).ThenInclude(br => br.Room)
                .Where(b => unitIds.Contains(b.UnitId) &&
                            b.CheckOutDate.Date == targetDate &&
                            b.Status == BookingStatus.CheckedIn)
                .OrderBy(b => b.CheckOutDate)
                .AsNoTracking()
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
            });

            return Result.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting upcoming check-outs for user {UserId}", userId);
            return Result.Failure<IEnumerable<UpcomingCheckOutResponse>>(
                new Error("GetCheckOutsFailed", "Failed to retrieve upcoming check-outs", 500));
        }
    }

    // =========================================================================
    // PAYMENT MANAGEMENT
    // =========================================================================

    public async Task<Result<IEnumerable<PaymentHistoryResponse>>> GetPaymentHistoryAsync(
        string userId,
        PaymentFilter filter)
    {
        try
        {
            var unitIds = await GetUserAdminUnitIdsAsync(userId);
            if (!unitIds.Any())
                return Result.Failure<IEnumerable<PaymentHistoryResponse>>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var query = _context.Payments
                .Include(p => p.Booking).ThenInclude(b => b.Unit)
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

            if (!string.IsNullOrWhiteSpace(filter.SearchKeyword))
            {
                var kw = filter.SearchKeyword.ToLower().Trim();
                query = query.Where(p =>
                    p.TransactionId.ToLower().Contains(kw) ||
                    p.Booking.BookingNumber.ToLower().Contains(kw) ||
                    p.Booking.Unit.Name.ToLower().Contains(kw) ||
                    (p.Booking.User.FullName != null && p.Booking.User.FullName.ToLower().Contains(kw)) ||
                    (p.Booking.User.Email != null && p.Booking.User.Email.ToLower().Contains(kw)));
            }

            var payments = await query
                .OrderByDescending(p => p.PaymentDate)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .AsNoTracking()
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
            });

            return Result.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment history for user {UserId}", userId);
            return Result.Failure<IEnumerable<PaymentHistoryResponse>>(
                new Error("GetPaymentsFailed", "Failed to retrieve payment history", 500));
        }
    }

    public async Task<Result<IEnumerable<PendingPaymentResponse>>> GetPendingPaymentsAsync(string userId)
    {
        try
        {
            var unitIds = await GetUserAdminUnitIdsAsync(userId);
            if (!unitIds.Any())
                return Result.Failure<IEnumerable<PendingPaymentResponse>>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var pendingBookings = await _context.Bookings
                .Include(b => b.Unit)
                .Include(b => b.User)
                .Where(b => unitIds.Contains(b.UnitId) &&
                            (b.PaymentStatus == PaymentStatus.Pending ||
                             b.PaymentStatus == PaymentStatus.PartiallyPaid) &&
                            b.Status != BookingStatus.Cancelled)
                .OrderBy(b => b.CheckInDate)
                .AsNoTracking()
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
            });

            return Result.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending payments for user {UserId}", userId);
            return Result.Failure<IEnumerable<PendingPaymentResponse>>(
                new Error("GetPendingPaymentsFailed", "Failed to retrieve pending payments", 500));
        }
    }

    // =========================================================================
    // POLICY MANAGEMENT
    // =========================================================================

    public async Task<Result<IEnumerable<PolicyDetailResponse>>> GetUnitPoliciesAsync(
        string userId, int unitId)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure<IEnumerable<PolicyDetailResponse>>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var policies = await _context.GeneralPolicies
                .Include(p => p.CancellationPolicy)
                .Where(p => p.UnitId == unitId && p.IsActive)
                .AsNoTracking()
                .ToListAsync();

            return Result.Success(policies.Select(MapToPolicyDetailResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting policies for unit {UnitId}", unitId);
            return Result.Failure<IEnumerable<PolicyDetailResponse>>(
                new Error("GetPoliciesFailed", "Failed to retrieve policies", 500));
        }
    }

    public async Task<Result<PolicyDetailResponse>> CreateUnitPolicyAsync(
        string userId, int unitId, CreatePolicyRequest request)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure<PolicyDetailResponse>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var policy = new GeneralPolicy
            {
                Title = request.Title,
                Description = request.Description,
                PolicyType = request.PolicyType,
                PolicyCategory = request.PolicyCategory,
                CustomPolicyName = request.CustomPolicyName,
                CancellationPolicyId = request.CancellationPolicyId,
                IsMandatory = request.IsMandatory,
                IsHighlighted = request.IsHighlighted,
                UnitId = unitId,
                IsActive = true
            };

            _context.GeneralPolicies.Add(policy);
            await _context.SaveChangesAsync();

            var created = await _context.GeneralPolicies
                .Include(p => p.CancellationPolicy)
                .AsNoTracking()
                .FirstAsync(p => p.Id == policy.Id);

            _logger.LogInformation("Policy {PolicyId} created for unit {UnitId} by user {UserId}", policy.Id, unitId, userId);
            return Result.Success(MapToPolicyDetailResponse(created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating policy for unit {UnitId}", unitId);
            return Result.Failure<PolicyDetailResponse>(
                new Error("CreatePolicyFailed", "Failed to create policy", 500));
        }
    }

    public async Task<Result<PolicyDetailResponse>> UpdateUnitPolicyAsync(
        string userId, int policyId, UpdatePolicyRequest request)
    {
        try
        {
            var policy = await _context.GeneralPolicies
                .Include(p => p.CancellationPolicy)
                .FirstOrDefaultAsync(p => p.Id == policyId);

            if (policy == null)
                return Result.Failure<PolicyDetailResponse>(new Error("NotFound", "Policy not found", 404));

            if (policy.UnitId.HasValue)
            {
                var hasAccess = await IsAdminOfUnitAsync(userId, policy.UnitId.Value);
                if (!hasAccess.Value)
                    return Result.Failure<PolicyDetailResponse>(
                        new Error("NoAccess", "You do not have access to this policy", 403));
            }

            if (request.Title != null) policy.Title = request.Title;
            if (request.Description != null) policy.Description = request.Description;
            if (request.PolicyCategory.HasValue) policy.PolicyCategory = request.PolicyCategory;
            if (request.CancellationPolicyId.HasValue) policy.CancellationPolicyId = request.CancellationPolicyId;
            if (request.IsMandatory.HasValue) policy.IsMandatory = request.IsMandatory.Value;
            if (request.IsHighlighted.HasValue) policy.IsHighlighted = request.IsHighlighted.Value;
            if (request.IsActive.HasValue) policy.IsActive = request.IsActive.Value;

            await _context.SaveChangesAsync();
            return Result.Success(MapToPolicyDetailResponse(policy));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating policy {PolicyId}", policyId);
            return Result.Failure<PolicyDetailResponse>(
                new Error("UpdatePolicyFailed", "Failed to update policy", 500));
        }
    }

    public async Task<Result> DeleteUnitPolicyAsync(string userId, int policyId)
    {
        try
        {
            var policy = await _context.GeneralPolicies.FirstOrDefaultAsync(p => p.Id == policyId);
            if (policy == null)
                return Result.Failure(new Error("NotFound", "Policy not found", 404));

            if (policy.UnitId.HasValue)
            {
                var hasAccess = await IsAdminOfUnitAsync(userId, policy.UnitId.Value);
                if (!hasAccess.Value)
                    return Result.Failure(new Error("NoAccess", "You do not have access to this policy", 403));
            }

            policy.IsActive = false;
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting policy {PolicyId}", policyId);
            return Result.Failure(new Error("DeletePolicyFailed", "Failed to delete policy", 500));
        }
    }

    public async Task<Result<IEnumerable<PolicyDetailResponse>>> GetSubUnitPoliciesAsync(
        string userId, int subUnitId)
    {
        try
        {
            var subUnit = await _context.SubUnits
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

            if (subUnit == null)
                return Result.Failure<IEnumerable<PolicyDetailResponse>>(
                    new Error("NotFound", "SubUnit not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(userId, subUnit.UnitId);
            if (!hasAccess.Value)
                return Result.Failure<IEnumerable<PolicyDetailResponse>>(
                    new Error("NoAccess", "You do not have access to this subunit", 403));

            var policies = await _context.GeneralPolicies
                .Include(p => p.CancellationPolicy)
                .Where(p => p.SubUnitId == subUnitId)
                .AsNoTracking()
                .ToListAsync();

            return Result.Success(policies.Select(MapToPolicyDetailResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting policies for subunit {SubUnitId}", subUnitId);
            return Result.Failure<IEnumerable<PolicyDetailResponse>>(
                new Error("GetPoliciesFailed", "Failed to retrieve policies", 500));
        }
    }

    public async Task<Result<PolicyDetailResponse>> CreateSubUnitPolicyAsync(
        string userId, int subUnitId, CreatePolicyRequest request)
    {
        try
        {
            var subUnit = await _context.SubUnits
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

            if (subUnit == null)
                return Result.Failure<PolicyDetailResponse>(new Error("NotFound", "SubUnit not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(userId, subUnit.UnitId);
            if (!hasAccess.Value)
                return Result.Failure<PolicyDetailResponse>(
                    new Error("NoAccess", "You do not have access to this subunit", 403));

            var policy = new GeneralPolicy
            {
                Title = request.Title,
                Description = request.Description,
                PolicyType = request.PolicyType,
                PolicyCategory = request.PolicyCategory,
                CustomPolicyName = request.CustomPolicyName,
                CancellationPolicyId = request.CancellationPolicyId,
                IsMandatory = request.IsMandatory,
                IsHighlighted = request.IsHighlighted,
                SubUnitId = subUnitId,
                IsActive = true
            };

            _context.GeneralPolicies.Add(policy);
            await _context.SaveChangesAsync();

            var created = await _context.GeneralPolicies
                .Include(p => p.CancellationPolicy)
                .AsNoTracking()
                .FirstAsync(p => p.Id == policy.Id);

            return Result.Success(MapToPolicyDetailResponse(created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating policy for subunit {SubUnitId}", subUnitId);
            return Result.Failure<PolicyDetailResponse>(
                new Error("CreatePolicyFailed", "Failed to create policy", 500));
        }
    }

    // =========================================================================
    // CANCELLATION POLICY MANAGEMENT
    // =========================================================================

    public async Task<Result<IEnumerable<CancellationPolicyResponse>>> GetAvailableCancellationPoliciesAsync()
    {
        try
        {
            var policies = await _context.CancellationPolicies
                .Where(p => p.IsActive)
                .OrderBy(p => p.FullRefundDays)
                .AsNoTracking()
                .ToListAsync();

            return Result.Success(policies.Select(MapToCancellationPolicyResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available cancellation policies");
            return Result.Failure<IEnumerable<CancellationPolicyResponse>>(
                new Error("GetPoliciesFailed", "Failed to retrieve cancellation policies", 500));
        }
    }

    //public async Task<Result<CancellationPolicyResponse>> GetUnitCancellationPolicyAsync(
    //    string userId, int unitId)
    //{
    //    try
    //    {
    //        var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
    //        if (!hasAccess.Value)
    //            return Result.Failure<CancellationPolicyResponse>(
    //                new Error("NoAccess", "You do not have access to this unit", 403));

    //        var unit = await _context.Units
    //            .AsNoTracking()
    //            .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

    //        if (unit?.CancellationPolicy == null)
    //            return Result.Failure<CancellationPolicyResponse>(
    //                new Error("NotFound", "Cancellation policy not found", 404));

    //        return Result.Success(MapToCancellationPolicyResponse(unit.CancellationPolicy));
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error getting cancellation policy for unit {UnitId}", unitId);
    //        return Result.Failure<CancellationPolicyResponse>(
    //            new Error("GetPolicyFailed", "Failed to retrieve cancellation policy", 500));
    //    }
    //}

    //public async Task<Result> SetUnitCancellationPolicyAsync(
    //    string userId, int unitId, int cancellationPolicyId)
    //{
    //    try
    //    {
    //        var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
    //        if (!hasAccess.Value)
    //            return Result.Failure(new Error("NoAccess", "You do not have access to this unit", 403));

    //        var unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);
    //        if (unit == null)
    //            return Result.Failure(new Error("NotFound", "Unit not found", 404));

    //        var policy = await _context.CancellationPolicies
    //            .FirstOrDefaultAsync(p => p.Id == cancellationPolicyId && p.IsActive);

    //        if (policy == null)
    //            return Result.Failure(new Error("NotFound", "Cancellation policy not found", 404));

    //        unit.UpdatedAt = DateTime.UtcNow.AddHours(3);
    //        await _context.SaveChangesAsync();

    //        return Result.Success();
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error setting cancellation policy for unit {UnitId}", unitId);
    //        return Result.Failure(new Error("SetPolicyFailed", "Failed to set cancellation policy", 500));
    //    }
    //}

    //public async Task<Result<CancellationPolicyResponse>> CreateCustomCancellationPolicyAsync(
    //    string userId, CreateCancellationPolicyRequest request)
    //{
    //    try
    //    {
    //        var policy = new CancellationPolicy
    //        {
    //            Name = request.Name,
    //            Description = request.Description,
    //            FullRefundDays = request.FullRefundDays,
    //            PartialRefundDays = request.PartialRefundDays,
    //            PartialRefundPercentage = request.PartialRefundPercentage,
    //            IsActive = true,
    //            IsDefault = request.IsDefault,
    //            CreatedAt = DateTime.UtcNow.AddHours(3)
    //        };

    //        _context.CancellationPolicies.Add(policy);
    //        await _context.SaveChangesAsync();

    //        return Result.Success(MapToCancellationPolicyResponse(policy));
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error creating custom cancellation policy");
    //        return Result.Failure<CancellationPolicyResponse>(
    //            new Error("CreatePolicyFailed", "Failed to create cancellation policy", 500));
    //    }
    //}

    // =========================================================================
    // AVAILABILITY (comprehensive)
    // =========================================================================

    public async Task<Result<Dictionary<DateTime, Contracts.Availability.UnitDayAvailability>>> GetUnitAvailabilityCalendarAsync(
        string userId, int unitId, int year, int month)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure<Dictionary<DateTime, Contracts.Availability.UnitDayAvailability>>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            return await _availabilityService.GetUnitAvailabilityCalendarAsync(unitId, year, month);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unit availability calendar");
            return Result.Failure<Dictionary<DateTime, Contracts.Availability.UnitDayAvailability>>(
                new Error("GetCalendarFailed", "Failed to get availability calendar", 500));
        }
    }

    public async Task<Result<Dictionary<DateTime, Contracts.Availability.SubUnitDayAvailability>>> GetSubUnitAvailabilityCalendarAsync(
        string userId, int subUnitId, int year, int month)
    {
        try
        {
            var subUnit = await _context.SubUnits
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

            if (subUnit == null)
                return Result.Failure<Dictionary<DateTime, Contracts.Availability.SubUnitDayAvailability>>(
                    new Error("NotFound", "SubUnit not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(userId, subUnit.UnitId);
            if (!hasAccess.Value)
                return Result.Failure<Dictionary<DateTime, Contracts.Availability.SubUnitDayAvailability>>(
                    new Error("NoAccess", "You do not have access to this subunit", 403));

            return await _availabilityService.GetSubUnitAvailabilityCalendarAsync(subUnitId, year, month);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subunit availability calendar");
            return Result.Failure<Dictionary<DateTime, Contracts.Availability.SubUnitDayAvailability>>(
                new Error("GetCalendarFailed", "Failed to get availability calendar", 500));
        }
    }

    public async Task<Result> SetUnitAvailabilityAsync(
        string userId, Contracts.Availability.SetUnitAvailabilityRequest request)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, request.UnitId);
            if (!hasAccess.Value)
                return Result.Failure(new Error("NoAccess", "You do not have access to this unit", 403));

            request = request with { UpdatedByUserId = userId };
            return await _availabilityService.SetUnitAvailabilityAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting unit availability");
            return Result.Failure(new Error("SetAvailabilityFailed", "Failed to set availability", 500));
        }
    }

    public async Task<Result> SetSubUnitAvailabilityAsync(
        string userId, Contracts.Availability.SetSubUnitAvailabilityRequest request)
    {
        try
        {
            var unitId = await _context.SubUnits
                .Where(s => s.Id == request.SubUnitId && !s.IsDeleted)
                .Select(s => (int?)s.UnitId)
                .FirstOrDefaultAsync();

            if (unitId == null)
                return Result.Failure(new Error("NotFound", "SubUnit not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(userId, unitId.Value);
            if (!hasAccess.Value)
                return Result.Failure(new Error("NoAccess", "You do not have access to this subunit", 403));

            request = request with { UpdatedByUserId = userId };
            return await _availabilityService.SetSubUnitAvailabilityAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting subunit availability");
            return Result.Failure(new Error("SetAvailabilityFailed", "Failed to set availability", 500));
        }
    }

    public async Task<Result> BlockUnitDatesAsync(
        string userId, Contracts.Availability.BlockUnitDatesRequest request)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, request.UnitId);
            if (!hasAccess.Value)
                return Result.Failure(new Error("NoAccess", "You do not have access to this unit", 403));

            request = request with { UpdatedByUserId = userId };
            return await _availabilityService.BlockUnitDatesAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error blocking unit dates");
            return Result.Failure(new Error("BlockDatesFailed", "Failed to block dates", 500));
        }
    }

    public async Task<Result> BlockSubUnitDatesAsync(
        string userId, Contracts.Availability.BlockDatesRequest request)
    {
        try
        {
            var unitId = await _context.SubUnits
                .Where(s => s.Id == request.SubUnitId && !s.IsDeleted)
                .Select(s => (int?)s.UnitId)
                .FirstOrDefaultAsync();

            if (unitId == null)
                return Result.Failure(new Error("NotFound", "SubUnit not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(userId, unitId.Value);
            if (!hasAccess.Value)
                return Result.Failure(new Error("NoAccess", "You do not have access to this subunit", 403));

            request = request with { UpdatedByUserId = userId };
            return await _availabilityService.BlockSubUnitDatesAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error blocking subunit dates");
            return Result.Failure(new Error("BlockDatesFailed", "Failed to block dates", 500));
        }
    }

    public async Task<Result> SetSubUnitSpecialPricingAsync(
        string userId, Contracts.Availability.SetSpecialPricingRequest request)
    {
        try
        {
            var unitId = await _context.SubUnits
                .Where(s => s.Id == request.SubUnitId && !s.IsDeleted)
                .Select(s => (int?)s.UnitId)
                .FirstOrDefaultAsync();

            if (unitId == null)
                return Result.Failure(new Error("NotFound", "SubUnit not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(userId, unitId.Value);
            if (!hasAccess.Value)
                return Result.Failure(new Error("NoAccess", "You do not have access to this subunit", 403));

            request = request with { UpdatedByUserId = userId };
            return await _availabilityService.SetSpecialPricingAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting special pricing");
            return Result.Failure(new Error("SetPricingFailed", "Failed to set special pricing", 500));
        }
    }

    public async Task<Result> UnblockDatesAsync(string userId, int availabilityId)
    {
        try
        {
            var unitAvailability = await _context.Set<UnitAvailability>()
                .FirstOrDefaultAsync(a => a.Id == availabilityId);

            if (unitAvailability != null)
            {
                var hasAccess = await IsAdminOfUnitAsync(userId, unitAvailability.UnitId);
                if (!hasAccess.Value)
                    return Result.Failure(new Error("NoAccess", "You do not have access to this availability", 403));
            }
            else
            {
                var subUnitUnitId = await _context.Set<SubUnitAvailability>()
                    .Where(a => a.Id == availabilityId)
                    .Select(a => (int?)a.SubUnit.UnitId)
                    .FirstOrDefaultAsync();

                if (subUnitUnitId == null)
                    return Result.Failure(new Error("NotFound", "Availability not found", 404));

                var hasAccess = await IsAdminOfUnitAsync(userId, subUnitUnitId.Value);
                if (!hasAccess.Value)
                    return Result.Failure(new Error("NoAccess", "You do not have access to this availability", 403));
            }

            return await _availabilityService.UnblockDatesAsync(availabilityId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unblocking dates");
            return Result.Failure(new Error("UnblockDatesFailed", "Failed to unblock dates", 500));
        }
    }

    public async Task<Result<IEnumerable<AvailabilityBlockResponse>>> GetUnitBlockedDatesAsync(
        string userId, int unitId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure<IEnumerable<AvailabilityBlockResponse>>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var query = _context.Set<UnitAvailability>().Where(a => a.UnitId == unitId);
            if (startDate.HasValue) query = query.Where(a => a.EndDate >= startDate.Value);
            if (endDate.HasValue) query = query.Where(a => a.StartDate <= endDate.Value);

            var blocks = await query.OrderBy(a => a.StartDate).AsNoTracking().ToListAsync();
            return Result.Success(blocks.Select(MapToAvailabilityBlockResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unit blocked dates");
            return Result.Failure<IEnumerable<AvailabilityBlockResponse>>(
                new Error("GetBlocksFailed", "Failed to retrieve blocked dates", 500));
        }
    }

    public async Task<Result<IEnumerable<AvailabilityBlockResponse>>> GetSubUnitBlockedDatesAsync(
        string userId, int subUnitId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var unitId = await _context.SubUnits
                .Where(s => s.Id == subUnitId && !s.IsDeleted)
                .Select(s => (int?)s.UnitId)
                .FirstOrDefaultAsync();

            if (unitId == null)
                return Result.Failure<IEnumerable<AvailabilityBlockResponse>>(
                    new Error("NotFound", "SubUnit not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(userId, unitId.Value);
            if (!hasAccess.Value)
                return Result.Failure<IEnumerable<AvailabilityBlockResponse>>(
                    new Error("NoAccess", "You do not have access to this subunit", 403));

            var query = _context.Set<SubUnitAvailability>().Where(a => a.SubUnitId == subUnitId);
            if (startDate.HasValue) query = query.Where(a => a.EndDate >= startDate.Value);
            if (endDate.HasValue) query = query.Where(a => a.StartDate <= endDate.Value);

            var blocks = await query.OrderBy(a => a.StartDate).AsNoTracking().ToListAsync();
            return Result.Success(blocks.Select(MapToSubUnitAvailabilityBlockResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subunit blocked dates");
            return Result.Failure<IEnumerable<AvailabilityBlockResponse>>(
                new Error("GetBlocksFailed", "Failed to retrieve blocked dates", 500));
        }
    }

    public async Task<Result<UnitAvailabilityStatus>> CheckUnitAvailabilityAsync(
        string userId, int unitId, DateTime checkIn, DateTime checkOut)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure<UnitAvailabilityStatus>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            return await _availabilityService.CheckUnitAvailabilityAsync(unitId, checkIn, checkOut);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking unit availability");
            return Result.Failure<UnitAvailabilityStatus>(
                new Error("CheckAvailabilityFailed", "Failed to check availability", 500));
        }
    }

    public async Task<Result<SubUnitAvailabilityStatus>> CheckSubUnitAvailabilityAsync(
        string userId, int subUnitId, DateTime checkIn, DateTime checkOut)
    {
        try
        {
            var unitId = await _context.SubUnits
                .Where(s => s.Id == subUnitId && !s.IsDeleted)
                .Select(s => (int?)s.UnitId)
                .FirstOrDefaultAsync();

            if (unitId == null)
                return Result.Failure<SubUnitAvailabilityStatus>(
                    new Error("NotFound", "SubUnit not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(userId, unitId.Value);
            if (!hasAccess.Value)
                return Result.Failure<SubUnitAvailabilityStatus>(
                    new Error("NoAccess", "You do not have access to this subunit", 403));

            return await _availabilityService.CheckSubUnitAvailabilityAsync(subUnitId, checkIn, checkOut);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking subunit availability");
            return Result.Failure<SubUnitAvailabilityStatus>(
                new Error("CheckAvailabilityFailed", "Failed to check availability", 500));
        }
    }

    public async Task<Result<List<AvailableSubUnitInfo>>> GetAvailableSubUnitsAsync(
        string userId, int unitId, DateTime checkIn, DateTime checkOut)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure<List<AvailableSubUnitInfo>>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var availableIds = await _availabilityService.GetAvailableSubUnitIdsAsync(unitId, checkIn, checkOut);

            if (!availableIds.IsSuccess || !availableIds.Value.Any())
                return Result.Success(new List<AvailableSubUnitInfo>());

            var subUnits = await _context.SubUnits
                .Where(s => availableIds.Value.Contains(s.Id) && !s.IsDeleted)
                .Select(s => new AvailableSubUnitInfo
                {
                    Id = s.Id,
                    RoomNumber = s.RoomNumber,
                    TypeId = s.SubUnitTypeId,
                    PricePerNight = s.PricePerNight,
                    MaxOccupancy = s.MaxOccupancy,
                    IsAvailable = s.IsAvailable
                })
                .AsNoTracking()
                .ToListAsync();

            return Result.Success(subUnits);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available subunits");
            return Result.Failure<List<AvailableSubUnitInfo>>(
                new Error("GetAvailableFailed", "Failed to get available subunits", 500));
        }
    }

    // =========================================================================
    // COMPREHENSIVE REPORTS
    // =========================================================================

    public async Task<Result<FinancialReportResponse>> GetFinancialReportAsync(
        string userId, FinancialReportFilter filter)
    {
        try
        {
            var unitIds = await GetUserAdminUnitIdsAsync(userId);
            if (!unitIds.Any())
                return Result.Failure<FinancialReportResponse>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            if (filter.UnitId.HasValue) unitIds = [filter.UnitId.Value];

            var bookings = await _context.Bookings
                .Where(b => unitIds.Contains(b.UnitId) &&
                            b.CreatedAt >= filter.StartDate &&
                            b.CreatedAt <= filter.EndDate &&
                            b.Status == BookingStatus.Completed)
                .Include(b => b.Unit)
                .Include(b => b.Payments)
                .AsNoTracking()
                .ToListAsync();

            var totalRevenue = bookings.Sum(b => b.TotalPrice);
            var days = (filter.EndDate - filter.StartDate).Days + 1;
            var avgDailyRevenue = days > 0 ? totalRevenue / days : 0;

            var revenueByUnit = bookings
                .GroupBy(b => new { b.UnitId, b.Unit.Name })
                .Select(g => new RevenueByUnit
                {
                    UnitId = g.Key.UnitId,
                    UnitName = g.Key.Name,
                    Revenue = g.Sum(b => b.TotalPrice),
                    BookingCount = g.Count(),
                    AverageBookingValue = g.Average(b => b.TotalPrice),
                    ContributionPercentage = totalRevenue > 0
                        ? g.Sum(b => b.TotalPrice) / totalRevenue * 100 : 0
                }).OrderByDescending(r => r.Revenue).ToList();

            var monthlyBreakdown = bookings
                .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                .Select(g => new RevenueByMonth
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Year = g.Key.Year,
                    Revenue = g.Sum(b => b.TotalPrice),
                    BookingCount = g.Count(),
                    GrowthRate = 0
                }).OrderBy(r => r.Year).ThenBy(r => r.Month).ToList();

            var paymentMethods = bookings
                .SelectMany(b => b.Payments)
                .GroupBy(p => p.PaymentMethod)
                .Select(g => new PaymentMethodBreakdown
                {
                    PaymentMethod = g.Key.ToString(),
                    Amount = g.Sum(p => p.Amount),
                    TransactionCount = g.Count(),
                    Percentage = totalRevenue > 0 ? g.Sum(p => p.Amount) / totalRevenue * 100 : 0
                }).ToList();

            return Result.Success(new FinancialReportResponse
            {
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                TotalRevenue = totalRevenue,
                TotalExpenses = 0,
                NetIncome = totalRevenue,
                AverageDailyRevenue = avgDailyRevenue,
                ProjectedMonthlyRevenue = avgDailyRevenue * 30,
                YearToDateRevenue = totalRevenue,
                GrowthRate = 0,
                RevenueByUnit = revenueByUnit,
                MonthlyBreakdown = monthlyBreakdown,
                PaymentMethods = paymentMethods,
                RevenueSources = []
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating financial report");
            return Result.Failure<FinancialReportResponse>(
                new Error("ReportFailed", "Failed to generate financial report", 500));
        }
    }

    public async Task<Result<OccupancyStatisticsResponse>> GetOccupancyStatisticsAsync(
        string userId, OccupancyFilter filter)
    {
        try
        {
            var unitIds = await GetUserAdminUnitIdsAsync(userId);
            if (!unitIds.Any())
                return Result.Failure<OccupancyStatisticsResponse>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            if (filter.UnitId.HasValue) unitIds = [filter.UnitId.Value];

            var occupancyRate = await CalculateOccupancyRateAsync(unitIds, filter.StartDate, filter.EndDate);

            return Result.Success(new OccupancyStatisticsResponse
            {
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                OverallOccupancyRate = occupancyRate,
                AverageDailyRate = 0,
                RevPAR = 0,
                TotalAvailableRoomNights = 0,
                TotalOccupiedRoomNights = 0,
                UnitOccupancy = [],
                DailyOccupancy = [],
                TopPerformingRooms = [],
                LowPerformingRooms = []
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating occupancy statistics");
            return Result.Failure<OccupancyStatisticsResponse>(
                new Error("StatisticsFailed", "Failed to generate occupancy statistics", 500));
        }
    }

    public async Task<Result<BookingTrendsResponse>> GetBookingTrendsAsync(
        string userId, TrendsFilter filter)
    {
        try
        {
            var unitIds = await GetUserAdminUnitIdsAsync(userId);
            if (!unitIds.Any())
                return Result.Failure<BookingTrendsResponse>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            if (filter.UnitId.HasValue) unitIds = [filter.UnitId.Value];

            var bookings = await _context.Bookings
                .Where(b => unitIds.Contains(b.UnitId) &&
                            b.CreatedAt >= filter.StartDate &&
                            b.CreatedAt <= filter.EndDate)
                .AsNoTracking()
                .ToListAsync();

            return Result.Success(new BookingTrendsResponse
            {
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                TotalBookings = bookings.Count,
                AverageLeadTime = (decimal)(bookings.Any()
                    ? bookings.Average(b => (b.CheckInDate - b.CreatedAt).Days) : 0),
                AverageLengthOfStay = (decimal)(bookings.Any() ? bookings.Average(b => b.NumberOfNights) : 0),
                BookingConversionRate = 0,
                BookingsByPeriod = [],
                BookingsByDayOfWeek = [],
                BookingsBySource = [],
                SeasonalTrends = [],
                PeakPeriods = []
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating booking trends");
            return Result.Failure<BookingTrendsResponse>(
                new Error("TrendsFailed", "Failed to generate booking trends", 500));
        }
    }

    public async Task<Result<CustomerInsightsResponse>> GetCustomerInsightsAsync(
        string userId, InsightsFilter filter)
    {
        try
        {
            var unitIds = await GetUserAdminUnitIdsAsync(userId);
            if (!unitIds.Any())
                return Result.Failure<CustomerInsightsResponse>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            return Result.Success(new CustomerInsightsResponse
            {
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                TotalUniqueGuests = 0,
                ReturningGuests = 0,
                ReturnGuestRate = 0,
                AverageGuestsPerBooking = 0,
                AverageBookingValue = 0,
                CustomerLifetimeValue = 0,
                Demographics = [],
                TopGuests = [],
                Preferences = [],
                ReviewInsights = []
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating customer insights");
            return Result.Failure<CustomerInsightsResponse>(
                new Error("InsightsFailed", "Failed to generate customer insights", 500));
        }
    }

    public async Task<Result<PerformanceComparisonResponse>> GetPerformanceComparisonAsync(
        string userId, ComparisonFilter filter)
    {
        try
        {
            var unitIds = await GetUserAdminUnitIdsAsync(userId);
            if (!unitIds.Any())
                return Result.Failure<PerformanceComparisonResponse>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            return Result.Success(new PerformanceComparisonResponse
            {
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                UnitPerformances = [],
                MetricComparisons = [],
                BestPerformer = new UnitPerformance(),
                WorstPerformer = new UnitPerformance()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating performance comparison");
            return Result.Failure<PerformanceComparisonResponse>(
                new Error("ComparisonFailed", "Failed to generate performance comparison", 500));
        }
    }

    public async Task<Result<RevenueBreakdownResponse>> GetRevenueBreakdownAsync(
        string userId, RevenueBreakdownFilter filter)
    {
        try
        {
            var unitIds = await GetUserAdminUnitIdsAsync(userId);
            if (!unitIds.Any())
                return Result.Failure<RevenueBreakdownResponse>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            return Result.Success(new RevenueBreakdownResponse
            {
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                TotalRevenue = 0,
                ByRoomType = [],
                ByBookingType = [],
                ByPaymentMethod = [],
                ByDayType = [],
                ByMonth = []
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating revenue breakdown");
            return Result.Failure<RevenueBreakdownResponse>(
                new Error("BreakdownFailed", "Failed to generate revenue breakdown", 500));
        }
    }

    public async Task<Result<CancellationAnalyticsResponse>> GetCancellationAnalyticsAsync(
        string userId, CancellationFilter filter)
    {
        try
        {
            var unitIds = await GetUserAdminUnitIdsAsync(userId);
            if (!unitIds.Any())
                return Result.Failure<CancellationAnalyticsResponse>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            return Result.Success(new CancellationAnalyticsResponse
            {
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                TotalCancellations = 0,
                CancellationRate = 0,
                LostRevenue = 0,
                AverageCancellationLeadTime = 0,
                ByReason = [],
                ByTimeframe = [],
                ByUnit = [],
                RefundSummary = []
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating cancellation analytics");
            return Result.Failure<CancellationAnalyticsResponse>(
                new Error("AnalyticsFailed", "Failed to generate cancellation analytics", 500));
        }
    }

    public async Task<Result<PricingOptimizationResponse>> GetPricingOptimizationSuggestionsAsync(
        string userId, int unitId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure<PricingOptimizationResponse>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var unit = await _context.Units
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

            if (unit == null)
                return Result.Failure<PricingOptimizationResponse>(
                    new Error("NotFound", "Unit not found", 404));

            return Result.Success(new PricingOptimizationResponse
            {
                UnitId = unitId,
                UnitName = unit.Name,
                AnalysisPeriodStart = startDate,
                AnalysisPeriodEnd = endDate,
                CurrentAverageRate = unit.BasePrice,
                RecommendedAverageRate = unit.BasePrice,
                PotentialRevenueIncrease = 0,
                Suggestions = [],
                CompetitorAnalysis = [],
                DemandForecasts = []
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating pricing optimization");
            return Result.Failure<PricingOptimizationResponse>(
                new Error("OptimizationFailed", "Failed to generate pricing optimization", 500));
        }
    }

    public async Task<Result<byte[]>> ExportReportToExcelAsync(string userId, ExportReportRequest request)
    {
        _logger.LogInformation("Excel export requested by user {UserId}", userId);
        return Result.Failure<byte[]>(new Error("NotImplemented", "Excel export not yet implemented", 501));
    }

    public async Task<Result<byte[]>> ExportReportToPdfAsync(string userId, ExportReportRequest request)
    {
        _logger.LogInformation("PDF export requested by user {UserId}", userId);
        return Result.Failure<byte[]>(new Error("NotImplemented", "PDF export not yet implemented", 501));
    }

    // =========================================================================
    // SUBUNIT MANAGEMENT (EXTENDED)
    // =========================================================================

    public async Task<Result<SubUnitCreatedResponse>> CreateSubUnitAsync(
        string userId, int unitId, CreateSubUnitRequest request)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure<SubUnitCreatedResponse>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var unitExists = await _context.Units.AnyAsync(u => u.Id == unitId && !u.IsDeleted);
            if (!unitExists)
                return Result.Failure<SubUnitCreatedResponse>(new Error("NotFound", "Unit not found", 404));

            var subUnitType = await _context.Set<SubUnitTypee>()
                .FirstOrDefaultAsync(st => st.Id == request.SubUnitTypeId && st.IsActive);

            if (subUnitType == null)
                return Result.Failure<SubUnitCreatedResponse>(
                    new Error("InvalidSubUnitType", "SubUnit type not found or inactive", 400));

            var unitTypeId = await _context.Units
            .Where(u => u.Id == unitId && !u.IsDeleted)
            .Select(u => u.UnitTypeId)
            .FirstOrDefaultAsync();

            var isAllowed = await _context.UnitTypeSubUnitTypes
                .AnyAsync(x => x.UnitTypeId == unitTypeId && x.SubUnitTypeId == request.SubUnitTypeId);

            if (!isAllowed)
                return Result.Failure<SubUnitCreatedResponse>(
                    new Error("SubUnitTypeNotAllowed",
                        $"SubUnit type '{subUnitType.Name}' is not allowed for this unit's type", 400));

            var roomExists = await _context.SubUnits.AnyAsync(s =>
                s.UnitId == unitId && s.RoomNumber == request.RoomNumber && !s.IsDeleted);

            if (roomExists)
                return Result.Failure<SubUnitCreatedResponse>(
                    new Error("DuplicateRoomNumber",
                        $"Room number '{request.RoomNumber}' already exists for this unit", 400));

            RoomConfiguration? roomConfig = null;
            if (request.RoomConfig != null)
                roomConfig = new RoomConfiguration
                {
                    Bedrooms = request.RoomConfig.Bedrooms,
                    Bathrooms = request.RoomConfig.Bathrooms,
                    Size = request.RoomConfig.Size,
                    BedType = request.RoomConfig.BedType,
                    BedCount = request.RoomConfig.BedCount,
                    HasKitchen = request.RoomConfig.HasKitchen,
                    HasBalcony = request.RoomConfig.HasBalcony,
                    ViewType = request.RoomConfig.ViewType,
                    Floor = request.RoomConfig.Floor
                };

            var subUnit = new Domain.Entities.SubUnit
            {
                UnitId = unitId,
                RoomNumber = request.RoomNumber,
                SubUnitTypeId = request.SubUnitTypeId,
                PricePerNight = request.PricePerNight,
                MaxOccupancy = request.MaxOccupancy,
                Bedrooms = request.Bedrooms,
                Bathrooms = request.Bathrooms,
                Size = request.Size,
                Description = request.Description,
                IsAvailable = request.IsAvailable,
                RoomConfig = roomConfig,
                IsDeleted = false
            };

            _context.SubUnits.Add(subUnit);
            await _context.SaveChangesAsync();

            List<AmenityResponse>? amenitiesResponse = null;
            if (request.AmenityIds?.Any() == true)
            {
                var amenities = await _context.Amenities
                    .Where(a => request.AmenityIds.Contains(a.Id))
                    .ToListAsync();

                await _context.SubUniteAmenities.AddRangeAsync(amenities.Select(a => new SubUniteAmenity
                {
                    SubUnitId = subUnit.Id,
                    AmenityId = a.Id,
                    IsAvailable = true
                }));
                await _context.SaveChangesAsync();

                amenitiesResponse = amenities.Select(a => new AmenityResponse
                {
                    Id = a.Id,
                    Name = a.Name,
                    Category = a.Category,
                    IsAvailable = true
                }).ToList();
            }

            _logger.LogInformation("SubUnit {SubUnitId} created for unit {UnitId} by user {UserId}",
                subUnit.Id, unitId, userId);

            return Result.Success(new SubUnitCreatedResponse
            {
                Id = subUnit.Id,
                UnitId = subUnit.UnitId,
                RoomNumber = subUnit.RoomNumber,
                SubUnitTypeName = subUnitType.Name,
                PricePerNight = subUnit.PricePerNight,
                MaxOccupancy = subUnit.MaxOccupancy,
                Bedrooms = subUnit.Bedrooms,
                Bathrooms = subUnit.Bathrooms,
                Size = subUnit.Size,
                IsAvailable = subUnit.IsAvailable,
                Description = subUnit.Description,
                CreatedAt = DateTime.UtcNow.AddHours(3),
                Amenities = amenitiesResponse
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subunit for unit {UnitId} by user {UserId}", unitId, userId);
            return Result.Failure<SubUnitCreatedResponse>(
                new Error("CreateSubUnitFailed", "Failed to create subunit", 500));
        }
    }

    public async Task<Result<SubUnitResponse>> UpdateSubUnitAsync(
        string userId, int subUnitId, UpdateSubUnitRequest request)
    {
        try
        {
            var subUnit = await _context.SubUnits
                .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

            if (subUnit == null)
                return Result.Failure<SubUnitResponse>(new Error("NotFound", "SubUnit not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(userId, subUnit.UnitId);
            if (!hasAccess.Value)
                return Result.Failure<SubUnitResponse>(
                    new Error("NoAccess", "You do not have access to this subunit", 403));

            if (request.RoomNumber != null) subUnit.RoomNumber = request.RoomNumber;
            if (request.Description != null) subUnit.Description = request.Description;
            if (request.MaxOccupancy.HasValue) subUnit.MaxOccupancy = request.MaxOccupancy.Value;
            if (request.Bedrooms.HasValue) subUnit.Bedrooms = request.Bedrooms.Value;
            if (request.Bathrooms.HasValue) subUnit.Bathrooms = request.Bathrooms.Value;
            if (request.Size.HasValue) subUnit.Size = request.Size.Value;

            await _context.SaveChangesAsync();

            return Result.Success(new SubUnitResponse
            {
                Id = subUnit.Id,
                UnitId = subUnit.UnitId,
                RoomNumber = subUnit.RoomNumber,
                PricePerNight = subUnit.PricePerNight,
                MaxOccupancy = subUnit.MaxOccupancy,
                IsAvailable = subUnit.IsAvailable,
                UpdatedAt = DateTime.UtcNow.AddHours(3)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subunit {SubUnitId}", subUnitId);
            return Result.Failure<SubUnitResponse>(new Error("UpdateFailed", "Failed to update subunit", 500));
        }
    }

    public async Task<Result> UpdateSubUnitPricingAsync(
        string userId, int subUnitId, UpdateSubUnitPricingRequest request)
    {
        try
        {
            var subUnit = await _context.SubUnits
                .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

            if (subUnit == null)
                return Result.Failure(new Error("NotFound", "SubUnit not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(userId, subUnit.UnitId);
            if (!hasAccess.Value)
                return Result.Failure(new Error("NoAccess", "You do not have access to this subunit", 403));

            subUnit.PricePerNight = request.PricePerNight;
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subunit pricing");
            return Result.Failure(new Error("UpdateFailed", "Failed to update pricing", 500));
        }
    }

    public async Task<Result> ToggleSubUnitStatusAsync(string userId, int subUnitId)
    {
        try
        {
            var subUnit = await _context.SubUnits
                .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

            if (subUnit == null)
                return Result.Failure(new Error("NotFound", "SubUnit not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(userId, subUnit.UnitId);
            if (!hasAccess.Value)
                return Result.Failure(new Error("NoAccess", "You do not have access to this subunit", 403));

            subUnit.IsAvailable = !subUnit.IsAvailable;
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling subunit status");
            return Result.Failure(new Error("ToggleFailed", "Failed to toggle status", 500));
        }
    }

    public async Task<Result<IEnumerable<SubUnitBookingHistoryResponse>>> GetSubUnitBookingHistoryAsync(
        string userId, int subUnitId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var unitId = await _context.SubUnits
                .Where(s => s.Id == subUnitId && !s.IsDeleted)
                .Select(s => (int?)s.UnitId)
                .FirstOrDefaultAsync();

            if (unitId == null)
                return Result.Failure<IEnumerable<SubUnitBookingHistoryResponse>>(
                    new Error("NotFound", "SubUnit not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(userId, unitId.Value);
            if (!hasAccess.Value)
                return Result.Failure<IEnumerable<SubUnitBookingHistoryResponse>>(
                    new Error("NoAccess", "You do not have access to this subunit", 403));

            var query = _context.BookingRooms
                .Include(br => br.Booking).ThenInclude(b => b.User)
                .Where(br => br.RoomId == subUnitId);

            if (startDate.HasValue) query = query.Where(br => br.Booking.CheckInDate >= startDate.Value);
            if (endDate.HasValue) query = query.Where(br => br.Booking.CheckOutDate <= endDate.Value);

            var bookings = await query
                .OrderByDescending(br => br.Booking.CheckInDate)
                .Select(br => new SubUnitBookingHistoryResponse
                {
                    BookingId = br.BookingId,
                    BookingNumber = br.Booking.BookingNumber,
                    CheckInDate = br.Booking.CheckInDate,
                    CheckOutDate = br.Booking.CheckOutDate,
                    GuestName = br.Booking.User.FullName ?? "N/A",
                    Price = br.PricePerNight,
                    Status = br.Booking.Status.ToString()
                })
                .AsNoTracking()
                .ToListAsync();

            return Result.Success<IEnumerable<SubUnitBookingHistoryResponse>>(bookings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subunit booking history");
            return Result.Failure<IEnumerable<SubUnitBookingHistoryResponse>>(
                new Error("GetHistoryFailed", "Failed to get booking history", 500));
        }
    }

    // =========================================================================
    // AMENITIES MANAGEMENT
    // =========================================================================

    public async Task<Result<IEnumerable<AmenityResponse>>> GetAvailableAmenitiesAsync()
    {
        try
        {
            var amenities = await _context.Amenities
                .OrderBy(a => a.Category).ThenBy(a => a.Name)
                .AsNoTracking()
                .ToListAsync();

            return Result.Success(amenities.Select(a => new AmenityResponse
            {
                Id = a.Id,
                Name = a.Name,
                Category = a.Category,
                IsAvailable = true
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available amenities");
            return Result.Failure<IEnumerable<AmenityResponse>>(
                new Error("GetAmenitiesFailed", "Failed to get amenities", 500));
        }
    }

    public async Task<Result<IEnumerable<AmenityResponse>>> GetUnitAmenitiesAsync(
        string userId, int unitId)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure<IEnumerable<AmenityResponse>>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var amenities = await _context.UnitAmenities
                .Include(ua => ua.Amenity)
                .Where(ua => ua.UnitId == unitId)
                .AsNoTracking()
                .Select(ua => new AmenityResponse
                {
                    Id = ua.Amenity.Id,
                    Name = ua.Amenity.Name,
                    Category = ua.Amenity.Category,
                    IsAvailable = ua.IsAvailable
                })
                .ToListAsync();

            return Result.Success<IEnumerable<AmenityResponse>>(amenities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unit amenities");
            return Result.Failure<IEnumerable<AmenityResponse>>(
                new Error("GetAmenitiesFailed", "Failed to get unit amenities", 500));
        }
    }

    public async Task<Result> UpdateUnitAmenitiesAsync(
        string userId, int unitId, UpdateAmenitiesRequest request)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure(new Error("NoAccess", "You do not have access to this unit", 403));

            var existing = await _context.UnitAmenities.Where(ua => ua.UnitId == unitId).ToListAsync();
            _context.UnitAmenities.RemoveRange(existing);

            await _context.UnitAmenities.AddRangeAsync(request.AmenityIds.Select(id =>
                new Domain.Entities.UnitAmenity
                {
                    UnitId = unitId,
                    AmenityId = id,
                    IsAvailable = !request.UnavailableAmenityIds?.Contains(id) ?? true
                }));

            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating unit amenities");
            return Result.Failure(new Error("UpdateFailed", "Failed to update amenities", 500));
        }
    }

    public async Task<Result<IEnumerable<AmenityResponse>>> GetSubUnitAmenitiesAsync(
        string userId, int subUnitId)
    {
        try
        {
            var unitId = await _context.SubUnits
                .Where(s => s.Id == subUnitId && !s.IsDeleted)
                .Select(s => (int?)s.UnitId)
                .FirstOrDefaultAsync();

            if (unitId == null)
                return Result.Failure<IEnumerable<AmenityResponse>>(
                    new Error("NotFound", "SubUnit not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(userId, unitId.Value);
            if (!hasAccess.Value)
                return Result.Failure<IEnumerable<AmenityResponse>>(
                    new Error("NoAccess", "You do not have access to this subunit", 403));

            var amenities = await _context.SubUniteAmenities
                .Include(sa => sa.Amenity)
                .Where(sa => sa.SubUnitId == subUnitId)
                .AsNoTracking()
                .Select(sa => new AmenityResponse
                {
                    Id = sa.Amenity.Id,
                    Name = sa.Amenity.Name,
                    Category = sa.Amenity.Category,
                    IsAvailable = sa.IsAvailable
                })
                .ToListAsync();

            return Result.Success<IEnumerable<AmenityResponse>>(amenities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subunit amenities");
            return Result.Failure<IEnumerable<AmenityResponse>>(
                new Error("GetAmenitiesFailed", "Failed to get subunit amenities", 500));
        }
    }

    public async Task<Result> UpdateSubUnitAmenitiesAsync(
        string userId, int subUnitId, UpdateAmenitiesRequest request)
    {
        try
        {
            var unitId = await _context.SubUnits
                .Where(s => s.Id == subUnitId && !s.IsDeleted)
                .Select(s => (int?)s.UnitId)
                .FirstOrDefaultAsync();

            if (unitId == null)
                return Result.Failure(new Error("NotFound", "SubUnit not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(userId, unitId.Value);
            if (!hasAccess.Value)
                return Result.Failure(new Error("NoAccess", "You do not have access to this subunit", 403));

            var existing = await _context.SubUniteAmenities
                .Where(sa => sa.SubUnitId == subUnitId).ToListAsync();
            _context.SubUniteAmenities.RemoveRange(existing);

            await _context.SubUniteAmenities.AddRangeAsync(request.AmenityIds.Select(id =>
                new SubUniteAmenity
                {
                    SubUnitId = subUnitId,
                    AmenityId = id,
                    IsAvailable = !request.UnavailableAmenityIds?.Contains(id) ?? true
                }));

            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subunit amenities");
            return Result.Failure(new Error("UpdateFailed", "Failed to update amenities", 500));
        }
    }

    // =========================================================================
    // IMAGE MANAGEMENT
    // =========================================================================

    public async Task<Result<IEnumerable<ImageDetailResponse>>> GetUnitImagesAsync(
        string userId, int unitId)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure<IEnumerable<ImageDetailResponse>>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var images = await _context.Set<Domain.Entities.UnitImage>()
                .Where(i => i.UnitId == unitId && !i.IsDeleted)
                .OrderBy(i => i.DisplayOrder)
                .Select(i => new ImageDetailResponse
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    ThumbnailUrl = i.ThumbnailUrl,
                    IsPrimary = i.IsPrimary,
                    DisplayOrder = i.DisplayOrder,
                    Caption = i.Caption,
                    AltText = i.AltText,
                    Width = i.Width,
                    Height = i.Height,
                    FileSizeBytes = i.FileSizeBytes,
                    UploadedAt = i.UploadedAt
                })
                .AsNoTracking()
                .ToListAsync();

            return Result.Success<IEnumerable<ImageDetailResponse>>(images);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unit images for user {UserId}", userId);
            return Result.Failure<IEnumerable<ImageDetailResponse>>(
                new Error("GetImagesFailed", "Failed to get unit images", 500));
        }
    }

    public async Task<Result> UpdateImageOrderAsync(
        string userId, int unitId, UpdateImageOrderRequest request)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure(new Error("NoAccess", "You do not have access to this unit", 403));

            var images = await _context.Set<Domain.Entities.UnitImage>()
                .Where(i => i.UnitId == unitId && !i.IsDeleted)
                .ToListAsync();

            if (request.PrimaryImageId.HasValue)
            {
                foreach (var img in images) img.IsPrimary = false;
                var primary = images.FirstOrDefault(i => i.Id == request.PrimaryImageId.Value);
                if (primary != null) primary.IsPrimary = true;
            }

            if (request.ImageOrders?.Any() == true)
                foreach (var order in request.ImageOrders)
                {
                    var img = images.FirstOrDefault(i => i.Id == order.ImageId);
                    if (img != null) img.DisplayOrder = order.Order;
                }

            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating image order for user {UserId}", userId);
            return Result.Failure(new Error("UpdateOrderFailed", "Failed to update image order", 500));
        }
    }

    public async Task<Result> DeleteUnitImageAsync(string userId, int imageId)
    {
        try
        {
            var image = await _context.Set<Domain.Entities.UnitImage>()
                .FirstOrDefaultAsync(i => i.Id == imageId && !i.IsDeleted);

            if (image == null)
                return Result.Failure(new Error("NotFound", "Image not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(userId, image.UnitId);
            if (!hasAccess.Value)
                return Result.Failure(new Error("NoAccess", "You do not have access to this image", 403));

            image.IsDeleted = true;
            image.DeletedAt = DateTime.UtcNow.AddHours(3);
            image.DeletedByUserId = userId;
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting unit image for user {UserId}", userId);
            return Result.Failure(new Error("DeleteImageFailed", "Failed to delete image", 500));
        }
    }

    public async Task<Result<IEnumerable<ImageDetailResponse>>> GetSubUnitImagesAsync(
        string userId, int subUnitId)
    {
        try
        {
            var unitId = await _context.SubUnits
                .Where(s => s.Id == subUnitId && !s.IsDeleted)
                .Select(s => (int?)s.UnitId)
                .FirstOrDefaultAsync();

            if (unitId == null)
                return Result.Failure<IEnumerable<ImageDetailResponse>>(
                    new Error("NotFound", "SubUnit not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(userId, unitId.Value);
            if (!hasAccess.Value)
                return Result.Failure<IEnumerable<ImageDetailResponse>>(
                    new Error("NoAccess", "You do not have access to this subunit", 403));

            var images = await _context.Set<Domain.Entities.SubUnitImage>()
                .Where(i => i.SubUnitId == subUnitId && !i.IsDeleted)
                .OrderBy(i => i.DisplayOrder)
                .Select(i => new ImageDetailResponse
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    ThumbnailUrl = i.ThumbnailUrl,
                    IsPrimary = i.IsPrimary,
                    DisplayOrder = i.DisplayOrder,
                    Caption = i.Caption,
                    AltText = i.AltText,
                    Width = i.Width,
                    Height = i.Height,
                    FileSizeBytes = i.FileSizeBytes,
                    UploadedAt = i.UploadedAt
                })
                .AsNoTracking()
                .ToListAsync();

            return Result.Success<IEnumerable<ImageDetailResponse>>(images);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subunit images for user {UserId}", userId);
            return Result.Failure<IEnumerable<ImageDetailResponse>>(
                new Error("GetImagesFailed", "Failed to get subunit images", 500));
        }
    }

    public async Task<Result> DeleteSubUnitImageAsync(string userId, int imageId)
    {
        try
        {
            var image = await _context.Set<Domain.Entities.SubUnitImage>()
                .Include(i => i.SubUnit)
                .FirstOrDefaultAsync(i => i.Id == imageId && !i.IsDeleted);

            if (image == null)
                return Result.Failure(new Error("NotFound", "Image not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(userId, image.SubUnit.UnitId);
            if (!hasAccess.Value)
                return Result.Failure(new Error("NoAccess", "You do not have access to this image", 403));

            image.IsDeleted = true;
            image.DeletedAt = DateTime.UtcNow.AddHours(3);
            image.DeletedByUserId = userId;
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting subunit image for user {UserId}", userId);
            return Result.Failure(new Error("DeleteImageFailed", "Failed to delete image", 500));
        }
    }

    // =========================================================================
    // BULK OPERATIONS
    // =========================================================================

    public async Task<Result> BulkUpdateSubUnitAvailabilityAsync(
        string userId, BulkAvailabilityUpdateRequest request)
    {
        try
        {
            var adminUnitIds = await GetUserAdminUnitIdsAsync(userId);
            if (!adminUnitIds.Any())
                return Result.Failure(new Error("NoAccess", "User is not a hotel administrator", 403));

            var validSubUnitIds = await _context.SubUnits
                .Where(s => request.SubUnitIds.Contains(s.Id) &&
                            adminUnitIds.Contains(s.UnitId) &&
                            !s.IsDeleted)
                .Select(s => s.Id)
                .ToListAsync();

            if (validSubUnitIds.Count != request.SubUnitIds.Count)
                return Result.Failure(new Error("InvalidSubUnits", "Some subunits not found or not accessible", 400));

            var now = DateTime.UtcNow.AddHours(3);
            await _context.Set<SubUnitAvailability>().AddRangeAsync(
                request.SubUnitIds.Select(id => new SubUnitAvailability
                {
                    SubUnitId = id,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    IsAvailable = request.IsAvailable,
                    Reason = request.Reason,
                    SpecialPrice = request.SpecialPrice,
                    WeekendPrice = request.WeekendPrice,
                    UpdatedByUserId = userId,
                    CreatedAt = now
                }));

            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating availability for user {UserId}", userId);
            return Result.Failure(new Error("BulkUpdateFailed", "Failed to bulk update availability", 500));
        }
    }

    // =========================================================================
    // OFFERS MANAGEMENT
    // =========================================================================

    public string GetCloudFrontUrl(string s3Key)
    {
        if (string.IsNullOrEmpty(s3Key)) return string.Empty;
        return string.IsNullOrEmpty(CloudFrontUrl)
            ? $"https://{BucketName}.s3.amazonaws.com/{s3Key}"
            : $"https://{CloudFrontUrl}/{s3Key}";
    }

    public async Task<Result<OfferResponse>> CreateUnitOfferAsync(
        string userId, CreateOfferRequest request)
    {
        try
        {
            // Lightweight — just grab the first unit ID
            var unitId = await _context.Set<UniteAdmin>()
                .Where(a => a.UserId == userId && a.IsActive)
                .Select(a => (int?)a.UnitId)
                .FirstOrDefaultAsync();

            if (unitId == null)
                return Result.Failure<OfferResponse>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            string? imageUrl = null;
            if (request.ImageFile != null)
            {
                var uploadResult = await UploadOfferImageAsync(request.ImageFile, userId);
                if (uploadResult.IsSuccess) imageUrl = uploadResult.Value;
            }

            var offer = new Offer
            {
                Title = request.Title,
                UserId = userId,
                Description = request.Description,
                ImageUrl = GetCloudFrontUrl(imageUrl ?? string.Empty),
                UnitId = unitId,
                DiscountPercentage = request.DiscountPercentage,
                DiscountAmount = request.DiscountAmount,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsActive = true,
                IsFeatured = false,
                CreatedAt = DateTime.UtcNow.AddHours(3)
            };

            _context.Offers.Add(offer);
            await _context.SaveChangesAsync();

            backgroundJobClient.Enqueue<OfferEmailJob>(
            j => j.SendOfferEmailsAsync(offer.Id));

            var unitName = await _context.Units
                .Where(u => u.Id == unitId)
                .Select(u => u.Name)
                .FirstOrDefaultAsync() ?? string.Empty;

            return Result.Success(new OfferResponse
            {
                Id = offer.Id,
                Title = offer.Title,
                Description = offer.Description,
                ImageUrl = offer.ImageUrl,
                UnitId = offer.UnitId,
                UnitName = unitName,
                DiscountPercentage = offer.DiscountPercentage,
                DiscountAmount = offer.DiscountAmount,
                StartDate = offer.StartDate,
                EndDate = offer.EndDate,
                IsActive = offer.IsActive,
                IsFeatured = offer.IsFeatured,
                CreatedAt = offer.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating offer");
            return Result.Failure<OfferResponse>(
                new Error("CreateOfferFailed", "Failed to create offer", 500));
        }
    }

    public async Task<Result<string>> UploadOfferImageAsync(IFormFile image, string userId)
    {
        try
        {
            var transferUtility = new TransferUtility(s3Client);
            var timestamp = DateTime.UtcNow.Ticks;
            var key = $"offers/{userId}/{timestamp}.webp";

            using var stream = new MemoryStream();
            await ConvertToWebpAsync(image.OpenReadStream(), stream, 75);
            stream.Position = 0;

            await transferUtility.UploadAsync(new TransferUtilityUploadRequest
            {
                InputStream = stream,
                Key = key,
                BucketName = BucketName,
                ContentType = "image/webp",
                CannedACL = S3CannedACL.Private
            });

            return Result.Success(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading offer image");
            return Result.Failure<string>(new Error("UploadFailed", "Failed to upload image", 500));
        }
    }

    private async Task ConvertToWebpAsync(Stream input, Stream output, int quality)
    {
        await Task.Run(() =>
        {
            using var image = Image.Load(input);
            image.Save(output, new WebpEncoder
            {
                Quality = 75,
                Method = WebpEncodingMethod.Fastest,
                SkipMetadata = true
            });
        });
    }

    public async Task<Result<IEnumerable<OfferResponse>>> GetMyUnitOffersAsync(string userId)
    {
        try
        {
            var unitIds = await GetUserAdminUnitIdsAsync(userId);
            if (!unitIds.Any())
                return Result.Failure<IEnumerable<OfferResponse>>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var offers = await _context.Offers
                .Include(o => o.Unit)
                .Where(o => o.UnitId.HasValue && unitIds.Contains(o.UnitId.Value))
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OfferResponse
                {
                    Id = o.Id,
                    Title = o.Title,
                    Description = o.Description,
                    ImageUrl = o.ImageUrl,
                    UnitId = o.UnitId,
                    UnitName = o.Unit!.Name,
                    DiscountPercentage = o.DiscountPercentage,
                    DiscountAmount = o.DiscountAmount,
                    StartDate = o.StartDate,
                    EndDate = o.EndDate,
                    IsActive = o.IsActive,
                    IsFeatured = o.IsFeatured,
                    CreatedAt = o.CreatedAt
                })
                .AsNoTracking()
                .ToListAsync();

            return Result.Success<IEnumerable<OfferResponse>>(offers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting offers for user {UserId}", userId);
            return Result.Failure<IEnumerable<OfferResponse>>(
                new Error("GetOffersFailed", "Failed to retrieve offers", 500));
        }
    }

    public async Task<Result<OfferResponse>> UpdateOfferAsync(
        string userId, int offerId, UpdateOfferRequest request)
    {
        try
        {
            var offer = await _context.Offers.Include(o => o.Unit)
                .FirstOrDefaultAsync(o => o.Id == offerId);

            if (offer == null)
                return Result.Failure<OfferResponse>(new Error("NotFound", "Offer not found", 404));

            if (!offer.UnitId.HasValue)
                return Result.Failure<OfferResponse>(new Error("InvalidOffer", "This is a system-wide offer", 403));

            var hasAccess = await IsAdminOfUnitAsync(userId, offer.UnitId.Value);
            if (!hasAccess.Value)
                return Result.Failure<OfferResponse>(
                    new Error("NoAccess", "You do not have access to this offer", 403));

            if (request.Title != null) offer.Title = request.Title;
            if (request.Description != null) offer.Description = request.Description;
            if (request.DiscountPercentage.HasValue) offer.DiscountPercentage = request.DiscountPercentage;
            if (request.DiscountAmount.HasValue) offer.DiscountAmount = request.DiscountAmount;
            if (request.StartDate.HasValue) offer.StartDate = request.StartDate.Value;
            if (request.EndDate.HasValue) offer.EndDate = request.EndDate.Value;
            if (request.IsActive.HasValue) offer.IsActive = request.IsActive.Value;

            if (request.ImageFile != null)
            {
                var uploadResult = await UploadOfferImageAsync(request.ImageFile, userId);
                if (uploadResult.IsSuccess) offer.ImageUrl = uploadResult.Value;
            }

            await _context.SaveChangesAsync();

            return Result.Success(new OfferResponse
            {
                Id = offer.Id,
                Title = offer.Title,
                Description = offer.Description,
                ImageUrl = offer.ImageUrl,
                UnitId = offer.UnitId,
                UnitName = offer.Unit?.Name ?? string.Empty,
                DiscountPercentage = offer.DiscountPercentage,
                DiscountAmount = offer.DiscountAmount,
                StartDate = offer.StartDate,
                EndDate = offer.EndDate,
                IsActive = offer.IsActive,
                IsFeatured = offer.IsFeatured,
                CreatedAt = offer.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating offer {OfferId}", offerId);
            return Result.Failure<OfferResponse>(
                new Error("UpdateOfferFailed", "Failed to update offer", 500));
        }
    }

    public async Task<Result> DeleteOfferAsync(string userId, int offerId)
    {
        try
        {
            var offer = await _context.Offers.FirstOrDefaultAsync(o => o.Id == offerId);
            if (offer == null)
                return Result.Failure(new Error("NotFound", "Offer not found", 404));

            if (!offer.UnitId.HasValue)
                return Result.Failure(new Error("InvalidOffer", "Cannot delete system-wide offer", 403));

            var hasAccess = await IsAdminOfUnitAsync(userId, offer.UnitId.Value);
            if (!hasAccess.Value)
                return Result.Failure(new Error("NoAccess", "You do not have access to this offer", 403));

            _context.Offers.Remove(offer);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting offer {OfferId}", offerId);
            return Result.Failure(new Error("DeleteOfferFailed", "Failed to delete offer", 500));
        }
    }

    // =========================================================================
    // CUSTOM POLICIES MANAGEMENT
    // =========================================================================

    public async Task<Result<IEnumerable<UnitCustomPolicyResponse>>> GetUnitCustomPoliciesAsync(
        string userId, int unitId)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure<IEnumerable<UnitCustomPolicyResponse>>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var policies = await _context.Set<UnitCustomPolicy>()
                .Where(p => p.UnitId == unitId && p.IsActive)
                .OrderBy(p => p.DisplayOrder).ThenBy(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            return Result.Success(policies.Select(p => new UnitCustomPolicyResponse
            {
                Id = p.Id,
                UnitId = p.UnitId,
                Title = p.Title,
                Description = p.Description,
                Category = p.Category,
                DisplayOrder = p.DisplayOrder,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting custom policies for unit {UnitId}", unitId);
            return Result.Failure<IEnumerable<UnitCustomPolicyResponse>>(
                new Error("GetPoliciesFailed", "Failed to retrieve custom policies", 500));
        }
    }

    public async Task<Result<UnitCustomPolicyResponse>> CreateUnitCustomPolicyAsync(
        string userId, int unitId, CreateUnitCustomPolicyRequest request)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure<UnitCustomPolicyResponse>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var policy = new UnitCustomPolicy
            {
                UnitId = unitId,
                Title = request.Title,
                Description = request.Description,
                Category = request.Category,
                DisplayOrder = request.DisplayOrder,
                IsActive = true,
                UserId = userId,
                CreatedAt = DateTime.UtcNow.AddHours(3)
            };

            await _context.Set<UnitCustomPolicy>().AddAsync(policy);
            await _context.SaveChangesAsync();

            return Result.Success(new UnitCustomPolicyResponse
            {
                Id = policy.Id,
                UnitId = policy.UnitId,
                Title = policy.Title,
                Description = policy.Description,
                Category = policy.Category,
                DisplayOrder = policy.DisplayOrder,
                IsActive = policy.IsActive,
                CreatedAt = policy.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating custom policy for unit {UnitId}", unitId);
            return Result.Failure<UnitCustomPolicyResponse>(
                new Error("error", "Failed to create custom policy", 500));
        }
    }

    public async Task<Result<UnitCustomPolicyResponse>> UpdateUnitCustomPolicyAsync(
        string userId, int policyId, UpdateUnitCustomPolicyRequest request)
    {
        try
        {
            var policy = await _context.Set<UnitCustomPolicy>()
                .FirstOrDefaultAsync(p => p.Id == policyId);

            if (policy == null)
                return Result.Failure<UnitCustomPolicyResponse>(
                    new Error("NotFound", "Custom policy not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(userId, policy.UnitId);
            if (!hasAccess.Value)
                return Result.Failure<UnitCustomPolicyResponse>(
                    new Error("NoAccess", "You do not have access to this policy", 403));

            if (request.Title != null) policy.Title = request.Title;
            if (request.Description != null) policy.Description = request.Description;
            if (request.Category != null) policy.Category = request.Category;
            if (request.DisplayOrder.HasValue) policy.DisplayOrder = request.DisplayOrder.Value;
            if (request.IsActive.HasValue) policy.IsActive = request.IsActive.Value;
            policy.UpdatedAt = DateTime.UtcNow.AddHours(3);

            await _context.SaveChangesAsync();

            return Result.Success(new UnitCustomPolicyResponse
            {
                Id = policy.Id,
                UnitId = policy.UnitId,
                Title = policy.Title,
                Description = policy.Description,
                Category = policy.Category,
                DisplayOrder = policy.DisplayOrder,
                IsActive = policy.IsActive,
                CreatedAt = policy.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating custom policy {PolicyId}", policyId);
            return Result.Failure<UnitCustomPolicyResponse>(
                new Error("UpdatePolicyFailed", "Failed to update custom policy", 500));
        }
    }

    public async Task<Result> DeleteUnitCustomPolicyAsync(string userId, int policyId)
    {
        try
        {
            var policy = await _context.Set<UnitCustomPolicy>()
                .FirstOrDefaultAsync(p => p.Id == policyId);

            if (policy == null)
                return Result.Failure(new Error("NotFound", "Custom policy not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(userId, policy.UnitId);
            if (!hasAccess.Value)
                return Result.Failure(new Error("NoAccess", "You do not have access to this policy", 403));

            _context.Set<UnitCustomPolicy>().Remove(policy);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting custom policy {PolicyId}", policyId);
            return Result.Failure(new Error("DeletePolicyFailed", "Failed to delete custom policy", 500));
        }
    }

    public async Task<Result> ReorderUnitCustomPoliciesAsync(
        string userId, int unitId, List<int> policyIds)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure(new Error("NoAccess", "You do not have access to this unit", 403));

            var policies = await _context.Set<UnitCustomPolicy>()
                .Where(p => p.UnitId == unitId && policyIds.Contains(p.Id))
                .ToListAsync();

            var now = DateTime.UtcNow.AddHours(3);
            foreach (var (policyId, index) in policyIds.Select((id, i) => (id, i)))
            {
                var policy = policies.FirstOrDefault(p => p.Id == policyId);
                if (policy != null) { policy.DisplayOrder = index; policy.UpdatedAt = now; }
            }

            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering custom policies for unit {UnitId}", unitId);
            return Result.Failure(new Error("ReorderFailed", "Failed to reorder custom policies", 500));
        }
    }

    #region PACKAGE MANAGEMENT

    public async Task<Result<IEnumerable<PackageResponse>>> GetUnitPackagesAsync(
        string userId, int unitId)
    {
        var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
        if (!hasAccess.Value)
            return Result.Failure<IEnumerable<PackageResponse>>(
                new Error("NoAccess", "You do not have access to this unit", 403));

        var packages = await _context.Set<Package>()
            .Where(p => p.UnitId == unitId)
            .AsNoTracking()
            .ToListAsync();

        return Result.Success(packages.Select(MapToPackageResponse));
    }

    public async Task<Result<PackageResponse>> CreatePackageAsync(
        string userId, int unitId, CreatePackageRequest request)
    {
        var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
        if (!hasAccess.Value)
            return Result.Failure<PackageResponse>(
                new Error("NoAccess", "You do not have access to this unit", 403));

        var package = new Package
        {
            UnitId = unitId,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            FeaturesJson = JsonSerializer.Serialize(request.Features),
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow.AddHours(3)
        };

        _context.Set<Package>().Add(package);
        await _context.SaveChangesAsync();

        return Result.Success(MapToPackageResponse(package));
    }

    public async Task<Result<PackageResponse>> UpdatePackageAsync(
        string userId, int packageId, UpdatePackageRequest request)
    {
        var package = await _context.Set<Package>()
            .FirstOrDefaultAsync(p => p.Id == packageId);

        if (package == null)
            return Result.Failure<PackageResponse>(
                new Error("NotFound", "Package not found", 404));

        var hasAccess = await IsAdminOfUnitAsync(userId, package.UnitId);
        if (!hasAccess.Value)
            return Result.Failure<PackageResponse>(
                new Error("NoAccess", "You do not have access to this package", 403));

        if (request.Name != null) package.Name = request.Name;
        if (request.Description != null) package.Description = request.Description;
        if (request.Price.HasValue) package.Price = request.Price.Value;
        if (request.Features != null) package.FeaturesJson = JsonSerializer.Serialize(request.Features);
        if (request.IsActive.HasValue) package.IsActive = request.IsActive.Value;
        package.UpdatedAt = DateTime.UtcNow.AddHours(3);

        await _context.SaveChangesAsync();
        return Result.Success(MapToPackageResponse(package));
    }

    public async Task<Result> DeletePackageAsync(string userId, int packageId)
    {
        var package = await _context.Set<Package>()
            .FirstOrDefaultAsync(p => p.Id == packageId);

        if (package == null)
            return Result.Failure(new Error("NotFound", "Package not found", 404));

        var hasAccess = await IsAdminOfUnitAsync(userId, package.UnitId);
        if (!hasAccess.Value)
            return Result.Failure(new Error("NoAccess", "You do not have access to this package", 403));

        _context.Set<Package>().Remove(package);
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    private static PackageResponse MapToPackageResponse(Package p) => new()
    {
        Id = p.Id,
        UnitId = p.UnitId,
        Name = p.Name,
        Description = p.Description,
        Price = p.Price,
        Features = JsonSerializer.Deserialize<List<string>>(p.FeaturesJson) ?? [],
        IsActive = p.IsActive,
        CreatedAt = p.CreatedAt
    };

    #endregion

    public async Task<Result<CurrencyResponse>> GetUnitCurrencyAsync(string userId, int unitId)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure<CurrencyResponse>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            return await currencyService.GetUnitCurrencyAsync(unitId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting currency for unit {UnitId}", unitId);
            return Result.Failure<CurrencyResponse>(
                new Error("GetCurrencyFailed", "Failed to retrieve unit currency", 500));
        }
    }

    // ── 3. Replace UpdateUnitCurrencyAsync ───────────────────────────────────────

    public async Task<Result> UpdateUnitCurrencyAsync(
        string userId, int unitId, SetUnitCurrencyRequest request)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            return await currencyService.SetUnitCurrencyAsync(unitId, request.CurrencyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating currency for unit {UnitId}", unitId);
            return Result.Failure(
                new Error("UpdateCurrencyFailed", "Failed to update unit currency", 500));
        }
    }

    // =========================================================================
    // UNIT RANK MANAGEMENT
    // =========================================================================

    public async Task<Result> UpdateUnitRankAsync(string userId, int unitId, int rank)
    {
        try
        {
            if (rank < 1 || rank > 5)
                return Result.Failure(new Error("InvalidRank", "Rank must be between 1 and 5 stars", 400));

            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure(new Error("NoAccess", "You do not have access to this unit", 403));

            var unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);
            if (unit == null)
                return Result.Failure(new Error("NotFound", "Unit not found", 404));

            unit.Rank = rank;
            unit.UpdatedAt = DateTime.UtcNow.AddHours(3);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating unit rank");
            return Result.Failure(new Error("UpdateRankFailed", "Failed to update unit rank", 500));
        }
    }

    public async Task<Result<int?>> GetUnitRankAsync(string userId, int unitId)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure<int?>(new Error("NoAccess", "You do not have access to this unit", 403));

            var rank = await _context.Units
                .Where(u => u.Id == unitId && !u.IsDeleted)
                .Select(u => u.Rank)
                .FirstOrDefaultAsync();

            return Result.Success(rank);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unit rank");
            return Result.Failure<int?>(new Error("GetRankFailed", "Failed to retrieve unit rank", 500));
        }
    }

    // =========================================================================
    // UNIT OPTION VALUES
    // =========================================================================

    public async Task<Result<IEnumerable<UnitOptionValueResponse>>> GetUnitOptionValuesAsync(
        string userId, int unitId)
    {
        try
        {
            var access = await IsAdminOfUnitAsync(userId, unitId);
            if (!access.Value)
                return Result.Failure<IEnumerable<UnitOptionValueResponse>>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var unitTypeId = await _context.Units
                .Where(u => u.Id == unitId && !u.IsDeleted)
                .Select(u => (int?)u.UnitTypeId)
                .FirstOrDefaultAsync();

            if (unitTypeId == null)
                return Result.Failure<IEnumerable<UnitOptionValueResponse>>(
                    new Error("NotFound", "Unit not found", 404));

            var typeOptions = await _context.Set<UnitTypeOption>()
                .Include(o => o.Selections.OrderBy(s => s.DisplayOrder))
                .Where(o => o.UnitTypeId == unitTypeId && o.IsActive)
                .OrderBy(o => o.DisplayOrder).ThenBy(o => o.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            var savedValues = await _context.Set<UnitOptionValue>()
                .Where(v => v.UnitId == unitId)
                .AsNoTracking()
                .ToListAsync();

            var responses = typeOptions.Select(opt => new UnitOptionValueResponse
            {
                UnitTypeOptionId = opt.Id,
                OptionName = opt.Name,
                InputType = opt.InputType.ToString(),
                IsRequired = opt.IsRequired,
                Values = savedValues.Where(v => v.UnitTypeOptionId == opt.Id).Select(v => v.Value).ToList(),
                AvailableSelections = opt.InputType is OptionInputType.Select or OptionInputType.MultiSelect
                    ? opt.Selections.Select(s => new TypeOptionSelectionDto
                    { Id = s.Id, Value = s.Value, DisplayOrder = s.DisplayOrder }).ToList()
                    : null
            });

            return Result.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unit option values for unit {UnitId}", unitId);
            return Result.Failure<IEnumerable<UnitOptionValueResponse>>(
                new Error("GetValuesFailed", "Failed to retrieve unit option values", 500));
        }
    }

    public async Task<Result> SaveUnitOptionValuesAsync(
        string userId, int unitId, SaveUnitOptionValuesRequest request)
    {
        try
        {
            var access = await IsAdminOfUnitAsync(userId, unitId);
            if (!access.Value)
                return Result.Failure(new Error("NoAccess", "You do not have access to this unit", 403));

            var unitTypeId = await _context.Units
                .Where(u => u.Id == unitId && !u.IsDeleted)
                .Select(u => (int?)u.UnitTypeId)
                .FirstOrDefaultAsync();

            if (unitTypeId == null)
                return Result.Failure(new Error("NotFound", "Unit not found", 404));

            var optionIds = request.Options.Select(o => o.UnitTypeOptionId).Distinct().ToList();
            var validOptions = await _context.Set<UnitTypeOption>()
                .Where(o => optionIds.Contains(o.Id) && o.UnitTypeId == unitTypeId && o.IsActive)
                .ToListAsync();

            if (validOptions.Count != optionIds.Count)
                return Result.Failure(new Error("InvalidOptions",
                    "One or more option IDs are invalid or do not belong to this unit's type", 400));

            foreach (var opt in validOptions.Where(o => o.IsRequired))
            {
                var input = request.Options.FirstOrDefault(i => i.UnitTypeOptionId == opt.Id);
                if (input is null || input.Values.Count == 0 || input.Values.All(string.IsNullOrWhiteSpace))
                    return Result.Failure(new Error("RequiredOptionMissing",
                        $"Option '{opt.Name}' is required and must have a value", 400));
            }

            var now = DateTime.UtcNow.AddHours(3);
            foreach (var input in request.Options)
            {
                var existing = await _context.Set<UnitOptionValue>()
                    .Where(v => v.UnitId == unitId && v.UnitTypeOptionId == input.UnitTypeOptionId)
                    .ToListAsync();

                _context.Set<UnitOptionValue>().RemoveRange(existing);

                var newValues = input.Values
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .Select(v => new UnitOptionValue
                    {
                        UnitId = unitId,
                        UnitTypeOptionId = input.UnitTypeOptionId,
                        Value = v,
                        CreatedAt = now
                    }).ToList();

                if (newValues.Count > 0)
                    _context.Set<UnitOptionValue>().AddRange(newValues);
            }

            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving unit option values for unit {UnitId}", unitId);
            return Result.Failure(new Error("SaveValuesFailed", "Failed to save unit option values", 500));
        }
    }

    // =========================================================================
    // SUBUNIT OPTION VALUES
    // =========================================================================

    public async Task<Result<IEnumerable<SubUnitOptionValueResponse>>> GetSubUnitOptionValuesAsync(
        string userId, int subUnitId)
    {
        try
        {
            var subUnit = await _context.SubUnits
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

            if (subUnit is null)
                return Result.Failure<IEnumerable<SubUnitOptionValueResponse>>(
                    new Error("NotFound", "SubUnit not found", 404));

            var access = await IsAdminOfUnitAsync(userId, subUnit.UnitId);
            if (!access.Value)
                return Result.Failure<IEnumerable<SubUnitOptionValueResponse>>(
                    new Error("NoAccess", "You do not have access to this subunit", 403));

            var typeOptions = await _context.Set<SubUnitTypeOption>()
                .Include(o => o.Selections.OrderBy(s => s.DisplayOrder))
                .Where(o => o.SubUnitTypeId == subUnit.SubUnitTypeId && o.IsActive)
                .OrderBy(o => o.DisplayOrder).ThenBy(o => o.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            var savedValues = await _context.Set<SubUnitOptionValue>()
                .Where(v => v.SubUnitId == subUnitId)
                .AsNoTracking()
                .ToListAsync();

            var responses = typeOptions.Select(opt => new SubUnitOptionValueResponse
            {
                SubUnitTypeOptionId = opt.Id,
                OptionName = opt.Name,
                InputType = opt.InputType.ToString(),
                IsRequired = opt.IsRequired,
                Values = savedValues.Where(v => v.SubUnitTypeOptionId == opt.Id).Select(v => v.Value).ToList(),
                AvailableSelections = opt.InputType is OptionInputType.Select or OptionInputType.MultiSelect
                    ? opt.Selections.Select(s => new TypeOptionSelectionDto
                    { Id = s.Id, Value = s.Value, DisplayOrder = s.DisplayOrder }).ToList()
                    : null
            });

            return Result.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subunit option values for subunit {SubUnitId}", subUnitId);
            return Result.Failure<IEnumerable<SubUnitOptionValueResponse>>(
                new Error("GetValuesFailed", "Failed to retrieve subunit option values", 500));
        }
    }

    public async Task<Result> SaveSubUnitOptionValuesAsync(
        string userId, int subUnitId, SaveSubUnitOptionValuesRequest request)
    {
        try
        {
            var subUnit = await _context.SubUnits
                .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

            if (subUnit is null)
                return Result.Failure(new Error("NotFound", "SubUnit not found", 404));

            var access = await IsAdminOfUnitAsync(userId, subUnit.UnitId);
            if (!access.Value)
                return Result.Failure(new Error("NoAccess", "You do not have access to this subunit", 403));

            var optionIds = request.Options.Select(o => o.SubUnitTypeOptionId).Distinct().ToList();
            var validOptions = await _context.Set<SubUnitTypeOption>()
                .Where(o => optionIds.Contains(o.Id) &&
                            o.SubUnitTypeId == subUnit.SubUnitTypeId && o.IsActive)
                .ToListAsync();

            if (validOptions.Count != optionIds.Count)
                return Result.Failure(new Error("InvalidOptions",
                    "One or more option IDs are invalid or do not belong to this subunit's type", 400));

            foreach (var opt in validOptions.Where(o => o.IsRequired))
            {
                var input = request.Options.FirstOrDefault(i => i.SubUnitTypeOptionId == opt.Id);
                if (input is null || input.Values.Count == 0 || input.Values.All(string.IsNullOrWhiteSpace))
                    return Result.Failure(new Error("RequiredOptionMissing",
                        $"Option '{opt.Name}' is required and must have a value", 400));
            }

            var now = DateTime.UtcNow.AddHours(3);
            foreach (var input in request.Options)
            {
                var existing = await _context.Set<SubUnitOptionValue>()
                    .Where(v => v.SubUnitId == subUnitId && v.SubUnitTypeOptionId == input.SubUnitTypeOptionId)
                    .ToListAsync();

                _context.Set<SubUnitOptionValue>().RemoveRange(existing);

                var newValues = input.Values
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .Select(v => new SubUnitOptionValue
                    {
                        SubUnitId = subUnitId,
                        SubUnitTypeOptionId = input.SubUnitTypeOptionId,
                        Value = v,
                        CreatedAt = now
                    }).ToList();

                if (newValues.Count > 0)
                    _context.Set<SubUnitOptionValue>().AddRange(newValues);
            }

            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving subunit option values for subunit {SubUnitId}", subUnitId);
            return Result.Failure(new Error("SaveValuesFailed", "Failed to save subunit option values", 500));
        }
    }

    // =========================================================================
    // IMAGE UPLOAD
    // =========================================================================

    public async Task<Result<ImageDetailResponse>> UploadUnitImageAsync(
        string userId, IFormFile image, string? caption = null)
    {
        try
        {
            // Lightweight — just grab the first unit ID
            var unitId = await _context.Set<UniteAdmin>()
                .Where(a => a.UserId == userId && a.IsActive)
                .Select(a => (int?)a.UnitId)
                .FirstOrDefaultAsync();

            if (unitId == null)
                return Result.Failure<ImageDetailResponse>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var uploadResult = await service.UploadUnitImageAsync(image, unitId.Value, userId);
            if (!uploadResult.IsSuccess)
                return Result.Failure<ImageDetailResponse>(uploadResult.Error);

            var unitImage = new Domain.Entities.UnitImage
            {
                UnitId = unitId.Value,
                ImageUrl = uploadResult.Value.ImageUrl,
                S3Key = uploadResult.Value.S3Key,
                S3Bucket = "hujjzy-bucket",
                ThumbnailUrl = null,
                MediumUrl = null,
                Caption = caption,
                IsPrimary = false,
                DisplayOrder = 0,
                UploadedByUserId = userId,
                UploadedAt = DateTime.UtcNow.AddHours(3),
                ProcessingStatus = ImageProcessingStatus.Completed,
                MimeType = "image/webp"
            };

            _context.Set<Domain.Entities.UnitImage>().Add(unitImage);
            await _context.SaveChangesAsync();

            return Result.Success(new ImageDetailResponse
            {
                Id = unitImage.Id,
                ImageUrl = unitImage.ImageUrl,
                ThumbnailUrl = unitImage.ThumbnailUrl,
                IsPrimary = unitImage.IsPrimary,
                DisplayOrder = unitImage.DisplayOrder,
                Caption = unitImage.Caption,
                UploadedAt = unitImage.UploadedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading unit image for user {UserId}", userId);
            return Result.Failure<ImageDetailResponse>(
                new Error("UploadFailed", $"Failed to upload image: {ex.Message}", 500));
        }
    }

    // =========================================================================
    // HELPER / VALIDATION
    // =========================================================================

    public async Task<Result<bool>> IsAdminOfUnitAsync(string userId, int unitId)
    {
        var isAdmin = await _context.Set<UniteAdmin>()
            .AnyAsync(a => a.UnitId == unitId && a.UserId == userId && a.IsActive);
        return Result.Success(isAdmin);
    }

    public async Task<Result<bool>> IsBookingForMyUnitAsync(string userId, int bookingId)
    {
        var isAdmin = await _context.Bookings
            .Where(b => b.Id == bookingId)
            .AnyAsync(b => b.Unit.Admins.Any(a => a.UserId == userId && a.IsActive));
        return Result.Success(isAdmin);
    }

    private async Task<decimal> CalculateOccupancyRateAsync(
        List<int> unitIds, DateTime startDate, DateTime endDate)
    {
        var totalRoomNights = await _context.SubUnits
            .Where(s => unitIds.Contains(s.UnitId) && !s.IsDeleted)
            .CountAsync() * (endDate - startDate).Days;

        if (totalRoomNights == 0) return 0;

        var occupiedNights = await _context.BookingRooms
            .Where(br => unitIds.Contains(br.Room.UnitId) &&
                         br.Booking.CheckInDate < endDate &&
                         br.Booking.CheckOutDate > startDate &&
                         br.Booking.Status != BookingStatus.Cancelled)
            .SumAsync(br => (int?)br.NumberOfNights) ?? 0;

        return (decimal)occupiedNights / totalRoomNights * 100;
    }

    // =========================================================================
    // MAPPING METHODS
    // =========================================================================

    private UnitComprehensiveResponse MapToComprehensiveResponse(
        Domain.Entities.Unit unit, List<GeneralPolicy> policies, string? defaultCurrencyCode = null)
    {

        var customPolicies = unit.CustomPolicies?
            .Where(p => p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .Select(p => new CustomPolicyDetail
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Category = p.Category,
                DisplayOrder = p.DisplayOrder,
                IsActive = p.IsActive
            }).ToList() ?? [];

        return new UnitComprehensiveResponse
        {
            Rank = unit.Rank,
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
            UpdatedAt = unit.UpdatedAt,
            Currency = unit.Currency?.Code ?? defaultCurrencyCode,
            CustomPolicies = customPolicies,
            OptionValues = MapUnitOptionValues(unit.OptionValues),
            IsStandAlone = unit.UnitType.IsStandalone
        };
    }

    private UnitResponse MapToUnitResponse(Domain.Entities.Unit unit) => new()
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
        UpdatedAt = unit.UpdatedAt,
        Rank = unit.Rank
    };

    private BookingDetailsResponse MapToBookingDetailsResponse(Domain.Entities.Booking booking) => new()
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
        Payments = booking.Payments.Select(p => new Contracts.hoteladmincont.PaymentInfo
        {
            Id = p.Id,
            TransactionId = p.TransactionId,
            Amount = p.Amount,
            PaymentMethod = p.PaymentMethod.ToString(),
            Status = p.Status.ToString(),
            PaymentDate = p.PaymentDate
        }).ToList(),
        CreatedAt = booking.CreatedAt,
        UpdatedAt = booking.UpdatedAt,
        GuestFirstName = booking.GuestFirstName,
        GuestLastName = booking.GuestLastName,
        GuestEmail2 = booking.GuestEmail,
        GuestPhone2 = booking.GuestPhone,
        GuestSpecialRequirements = booking.SpecialRequests
    };

    private SubUnitComprehensiveDetail MapToSubUnitComprehensiveDetail(Domain.Entities.SubUnit subUnit, string? defaultCurrencyCode = null) => new()
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
        }).ToList(),
        OptionValues = MapSubUnitOptionValues(subUnit.OptionValues),
        Currency = subUnit.Unit.Currency?.Code ?? defaultCurrencyCode
    };

    private ReviewResponse MapToReviewResponse(Domain.Entities.Review review) => new()
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

    private PolicyDetailResponse MapToPolicyDetailResponse(GeneralPolicy policy) => new()
    {
        Id = policy.Id,
        Title = policy.Title,
        Description = policy.Description,
        PolicyType = policy.PolicyType.ToString(),
        PolicyCategory = policy.PolicyCategory?.ToString(),
        CustomPolicyName = policy.CustomPolicyName,
        CancellationPolicyId = policy.CancellationPolicyId,
        CancellationPolicyName = policy.CancellationPolicy?.Name,
        IsMandatory = policy.IsMandatory,
        IsHighlighted = policy.IsHighlighted,
        IsActive = policy.IsActive,
        UnitId = policy.UnitId,
        SubUnitId = policy.SubUnitId
    };

    private CancellationPolicyResponse MapToCancellationPolicyResponse(CancellationPolicy policy) => new()
    {
        Id = policy.Id,
        Name = policy.Name,
        Description = policy.Description,
        FullRefundDays = policy.FullRefundDays,
        PartialRefundDays = policy.PartialRefundDays,
        PartialRefundPercentage = policy.PartialRefundPercentage,
        IsActive = policy.IsActive,
        IsDefault = policy.IsDefault,
        CreatedAt = policy.CreatedAt
    };

    private AvailabilityBlockResponse MapToAvailabilityBlockResponse(UnitAvailability a) => new()
    {
        Id = a.Id,
        StartDate = a.StartDate,
        EndDate = a.EndDate,
        IsAvailable = a.IsAvailable,
        Reason = a.Reason?.ToString(),
        SpecialPrice = null,
        WeekendPrice = null,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt,
        UpdatedByUserId = a.UpdatedByUserId
    };

    private AvailabilityBlockResponse MapToSubUnitAvailabilityBlockResponse(SubUnitAvailability a) => new()
    {
        Id = a.Id,
        StartDate = a.StartDate,
        EndDate = a.EndDate,
        IsAvailable = a.IsAvailable,
        Reason = a.Reason?.ToString(),
        SpecialPrice = a.SpecialPrice,
        WeekendPrice = a.WeekendPrice,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt,
        UpdatedByUserId = a.UpdatedByUserId
    };

    private static List<Contracts.SubUnit.OptionValueResponse> MapUnitOptionValues(
    IEnumerable<UnitOptionValue>? optionValues)
    {
        if (optionValues == null) return [];
        return optionValues
            .GroupBy(ov => ov.UnitTypeOptionId)
            .Select(g => new Contracts.SubUnit.OptionValueResponse
            {
                OptionId = g.Key,
                OptionName = g.First().UnitTypeOption.Name,
                InputType = g.First().UnitTypeOption.InputType.ToString(),
                Values = g.Select(ov => ov.Value).ToList()
            }).ToList();
    }

    private static List<Contracts.SubUnit.OptionValueResponse> MapSubUnitOptionValues(
        IEnumerable<SubUnitOptionValue>? optionValues)
    {
        if (optionValues == null) return [];
        return optionValues
            .GroupBy(ov => ov.SubUnitTypeOptionId)
            .Select(g => new Contracts.SubUnit.OptionValueResponse
            {
                OptionId = g.Key,
                OptionName = g.First().SubUnitTypeOption.Name,
                InputType = g.First().SubUnitTypeOption.InputType.ToString(),
                Values = g.Select(ov => ov.Value).ToList()
            }).ToList();
    }

    // ════════════════════════════════════════════════════════════════════════════
    //  ADD THIS REGION TO HotelAdminService.cs
    //  (paste after the existing "HELPER / VALIDATION" region)
    // ════════════════════════════════════════════════════════════════════════════

    // =========================================================================
    // ADMIN MANAGEMENT
    // =========================================================================

    public async Task<Result<IEnumerable<UnitAdminResponse>>> GetUnitAdminsAsync(
        string requestingUserId,
        int unitId,
        bool? isActive = null)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(requestingUserId, unitId);
            if (!hasAccess.Value)
                return Result.Failure<IEnumerable<UnitAdminResponse>>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var query = _context.Set<UniteAdmin>()
                .Include(a => a.User)
                .Include(a => a.Unit)
                .Where(a => a.UnitId == unitId)
                .AsQueryable();

            if (isActive.HasValue)
                query = query.Where(a => a.IsActive == isActive.Value);

            var admins = await query
                .OrderBy(a => a.AssignedAt)
                .AsNoTracking()
                .ToListAsync();

            return Result.Success(admins.Select(MapToUnitAdminResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admins for unit {UnitId}", unitId);
            return Result.Failure<IEnumerable<UnitAdminResponse>>(
                new Error("GetAdminsFailed", "Failed to retrieve unit admins", 500));
        }
    }

    public async Task<Result<UnitAdminResponse>> GetUnitAdminByIdAsync(
        string requestingUserId,
        int unitAdminId)
    {
        try
        {
            var record = await _context.Set<UniteAdmin>()
                .Include(a => a.User)
                .Include(a => a.Unit)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == unitAdminId);

            if (record == null)
                return Result.Failure<UnitAdminResponse>(
                    new Error("NotFound", "Admin record not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(requestingUserId, record.UnitId);
            if (!hasAccess.Value)
                return Result.Failure<UnitAdminResponse>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            return Result.Success(MapToUnitAdminResponse(record));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin record {UnitAdminId}", unitAdminId);
            return Result.Failure<UnitAdminResponse>(
                new Error("GetAdminFailed", "Failed to retrieve admin record", 500));
        }
    }

    public async Task<Result<UnitAdminResponse>> AddUnitAdminAsync(
        string requestingUserId,
        int unitId,
        AddUnitAdminRequest request)
    {
        try
        {
            // Requester must be an active admin of this unit
            var hasAccess = await IsAdminOfUnitAsync(requestingUserId, unitId);
            if (!hasAccess.Value)
                return Result.Failure<UnitAdminResponse>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            // Resolve the user by email
            var targetUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == request.UserEmail);

            if (targetUser == null)
                return Result.Failure<UnitAdminResponse>(
                    new Error("UserNotFound", $"No user found with email '{request.UserEmail}'", 404));

            // Check if the unit exists
            var unitExists = await _context.Units.AnyAsync(u => u.Id == unitId && !u.IsDeleted);
            if (!unitExists)
                return Result.Failure<UnitAdminResponse>(
                    new Error("NotFound", "Unit not found", 404));

            // Check for an existing record (active or inactive)
            var existing = await _context.Set<UniteAdmin>()
                .FirstOrDefaultAsync(a => a.UnitId == unitId && a.UserId == targetUser.Id);

            if (existing != null)
            {
                if (existing.IsActive)
                    return Result.Failure<UnitAdminResponse>(
                        new Error("AlreadyAdmin", "This user is already an active admin for this unit", 409));

                // Re-activate the existing record
                existing.IsActive = true;
                existing.AssignedAt = DateTime.UtcNow.AddHours(3);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Admin {TargetUserId} re-activated for unit {UnitId} by {RequestingUserId}",
                    targetUser.Id, unitId, requestingUserId);
            }
            else
            {
                // Create a fresh record
                existing = new UniteAdmin
                {
                    UserId = targetUser.Id,
                    UnitId = unitId,
                    IsActive = true,
                    AssignedAt = DateTime.UtcNow.AddHours(3)
                };

                _context.Set<UniteAdmin>().Add(existing);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Admin {TargetUserId} added to unit {UnitId} by {RequestingUserId}",
                    targetUser.Id, unitId, requestingUserId);
            }

            // Return the saved record with navigation props loaded
            var saved = await _context.Set<UniteAdmin>()
                .Include(a => a.User)
                .Include(a => a.Unit)
                .AsNoTracking()
                .FirstAsync(a => a.Id == existing.Id);

            return Result.Success(MapToUnitAdminResponse(saved));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding admin to unit {UnitId}", unitId);
            return Result.Failure<UnitAdminResponse>(
                new Error("AddAdminFailed", "Failed to add admin", 500));
        }
    }

    public async Task<Result> RemoveUnitAdminAsync(
        string requestingUserId,
        int unitAdminId)
    {
        try
        {
            var record = await _context.Set<UniteAdmin>()
                .FirstOrDefaultAsync(a => a.Id == unitAdminId);

            if (record == null)
                return Result.Failure(new Error("NotFound", "Admin record not found", 404));

            // Requester must be an active admin of the same unit
            var hasAccess = await IsAdminOfUnitAsync(requestingUserId, record.UnitId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            // Prevent self-removal
            if (record.UserId == requestingUserId)
                return Result.Failure(
                    new Error("SelfRemovalNotAllowed", "You cannot remove yourself as admin", 400));

            // Prevent removing the last active admin
            var activeAdminCount = await _context.Set<UniteAdmin>()
                .CountAsync(a => a.UnitId == record.UnitId && a.IsActive);

            if (activeAdminCount <= 1)
                return Result.Failure(
                    new Error("LastAdmin", "Cannot remove the last active admin for this unit", 400));

            _context.Set<UniteAdmin>().Remove(record);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Admin record {UnitAdminId} (user {UserId}) removed from unit {UnitId} by {RequestingUserId}",
                unitAdminId, record.UserId, record.UnitId, requestingUserId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing admin record {UnitAdminId}", unitAdminId);
            return Result.Failure(new Error("RemoveAdminFailed", "Failed to remove admin", 500));
        }
    }

    public async Task<Result> DeactivateUnitAdminAsync(
        string requestingUserId,
        int unitAdminId)
    {
        try
        {
            var record = await _context.Set<UniteAdmin>()
                .FirstOrDefaultAsync(a => a.Id == unitAdminId);

            if (record == null)
                return Result.Failure(new Error("NotFound", "Admin record not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(requestingUserId, record.UnitId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            if (record.UserId == requestingUserId)
                return Result.Failure(
                    new Error("SelfDeactivationNotAllowed", "You cannot deactivate yourself", 400));

            if (!record.IsActive)
                return Result.Failure(
                    new Error("AlreadyInactive", "Admin is already inactive", 400));

            // Prevent deactivating the last active admin
            var activeAdminCount = await _context.Set<UniteAdmin>()
                .CountAsync(a => a.UnitId == record.UnitId && a.IsActive);

            if (activeAdminCount <= 1)
                return Result.Failure(
                    new Error("LastAdmin", "Cannot deactivate the last active admin for this unit", 400));

            record.IsActive = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Admin {UserId} deactivated for unit {UnitId} by {RequestingUserId}",
                record.UserId, record.UnitId, requestingUserId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating admin record {UnitAdminId}", unitAdminId);
            return Result.Failure(new Error("DeactivateFailed", "Failed to deactivate admin", 500));
        }
    }

    public async Task<Result> ActivateUnitAdminAsync(
        string requestingUserId,
        int unitAdminId)
    {
        try
        {
            var record = await _context.Set<UniteAdmin>()
                .FirstOrDefaultAsync(a => a.Id == unitAdminId);

            if (record == null)
                return Result.Failure(new Error("NotFound", "Admin record not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(requestingUserId, record.UnitId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            if (record.IsActive)
                return Result.Failure(
                    new Error("AlreadyActive", "Admin is already active", 400));

            record.IsActive = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Admin {UserId} activated for unit {UnitId} by {RequestingUserId}",
                record.UserId, record.UnitId, requestingUserId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating admin record {UnitAdminId}", unitAdminId);
            return Result.Failure(new Error("ActivateFailed", "Failed to activate admin", 500));
        }
    }

    public async Task<Result<bool>> ToggleUnitAdminStatusAsync(
        string requestingUserId,
        int unitAdminId)
    {
        try
        {
            var record = await _context.Set<UniteAdmin>()
                .FirstOrDefaultAsync(a => a.Id == unitAdminId);

            if (record == null)
                return Result.Failure<bool>(new Error("NotFound", "Admin record not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(requestingUserId, record.UnitId);
            if (!hasAccess.Value)
                return Result.Failure<bool>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            if (record.UserId == requestingUserId)
                return Result.Failure<bool>(
                    new Error("SelfToggleNotAllowed", "You cannot toggle your own admin status", 400));

            // If trying to deactivate, ensure there's at least one other active admin
            if (record.IsActive)
            {
                var activeAdminCount = await _context.Set<UniteAdmin>()
                    .CountAsync(a => a.UnitId == record.UnitId && a.IsActive);

                if (activeAdminCount <= 1)
                    return Result.Failure<bool>(
                        new Error("LastAdmin", "Cannot deactivate the last active admin for this unit", 400));
            }

            record.IsActive = !record.IsActive;
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Admin {UserId} status toggled to {Status} for unit {UnitId} by {RequestingUserId}",
                record.UserId, record.IsActive, record.UnitId, requestingUserId);

            return Result.Success(record.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling admin record {UnitAdminId}", unitAdminId);
            return Result.Failure<bool>(new Error("ToggleFailed", "Failed to toggle admin status", 500));
        }
    }

    public async Task<Result<AdminActivitySummary>> GetAdminActivitySummaryAsync(
        string requestingUserId,
        string targetUserId)
    {
        try
        {
            // Requester must share at least one unit with the target admin
            var requestingUnitIds = await GetUserAdminUnitIdsAsync(requestingUserId);
            var targetUnitIds = await _context.Set<UniteAdmin>()
                .Where(a => a.UserId == targetUserId)
                .Select(a => a.UnitId)
                .ToListAsync();

            var sharedUnitIds = requestingUnitIds.Intersect(targetUnitIds).ToList();
            if (!sharedUnitIds.Any())
                return Result.Failure<AdminActivitySummary>(
                    new Error("NoAccess", "You do not share any units with this admin", 403));

            // Fetch the target user
            var targetUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == targetUserId);

            if (targetUser == null)
                return Result.Failure<AdminActivitySummary>(
                    new Error("NotFound", "User not found", 404));

            // Load all their admin records (scoped to units the requester can see)
            var adminRecords = await _context.Set<UniteAdmin>()
                .Include(a => a.Unit)
                .Where(a => a.UserId == targetUserId && sharedUnitIds.Contains(a.UnitId))
                .AsNoTracking()
                .ToListAsync();

            var summary = new AdminActivitySummary
            {
                UserId = targetUser.Id,
                FullName = targetUser.FullName ?? string.Empty,
                Email = targetUser.Email ?? string.Empty,
                TotalUnitsManaged = adminRecords.Count,
                ActiveUnitsManaged = adminRecords.Count(a => a.IsActive),
                ManagedUnits = adminRecords.Select(a => new ManagedUnitInfo
                {
                    UnitId = a.UnitId,
                    UnitName = a.Unit.Name,
                    IsActive = a.IsActive,
                    AssignedAt = a.AssignedAt
                }).ToList()
            };

            return Result.Success(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting admin activity summary for user {TargetUserId}", targetUserId);
            return Result.Failure<AdminActivitySummary>(
                new Error("SummaryFailed", "Failed to retrieve admin activity summary", 500));
        }
    }

    public async Task<Result> TransferAdminUnitsAsync(
        string requestingUserId,
        string fromUserId,
        string toUserEmail,
        bool deactivateOriginal = false)
    {
        try
        {
            // Requester must share at least one unit with fromUserId
            var requestingUnitIds = await GetUserAdminUnitIdsAsync(requestingUserId);
            var fromUnitIds = await _context.Set<UniteAdmin>()
                .Where(a => a.UserId == fromUserId && a.IsActive)
                .Select(a => a.UnitId)
                .ToListAsync();

            var transferableUnitIds = requestingUnitIds.Intersect(fromUnitIds).ToList();
            if (!transferableUnitIds.Any())
                return Result.Failure(
                    new Error("NoAccess", "You do not share any units with the source admin", 403));

            // Resolve the destination user
            var toUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == toUserEmail);

            if (toUser == null)
                return Result.Failure(
                    new Error("UserNotFound", $"No user found with email '{toUserEmail}'", 404));

            var now = DateTime.UtcNow.AddHours(3);

            foreach (var unitId in transferableUnitIds)
            {
                // Check if the target is already an admin
                var existing = await _context.Set<UniteAdmin>()
                    .FirstOrDefaultAsync(a => a.UnitId == unitId && a.UserId == toUser.Id);

                if (existing != null)
                {
                    // Just make sure they're active
                    if (!existing.IsActive)
                    {
                        existing.IsActive = true;
                        existing.AssignedAt = now;
                    }
                }
                else
                {
                    _context.Set<UniteAdmin>().Add(new UniteAdmin
                    {
                        UserId = toUser.Id,
                        UnitId = unitId,
                        IsActive = true,
                        AssignedAt = now
                    });
                }

                // Optionally deactivate the source admin
                if (deactivateOriginal && fromUserId != requestingUserId)
                {
                    var fromRecord = await _context.Set<UniteAdmin>()
                        .FirstOrDefaultAsync(a => a.UnitId == unitId && a.UserId == fromUserId);

                    if (fromRecord != null)
                        fromRecord.IsActive = false;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Admin units transferred from {FromUserId} to {ToUserId} " +
                "({Count} units) by {RequestingUserId}. DeactivateOriginal={Deactivate}",
                fromUserId, toUser.Id, transferableUnitIds.Count,
                requestingUserId, deactivateOriginal);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transferring admin units from {FromUserId}", fromUserId);
            return Result.Failure(new Error("TransferFailed", "Failed to transfer admin units", 500));
        }
    }

    // ─── Private mapping helper ──────────────────────────────────────────────────

    private static UnitAdminResponse MapToUnitAdminResponse(UniteAdmin a) => new()
    {
        Id = a.Id,
        UserId = a.UserId,
        FullName = a.User.FullName ?? string.Empty,
        Email = a.User.Email ?? string.Empty,
        PhoneNumber = a.User.PhoneNumber,
        AvatarUrl = a.User.AvatarUrl,
        UnitId = a.UnitId,
        UnitName = a.Unit.Name,
        IsActive = a.IsActive,
        AssignedAt = a.AssignedAt,
        LastLoginAt = a.User.LastLoginAt
    };


}