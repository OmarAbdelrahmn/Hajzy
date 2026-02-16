using Amazon.S3;
using Amazon.S3.Transfer;
using Application.Abstraction;
using Application.Contracts.hoteladmincont;
using Application.Service.Avilabilaties;
using Application.Service.S3Image;
using Domain;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;

namespace Application.Service.HotelAdmin;

public class HotelAdminService(
    ApplicationDbcontext context,
    ILogger<HotelAdminService> logger,
    IAvailabilityService availabilityService,
    IS3ImageService service,
    IAmazonS3 _s3Client

    ) : IHotelAdminService
{
    private readonly ApplicationDbcontext _context = context;
    private readonly ILogger<HotelAdminService> _logger = logger;
    private readonly IAvailabilityService _availabilityService = availabilityService;
    private readonly IS3ImageService service = service;
    private readonly IAmazonS3 s3Client = _s3Client;
    private const string CloudFrontUrl = "";
    private const string BucketName = "hujjzy-bucket";


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

            // ADD SMART SEARCH
            if (!string.IsNullOrWhiteSpace(filter.SearchKeyword))
            {
                var keyword = filter.SearchKeyword.ToLower().Trim();
                query = query.Where(u =>
                    u.Name.ToLower().Contains(keyword) ||
                    u.Description.ToLower().Contains(keyword) ||
                    u.Address.ToLower().Contains(keyword) ||
                    u.UnitType.Name.ToLower().Contains(keyword) ||
                    u.City.Name.ToLower().Contains(keyword)
                );
            }

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
            if (request.Rank.HasValue)
            {
                if (request.Rank.Value < 1 || request.Rank.Value > 5)
                    return Result.Failure<UnitResponse>(
                        new Error("InvalidRank", "Rank must be between 1 and 5 stars", 400));

                unit.Rank = request.Rank.Value;
            }

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

    public async Task<Result<Application.Service.HotelAdmin.IHotelAdminService.PaginatedResponse<BookingComprehensiveResponse>>> GetMyUnitBookingsAsync(
        string userId,
        Contracts.hoteladmincont.BookingFilter filter)
    {
        try
        {
            var adminUnits = await GetUserAdminUnitsAsync(userId);
            if (!adminUnits.Any())
                return Result.Failure<Application.Service.HotelAdmin.IHotelAdminService.PaginatedResponse<BookingComprehensiveResponse>>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var unitIds = adminUnits.Select(u => u.Id).ToList();

            var query = _context.Bookings
                .AsNoTracking()
                .Where(b => unitIds.Contains(b.UnitId));

            // Apply filters
            if (filter.Status.HasValue)
                query = query.Where(b => b.Status == filter.Status.Value);

            if (filter.PaymentStatus.HasValue)
                query = query.Where(b => b.PaymentStatus == filter.PaymentStatus.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(b => b.CheckInDate >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(b => b.CheckOutDate <= filter.EndDate.Value);

            // ADD SMART SEARCH
            if (!string.IsNullOrWhiteSpace(filter.SearchKeyword))
            {
                var keyword = filter.SearchKeyword.ToLower().Trim();
                query = query.Where(b =>
                    b.BookingNumber.ToLower().Contains(keyword) ||
                    b.Unit.Name.ToLower().Contains(keyword) ||
                    (b.User.FullName != null && b.User.FullName.ToLower().Contains(keyword)) ||
                    (b.User.Email != null && b.User.Email.ToLower().Contains(keyword)) ||
                    (b.User.PhoneNumber != null && b.User.PhoneNumber.Contains(keyword))
                );
            }

            // Get count first
            var totalCount = await query.CountAsync();

            // Then get data with projection
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
                    UserCountry = b.User.Country
                })
                .ToListAsync();

            var paginatedResult = CreatePaginatedResponse(
                responses,
                totalCount,
                filter.Page,
                filter.PageSize);

            return Result.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bookings for user {UserId}. Message: {Message}, StackTrace: {StackTrace}",
                userId, ex.Message, ex.StackTrace);
            return Result.Failure<Application.Service.HotelAdmin.IHotelAdminService.PaginatedResponse<BookingComprehensiveResponse>>(
                new Error("GetBookingsFailed", "Failed to retrieve bookings", 500));
        }
    }

    private Application.Service.HotelAdmin.IHotelAdminService.PaginatedResponse<T> CreatePaginatedResponse<T>(
        IEnumerable<T> items,
        int totalCount,
        int page,
        int pageSize)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new Application.Service.HotelAdmin.IHotelAdminService.PaginatedResponse<T>
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

            // ADD SMART SEARCH
            if (!string.IsNullOrWhiteSpace(filter.SearchKeyword))
            {
                var keyword = filter.SearchKeyword.ToLower().Trim();
                query = query.Where(r =>
                    r.Unit.Name.ToLower().Contains(keyword) ||
                    (r.User.FullName != null && r.User.FullName.ToLower().Contains(keyword)) ||
                    (r.Comment != null && r.Comment.ToLower().Contains(keyword)) ||
                    (r.OwnerResponse != null && r.OwnerResponse.ToLower().Contains(keyword))
                );
            }

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

            // ADD SMART SEARCH
            if (!string.IsNullOrWhiteSpace(filter.SearchKeyword))
            {
                var keyword = filter.SearchKeyword.ToLower().Trim();
                query = query.Where(p =>
                    p.TransactionId.ToLower().Contains(keyword) ||
                    p.Booking.BookingNumber.ToLower().Contains(keyword) ||
                    p.Booking.Unit.Name.ToLower().Contains(keyword) ||
                    (p.Booking.User.FullName != null && p.Booking.User.FullName.ToLower().Contains(keyword)) ||
                    (p.Booking.User.Email != null && p.Booking.User.Email.ToLower().Contains(keyword))
                );
            }

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
        var options = new List<string>();
        try
        {
            options = System.Text.Json.JsonSerializer.Deserialize<List<string>>(
                unit.OptionsJson) ?? new List<string>();
        }
        catch
        {
            options = new List<string>();
        }

        var customPolicies = unit.CustomPolicies?.Where(p => p.IsActive)
       .OrderBy(p => p.DisplayOrder)
       .Select(p => new CustomPolicyDetail
       {
           Id = p.Id,
           Title = p.Title,
           Description = p.Description,
           Category = p.Category,
           DisplayOrder = p.DisplayOrder,
           IsActive = p.IsActive
       }).ToList() ?? new List<CustomPolicyDetail>();


        return new UnitComprehensiveResponse
        {
            Rank = unit.Rank, // NEW
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
            Options = options,
            Currency = unit.PriceCurrency.ToString(),
            CustomPolicies = customPolicies
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
            UpdatedAt = unit.UpdatedAt,
            Rank = unit.Rank, // NEW
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

    #region Policy Management

    public async Task<Result<IEnumerable<PolicyDetailResponse>>> GetUnitPoliciesAsync(
        string userId,
        int unitId)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure<IEnumerable<PolicyDetailResponse>>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var policies = await _context.GeneralPolicies
                .Include(p => p.CancellationPolicy)
                .Where(p => p.UnitId == unitId && !p.UnitId.HasValue || p.UnitId == unitId)
                .AsNoTracking()
                .ToListAsync();

            var responses = policies.Select(MapToPolicyDetailResponse);
            return Result.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting policies for unit {UnitId}", unitId);
            return Result.Failure<IEnumerable<PolicyDetailResponse>>(
                new Error("GetPoliciesFailed", "Failed to retrieve policies", 500));
        }
    }

    public async Task<Result<PolicyDetailResponse>> CreateUnitPolicyAsync(
        string userId,
        int unitId,
        CreatePolicyRequest request)
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

            var createdPolicy = await _context.GeneralPolicies
                .Include(p => p.CancellationPolicy)
                .FirstAsync(p => p.Id == policy.Id);

            _logger.LogInformation(
                "Policy {PolicyId} created for unit {UnitId} by user {UserId}",
                policy.Id, unitId, userId);

            return Result.Success(MapToPolicyDetailResponse(createdPolicy));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating policy for unit {UnitId}", unitId);
            return Result.Failure<PolicyDetailResponse>(
                new Error("CreatePolicyFailed", "Failed to create policy", 500));
        }
    }

    public async Task<Result<PolicyDetailResponse>> UpdateUnitPolicyAsync(
        string userId,
        int policyId,
        UpdatePolicyRequest request)
    {
        try
        {
            var policy = await _context.GeneralPolicies
                .Include(p => p.Unit)
                .Include(p => p.CancellationPolicy)
                .FirstOrDefaultAsync(p => p.Id == policyId);

            if (policy == null)
                return Result.Failure<PolicyDetailResponse>(
                    new Error("NotFound", "Policy not found", 404));

            if (policy.UnitId.HasValue)
            {
                var hasAccess = await IsAdminOfUnitAsync(userId, policy.UnitId.Value);
                if (!hasAccess.Value)
                    return Result.Failure<PolicyDetailResponse>(
                        new Error("NoAccess", "You do not have access to this policy", 403));
            }

            // Update fields
            if (request.Title != null) policy.Title = request.Title;
            if (request.Description != null) policy.Description = request.Description;
            if (request.PolicyCategory.HasValue) policy.PolicyCategory = request.PolicyCategory;
            if (request.CancellationPolicyId.HasValue) policy.CancellationPolicyId = request.CancellationPolicyId;
            if (request.IsMandatory.HasValue) policy.IsMandatory = request.IsMandatory.Value;
            if (request.IsHighlighted.HasValue) policy.IsHighlighted = request.IsHighlighted.Value;
            if (request.IsActive.HasValue) policy.IsActive = request.IsActive.Value;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Policy {PolicyId} updated by user {UserId}",
                policyId, userId);

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
            var policy = await _context.GeneralPolicies
                .Include(p => p.Unit)
                .FirstOrDefaultAsync(p => p.Id == policyId);

            if (policy == null)
                return Result.Failure(new Error("NotFound", "Policy not found", 404));

            if (policy.UnitId.HasValue)
            {
                var hasAccess = await IsAdminOfUnitAsync(userId, policy.UnitId.Value);
                if (!hasAccess.Value)
                    return Result.Failure(
                        new Error("NoAccess", "You do not have access to this policy", 403));
            }

            policy.IsActive = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Policy {PolicyId} deleted by user {UserId}",
                policyId, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting policy {PolicyId}", policyId);
            return Result.Failure(new Error("DeletePolicyFailed", "Failed to delete policy", 500));
        }
    }

    public async Task<Result<IEnumerable<PolicyDetailResponse>>> GetSubUnitPoliciesAsync(
        string userId,
        int subUnitId)
    {
        try
        {
            var subUnit = await _context.SubUnits
                .Include(s => s.Unit)
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

            var responses = policies.Select(MapToPolicyDetailResponse);
            return Result.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting policies for subunit {SubUnitId}", subUnitId);
            return Result.Failure<IEnumerable<PolicyDetailResponse>>(
                new Error("GetPoliciesFailed", "Failed to retrieve policies", 500));
        }
    }

    public async Task<Result<PolicyDetailResponse>> CreateSubUnitPolicyAsync(
        string userId,
        int subUnitId,
        CreatePolicyRequest request)
    {
        try
        {
            var subUnit = await _context.SubUnits
                .Include(s => s.Unit)
                .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

            if (subUnit == null)
                return Result.Failure<PolicyDetailResponse>(
                    new Error("NotFound", "SubUnit not found", 404));

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

            var createdPolicy = await _context.GeneralPolicies
                .Include(p => p.CancellationPolicy)
                .FirstAsync(p => p.Id == policy.Id);

            _logger.LogInformation(
                "Policy {PolicyId} created for subunit {SubUnitId} by user {UserId}",
                policy.Id, subUnitId, userId);

            return Result.Success(MapToPolicyDetailResponse(createdPolicy));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating policy for subunit {SubUnitId}", subUnitId);
            return Result.Failure<PolicyDetailResponse>(
                new Error("CreatePolicyFailed", "Failed to create policy", 500));
        }
    }

    #endregion

    #region Cancellation Policy Management

    public async Task<Result<IEnumerable<CancellationPolicyResponse>>> GetAvailableCancellationPoliciesAsync()
    {
        try
        {
            var policies = await _context.CancellationPolicies
                .Where(p => p.IsActive)
                .OrderBy(p => p.FullRefundDays)
                .AsNoTracking()
                .ToListAsync();

            var responses = policies.Select(MapToCancellationPolicyResponse);
            return Result.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available cancellation policies");
            return Result.Failure<IEnumerable<CancellationPolicyResponse>>(
                new Error("GetPoliciesFailed", "Failed to retrieve cancellation policies", 500));
        }
    }

    public async Task<Result<CancellationPolicyResponse>> GetUnitCancellationPolicyAsync(
        string userId,
        int unitId)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure<CancellationPolicyResponse>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var unit = await _context.Units
                .Include(u => u.CancellationPolicy)
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

            if (unit == null || unit.CancellationPolicy == null)
                return Result.Failure<CancellationPolicyResponse>(
                    new Error("NotFound", "Cancellation policy not found", 404));

            return Result.Success(MapToCancellationPolicyResponse(unit.CancellationPolicy));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cancellation policy for unit {UnitId}", unitId);
            return Result.Failure<CancellationPolicyResponse>(
                new Error("GetPolicyFailed", "Failed to retrieve cancellation policy", 500));
        }
    }

    public async Task<Result> SetUnitCancellationPolicyAsync(
        string userId,
        int unitId,
        int cancellationPolicyId)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var unit = await _context.Units
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

            if (unit == null)
                return Result.Failure(new Error("NotFound", "Unit not found", 404));

            var policy = await _context.CancellationPolicies
                .FirstOrDefaultAsync(p => p.Id == cancellationPolicyId && p.IsActive);

            if (policy == null)
                return Result.Failure(new Error("NotFound", "Cancellation policy not found", 404));

            unit.CancellationPolicyId = cancellationPolicyId;
            unit.UpdatedAt = DateTime.UtcNow.AddHours(3);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Cancellation policy {PolicyId} set for unit {UnitId} by user {UserId}",
                cancellationPolicyId, unitId, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cancellation policy for unit {UnitId}", unitId);
            return Result.Failure(
                new Error("SetPolicyFailed", "Failed to set cancellation policy", 500));
        }
    }

    public async Task<Result<CancellationPolicyResponse>> CreateCustomCancellationPolicyAsync(
        string userId,
        CreateCancellationPolicyRequest request)
    {
        try
        {
            var policy = new CancellationPolicy
            {
                Name = request.Name,
                Description = request.Description,
                FullRefundDays = request.FullRefundDays,
                PartialRefundDays = request.PartialRefundDays,
                PartialRefundPercentage = request.PartialRefundPercentage,
                IsActive = true,
                IsDefault = request.IsDefault,
                CreatedAt = DateTime.UtcNow.AddHours(3)
            };

            _context.CancellationPolicies.Add(policy);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Custom cancellation policy {PolicyId} created by user {UserId}",
                policy.Id, userId);

            return Result.Success(MapToCancellationPolicyResponse(policy));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating custom cancellation policy");
            return Result.Failure<CancellationPolicyResponse>(
                new Error("CreatePolicyFailed", "Failed to create cancellation policy", 500));
        }
    }

    #endregion

    #region Helper Methods

    private PolicyDetailResponse MapToPolicyDetailResponse(GeneralPolicy policy)
    {
        return new PolicyDetailResponse
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
    }
    private async Task<List<Domain.Entities.Unit>> GetUserAdminUnitsAsync(string userId)
    {
        return await _context.Units.Include(u => u.CustomPolicies.Where(p => p.IsActive))
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
    private CancellationPolicyResponse MapToCancellationPolicyResponse(CancellationPolicy policy)
    {
        return new CancellationPolicyResponse
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
    }

    #endregion

    #region Availability Management

    public async Task<Result<Dictionary<DateTime, Contracts.Availability.UnitDayAvailability>>> GetUnitAvailabilityCalendarAsync(
        string userId,
        int unitId,
        int year,
        int month)
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
        string userId,
        int subUnitId,
        int year,
        int month)
    {
        try
        {
            var subUnit = await _context.SubUnits
                .Include(s => s.Unit)
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
        string userId,
        Contracts.Availability.SetUnitAvailabilityRequest request)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, request.UnitId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this unit", 403));

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
        string userId,
        Contracts.Availability.SetSubUnitAvailabilityRequest request)
    {
        try
        {
            var subUnit = await _context.SubUnits
                .Include(s => s.Unit)
                .FirstOrDefaultAsync(s => s.Id == request.SubUnitId && !s.IsDeleted);

            if (subUnit == null)
                return Result.Failure(new Error("NotFound", "SubUnit not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(userId, subUnit.UnitId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this subunit", 403));

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
        string userId,
        Contracts.Availability.BlockUnitDatesRequest request)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, request.UnitId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this unit", 403));

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
        string userId,
        Contracts.Availability.BlockDatesRequest request)
    {
        try
        {
            var subUnit = await _context.SubUnits
                .Include(s => s.Unit)
                .FirstOrDefaultAsync(s => s.Id == request.SubUnitId && !s.IsDeleted);

            if (subUnit == null)
                return Result.Failure(new Error("NotFound", "SubUnit not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(userId, subUnit.UnitId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this subunit", 403));

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
        string userId,
        Contracts.Availability.SetSpecialPricingRequest request)
    {
        try
        {
            var subUnit = await _context.SubUnits
                .Include(s => s.Unit)
                .FirstOrDefaultAsync(s => s.Id == request.SubUnitId && !s.IsDeleted);

            if (subUnit == null)
                return Result.Failure(new Error("NotFound", "SubUnit not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(userId, subUnit.UnitId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this subunit", 403));

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
            // Check access through unit/subunit
            var unitAvailability = await _context.Set<UnitAvailability>()
                .Include(a => a.Unit)
                .FirstOrDefaultAsync(a => a.Id == availabilityId);

            if (unitAvailability != null)
            {
                var hasAccess = await IsAdminOfUnitAsync(userId, unitAvailability.UnitId);
                if (!hasAccess.Value)
                    return Result.Failure(
                        new Error("NoAccess", "You do not have access to this availability", 403));
            }
            else
            {
                var subUnitAvailability = await _context.Set<SubUnitAvailability>()
                    .Include(a => a.SubUnit)
                        .ThenInclude(s => s.Unit)
                    .FirstOrDefaultAsync(a => a.Id == availabilityId);

                if (subUnitAvailability != null)
                {
                    var hasAccess = await IsAdminOfUnitAsync(userId, subUnitAvailability.SubUnit.UnitId);
                    if (!hasAccess.Value)
                        return Result.Failure(
                            new Error("NoAccess", "You do not have access to this availability", 403));
                }
                else
                {
                    return Result.Failure(new Error("NotFound", "Availability not found", 404));
                }
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
        string userId,
        int unitId,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure<IEnumerable<AvailabilityBlockResponse>>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var query = _context.Set<UnitAvailability>()
                .Where(a => a.UnitId == unitId);

            if (startDate.HasValue)
                query = query.Where(a => a.EndDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.StartDate <= endDate.Value);

            var blocks = await query
                .OrderBy(a => a.StartDate)
                .AsNoTracking()
                .ToListAsync();

            var responses = blocks.Select(MapToAvailabilityBlockResponse);
            return Result.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unit blocked dates");
            return Result.Failure<IEnumerable<AvailabilityBlockResponse>>(
                new Error("GetBlocksFailed", "Failed to retrieve blocked dates", 500));
        }
    }

    public async Task<Result<IEnumerable<AvailabilityBlockResponse>>> GetSubUnitBlockedDatesAsync(
        string userId,
        int subUnitId,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            var subUnit = await _context.SubUnits
                .Include(s => s.Unit)
                .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

            if (subUnit == null)
                return Result.Failure<IEnumerable<AvailabilityBlockResponse>>(
                    new Error("NotFound", "SubUnit not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(userId, subUnit.UnitId);
            if (!hasAccess.Value)
                return Result.Failure<IEnumerable<AvailabilityBlockResponse>>(
                    new Error("NoAccess", "You do not have access to this subunit", 403));

            var query = _context.Set<SubUnitAvailability>()
                .Where(a => a.SubUnitId == subUnitId);

            if (startDate.HasValue)
                query = query.Where(a => a.EndDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.StartDate <= endDate.Value);

            var blocks = await query
                .OrderBy(a => a.StartDate)
                .AsNoTracking()
                .ToListAsync();

            var responses = blocks.Select(MapToSubUnitAvailabilityBlockResponse);
            return Result.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subunit blocked dates");
            return Result.Failure<IEnumerable<AvailabilityBlockResponse>>(
                new Error("GetBlocksFailed", "Failed to retrieve blocked dates", 500));
        }
    }

    public async Task<Result<UnitAvailabilityStatus>> CheckUnitAvailabilityAsync(
        string userId,
        int unitId,
        DateTime checkIn,
        DateTime checkOut)
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
        string userId,
        int subUnitId,
        DateTime checkIn,
        DateTime checkOut)
    {
        try
        {
            var subUnit = await _context.SubUnits
                .Include(s => s.Unit)
                .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

            if (subUnit == null)
                return Result.Failure<SubUnitAvailabilityStatus>(
                    new Error("NotFound", "SubUnit not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(userId, subUnit.UnitId);
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
        string userId,
        int unitId,
        DateTime checkIn,
        DateTime checkOut)
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

    #endregion

    #region Comprehensive Reports

    public async Task<Result<FinancialReportResponse>> GetFinancialReportAsync(
        string userId,
        FinancialReportFilter filter)
    {
        try
        {
            var adminUnits = await GetUserAdminUnitsAsync(userId);
            if (!adminUnits.Any())
                return Result.Failure<FinancialReportResponse>(
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
            var days = (filter.EndDate - filter.StartDate).Days + 1;
            var avgDailyRevenue = days > 0 ? totalRevenue / days : 0;

            // Group by unit
            var revenueByUnit = bookings
                .GroupBy(b => new { b.UnitId, b.Unit.Name })
                .Select(g => new RevenueByUnit
                {
                    UnitId = g.Key.UnitId,
                    UnitName = g.Key.Name,
                    Revenue = g.Sum(b => b.TotalPrice),
                    BookingCount = g.Count(),
                    AverageBookingValue = g.Average(b => b.TotalPrice),
                    ContributionPercentage = totalRevenue > 0 ? (g.Sum(b => b.TotalPrice) / totalRevenue * 100) : 0
                })
                .OrderByDescending(r => r.Revenue)
                .ToList();

            // Monthly breakdown
            var monthlyBreakdown = bookings
                .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                .Select(g => new RevenueByMonth
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Year = g.Key.Year,
                    Revenue = g.Sum(b => b.TotalPrice),
                    BookingCount = g.Count(),
                    GrowthRate = 0 // Calculate separately if needed
                })
                .OrderBy(r => r.Year)
                .ThenBy(r => r.Month)
                .ToList();

            // Payment methods
            var paymentMethods = bookings
                .SelectMany(b => b.Payments)
                .GroupBy(p => p.PaymentMethod)
                .Select(g => new PaymentMethodBreakdown
                {
                    PaymentMethod = g.Key.ToString(),
                    Amount = g.Sum(p => p.Amount),
                    TransactionCount = g.Count(),
                    Percentage = totalRevenue > 0 ? (g.Sum(p => p.Amount) / totalRevenue * 100) : 0
                })
                .ToList();

            var report = new FinancialReportResponse
            {
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                TotalRevenue = totalRevenue,
                TotalExpenses = 0, // Implement if expense tracking exists
                NetIncome = totalRevenue,
                AverageDailyRevenue = avgDailyRevenue,
                ProjectedMonthlyRevenue = avgDailyRevenue * 30,
                YearToDateRevenue = totalRevenue,
                GrowthRate = 0, // Calculate based on previous period
                RevenueByUnit = revenueByUnit,
                MonthlyBreakdown = monthlyBreakdown,
                PaymentMethods = paymentMethods,
                RevenueSources = new List<RevenueSource>()
            };

            return Result.Success(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating financial report");
            return Result.Failure<FinancialReportResponse>(
                new Error("ReportFailed", "Failed to generate financial report", 500));
        }
    }

    // Additional report methods will be implemented in Part 3...
    // For brevity, showing the pattern with FinancialReport

    #endregion

    #region Helper Methods (Continued)

    private AvailabilityBlockResponse MapToAvailabilityBlockResponse(UnitAvailability availability)
    {
        return new AvailabilityBlockResponse
        {
            Id = availability.Id,
            StartDate = availability.StartDate,
            EndDate = availability.EndDate,
            IsAvailable = availability.IsAvailable,
            Reason = availability.Reason?.ToString(),
            SpecialPrice = null,
            WeekendPrice = null,
            CreatedAt = availability.CreatedAt,
            UpdatedAt = availability.UpdatedAt,
            UpdatedByUserId = availability.UpdatedByUserId
        };
    }

    private AvailabilityBlockResponse MapToSubUnitAvailabilityBlockResponse(SubUnitAvailability availability)
    {
        return new AvailabilityBlockResponse
        {
            Id = availability.Id,
            StartDate = availability.StartDate,
            EndDate = availability.EndDate,
            IsAvailable = availability.IsAvailable,
            Reason = availability.Reason?.ToString(),
            SpecialPrice = availability.SpecialPrice,
            WeekendPrice = availability.WeekendPrice,
            CreatedAt = availability.CreatedAt,
            UpdatedAt = availability.UpdatedAt,
            UpdatedByUserId = availability.UpdatedByUserId
        };
    }

    #endregion

    #region Additional Reports

    public async Task<Result<OccupancyStatisticsResponse>> GetOccupancyStatisticsAsync(
        string userId,
        OccupancyFilter filter)
    {
        try
        {
            var adminUnits = await GetUserAdminUnitsAsync(userId);
            if (!adminUnits.Any())
                return Result.Failure<OccupancyStatisticsResponse>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var unitIds = filter.UnitId.HasValue
                ? new List<int> { filter.UnitId.Value }
                : adminUnits.Select(u => u.Id).ToList();

            var occupancyRate = await CalculateOccupancyRateAsync(unitIds, filter.StartDate, filter.EndDate);

            var response = new OccupancyStatisticsResponse
            {
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                OverallOccupancyRate = occupancyRate,
                AverageDailyRate = 0,
                RevPAR = 0,
                TotalAvailableRoomNights = 0,
                TotalOccupiedRoomNights = 0,
                UnitOccupancy = new List<UnitOccupancyDetail>(),
                DailyOccupancy = new List<DailyOccupancy>(),
                TopPerformingRooms = new List<SubUnitOccupancyDetail>(),
                LowPerformingRooms = new List<SubUnitOccupancyDetail>()
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating occupancy statistics");
            return Result.Failure<OccupancyStatisticsResponse>(
                new Error("StatisticsFailed", "Failed to generate occupancy statistics", 500));
        }
    }

    public async Task<Result<BookingTrendsResponse>> GetBookingTrendsAsync(
        string userId,
        TrendsFilter filter)
    {
        try
        {
            var adminUnits = await GetUserAdminUnitsAsync(userId);
            if (!adminUnits.Any())
                return Result.Failure<BookingTrendsResponse>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var unitIds = filter.UnitId.HasValue
                ? new List<int> { filter.UnitId.Value }
                : adminUnits.Select(u => u.Id).ToList();

            var bookings = await _context.Bookings
                .Where(b => unitIds.Contains(b.UnitId) &&
                           b.CreatedAt >= filter.StartDate &&
                           b.CreatedAt <= filter.EndDate)
                .ToListAsync();

            var response = new BookingTrendsResponse
            {
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                TotalBookings = bookings.Count,
                AverageLeadTime = (decimal)(bookings.Count != 0 ? bookings.Average(b => (b.CheckInDate - b.CreatedAt).Days) : 0),
                AverageLengthOfStay = (decimal)(bookings.Any() ? bookings.Average(b => b.NumberOfNights) : 0),
                BookingConversionRate = 0,
                BookingsByPeriod = new List<BookingByPeriod>(),
                BookingsByDayOfWeek = new List<BookingByDayOfWeek>(),
                BookingsBySource = new List<BookingBySource>(),
                SeasonalTrends = new List<SeasonalTrend>(),
                PeakPeriods = new List<PeakPeriod>()
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating booking trends");
            return Result.Failure<BookingTrendsResponse>(
                new Error("TrendsFailed", "Failed to generate booking trends", 500));
        }
    }

    public async Task<Result<CustomerInsightsResponse>> GetCustomerInsightsAsync(
        string userId,
        InsightsFilter filter)
    {
        try
        {
            var adminUnits = await GetUserAdminUnitsAsync(userId);
            if (!adminUnits.Any())
                return Result.Failure<CustomerInsightsResponse>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var response = new CustomerInsightsResponse
            {
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                TotalUniqueGuests = 0,
                ReturningGuests = 0,
                ReturnGuestRate = 0,
                AverageGuestsPerBooking = 0,
                AverageBookingValue = 0,
                CustomerLifetimeValue = 0,
                Demographics = new List<GuestDemographic>(),
                TopGuests = new List<TopGuest>(),
                Preferences = new List<GuestPreference>(),
                ReviewInsights = new List<ReviewSummary>()
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating customer insights");
            return Result.Failure<CustomerInsightsResponse>(
                new Error("InsightsFailed", "Failed to generate customer insights", 500));
        }
    }

    public async Task<Result<PerformanceComparisonResponse>> GetPerformanceComparisonAsync(
        string userId,
        ComparisonFilter filter)
    {
        try
        {
            var adminUnits = await GetUserAdminUnitsAsync(userId);
            if (!adminUnits.Any())
                return Result.Failure<PerformanceComparisonResponse>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var response = new PerformanceComparisonResponse
            {
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                UnitPerformances = new List<UnitPerformance>(),
                MetricComparisons = new List<MetricComparison>(),
                BestPerformer = new UnitPerformance(),
                WorstPerformer = new UnitPerformance()
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating performance comparison");
            return Result.Failure<PerformanceComparisonResponse>(
                new Error("ComparisonFailed", "Failed to generate performance comparison", 500));
        }
    }

    public async Task<Result<RevenueBreakdownResponse>> GetRevenueBreakdownAsync(
        string userId,
        RevenueBreakdownFilter filter)
    {
        try
        {
            var adminUnits = await GetUserAdminUnitsAsync(userId);
            if (!adminUnits.Any())
                return Result.Failure<RevenueBreakdownResponse>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var response = new RevenueBreakdownResponse
            {
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                TotalRevenue = 0,
                ByRoomType = new List<RevenueByRoomType>(),
                ByBookingType = new List<RevenueByBookingType>(),
                ByPaymentMethod = new List<RevenueByPaymentMethod>(),
                ByDayType = new List<RevenueByDayType>(),
                ByMonth = new List<RevenueByMonth>()
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating revenue breakdown");
            return Result.Failure<RevenueBreakdownResponse>(
                new Error("BreakdownFailed", "Failed to generate revenue breakdown", 500));
        }
    }

    public async Task<Result<CancellationAnalyticsResponse>> GetCancellationAnalyticsAsync(
        string userId,
        CancellationFilter filter)
    {
        try
        {
            var adminUnits = await GetUserAdminUnitsAsync(userId);
            if (!adminUnits.Any())
                return Result.Failure<CancellationAnalyticsResponse>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var response = new CancellationAnalyticsResponse
            {
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                TotalCancellations = 0,
                CancellationRate = 0,
                LostRevenue = 0,
                AverageCancellationLeadTime = 0,
                ByReason = new List<CancellationByReason>(),
                ByTimeframe = new List<CancellationByTimeframe>(),
                ByUnit = new List<CancellationByUnit>(),
                RefundSummary = new List<RefundSummary>()
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating cancellation analytics");
            return Result.Failure<CancellationAnalyticsResponse>(
                new Error("AnalyticsFailed", "Failed to generate cancellation analytics", 500));
        }
    }

    public async Task<Result<PricingOptimizationResponse>> GetPricingOptimizationSuggestionsAsync(
        string userId,
        int unitId,
        DateTime startDate,
        DateTime endDate)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure<PricingOptimizationResponse>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var unit = await _context.Units
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

            if (unit == null)
                return Result.Failure<PricingOptimizationResponse>(
                    new Error("NotFound", "Unit not found", 404));

            var response = new PricingOptimizationResponse
            {
                UnitId = unitId,
                UnitName = unit.Name,
                AnalysisPeriodStart = startDate,
                AnalysisPeriodEnd = endDate,
                CurrentAverageRate = unit.BasePrice,
                RecommendedAverageRate = unit.BasePrice,
                PotentialRevenueIncrease = 0,
                Suggestions = new List<PricingSuggestion>(),
                CompetitorAnalysis = new List<CompetitorPricing>(),
                DemandForecasts = new List<DemandForecast>()
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating pricing optimization");
            return Result.Failure<PricingOptimizationResponse>(
                new Error("OptimizationFailed", "Failed to generate pricing optimization", 500));
        }
    }

    public async Task<Result<byte[]>> ExportReportToExcelAsync(
        string userId,
        ExportReportRequest request)
    {
        try
        {
            // Implementation would use a library like EPPlus or ClosedXML
            // to generate Excel files
            _logger.LogInformation("Excel export requested by user {UserId}", userId);

            return Result.Failure<byte[]>(
                new Error("NotImplemented", "Excel export not yet implemented", 501));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting report to Excel");
            return Result.Failure<byte[]>(
                new Error("ExportFailed", "Failed to export report", 500));
        }
    }

    public async Task<Result<byte[]>> ExportReportToPdfAsync(
        string userId,
        ExportReportRequest request)
    {
        try
        {
            // Implementation would use a library like iTextSharp or QuestPDF
            _logger.LogInformation("PDF export requested by user {UserId}", userId);

            return Result.Failure<byte[]>(
                new Error("NotImplemented", "PDF export not yet implemented", 501));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting report to PDF");
            return Result.Failure<byte[]>(
                new Error("ExportFailed", "Failed to export report", 500));
        }
    }

    #endregion

    #region SubUnit Management Extended

    // Application/Service/HotelAdmin/HotelAdminService.cs

    // Add this method in the #region SUBUNIT MANAGEMENT section:

    public async Task<Result<SubUnitCreatedResponse>> CreateSubUnitAsync(
        string userId,
        int unitId,
        CreateSubUnitRequest request)
    {
        try
        {
            // Verify admin has access to this unit
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure<SubUnitCreatedResponse>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            // Verify unit exists
            var unit = await _context.Units
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

            if (unit == null)
                return Result.Failure<SubUnitCreatedResponse>(
                    new Error("NotFound", "Unit not found", 404));

            // Verify subunit type exists
            var subUnitType = await _context.Set<SubUnitTypee>()
                .FirstOrDefaultAsync(st => st.Id == request.SubUnitTypeId && st.IsActive);

            if (subUnitType == null)
                return Result.Failure<SubUnitCreatedResponse>(
                    new Error("InvalidSubUnitType", "SubUnit type not found or inactive", 400));

            // Check if room number already exists for this unit
            var roomExists = await _context.SubUnits
                .AnyAsync(s => s.UnitId == unitId &&
                              s.RoomNumber == request.RoomNumber &&
                              !s.IsDeleted);

            if (roomExists)
                return Result.Failure<SubUnitCreatedResponse>(
                    new Error("DuplicateRoomNumber",
                        $"Room number '{request.RoomNumber}' already exists for this unit", 400));

            // Create room configuration if provided
            RoomConfiguration? roomConfig = null;
            if (request.RoomConfig != null)
            {
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
            }

            // Create the subunit
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

            // Add amenities if provided
            List<AmenityResponse>? amenitiesResponse = null;
            if (request.AmenityIds != null && request.AmenityIds.Any())
            {
                // Verify amenities exist
                var amenities = await _context.Amenities
                    .Where(a => request.AmenityIds.Contains(a.Id))
                    .ToListAsync();

                if (amenities.Count != request.AmenityIds.Count)
                {
                    _logger.LogWarning(
                        "Some amenity IDs were not found. Requested: {RequestedCount}, Found: {FoundCount}",
                        request.AmenityIds.Count, amenities.Count);
                }

                // Create amenity associations
                var subUnitAmenities = amenities.Select(a => new SubUniteAmenity
                {
                    SubUnitId = subUnit.Id,
                    AmenityId = a.Id,
                    IsAvailable = true
                }).ToList();

                await _context.SubUniteAmenities.AddRangeAsync(subUnitAmenities);
                await _context.SaveChangesAsync();

                amenitiesResponse = amenities.Select(a => new AmenityResponse
                {
                    Id = a.Id,
                    Name = a.Name,
                    Category = a.Category,
                    IsAvailable = true
                }).ToList();
            }

            _logger.LogInformation(
                "SubUnit {SubUnitId} (Room: {RoomNumber}) created for unit {UnitId} by user {UserId}",
                subUnit.Id, subUnit.RoomNumber, unitId, userId);

            // Return response
            var response = new SubUnitCreatedResponse
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
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating subunit for unit {UnitId} by user {UserId}",
                unitId, userId);
            return Result.Failure<SubUnitCreatedResponse>(
                new Error("CreateSubUnitFailed", "Failed to create subunit", 500));
        }
    }

    public async Task<Result<SubUnitResponse>> UpdateSubUnitAsync(
        string userId,
        int subUnitId,
        UpdateSubUnitRequest request)
    {
        try
        {
            var subUnit = await _context.SubUnits
                .Include(s => s.Unit)
                .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

            if (subUnit == null)
                return Result.Failure<SubUnitResponse>(
                    new Error("NotFound", "SubUnit not found", 404));

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

            var response = new SubUnitResponse
            {
                Id = subUnit.Id,
                UnitId = subUnit.UnitId,
                RoomNumber = subUnit.RoomNumber,
                PricePerNight = subUnit.PricePerNight,
                MaxOccupancy = subUnit.MaxOccupancy,
                IsAvailable = subUnit.IsAvailable,
                UpdatedAt = DateTime.UtcNow.AddHours(3)
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subunit {SubUnitId}", subUnitId);
            return Result.Failure<SubUnitResponse>(
                new Error("UpdateFailed", "Failed to update subunit", 500));
        }
    }

    public async Task<Result> UpdateSubUnitPricingAsync(
        string userId,
        int subUnitId,
        UpdateSubUnitPricingRequest request)
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

            subUnit.PricePerNight = request.PricePerNight;
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "SubUnit {SubUnitId} pricing updated to {Price} by user {UserId}",
                subUnitId, request.PricePerNight, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subunit pricing");
            return Result.Failure(new Error("UpdateFailed", "Failed to update pricing", 500));
        }
    }

    public async Task<Result> ToggleSubUnitStatusAsync(
        string userId,
        int subUnitId)
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
        string userId,
        int subUnitId,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            var subUnit = await _context.SubUnits
                .Include(s => s.Unit)
                .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

            if (subUnit == null)
                return Result.Failure<IEnumerable<SubUnitBookingHistoryResponse>>(
                    new Error("NotFound", "SubUnit not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(userId, subUnit.UnitId);
            if (!hasAccess.Value)
                return Result.Failure<IEnumerable<SubUnitBookingHistoryResponse>>(
                    new Error("NoAccess", "You do not have access to this subunit", 403));

            var query = _context.BookingRooms
                .Include(br => br.Booking)
                    .ThenInclude(b => b.User)
                .Where(br => br.RoomId == subUnitId);

            if (startDate.HasValue)
                query = query.Where(br => br.Booking.CheckInDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(br => br.Booking.CheckOutDate <= endDate.Value);

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

    #endregion

    #region Amenities Management

    public async Task<Result<IEnumerable<AmenityResponse>>> GetAvailableAmenitiesAsync()
    {
        try
        {
            var amenities = await _context.Amenities
                .OrderBy(a => a.Category)
                .ThenBy(a => a.Name)
                .ToListAsync();

            var responses = amenities.Select(a => new AmenityResponse
            {
                Id = a.Id,
                Name = a.Name,
                Category = a.Category,
                IsAvailable = true
            });

            return Result.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available amenities");
            return Result.Failure<IEnumerable<AmenityResponse>>(
                new Error("GetAmenitiesFailed", "Failed to get amenities", 500));
        }
    }

    public async Task<Result<IEnumerable<AmenityResponse>>> GetUnitAmenitiesAsync(
        string userId,
        int unitId)
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
        string userId,
        int unitId,
        UpdateAmenitiesRequest request)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            // Remove existing amenities
            var existing = await _context.UnitAmenities
                .Where(ua => ua.UnitId == unitId)
                .ToListAsync();

            _context.UnitAmenities.RemoveRange(existing);

            // Add new amenities
            var newAmenities = request.AmenityIds.Select(id => new Domain.Entities.UnitAmenity
            {
                UnitId = unitId,
                AmenityId = id,
                IsAvailable = !request.UnavailableAmenityIds?.Contains(id) ?? true
            });

            await _context.UnitAmenities.AddRangeAsync(newAmenities);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Unit {UnitId} amenities updated by user {UserId}",
                unitId, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating unit amenities");
            return Result.Failure(new Error("UpdateFailed", "Failed to update amenities", 500));
        }
    }

    public async Task<Result<IEnumerable<AmenityResponse>>> GetSubUnitAmenitiesAsync(
        string userId,
        int subUnitId)
    {
        try
        {
            var subUnit = await _context.SubUnits
                .Include(s => s.Unit)
                .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

            if (subUnit == null)
                return Result.Failure<IEnumerable<AmenityResponse>>(
                    new Error("NotFound", "SubUnit not found", 404));

            var hasAccess = await IsAdminOfUnitAsync(userId, subUnit.UnitId);
            if (!hasAccess.Value)
                return Result.Failure<IEnumerable<AmenityResponse>>(
                    new Error("NoAccess", "You do not have access to this subunit", 403));

            var amenities = await _context.SubUniteAmenities
                .Include(sa => sa.Amenity)
                .Where(sa => sa.SubUnitId == subUnitId)
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
        string userId,
        int subUnitId,
        UpdateAmenitiesRequest request)
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

            // Remove existing amenities
            var existing = await _context.SubUniteAmenities
                .Where(sa => sa.SubUnitId == subUnitId)
                .ToListAsync();

            _context.SubUniteAmenities.RemoveRange(existing);

            // Add new amenities
            var newAmenities = request.AmenityIds.Select(id => new SubUniteAmenity
            {
                SubUnitId = subUnitId,
                AmenityId = id,
                IsAvailable = !request.UnavailableAmenityIds?.Contains(id) ?? true
            });

            await _context.SubUniteAmenities.AddRangeAsync(newAmenities);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "SubUnit {SubUnitId} amenities updated by user {UserId}",
                subUnitId, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subunit amenities");
            return Result.Failure(new Error("UpdateFailed", "Failed to update amenities", 500));
        }
    }

    #endregion

    #region IMAGE MANAGEMENT

    public async Task<Result<IEnumerable<ImageDetailResponse>>> GetUnitImagesAsync(
        string userId,
        int unitId)
    {
        try
        {
            var adminUnits = await GetUserAdminUnitsAsync(userId);
            if (!adminUnits.Any())
                return Result.Failure<IEnumerable<ImageDetailResponse>>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var unit = adminUnits.FirstOrDefault(u => u.Id == unitId);
            if (unit == null)
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
        string userId,
        int unitId,
        UpdateImageOrderRequest request)
    {
        try
        {
            var adminUnits = await GetUserAdminUnitsAsync(userId);
            if (!adminUnits.Any())
                return Result.Failure(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var unit = adminUnits.FirstOrDefault(u => u.Id == unitId);
            if (unit == null)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var images = await _context.Set<Domain.Entities.UnitImage>()
                .Where(i => i.UnitId == unitId && !i.IsDeleted)
                .ToListAsync();

            // Reset primary if setting new primary
            if (request.PrimaryImageId.HasValue)
            {
                foreach (var img in images)
                    img.IsPrimary = false;

                var primaryImage = images.FirstOrDefault(i => i.Id == request.PrimaryImageId.Value);
                if (primaryImage != null)
                    primaryImage.IsPrimary = true;
            }

            // Update display order
            if (request.ImageOrders != null && request.ImageOrders.Any())
            {
                foreach (var order in request.ImageOrders)
                {
                    var image = images.FirstOrDefault(i => i.Id == order.ImageId);
                    if (image != null)
                        image.DisplayOrder = order.Order;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Image order updated for unit {UnitId} by user {UserId}",
                unitId, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating image order for user {UserId}", userId);
            return Result.Failure(
                new Error("UpdateOrderFailed", "Failed to update image order", 500));
        }
    }

    public async Task<Result> DeleteUnitImageAsync(string userId, int imageId)
    {
        try
        {
            var adminUnits = await GetUserAdminUnitsAsync(userId);
            if (!adminUnits.Any())
                return Result.Failure(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var image = await _context.Set<Domain.Entities.UnitImage>()
                .Include(i => i.Unit)
                .FirstOrDefaultAsync(i => i.Id == imageId && !i.IsDeleted);

            if (image == null)
                return Result.Failure(new Error("NotFound", "Image not found", 404));

            var unit = adminUnits.FirstOrDefault(u => u.Id == image.UnitId);
            if (unit == null)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this image", 403));

            image.IsDeleted = true;
            image.DeletedAt = DateTime.UtcNow.AddHours(3);
            image.DeletedByUserId = userId;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Image {ImageId} deleted by user {UserId}",
                imageId, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting unit image for user {UserId}", userId);
            return Result.Failure(
                new Error("DeleteImageFailed", "Failed to delete image", 500));
        }
    }

    public async Task<Result<IEnumerable<ImageDetailResponse>>> GetSubUnitImagesAsync(
        string userId,
        int subUnitId)
    {
        try
        {
            var adminUnits = await GetUserAdminUnitsAsync(userId);
            if (!adminUnits.Any())
                return Result.Failure<IEnumerable<ImageDetailResponse>>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var subUnit = await _context.SubUnits
                .Include(s => s.Unit)
                .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

            if (subUnit == null)
                return Result.Failure<IEnumerable<ImageDetailResponse>>(
                    new Error("NotFound", "SubUnit not found", 404));

            var unit = adminUnits.FirstOrDefault(u => u.Id == subUnit.UnitId);
            if (unit == null)
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
            var adminUnits = await GetUserAdminUnitsAsync(userId);
            if (!adminUnits.Any())
                return Result.Failure(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var image = await _context.Set<Domain.Entities.SubUnitImage>()
                .Include(i => i.SubUnit)
                    .ThenInclude(s => s.Unit)
                .FirstOrDefaultAsync(i => i.Id == imageId && !i.IsDeleted);

            if (image == null)
                return Result.Failure(new Error("NotFound", "Image not found", 404));

            var unit = adminUnits.FirstOrDefault(u => u.Id == image.SubUnit.UnitId);
            if (unit == null)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this image", 403));

            image.IsDeleted = true;
            image.DeletedAt = DateTime.UtcNow.AddHours(3);
            image.DeletedByUserId = userId;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "SubUnit image {ImageId} deleted by user {UserId}",
                imageId, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting subunit image for user {UserId}", userId);
            return Result.Failure(
                new Error("DeleteImageFailed", "Failed to delete image", 500));
        }
    }

    #endregion

    #region NOTIFICATIONS & ALERTS

    //public async Task<Result<IEnumerable<AdminNotificationResponse>>> GetAdminNotificationsAsync(
    //    string userId,
    //    NotificationFilter filter)
    //{
    //    try
    //    {
    //        var adminUnits = await GetUserAdminUnitsAsync(userId);
    //        if (!adminUnits.Any())
    //            return Result.Failure<IEnumerable<AdminNotificationResponse>>(
    //                new Error("NoAccess", "User is not a hotel administrator", 403));

    //        var unitIds = adminUnits.Select(u => u.Id).ToList();

    //        var query = _context.Set<UserNotification>()
    //            .Include(un => un.Notification)
    //                .ThenInclude(n => n.TargetUnit)
    //            .Where(un => un.UserId == userId)
    //            .AsQueryable();

    //        // Filter by read status
    //        if (filter.IsRead.HasValue)
    //            query = query.Where(un => un.IsRead == filter.IsRead.Value);

    //        // Filter by notification type
    //        if (filter.Type.HasValue)
    //            query = query.Where(un => un.Notification.Type == filter.Type.Value);

    //        // Filter by priority
    //        if (filter.Priority.HasValue)
    //            query = query.Where(un => un.Notification.Priority == filter.Priority.Value);

    //        // Filter by date range
    //        if (filter.StartDate.HasValue)
    //            query = query.Where(un => un.ReceivedAt >= filter.StartDate.Value);

    //        if (filter.EndDate.HasValue)
    //            query = query.Where(un => un.ReceivedAt <= filter.EndDate.Value);

    //        var notifications = await query
    //            .OrderByDescending(un => un.ReceivedAt)
    //            .Skip((filter.Page - 1) * filter.PageSize)
    //            .Take(filter.PageSize)
    //            .Select(un => new AdminNotificationResponse
    //            {
    //                Id = un.Id,
    //                NotificationId = un.NotificationId,
    //                Title = un.Notification.Title,
    //                Message = un.Notification.Message,
    //                Type = un.Notification.Type.ToString(),
    //                Priority = un.Notification.Priority.ToString(),
    //                IsRead = un.IsRead,
    //                ReadAt = un.ReadAt,
    //                ReceivedAt = un.ReceivedAt,
    //                TargetUnitId = un.Notification.TargetUnitId,
    //                TargetUnitName = un.Notification.TargetUnit != null ? un.Notification.TargetUnit.Name : null
    //            })
    //            .ToListAsync();

    //        return Result.Success<IEnumerable<AdminNotificationResponse>>(notifications);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error getting notifications for user {UserId}", userId);
    //        return Result.Failure<IEnumerable<AdminNotificationResponse>>(
    //            new Error("GetNotificationsFailed", "Failed to get notifications", 500));
    //    }
    //}

    //public async Task<Result> MarkNotificationAsReadAsync(string userId, int notificationId)
    //{
    //    try
    //    {
    //        var adminUnits = await GetUserAdminUnitsAsync(userId);
    //        if (!adminUnits.Any())
    //            return Result.Failure(
    //                new Error("NoAccess", "User is not a hotel administrator", 403));

    //        var userNotification = await _context.Set<UserNotification>()
    //            .FirstOrDefaultAsync(un => un.Id == notificationId && un.UserId == userId);

    //        if (userNotification == null)
    //            return Result.Failure(new Error("NotFound", "Notification not found", 404));

    //        if (!userNotification.IsRead)
    //        {
    //            userNotification.IsRead = true;
    //            userNotification.ReadAt = DateTime.UtcNow.AddHours(3);

    //            await _context.SaveChangesAsync();

    //            _logger.LogInformation(
    //                "Notification {NotificationId} marked as read by user {UserId}",
    //                notificationId, userId);
    //        }

    //        return Result.Success();
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error marking notification as read for user {UserId}", userId);
    //        return Result.Failure(
    //            new Error("MarkReadFailed", "Failed to mark notification as read", 500));
    //    }
    //}

    //public async Task<Result<AlertSettingsResponse>> GetAlertSettingsAsync(string userId)
    //{
    //    try
    //    {
    //        var adminUnits = await GetUserAdminUnitsAsync(userId);
    //        if (!adminUnits.Any())
    //            return Result.Failure<AlertSettingsResponse>(
    //                new Error("NoAccess", "User is not a hotel administrator", 403));

    //        // For now, return default settings
    //        // In a real implementation, these would be stored in a database table
    //        var settings = new AlertSettingsResponse
    //        {
    //            UserId = userId,
    //            EmailNotifications = true,
    //            SmsNotifications = false,
    //            PushNotifications = true,
    //            BookingAlerts = true,
    //            PaymentAlerts = true,
    //            ReviewAlerts = true,
    //            CancellationAlerts = true,
    //            MaintenanceAlerts = true,
    //            LowOccupancyAlerts = false,
    //            HighDemandAlerts = false,
    //            DailyDigest = true,
    //            WeeklyReport = true,
    //            MonthlyReport = true
    //        };

    //        return Result.Success(settings);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error getting alert settings for user {UserId}", userId);
    //        return Result.Failure<AlertSettingsResponse>(
    //            new Error("GetSettingsFailed", "Failed to get alert settings", 500));
    //    }
    //}

    //public async Task<Result> UpdateAlertSettingsAsync(
    //    string userId,
    //    UpdateAlertSettingsRequest request)
    //{
    //    try
    //    {
    //        var adminUnits = await GetUserAdminUnitsAsync(userId);
    //        if (!adminUnits.Any())
    //            return Result.Failure(
    //                new Error("NoAccess", "User is not a hotel administrator", 403));

    //        // In a real implementation, save these settings to a database table
    //        // For now, just log the update
    //        _logger.LogInformation(
    //            "Alert settings updated for user {UserId}",
    //            userId);

    //        return Result.Success();
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error updating alert settings for user {UserId}", userId);
    //        return Result.Failure(
    //            new Error("UpdateSettingsFailed", "Failed to update alert settings", 500));
    //    }
    //}

    #endregion

    #region BULK OPERATIONS

    public async Task<Result> BulkUpdateSubUnitAvailabilityAsync(
        string userId,
        BulkAvailabilityUpdateRequest request)
    {
        try
        {
            var adminUnits = await GetUserAdminUnitsAsync(userId);
            if (!adminUnits.Any())
                return Result.Failure(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var unitIds = adminUnits.Select(u => u.Id).ToList();

            // Verify all subunits belong to admin's units
            var subUnits = await _context.SubUnits
                .Where(s => request.SubUnitIds.Contains(s.Id) &&
                           unitIds.Contains(s.UnitId) &&
                           !s.IsDeleted)
                .ToListAsync();

            if (subUnits.Count != request.SubUnitIds.Count)
                return Result.Failure(
                    new Error("InvalidSubUnits", "Some subunits not found or not accessible", 400));

            // Create availability records for each subunit
            var availabilities = new List<SubUnitAvailability>();
            foreach (var subUnitId in request.SubUnitIds)
            {
                availabilities.Add(new SubUnitAvailability
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
            }

            await _context.Set<SubUnitAvailability>().AddRangeAsync(availabilities);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Bulk availability updated for {Count} subunits by user {UserId}",
                request.SubUnitIds.Count, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating availability for user {UserId}", userId);
            return Result.Failure(
                new Error("BulkUpdateFailed", "Failed to bulk update availability", 500));
        }
    }

    //public async Task<Result> BulkUpdatePricingAsync(
    //    string userId,
    //    BulkPricingUpdateRequest request)
    //{
    //    try
    //    {
    //        var adminUnits = await GetUserAdminUnitsAsync(userId);
    //        if (!adminUnits.Any())
    //            return Result.Failure(
    //                new Error("NoAccess", "User is not a hotel administrator", 403));

    //        var unitIds = adminUnits.Select(u => u.Id).ToList();

    //        // Verify all subunits belong to admin's units
    //        var subUnits = await _context.SubUnits
    //            .Where(s => request.SubUnitIds.Contains(s.Id) &&
    //                       unitIds.Contains(s.UnitId) &&
    //                       !s.IsDeleted)
    //            .ToListAsync();

    //        if (subUnits.Count != request.SubUnitIds.Count)
    //            return Result.Failure(
    //                new Error("InvalidSubUnits", "Some subunits not found or not accessible", 400));

    //        // Update pricing based on request type
    //        foreach (var subUnit in subUnits)
    //        {
    //            if (request.UpdateType == PricingUpdateType.Fixed)
    //            {
    //                subUnit.PricePerNight = request.NewPrice;
    //            }
    //            else if (request.UpdateType == PricingUpdateType.Percentage)
    //            {
    //                var adjustment = subUnit.PricePerNight * (request.PercentageChange / 100);
    //                subUnit.PricePerNight += adjustment;
    //            }
    //            else if (request.UpdateType == PricingUpdateType.Amount)
    //            {
    //                subUnit.PricePerNight += request.AmountChange;
    //            }

    //            // Ensure price doesn't go below minimum
    //            if (subUnit.PricePerNight < 0)
    //                subUnit.PricePerNight = 0;
    //        }

    //        await _context.SaveChangesAsync();

    //        _logger.LogInformation(
    //            "Bulk pricing updated for {Count} subunits by user {UserId}",
    //            request.SubUnitIds.Count, userId);

    //        return Result.Success();
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error bulk updating pricing for user {UserId}", userId);
    //        return Result.Failure(
    //            new Error("BulkUpdateFailed", "Failed to bulk update pricing", 500));
    //    }
    //}

    //public async Task<Result> CopyAvailabilitySettingsAsync(
    //    string userId,
    //    CopyAvailabilityRequest request)
    //{
    //    try
    //    {
    //        var adminUnits = await GetUserAdminUnitsAsync(userId);
    //        if (!adminUnits.Any())
    //            return Result.Failure(
    //                new Error("NoAccess", "User is not a hotel administrator", 403));

    //        var unitIds = adminUnits.Select(u => u.Id).ToList();

    //        // Verify source subunit
    //        var sourceSubUnit = await _context.SubUnits
    //            .FirstOrDefaultAsync(s => s.Id == request.SourceSubUnitId &&
    //                                     unitIds.Contains(s.UnitId) &&
    //                                     !s.IsDeleted);

    //        if (sourceSubUnit == null)
    //            return Result.Failure(
    //                new Error("SourceNotFound", "Source subunit not found or not accessible", 404));

    //        // Verify target subunits
    //        var targetSubUnits = await _context.SubUnits
    //            .Where(s => request.TargetSubUnitIds.Contains(s.Id) &&
    //                       unitIds.Contains(s.UnitId) &&
    //                       !s.IsDeleted)
    //            .ToListAsync();

    //        if (targetSubUnits.Count != request.TargetSubUnitIds.Count)
    //            return Result.Failure(
    //                new Error("InvalidTargets", "Some target subunits not found or not accessible", 400));

    //        // Get source availability settings
    //        var sourceAvailabilities = await _context.Set<SubUnitAvailability>()
    //            .Where(a => a.SubUnitId == request.SourceSubUnitId &&
    //                       a.StartDate >= request.StartDate &&
    //                       a.EndDate <= request.EndDate)
    //            .ToListAsync();

    //        if (!sourceAvailabilities.Any())
    //            return Result.Failure(
    //                new Error("NoSourceData", "No availability settings found for source subunit in the specified date range", 404));

    //        // Copy to target subunits
    //        var newAvailabilities = new List<SubUnitAvailability>();
    //        foreach (var targetSubUnitId in request.TargetSubUnitIds)
    //        {
    //            foreach (var sourceAvail in sourceAvailabilities)
    //            {
    //                newAvailabilities.Add(new SubUnitAvailability
    //                {
    //                    SubUnitId = targetSubUnitId,
    //                    StartDate = sourceAvail.StartDate,
    //                    EndDate = sourceAvail.EndDate,
    //                    IsAvailable = sourceAvail.IsAvailable,
    //                    Reason = sourceAvail.Reason,
    //                    SpecialPrice = request.CopyPricing ? sourceAvail.SpecialPrice : null,
    //                    WeekendPrice = request.CopyPricing ? sourceAvail.WeekendPrice : null,
    //                    UpdatedByUserId = userId,
    //                    CreatedAt = DateTime.UtcNow.AddHours(3)
    //                });
    //            }
    //        }

    //        await _context.Set<SubUnitAvailability>().AddRangeAsync(newAvailabilities);
    //        await _context.SaveChangesAsync();

    //        _logger.LogInformation(
    //            "Availability settings copied from subunit {SourceId} to {Count} target subunits by user {UserId}",
    //            request.SourceSubUnitId, request.TargetSubUnitIds.Count, userId);

    //        return Result.Success();
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error copying availability settings for user {UserId}", userId);
    //        return Result.Failure(
    //            new Error("CopyFailed", "Failed to copy availability settings", 500));
    //    }
    //}

    #endregion

    #region OFFERS MANAGEMENT
    public string GetCloudFrontUrl(string s3Key)
    {
        if (string.IsNullOrEmpty(s3Key))
            return string.Empty;

        if (string.IsNullOrEmpty(CloudFrontUrl))
            return $"https://{BucketName}.s3.amazonaws.com/{s3Key}";

        return $"https://{CloudFrontUrl}/{s3Key}";
    }
    public async Task<Result<OfferResponse>> CreateUnitOfferAsync(
        string userId,
        CreateOfferRequest request)
    {
        try
        {
            var adminUnits = await GetUserAdminUnitsAsync(userId);

            var unitId = adminUnits.Select(u => u.Id).FirstOrDefault();


            string? imageUrl = null;
            if (request.ImageFile != null)
            {
                var uploadResult = await UploadOfferImageAsync(
                    request.ImageFile, userId);

                if (uploadResult.IsSuccess)
                    imageUrl = uploadResult.Value;
            }


            var offer = new Offer
            {
                Title = request.Title,
                UserId = userId,
                Description = request.Description,
                ImageUrl = GetCloudFrontUrl(imageUrl),
                UnitId = unitId,
                DiscountPercentage = request.DiscountPercentage,
                DiscountAmount = request.DiscountAmount,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsActive = true,
                IsFeatured = false, // Hotel admin cannot create featured offers
                CreatedAt = DateTime.UtcNow.AddHours(3)
            };

            _context.Offers.Add(offer);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Offer {OfferId} created for unit {UnitId} by user {UserId}",
                offer.Id, unitId, userId);

            var unit = await _context.Units.FindAsync(unitId);

            return Result.Success(new OfferResponse
            {
                Id = offer.Id,
                Title = offer.Title,
                Description = offer.Description,
                ImageUrl = offer.ImageUrl,
                UnitId = offer.UnitId,
                UnitName = unit?.Name ?? string.Empty,
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
            _logger.LogError(ex, "Error creating offer for unit {UnitId}");
            return Result.Failure<OfferResponse>(
                new Error(ex.InnerException.Message, "Failed to create offer", 500));
        }
    }

    public async Task<Result<string>> UploadOfferImageAsync(
      Microsoft.AspNetCore.Http.IFormFile image,
      string userId)
    {
        try
        {
            var transferUtility = new TransferUtility(s3Client);
            var timestamp = DateTime.UtcNow.Ticks;

            var originalKey = $"offers/{userId}/{timestamp}.webp";
            using (var originalStream = new MemoryStream())
            {
                await ConvertToWebpAsync(image.OpenReadStream(), originalStream, 75);
                originalStream.Position = 0;

                await transferUtility.UploadAsync(new TransferUtilityUploadRequest
                {
                    InputStream = originalStream,
                    Key = originalKey,
                    BucketName = BucketName,
                    ContentType = "image/webp",
                    CannedACL = S3CannedACL.Private
                });
            }


            return Result.Success((originalKey));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading offer image");
            return Result.Failure<string>(
                new Error("UploadFailed", "Failed to upload image", 500));
        }
    }

    private async Task ConvertToWebpAsync(Stream input, Stream output, int quality)
    {
        await Task.Run(() =>
        {
            using var image = Image.Load(input);
            var encoder = new WebpEncoder
            {
                Quality = 75,
                Method = WebpEncodingMethod.Fastest,
                SkipMetadata = true
            };
            image.Save(output, encoder);
        });
    }


    public async Task<Result<IEnumerable<OfferResponse>>> GetMyUnitOffersAsync(
        string userId)
    {
        try
        {
            var adminUnits = await GetUserAdminUnitsAsync(userId);
            if (!adminUnits.Any())
                return Result.Failure<IEnumerable<OfferResponse>>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var unitIds = adminUnits.Select(u => u.Id).ToList();

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
        string userId,
        int offerId,
        UpdateOfferRequest request)
    {
        try
        {
            var offer = await _context.Offers
                .Include(o => o.Unit)
                .FirstOrDefaultAsync(o => o.Id == offerId);

            if (offer == null)
                return Result.Failure<OfferResponse>(
                    new Error("NotFound", "Offer not found", 404));

            if (!offer.UnitId.HasValue)
                return Result.Failure<OfferResponse>(
                    new Error("InvalidOffer", "This is a system-wide offer", 403));

            var hasAccess = await IsAdminOfUnitAsync(userId, offer.UnitId.Value);
            if (!hasAccess.Value)
                return Result.Failure<OfferResponse>(
                    new Error("NoAccess", "You do not have access to this offer", 403));

            // Update fields
            if (request.Title != null) offer.Title = request.Title;
            if (request.Description != null) offer.Description = request.Description;
            if (request.DiscountPercentage.HasValue) offer.DiscountPercentage = request.DiscountPercentage;
            if (request.DiscountAmount.HasValue) offer.DiscountAmount = request.DiscountAmount;
            if (request.StartDate.HasValue) offer.StartDate = request.StartDate.Value;
            if (request.EndDate.HasValue) offer.EndDate = request.EndDate.Value;
            if (request.IsActive.HasValue) offer.IsActive = request.IsActive.Value;

            // Handle image upload
            if (request.ImageFile != null)
            {
                var uploadResult = await UploadOfferImageAsync(
                    request.ImageFile, userId);

                if (uploadResult.IsSuccess)
                    offer.ImageUrl = uploadResult.Value;
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
            var offer = await _context.Offers
                .FirstOrDefaultAsync(o => o.Id == offerId);

            if (offer == null)
                return Result.Failure(new Error("NotFound", "Offer not found", 404));

            if (!offer.UnitId.HasValue)
                return Result.Failure(
                    new Error("InvalidOffer", "Cannot delete system-wide offer", 403));

            var hasAccess = await IsAdminOfUnitAsync(userId, offer.UnitId.Value);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this offer", 403));

            _context.Offers.Remove(offer);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Offer {OfferId} deleted by user {UserId}", offerId, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting offer {OfferId}", offerId);
            return Result.Failure(new Error("DeleteOfferFailed", "Failed to delete offer", 500));
        }
    }

    #endregion

    #region CUSTOM POLICIES MANAGEMENT

    public async Task<Result<IEnumerable<UnitCustomPolicyResponse>>> GetUnitCustomPoliciesAsync(
        string userId,
        int unitId)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure<IEnumerable<UnitCustomPolicyResponse>>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var policies = await _context.Set<UnitCustomPolicy>()
                .Where(p => p.UnitId == unitId && p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            var responses = policies.Select(p => new UnitCustomPolicyResponse
            {
                Id = p.Id,
                UnitId = p.UnitId,
                Title = p.Title,
                Description = p.Description,
                Category = p.Category,
                DisplayOrder = p.DisplayOrder,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            });

            return Result.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting custom policies for unit {UnitId}", unitId);
            return Result.Failure<IEnumerable<UnitCustomPolicyResponse>>(
                new Error("GetPoliciesFailed", "Failed to retrieve custom policies", 500));
        }
    }

    public async Task<Result<UnitCustomPolicyResponse>> CreateUnitCustomPolicyAsync(
        string userId,
        int unitId,
        CreateUnitCustomPolicyRequest request)
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

            _logger.LogInformation(
                "Custom policy {PolicyId} created for unit {UnitId} by user {UserId}",
                policy.Id, unitId, userId);

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
        string userId,
        int policyId,
        UpdateUnitCustomPolicyRequest request)
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

            // Update fields
            if (request.Title != null) policy.Title = request.Title;
            if (request.Description != null) policy.Description = request.Description;
            if (request.Category != null) policy.Category = request.Category;
            if (request.DisplayOrder.HasValue) policy.DisplayOrder = request.DisplayOrder.Value;
            if (request.IsActive.HasValue) policy.IsActive = request.IsActive.Value;

            policy.UpdatedAt = DateTime.UtcNow.AddHours(3);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Custom policy {PolicyId} updated by user {UserId}",
                policyId, userId);

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
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this policy", 403));

            _context.Set<UnitCustomPolicy>().Remove(policy);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Custom policy {PolicyId} deleted by user {UserId}",
                policyId, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting custom policy {PolicyId}", policyId);
            return Result.Failure(
                new Error("DeletePolicyFailed", "Failed to delete custom policy", 500));
        }
    }

    public async Task<Result> ReorderUnitCustomPoliciesAsync(
        string userId,
        int unitId,
        List<int> policyIds)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var policies = await _context.Set<UnitCustomPolicy>()
                .Where(p => p.UnitId == unitId && policyIds.Contains(p.Id))
                .ToListAsync();

            foreach (var (policyId, index) in policyIds.Select((id, idx) => (id, idx)))
            {
                var policy = policies.FirstOrDefault(p => p.Id == policyId);
                if (policy != null)
                {
                    policy.DisplayOrder = index;
                    policy.UpdatedAt = DateTime.UtcNow.AddHours(3);
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Custom policies reordered for unit {UnitId} by user {UserId}",
                unitId, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering custom policies for unit {UnitId}", unitId);
            return Result.Failure(
                new Error("ReorderFailed", "Failed to reorder custom policies", 500));
        }
    }

    #endregion

    #region UNIT OPTIONS MANAGEMENT

    public async Task<Result<List<string>>> GetUnitOptionsAsync(string userId, int unitId)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure<List<string>>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var unit = await _context.Units
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

            if (unit == null)
                return Result.Failure<List<string>>(
                    new Error("NotFound", "Unit not found", 404));

            var options = System.Text.Json.JsonSerializer.Deserialize<List<string>>(
                unit.OptionsJson) ?? new List<string>();

            return Result.Success(options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting options for unit {UnitId}", unitId);
            return Result.Failure<List<string>>(
                new Error("GetOptionsFailed", "Failed to retrieve unit options", 500));
        }
    }

    public async Task<Result> UpdateUnitOptionsAsync(
        string userId,
        int unitId,
        UpdateUnitOptionsRequest request)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var unit = await _context.Units
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

            if (unit == null)
                return Result.Failure(new Error("NotFound", "Unit not found", 404));

            unit.OptionsJson = System.Text.Json.JsonSerializer.Serialize(request.Options);
            unit.UpdatedAt = DateTime.UtcNow.AddHours(3);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Options updated for unit {UnitId} by user {UserId}",
                unitId, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating options for unit {UnitId}", unitId);
            return Result.Failure(
                new Error("UpdateOptionsFailed", "Failed to update unit options", 500));
        }
    }

    #endregion

    #region UNIT CURRENCY MANAGEMENT

    public async Task<Result<PriceCurrency>> GetUnitCurrencyAsync(string userId, int unitId)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure<PriceCurrency>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var unit = await _context.Units
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

            if (unit == null)
                return Result.Failure<PriceCurrency>(
                    new Error("NotFound", "Unit not found", 404));

            return Result.Success(unit.PriceCurrency);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting currency for unit {UnitId}", unitId);
            return Result.Failure<PriceCurrency>(
                new Error("GetCurrencyFailed", "Failed to retrieve unit currency", 500));
        }
    }

    public async Task<Result> UpdateUnitCurrencyAsync(
        string userId,
        int unitId,
        UpdateUnitCurrencyRequest request)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var unit = await _context.Units
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

            if (unit == null)
                return Result.Failure(new Error("NotFound", "Unit not found", 404));

            unit.PriceCurrency = request.Currency;
            unit.UpdatedAt = DateTime.UtcNow.AddHours(3);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Currency updated to {Currency} for unit {UnitId} by user {UserId}",
                request.Currency, unitId, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating currency for unit {UnitId}", unitId);
            return Result.Failure(
                new Error("UpdateCurrencyFailed", "Failed to update unit currency", 500));
        }
    }

    #endregion

    #region UNIT RANK MANAGEMENT

    /// <summary>
    /// Update unit star ranking (1-5 stars)
    /// </summary>
    public async Task<Result> UpdateUnitRankAsync(
        string userId,
        int unitId,
        int rank)
    {
        try
        {
            // Validate rank
            if (rank < 1 || rank > 5)
                return Result.Failure(
                    new Error("InvalidRank", "Rank must be between 1 and 5 stars", 400));

            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var unit = await _context.Units
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

            if (unit == null)
                return Result.Failure(new Error("NotFound", "Unit not found", 404));

            unit.Rank = rank;
            unit.UpdatedAt = DateTime.UtcNow.AddHours(3);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Unit {UnitId} rank updated to {Rank} stars by user {UserId}",
                unitId, rank, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating unit rank");
            return Result.Failure(new Error("UpdateRankFailed", "Failed to update unit rank", 500));
        }
    }

    /// <summary>
    /// Get unit rank
    /// </summary>
    public async Task<Result<int?>> GetUnitRankAsync(string userId, int unitId)
    {
        try
        {
            var hasAccess = await IsAdminOfUnitAsync(userId, unitId);
            if (!hasAccess.Value)
                return Result.Failure<int?>(
                    new Error("NoAccess", "You do not have access to this unit", 403));

            var unit = await _context.Units
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

            if (unit == null)
                return Result.Failure<int?>(new Error("NotFound", "Unit not found", 404));

            return Result.Success(unit.Rank);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unit rank");
            return Result.Failure<int?>(
                new Error("GetRankFailed", "Failed to retrieve unit rank", 500));
        }
    }

    #endregion
    public async Task<Result<ImageDetailResponse>> UploadUnitImageAsync(
        string userId,
        IFormFile image,
        string? caption = null)
    {
        try
        {
            var adminUnits = await GetUserAdminUnitsAsync(userId);

            if (!adminUnits.Any())
                return Result.Failure<ImageDetailResponse>(
                    new Error("NoAccess", "User is not a hotel administrator", 403));

            var query = adminUnits.AsQueryable();

            // Check access

            var unitId = adminUnits.First().Id;

            // Upload to S3
            var uploadResult = await service.UploadUnitImageAsync(image, unitId, userId);
            if (!uploadResult.IsSuccess)
                return Result.Failure<ImageDetailResponse>(uploadResult.Error);

            // Create database record
            var unitImage = new Domain.Entities.UnitImage
            {
                UnitId = unitId,
                ImageUrl = uploadResult.Value.ImageUrl,
                S3Key = uploadResult.Value.S3Key,
                S3Bucket = "hujjzy-bucket",
                ThumbnailUrl = null, // No thumbnail
                MediumUrl = null,    // No medium size
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

            // Return response
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
            _logger.LogError(ex, "Error uploading unit image for unit {UnitId}");
            return Result.Failure<ImageDetailResponse>(
                new Error("UploadFailed", $"Failed to upload image: {ex.Message}", 500));
        }
    }
}