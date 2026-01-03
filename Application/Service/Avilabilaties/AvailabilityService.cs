using Application.Abstraction;
using Application.Contracts.Availability;
using Application.Contracts.SubUnit;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Service.Availability;

public class AvailabilityService(
    ApplicationDbcontext context,
    ILogger<AvailabilityService> logger) : IAvailabilityService
{
    private readonly ApplicationDbcontext _context = context;
    private readonly ILogger<AvailabilityService> _logger = logger;

    // ============= UNIT AVAILABILITY =============

    public async Task<Result> SetUnitAvailabilityAsync(SetUnitAvailabilityRequest request)
    {
        try
        {
            var unit = await _context.Units
                .FirstOrDefaultAsync(u => u.Id == request.UnitId && !u.IsDeleted);

            if (unit == null)
                return Result.Failure(new Error("NotFound", "Unit not found", 404));

            var existing = await _context.Set<UnitAvailability>()
                .FirstOrDefaultAsync(a =>
                    a.UnitId == request.UnitId &&
                    a.StartDate == request.StartDate &&
                    a.EndDate == request.EndDate);

            if (existing != null)
            {
                existing.IsAvailable = request.IsAvailable;
                existing.Reason = request.Reason;
                existing.UpdatedAt = DateTime.UtcNow.AddHours(3);
                existing.UpdatedByUserId = request.UpdatedByUserId;
            }
            else
            {
                var availability = new UnitAvailability
                {
                    UnitId = request.UnitId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    IsAvailable = request.IsAvailable,
                    Reason = request.Reason,
                    UpdatedByUserId = request.UpdatedByUserId,
                    CreatedAt = DateTime.UtcNow.AddHours(3)
                };

                await _context.Set<UnitAvailability>().AddAsync(availability);
            }

            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting unit availability");
            return Result.Failure(new Error("SetFailed", "Failed to set availability", 500));
        }
    }

    public async Task<Result<List<AvailabilityResponse>>> GetUnitAvailabilityAsync(
        int unitId,
        DateTime startDate,
        DateTime endDate)
    {
        try
        {
            var availabilities = await _context.Set<UnitAvailability>()
                .Where(a => a.UnitId == unitId &&
                           a.StartDate >= startDate &&
                           a.EndDate <= endDate)
                .AsNoTracking()
                .ToListAsync();

            var responses = availabilities.Select(a => new AvailabilityResponse(
                a.Id,
                a.StartDate,
                a.EndDate,
                a.IsAvailable,
                a.Reason?.ToString(),
                null, // SpecialPrice not applicable for Unit
                null  // WeekendPrice not applicable for Unit
            )).ToList();

            return Result.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unit availability");
            return Result.Failure<List<AvailabilityResponse>>(
                new Error("GetFailed", "Failed to get availability", 500));
        }
    }

    public async Task<Result<bool>> IsUnitAvailableAsync(
        int unitId,
        DateTime checkIn,
        DateTime checkOut)
    {
        // Check if unit has any available subunits
        var hasAvailableRooms = await _context.SubUnits
            .Where(r => r.UnitId == unitId && !r.IsDeleted && r.IsAvailable)
            .AnyAsync();

        if (!hasAvailableRooms)
            return Result.Success(false);

        // Check for unit-level blocks
        var hasUnitBlock = await _context.Set<UnitAvailability>()
            .AnyAsync(a => a.UnitId == unitId &&
                          !a.IsAvailable &&
                          a.StartDate < checkOut &&
                          a.EndDate > checkIn);

        if (hasUnitBlock)
            return Result.Success(false);

        // Check if any rooms are available
        var bookedRoomIds = await _context.BookingRooms
            .Include(br => br.Booking)
            .Where(br => br.Room.UnitId == unitId &&
                        br.Booking.CheckInDate < checkOut &&
                        br.Booking.CheckOutDate > checkIn &&
                        br.Booking.Status != BookingStatus.Cancelled)
            .Select(br => br.RoomId)
            .ToListAsync();

        var totalRooms = await _context.SubUnits
            .Where(r => r.UnitId == unitId && !r.IsDeleted && r.IsAvailable)
            .CountAsync();

        return Result.Success(totalRooms > bookedRoomIds.Distinct().Count());
    }

    // ============= SUBUNIT AVAILABILITY =============

    public async Task<Result> SetSubUnitAvailabilityAsync(SetSubUnitAvailabilityRequest request)
    {
        try
        {
            var subUnit = await _context.SubUnits
                .FirstOrDefaultAsync(s => s.Id == request.SubUnitId && !s.IsDeleted);

            if (subUnit == null)
                return Result.Failure(new Error("NotFound", "SubUnit not found", 404));

            var existing = await _context.Set<SubUnitAvailability>()
                .FirstOrDefaultAsync(a =>
                    a.SubUnitId == request.SubUnitId &&
                    a.StartDate == request.StartDate &&
                    a.EndDate == request.EndDate);

            if (existing != null)
            {
                existing.IsAvailable = request.IsAvailable;
                existing.Reason = request.Reason;
                existing.SpecialPrice = request.SpecialPrice;
                existing.WeekendPrice = request.WeekendPrice;
                existing.UpdatedAt = DateTime.UtcNow.AddHours(3);
                existing.UpdatedByUserId = request.UpdatedByUserId;
            }
            else
            {
                var availability = new SubUnitAvailability
                {
                    SubUnitId = request.SubUnitId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    IsAvailable = request.IsAvailable,
                    Reason = request.Reason,
                    SpecialPrice = request.SpecialPrice,
                    WeekendPrice = request.WeekendPrice,
                    UpdatedByUserId = request.UpdatedByUserId,
                    CreatedAt = DateTime.UtcNow.AddHours(3)
                };

                await _context.Set<SubUnitAvailability>().AddAsync(availability);
            }

            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting subunit availability");
            return Result.Failure(new Error("SetFailed", "Failed to set availability", 500));
        }
    }

    public async Task<Result<List<AvailabilityResponse>>> GetSubUnitAvailabilityAsync(
        int subUnitId,
        DateTime startDate,
        DateTime endDate)
    {
        try
        {
            var availabilities = await _context.Set<SubUnitAvailability>()
                .Where(a => a.SubUnitId == subUnitId &&
                           a.StartDate >= startDate &&
                           a.EndDate <= endDate)
                .AsNoTracking()
                .ToListAsync();

            var responses = availabilities.Select(a => new AvailabilityResponse(
                a.Id,
                a.StartDate,
                a.EndDate,
                a.IsAvailable,
                a.Reason?.ToString(),
                a.SpecialPrice,
                a.WeekendPrice
            )).ToList();

            return Result.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subunit availability");
            return Result.Failure<List<AvailabilityResponse>>(
                new Error("GetFailed", "Failed to get availability", 500));
        }
    }

    public async Task<Result<bool>> IsSubUnitAvailableAsync(
        int subUnitId,
        DateTime checkIn,
        DateTime checkOut)
    {
        var subUnit = await _context.SubUnits
            .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

        if (subUnit == null || !subUnit.IsAvailable)
            return Result.Success(false);

        // Check availability records
        var hasBlock = await _context.Set<SubUnitAvailability>()
            .AnyAsync(a => a.SubUnitId == subUnitId &&
                          !a.IsAvailable &&
                          a.StartDate < checkOut &&
                          a.EndDate > checkIn);

        if (hasBlock)
            return Result.Success(false);

        // Check for bookings
        var hasBooking = await _context.BookingRooms
            .Include(br => br.Booking)
            .AnyAsync(br => br.RoomId == subUnitId &&
                           br.Booking.CheckInDate < checkOut &&
                           br.Booking.CheckOutDate > checkIn &&
                           br.Booking.Status != BookingStatus.Cancelled);

        return Result.Success(!hasBooking);
    }

    // ============= BATCH OPERATIONS =============

    public async Task<Result> BlockDatesAsync(BlockDatesRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var subUnit = await _context.SubUnits
                .FirstOrDefaultAsync(s => s.Id == request.SubUnitId && !s.IsDeleted);

            if (subUnit == null)
                return Result.Failure(new Error("NotFound", "SubUnit not found", 404));

            foreach (var dateRange in request.DateRanges)
            {
                var availability = new SubUnitAvailability
                {
                    SubUnitId = request.SubUnitId,
                    StartDate = dateRange.StartDate,
                    EndDate = dateRange.EndDate,
                    IsAvailable = false,
                    Reason = request.Reason,
                    UpdatedByUserId = request.UpdatedByUserId,
                    CreatedAt = DateTime.UtcNow.AddHours(3)
                };

                await _context.Set<SubUnitAvailability>().AddAsync(availability);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Blocked {Count} date ranges for subunit {SubUnitId}",
                request.DateRanges.Count, request.SubUnitId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error blocking dates");
            return Result.Failure(new Error("BlockFailed", "Failed to block dates", 500));
        }
    }

    public async Task<Result> SetSpecialPricingAsync(SetSpecialPricingRequests request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var subUnit = await _context.SubUnits
                .FirstOrDefaultAsync(s => s.Id == request.SubUnitId && !s.IsDeleted);

            if (subUnit == null)
                return Result.Failure(new Error("NotFound", "SubUnit not found", 404));

            foreach (var priceRange in request.PriceRanges)
            {
                var existing = await _context.Set<SubUnitAvailability>()
                    .FirstOrDefaultAsync(a =>
                        a.SubUnitId == request.SubUnitId &&
                        a.StartDate == priceRange.StartDate &&
                        a.EndDate == priceRange.EndDate);

                if (existing != null)
                {
                    existing.SpecialPrice = priceRange.WeekdayPrice;
                    existing.WeekendPrice = priceRange.WeekendPrice;
                    existing.UpdatedAt = DateTime.UtcNow.AddHours(3);
                    existing.UpdatedByUserId = request.UpdatedByUserId;
                }
                else
                {
                    var availability = new SubUnitAvailability
                    {
                        SubUnitId = request.SubUnitId,
                        StartDate = priceRange.StartDate,
                        EndDate = priceRange.EndDate,
                        IsAvailable = true,
                        SpecialPrice = priceRange.WeekdayPrice,
                        WeekendPrice = priceRange.WeekendPrice,
                        UpdatedByUserId = request.UpdatedByUserId,
                        CreatedAt = DateTime.UtcNow.AddHours(3)
                    };

                    await _context.Set<SubUnitAvailability>().AddAsync(availability);
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error setting special pricing");
            return Result.Failure(new Error("PricingFailed", "Failed to set pricing", 500));
        }
    }

    // ============= INITIALIZATION =============

    public async Task<Result> InitializeDefaultAvailabilityAsync(int subUnitId, int daysAhead = 365)
    {
        try
        {
            var subUnit = await _context.SubUnits
                .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

            if (subUnit == null)
                return Result.Failure(new Error("NotFound", "SubUnit not found", 404));

            // Check if already initialized
            var hasAvailability = await _context.Set<SubUnitAvailability>()
                .AnyAsync(a => a.SubUnitId == subUnitId);

            if (hasAvailability)
                return Result.Success(); // Already initialized

            // Create a single availability record for the next year
            var availability = new SubUnitAvailability
            {
                SubUnitId = subUnitId,
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(daysAhead),
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow.AddHours(3)
            };

            await _context.Set<SubUnitAvailability>().AddAsync(availability);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Initialized default availability for subunit {SubUnitId} for {Days} days",
                subUnitId, daysAhead);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing default availability");
            return Result.Failure(new Error("InitFailed", "Failed to initialize availability", 500));
        }
    }

    public async Task<Result> InitializeUnitDefaultAvailabilityAsync(int unitId, int daysAhead = 365)
    {
        try
        {
            var unit = await _context.Units
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

            if (unit == null)
                return Result.Failure(new Error("NotFound", "Unit not found", 404));

            // Check if already initialized
            var hasAvailability = await _context.Set<UnitAvailability>()
                .AnyAsync(a => a.UnitId == unitId);

            if (hasAvailability)
                return Result.Success(); // Already initialized

            // Create a single availability record for the next year
            var availability = new UnitAvailability
            {
                UnitId = unitId,
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(daysAhead),
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow.AddHours(3)
            };

            await _context.Set<UnitAvailability>().AddAsync(availability);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Initialized default availability for unit {UnitId} for {Days} days",
                unitId, daysAhead);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing unit default availability");
            return Result.Failure(new Error("InitFailed", "Failed to initialize unit availability", 500));
        }
    }

    public async Task<Result<Dictionary<DateTime, DayAvailability>>> GetAvailabilityCalendarAsync(
        int subUnitId,
        int year,
        int month)
    {
        try
        {
            var subUnit = await _context.SubUnits
                .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

            if (subUnit == null)
                return Result.Failure<Dictionary<DateTime, DayAvailability>>(
                    new Error("NotFound", "SubUnit not found", 404));

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // Get availability records
            var availabilities = await _context.Set<SubUnitAvailability>()
                .Where(a => a.SubUnitId == subUnitId &&
                           a.StartDate <= endDate &&
                           a.EndDate >= startDate)
                .AsNoTracking()
                .ToListAsync();

            // Get bookings
            var bookings = await _context.BookingRooms
                .Include(br => br.Booking)
                .Where(br => br.RoomId == subUnitId &&
                            br.Booking.CheckInDate <= endDate &&
                            br.Booking.CheckOutDate >= startDate &&
                            br.Booking.Status != BookingStatus.Cancelled)
                .Select(br => new { br.Booking.CheckInDate, br.Booking.CheckOutDate })
                .AsNoTracking()
                .ToListAsync();

            var calendar = new Dictionary<DateTime, DayAvailability>();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var isBooked = bookings.Any(b => date >= b.CheckInDate && date < b.CheckOutDate);

                var availability = availabilities.FirstOrDefault(a =>
                    date >= a.StartDate && date <= a.EndDate);

                var isAvailable = !isBooked &&
                                 (availability?.IsAvailable ?? true) &&
                                 subUnit.IsAvailable;

                var price = availability?.SpecialPrice;
                if (price == null && (date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Saturday))
                {
                    price = availability?.WeekendPrice ?? subUnit.PricePerNight;
                }
                else if (price == null)
                {
                    price = subUnit.PricePerNight;
                }

                calendar[date] = new DayAvailability(
                    date,
                    isAvailable,
                    isBooked,
                    price,
                    date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Saturday,
                    availability?.Reason
                );
            }

            return Result.Success(calendar);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting availability calendar");
            return Result.Failure<Dictionary<DateTime, DayAvailability>>(
                new Error("CalendarFailed", "Failed to get calendar", 500));
        }
    }
}