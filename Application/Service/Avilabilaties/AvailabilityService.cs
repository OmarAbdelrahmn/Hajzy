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

    #region UNIT AVAILABILITY

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
                existing.Reason = (Domain.Entities.UnavailabilityReason?)request.Reason;
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
                    Reason = (Domain.Entities.UnavailabilityReason?)request.Reason,
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
                null,
                null
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

    /// <summary>
    /// UPDATED: Check unit availability - handles BOTH standalone units and units with subunits
    /// </summary>
    public async Task<Result<UnitAvailabilityStatus>> CheckUnitAvailabilityAsync(
        int unitId,
        DateTime checkIn,
        DateTime checkOut)
    {
        try
        {
            // Check if unit exists and is active
            var unit = await _context.Units
                .Include(u => u.Rooms)
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

            if (unit == null)
                return Result.Failure<UnitAvailabilityStatus>(
                    new Error("NotFound", "Unit not found", 404));

            if (!unit.IsActive)
                return Result.Success(new UnitAvailabilityStatus
                {
                    IsAvailable = false,
                    Reason = "Unit is not active",
                    IsStandaloneUnit = true,
                    AvailableSubUnitsCount = 0,
                    TotalSubUnitsCount = 0
                });

            // Determine if this is a standalone unit (no subunits)
            var subUnits = unit.Rooms
                .Where(r => !r.IsDeleted && r.IsAvailable)
                .ToList();

            var isStandaloneUnit = !subUnits.Any();

            // Check for manual blocks (ONLY unavailable records block)
            var hasUnitBlock = await _context.Set<UnitAvailability>()
                .AnyAsync(a => a.UnitId == unitId &&
                              !a.IsAvailable &&
                              a.StartDate < checkOut &&
                              a.EndDate > checkIn);

            if (hasUnitBlock)
            {
                var block = await _context.Set<UnitAvailability>()
                    .FirstOrDefaultAsync(a => a.UnitId == unitId &&
                                  !a.IsAvailable &&
                                  a.StartDate < checkOut &&
                                  a.EndDate > checkIn);

                return Result.Success(new UnitAvailabilityStatus
                {
                    IsAvailable = false,
                    Reason = block?.Reason?.ToString() ?? "Unit blocked",
                    HasManualBlock = true,
                    IsStandaloneUnit = isStandaloneUnit,
                    AvailableSubUnitsCount = 0,
                    TotalSubUnitsCount = subUnits.Count
                });
            }

            // Check for active bookings
            var hasActiveBooking = await _context.Bookings
                .AnyAsync(b => b.UnitId == unitId &&
                              b.CheckInDate < checkOut &&
                              b.CheckOutDate > checkIn &&
                              b.Status != BookingStatus.Cancelled &&
                              b.Status != BookingStatus.Completed);

            if (hasActiveBooking)
                return Result.Success(new UnitAvailabilityStatus
                {
                    IsAvailable = false,
                    Reason = isStandaloneUnit ? "Unit is booked" : "Entire unit is booked",
                    HasActiveBooking = true,
                    IsStandaloneUnit = isStandaloneUnit,
                    AvailableSubUnitsCount = 0,
                    TotalSubUnitsCount = subUnits.Count
                });

            // FOR STANDALONE UNITS: If no blocks and no bookings, it's available
            if (isStandaloneUnit)
            {
                return Result.Success(new UnitAvailabilityStatus
                {
                    IsAvailable = true,
                    Reason = null,
                    HasManualBlock = false,
                    HasActiveBooking = false,
                    IsStandaloneUnit = true,
                    AvailableSubUnitsCount = 0,
                    TotalSubUnitsCount = 0
                });
            }

            // FOR UNITS WITH SUBUNITS: Check subunit-level bookings
            var bookedSubUnitIds = await _context.BookingRooms
                .Include(br => br.Booking)
                .Where(br => br.Room.UnitId == unitId &&
                            br.Booking.CheckInDate < checkOut &&
                            br.Booking.CheckOutDate > checkIn &&
                            br.Booking.Status != BookingStatus.Cancelled &&
                            br.Booking.Status != BookingStatus.Completed)
                .Select(br => br.RoomId)
                .Distinct()
                .ToListAsync();

            var availableSubUnitsCount = subUnits.Count - bookedSubUnitIds.Count;

            // For unit booking, ALL subunits must be free
            return Result.Success(new UnitAvailabilityStatus
            {
                IsAvailable = availableSubUnitsCount == subUnits.Count,
                Reason = availableSubUnitsCount == subUnits.Count ? null : "Some rooms are booked",
                HasManualBlock = false,
                HasActiveBooking = availableSubUnitsCount < subUnits.Count,
                IsStandaloneUnit = false,
                AvailableSubUnitsCount = availableSubUnitsCount,
                TotalSubUnitsCount = subUnits.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking unit availability");
            return Result.Failure<UnitAvailabilityStatus>(
                new Error("CheckFailed", "Failed to check availability", 500));
        }
    }

    public async Task<Result<Dictionary<DateTime, UnitDayAvailability>>> GetUnitAvailabilityCalendarAsync(
        int unitId,
        int year,
        int month)
    {
        try
        {
            if (month < 1 || month > 12)
            {
                return Result.Failure<Dictionary<DateTime, UnitDayAvailability>>(
                    new Error("InvalidMonth", "Month must be between 1 and 12", 400));
            }

            if (year < 1 || year > 9999)
            {
                return Result.Failure<Dictionary<DateTime, UnitDayAvailability>>(
                    new Error("InvalidYear", "Year must be between 1 and 9999", 400));
            }

            var unit = await _context.Units
                .Include(u => u.Rooms.Where(r => !r.IsDeleted))
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

            if (unit == null)
                return Result.Failure<Dictionary<DateTime, UnitDayAvailability>>(
                    new Error("NotFound", "Unit not found", 404));

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // Determine if standalone
            var isStandaloneUnit = !unit.Rooms.Any(r => !r.IsDeleted);

            // Get manual availability blocks
            var manualBlocks = await _context.Set<UnitAvailability>()
                .Where(a => a.UnitId == unitId &&
                           a.StartDate <= endDate &&
                           a.EndDate >= startDate)
                .AsNoTracking()
                .ToListAsync();

            // Get all bookings for this unit
            var unitBookings = await _context.Bookings
                .Where(b => b.UnitId == unitId &&
                           b.CheckInDate <= endDate &&
                           b.CheckOutDate >= startDate &&
                           b.Status != BookingStatus.Cancelled &&
                           b.Status != BookingStatus.Completed)
                .AsNoTracking()
                .ToListAsync();

            var calendar = new Dictionary<DateTime, UnitDayAvailability>();
            var totalSubUnits = unit.Rooms.Count;

            // For standalone units, subunit bookings are not relevant
            List<dynamic>? subUnitBookings = null;
            if (!isStandaloneUnit)
            {
                subUnitBookings = await _context.BookingRooms
                    .Include(br => br.Booking)
                    .Where(br => br.Room.UnitId == unitId &&
                                br.Booking.CheckInDate <= endDate &&
                                br.Booking.CheckOutDate >= startDate &&
                                br.Booking.Status != BookingStatus.Cancelled &&
                                br.Booking.Status != BookingStatus.Completed)
                    .Select(br => new { br.Booking.CheckInDate, br.Booking.CheckOutDate, br.RoomId })
                    .AsNoTracking()
                    .ToListAsync<dynamic>();
            }

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                // Check manual blocks
                var manualBlock = manualBlocks.FirstOrDefault(b =>
                    date >= b.StartDate && date <= b.EndDate);
                var isManuallyBlocked = manualBlock != null && !manualBlock.IsAvailable;

                // Check bookings (check-in inclusive, check-out exclusive)
                var isUnitBooked = unitBookings.Any(b =>
                    date >= b.CheckInDate && date < b.CheckOutDate);

                int bookedSubUnitsCount = 0;
                int availableSubUnits = totalSubUnits;

                if (!isStandaloneUnit && subUnitBookings != null)
                {
                    // Count booked subunits for this date
                    bookedSubUnitsCount = subUnitBookings
                        .Where(b => date >= b.CheckInDate && date < b.CheckOutDate)
                        .Select(b => b.RoomId)
                        .Distinct()
                        .Count();
                    availableSubUnits = totalSubUnits - bookedSubUnitsCount;
                }

                bool isAvailable;
                if (isStandaloneUnit)
                {
                    // Standalone: available if not blocked and not booked
                    isAvailable = !isManuallyBlocked && !isUnitBooked;
                }
                else
                {
                    // Units with subunits: all subunits must be free
                    isAvailable = !isManuallyBlocked && !isUnitBooked && availableSubUnits == totalSubUnits;
                }

                // Get price range from available subunits (or unit base price for standalone)
                decimal minPrice = unit.BasePrice;
                decimal maxPrice = unit.BasePrice;

                if (!isStandaloneUnit)
                {
                    var prices = unit.Rooms
                        .Where(r => r.IsAvailable)
                        .Select(r => r.PricePerNight)
                        .ToList();

                    if (prices.Any())
                    {
                        minPrice = prices.Min();
                        maxPrice = prices.Max();
                    }
                }

                calendar[date] = new UnitDayAvailability
                {
                    Date = date,
                    IsAvailable = isAvailable,
                    IsBooked = isUnitBooked || bookedSubUnitsCount > 0,
                    IsManuallyBlocked = isManuallyBlocked,
                    IsStandaloneUnit = isStandaloneUnit,
                    AvailableSubUnits = isStandaloneUnit ? 0 : (isUnitBooked ? 0 : availableSubUnits),
                    TotalSubUnits = totalSubUnits,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    UnavailabilityReason = (UnavailabilityReason?)(manualBlock?.Reason)
                };
            }

            return Result.Success(calendar);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unit availability calendar");
            return Result.Failure<Dictionary<DateTime, UnitDayAvailability>>(
                new Error("CalendarFailed", "Failed to get calendar", 500));
        }
    }

    #endregion

    #region SUBUNIT AVAILABILITY

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
                existing.Reason = (Domain.Entities.UnavailabilityReason?)request.Reason;
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
                    Reason = (Domain.Entities.UnavailabilityReason?)request.Reason,
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

    public async Task<Result<SubUnitAvailabilityStatus>> CheckSubUnitAvailabilityAsync(
        int subUnitId,
        DateTime checkIn,
        DateTime checkOut)
    {
        try
        {
            var subUnit = await _context.SubUnits
                .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

            if (subUnit == null || !subUnit.IsAvailable)
                return Result.Success(new SubUnitAvailabilityStatus
                {
                    IsAvailable = false,
                    Reason = "SubUnit not found or disabled"
                });

            // Check for manual blocks
            var hasBlock = await _context.Set<SubUnitAvailability>()
                .AnyAsync(a => a.SubUnitId == subUnitId &&
                              !a.IsAvailable &&
                              a.StartDate < checkOut &&
                              a.EndDate > checkIn);

            if (hasBlock)
            {
                var block = await _context.Set<SubUnitAvailability>()
                    .FirstOrDefaultAsync(a => a.SubUnitId == subUnitId &&
                                  !a.IsAvailable &&
                                  a.StartDate < checkOut &&
                                  a.EndDate > checkIn);

                return Result.Success(new SubUnitAvailabilityStatus
                {
                    IsAvailable = false,
                    Reason = block?.Reason?.ToString() ?? "SubUnit blocked",
                    HasManualBlock = true,
                    CurrentPrice = subUnit.PricePerNight
                });
            }

            // Check for unit-level bookings
            var hasUnitBooking = await _context.Bookings
                .AnyAsync(b => b.UnitId == subUnit.UnitId &&
                              b.BookingType == BookingType.UnitBooking &&
                              b.CheckInDate < checkOut &&
                              b.CheckOutDate > checkIn &&
                              b.Status != BookingStatus.Cancelled &&
                              b.Status != BookingStatus.Completed);

            if (hasUnitBooking)
                return Result.Success(new SubUnitAvailabilityStatus
                {
                    IsAvailable = false,
                    Reason = "Entire unit is booked",
                    HasActiveBooking = true,
                    CurrentPrice = subUnit.PricePerNight
                });

            // Check for room-specific bookings
            var hasBooking = await _context.BookingRooms
                .Include(br => br.Booking)
                .AnyAsync(br => br.RoomId == subUnitId &&
                               br.Booking.CheckInDate < checkOut &&
                               br.Booking.CheckOutDate > checkIn &&
                               br.Booking.Status != BookingStatus.Cancelled &&
                               br.Booking.Status != BookingStatus.Completed);

            if (hasBooking)
                return Result.Success(new SubUnitAvailabilityStatus
                {
                    IsAvailable = false,
                    Reason = "SubUnit is booked",
                    HasActiveBooking = true,
                    CurrentPrice = subUnit.PricePerNight
                });

            // Get special pricing if available
            var specialPricing = await _context.Set<SubUnitAvailability>()
                .FirstOrDefaultAsync(a => a.SubUnitId == subUnitId &&
                                         a.IsAvailable &&
                                         checkIn >= a.StartDate &&
                                         checkOut <= a.EndDate);

            return Result.Success(new SubUnitAvailabilityStatus
            {
                IsAvailable = true,
                CurrentPrice = specialPricing?.SpecialPrice ?? subUnit.PricePerNight,
                SpecialPrice = specialPricing?.SpecialPrice
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking subunit availability");
            return Result.Failure<SubUnitAvailabilityStatus>(
                new Error("CheckFailed", "Failed to check availability", 500));
        }
    }

    public async Task<Result<Dictionary<DateTime, SubUnitDayAvailability>>> GetSubUnitAvailabilityCalendarAsync(
        int subUnitId,
        int year,
        int month)
    {
        try
        {
            var subUnit = await _context.SubUnits
                .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

            if (subUnit == null)
                return Result.Failure<Dictionary<DateTime, SubUnitDayAvailability>>(
                    new Error("NotFound", "SubUnit not found", 404));

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var availabilities = await _context.Set<SubUnitAvailability>()
                .Where(a => a.SubUnitId == subUnitId &&
                           a.StartDate <= endDate &&
                           a.EndDate >= startDate)
                .AsNoTracking()
                .ToListAsync();

            var roomBookings = await _context.BookingRooms
                .Include(br => br.Booking)
                .Where(br => br.RoomId == subUnitId &&
                            br.Booking.CheckInDate <= endDate &&
                            br.Booking.CheckOutDate >= startDate &&
                            br.Booking.Status != BookingStatus.Cancelled &&
                            br.Booking.Status != BookingStatus.Completed)
                .Select(br => new { br.Booking.CheckInDate, br.Booking.CheckOutDate })
                .AsNoTracking()
                .ToListAsync();

            var unitBookings = await _context.Bookings
                .Where(b => b.UnitId == subUnit.UnitId &&
                           b.BookingType == BookingType.UnitBooking &&
                           b.CheckInDate <= endDate &&
                           b.CheckOutDate >= startDate &&
                           b.Status != BookingStatus.Cancelled &&
                           b.Status != BookingStatus.Completed)
                .Select(b => new { b.CheckInDate, b.CheckOutDate })
                .AsNoTracking()
                .ToListAsync();

            var calendar = new Dictionary<DateTime, SubUnitDayAvailability>();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var isBooked = roomBookings.Any(b => date >= b.CheckInDate && date < b.CheckOutDate) ||
                              unitBookings.Any(b => date >= b.CheckInDate && date < b.CheckOutDate);

                var availability = availabilities.FirstOrDefault(a =>
                    date >= a.StartDate && date <= a.EndDate);

                var isManuallyBlocked = availability != null && !availability.IsAvailable;
                var isAvailable = !isBooked && !isManuallyBlocked && subUnit.IsAvailable;

                var isWeekend = date.DayOfWeek == DayOfWeek.Friday ||
                               date.DayOfWeek == DayOfWeek.Saturday;

                var price = availability?.SpecialPrice;
                if (price == null && isWeekend)
                {
                    price = availability?.WeekendPrice ?? subUnit.PricePerNight;
                }
                else if (price == null)
                {
                    price = subUnit.PricePerNight;
                }

                calendar[date] = new SubUnitDayAvailability
                {
                    Date = date,
                    IsAvailable = isAvailable,
                    IsBooked = isBooked,
                    IsManuallyBlocked = isManuallyBlocked,
                    Price = price.Value,
                    IsWeekend = isWeekend,
                    SpecialPrice = availability?.SpecialPrice,
                    UnavailabilityReason = (UnavailabilityReason?)(availability?.Reason)
                };
            }

            return Result.Success(calendar);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subunit availability calendar");
            return Result.Failure<Dictionary<DateTime, SubUnitDayAvailability>>(
                new Error("CalendarFailed", "Failed to get calendar", 500));
        }
    }

    #endregion

    #region BULK QUERIES

    public async Task<Result<List<int>>> GetAvailableSubUnitIdsAsync(
        int unitId,
        DateTime checkIn,
        DateTime checkOut)
    {
        try
        {
            var allSubUnits = await _context.SubUnits
                .Where(s => s.UnitId == unitId && !s.IsDeleted && s.IsAvailable)
                .Select(s => s.Id)
                .ToListAsync();

            if (!allSubUnits.Any())
                return Result.Success(new List<int>());

            var hasUnitBooking = await _context.Bookings
                .AnyAsync(b => b.UnitId == unitId &&
                              b.BookingType == BookingType.UnitBooking &&
                              b.CheckInDate < checkOut &&
                              b.CheckOutDate > checkIn &&
                              b.Status != BookingStatus.Cancelled &&
                              b.Status != BookingStatus.Completed);

            if (hasUnitBooking)
                return Result.Success(new List<int>());

            var bookedSubUnitIds = await _context.BookingRooms
                .Include(br => br.Booking)
                .Where(br => allSubUnits.Contains(br.RoomId) &&
                            br.Booking.CheckInDate < checkOut &&
                            br.Booking.CheckOutDate > checkIn &&
                            br.Booking.Status != BookingStatus.Cancelled &&
                            br.Booking.Status != BookingStatus.Completed)
                .Select(br => br.RoomId)
                .Distinct()
                .ToListAsync();

            var blockedSubUnitIds = await _context.Set<SubUnitAvailability>()
                .Where(a => allSubUnits.Contains(a.SubUnitId) &&
                           !a.IsAvailable &&
                           a.StartDate < checkOut &&
                           a.EndDate > checkIn)
                .Select(a => a.SubUnitId)
                .Distinct()
                .ToListAsync();

            var availableIds = allSubUnits
                .Except(bookedSubUnitIds)
                .Except(blockedSubUnitIds)
                .ToList();

            return Result.Success(availableIds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available subunit IDs");
            return Result.Failure<List<int>>(
                new Error("GetFailed", "Failed to get available subunits", 500));
        }
    }

    public async Task<Result<Dictionary<int, bool>>> CheckMultipleSubUnitsAvailabilityAsync(
        List<int> subUnitIds,
        DateTime checkIn,
        DateTime checkOut)
    {
        try
        {
            var result = new Dictionary<int, bool>();

            foreach (var subUnitId in subUnitIds)
            {
                var check = await CheckSubUnitAvailabilityAsync(subUnitId, checkIn, checkOut);
                result[subUnitId] = check.IsSuccess && check.Value.IsAvailable;
            }

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking multiple subunits availability");
            return Result.Failure<Dictionary<int, bool>>(
                new Error("CheckFailed", "Failed to check multiple subunits", 500));
        }
    }

    #endregion

    #region BATCH OPERATIONS

    public async Task<Result> BlockSubUnitDatesAsync(BlockDatesRequest request)
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
                    Reason = (Domain.Entities.UnavailabilityReason?)request.Reason,
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

    public async Task<Result> BlockUnitDatesAsync(BlockUnitDatesRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var unit = await _context.Units
                .FirstOrDefaultAsync(u => u.Id == request.UnitId && !u.IsDeleted);

            if (unit == null)
                return Result.Failure(new Error("NotFound", "Unit not found", 404));

            foreach (var dateRange in request.DateRanges)
            {
                var availability = new UnitAvailability
                {
                    UnitId = request.UnitId,
                    StartDate = dateRange.StartDate,
                    EndDate = dateRange.EndDate,
                    IsAvailable = false,
                    Reason = (Domain.Entities.UnavailabilityReason?)request.Reason,
                    UpdatedByUserId = request.UpdatedByUserId,
                    CreatedAt = DateTime.UtcNow.AddHours(3)
                };

                await _context.Set<UnitAvailability>().AddAsync(availability);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Blocked {Count} date ranges for unit {UnitId}",
                request.DateRanges.Count, request.UnitId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error blocking unit dates");
            return Result.Failure(new Error("BlockFailed", "Failed to block unit dates", 500));
        }
    }

    public async Task<Result> SetSpecialPricingAsync(SetSpecialPricingRequest request)
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

    public async Task<Result> UnblockDatesAsync(int availabilityId)
    {
        try
        {
            var subUnitAvailability = await _context.Set<SubUnitAvailability>()
                .FirstOrDefaultAsync(a => a.Id == availabilityId);

            if (subUnitAvailability != null)
            {
                _context.Set<SubUnitAvailability>().Remove(subUnitAvailability);
                await _context.SaveChangesAsync();
                return Result.Success();
            }

            var unitAvailability = await _context.Set<UnitAvailability>()
                .FirstOrDefaultAsync(a => a.Id == availabilityId);

            if (unitAvailability != null)
            {
                _context.Set<UnitAvailability>().Remove(unitAvailability);
                await _context.SaveChangesAsync();
                return Result.Success();
            }

            return Result.Failure(new Error("NotFound", "Availability record not found", 404));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unblocking dates");
            return Result.Failure(new Error("UnblockFailed", "Failed to unblock dates", 500));
        }
    }

    #endregion

    #region BOOKING INTEGRATION

    public async Task<Result> MarkDatesAsBookedAsync(MarkDatesAsBookedRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Dates marked as booked for booking {BookingId} ({BookingType})",
                request.BookingId, request.BookingType);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking dates as booked");
            return Result.Failure(new Error("MarkFailed", "Failed to mark dates as booked", 500));
        }
    }

    public async Task<Result> FreeDatesFromBookingAsync(int bookingId)
    {
        try
        {
            _logger.LogInformation("Dates freed from booking {BookingId}", bookingId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error freeing dates from booking");
            return Result.Failure(new Error("FreeFailed", "Failed to free dates", 500));
        }
    }

    #endregion

    #region INITIALIZATION

    public async Task<Result> InitializeSubUnitAvailabilityAsync(int subUnitId, int daysAhead = 365)
    {
        try
        {
            var subUnit = await _context.SubUnits
                .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

            if (subUnit == null)
                return Result.Failure(new Error("NotFound", "SubUnit not found", 404));

            var hasAvailability = await _context.Set<SubUnitAvailability>()
                .AnyAsync(a => a.SubUnitId == subUnitId);

            if (hasAvailability)
                return Result.Success();

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
            _logger.LogError(ex, "Error initializing subunit default availability");
            return Result.Failure(new Error("InitFailed", "Failed to initialize availability", 500));
        }
    }

    public async Task<Result> InitializeUnitAvailabilityAsync(int unitId, int daysAhead = 365)
    {
        try
        {
            var unit = await _context.Units
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

            if (unit == null)
                return Result.Failure(new Error("NotFound", "Unit not found", 404));

            var hasAvailability = await _context.Set<UnitAvailability>()
                .AnyAsync(a => a.UnitId == unitId);

            if (hasAvailability)
                return Result.Success();

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

    #endregion
}