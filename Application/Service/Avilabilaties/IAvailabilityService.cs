using Application.Abstraction;
using Domain.Entities;

namespace Application.Service.Availability;

/// <summary>
/// Service for managing both Unit and SubUnit availability with smart defaults
/// </summary>
public interface IAvailabilityService
{
    // ============= UNIT AVAILABILITY =============

    /// <summary>
    /// Set availability for a unit for specific dates
    /// </summary>
    Task<Result> SetUnitAvailabilityAsync(SetUnitAvailabilityRequest request);

    /// <summary>
    /// Get unit availability for a date range
    /// </summary>
    Task<Result<List<AvailabilityResponse>>> GetUnitAvailabilityAsync(
        int unitId,
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Check if unit has any available rooms for given dates
    /// </summary>
    Task<Result<bool>> IsUnitAvailableAsync(
        int unitId,
        DateTime checkIn,
        DateTime checkOut);

    // ============= SUBUNIT AVAILABILITY =============

    /// <summary>
    /// Set availability for a subunit for specific dates
    /// </summary>
    Task<Result> SetSubUnitAvailabilityAsync(SetSubUnitAvailabilityRequest request);

    /// <summary>
    /// Get subunit availability for a date range
    /// </summary>
    Task<Result<List<AvailabilityResponse>>> GetSubUnitAvailabilityAsync(
        int subUnitId,
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Check if subunit is available for given dates
    /// </summary>
    Task<Result<bool>> IsSubUnitAvailableAsync(
        int subUnitId,
        DateTime checkIn,
        DateTime checkOut);

    // ============= BATCH OPERATIONS =============

    /// <summary>
    /// Block multiple dates at once (e.g., maintenance period)
    /// </summary>
    Task<Result> BlockDatesAsync(BlockDatesRequest request);

    /// <summary>
    /// Set special pricing for multiple dates (e.g., holiday season)
    /// </summary>
    Task<Result> SetSpecialPricingAsync(SetSpecialPricingRequest request);

    /// <summary>
    /// Initialize default availability for a new subunit (all days available)
    /// </summary>
    Task<Result> InitializeDefaultAvailabilityAsync(int subUnitId, int daysAhead = 365);

    /// <summary>
    /// Get calendar view of availability
    /// </summary>
    Task<Result<Dictionary<DateTime, DayAvailability>>> GetAvailabilityCalendarAsync(
        int subUnitId,
        int year,
        int month);
}

// ============= REQUEST MODELS =============

public record SetUnitAvailabilityRequest(
    int UnitId,
    DateTime StartDate,
    DateTime EndDate,
    bool IsAvailable,
    UnavailabilityReason? Reason,
    string? UpdatedByUserId
);

public record SetSubUnitAvailabilityRequest(
    int SubUnitId,
    DateTime StartDate,
    DateTime EndDate,
    bool IsAvailable,
    UnavailabilityReason? Reason,
    decimal? SpecialPrice,
    decimal? WeekendPrice,
    string? UpdatedByUserId
);

public record BlockDatesRequest(
    int SubUnitId,
    List<DateRange> DateRanges,
    UnavailabilityReason Reason,
    string UpdatedByUserId
);

public record SetSpecialPricingRequest(
    int SubUnitId,
    List<SpecialPriceRange> PriceRanges,
    string UpdatedByUserId
);

public record DateRange(DateTime StartDate, DateTime EndDate);

public record SpecialPriceRange(
    DateTime StartDate,
    DateTime EndDate,
    decimal? WeekdayPrice,
    decimal? WeekendPrice
);

// ============= RESPONSE MODELS =============

public record AvailabilityResponse(
    int Id,
    DateTime StartDate,
    DateTime EndDate,
    bool IsAvailable,
    string? Reason,
    decimal? SpecialPrice,
    decimal? WeekendPrice
);

public record DayAvailability(
    DateTime Date,
    bool IsAvailable,
    bool IsBooked,
    decimal? Price,
    bool IsWeekend,
    UnavailabilityReason? Reason
);