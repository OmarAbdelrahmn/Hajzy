using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Contracts.Availability;

internal class availability
{
}

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

public record SetSpecialPricingRequests(
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

//public record AvailabilityResponse(
//    int Id,
//    DateTime StartDate,
//    DateTime EndDate,
//    bool IsAvailable,
//    string? Reason,
//    decimal? SpecialPrice,
//    decimal? WeekendPrice
//);

public record DayAvailability(
    DateTime Date,
    bool IsAvailable,
    bool IsBooked,
    decimal? Price,
    bool IsWeekend,
    UnavailabilityReason? Reason
);