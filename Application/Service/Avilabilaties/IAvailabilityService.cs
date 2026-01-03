using Application.Abstraction;
using Application.Contracts.Availability;
using Application.Contracts.SubUnit;
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
    Task<Result> SetSpecialPricingAsync(SetSpecialPricingRequests request);

    /// <summary>
    /// Initialize default availability for a new subunit (all days available)
    /// </summary>
    Task<Result> InitializeDefaultAvailabilityAsync(int subUnitId, int daysAhead = 365);
    Task<Result> InitializeUnitDefaultAvailabilityAsync(int UnitId, int daysAhead = 365);

    /// <summary>
    /// Get calendar view of availability
    /// </summary>
    Task<Result<Dictionary<DateTime, DayAvailability>>> GetAvailabilityCalendarAsync(
        int subUnitId,
        int year,
        int month);
}


