using System.ComponentModel.DataAnnotations;
using Domain.Entities;

namespace Application.Contracts.Availability;

// ============================================================================
// AVAILABILITY REQUESTS
// ============================================================================

public record SetUnitAvailabilityRequest(
    int UnitId,
    DateTime StartDate,
    DateTime EndDate,
    bool IsAvailable,
    UnavailabilityReason? Reason = null,
    string? UpdatedByUserId = null
);

public record SetSubUnitAvailabilityRequest(
    int SubUnitId,
    DateTime StartDate,
    DateTime EndDate,
    bool IsAvailable,
    UnavailabilityReason? Reason = null,
    decimal? SpecialPrice = null,
    decimal? WeekendPrice = null,
    string? UpdatedByUserId = null
);

public record BlockDatesRequest(
    int SubUnitId,
    List<DateRange> DateRanges,
    UnavailabilityReason Reason,
    string UpdatedByUserId
);

public record BlockUnitDatesRequest(
    int UnitId,
    List<DateRange> DateRanges,
    UnavailabilityReason Reason,
    string UpdatedByUserId
);

public record SetSpecialPricingRequest(
    int SubUnitId,
    List<PriceRange> PriceRanges,
    string UpdatedByUserId
);

public record DateRange(
    DateTime StartDate,
    DateTime EndDate
);

public record PriceRange(
    DateTime StartDate,
    DateTime EndDate,
    decimal WeekdayPrice,
    decimal? WeekendPrice = null
);

public record MarkDatesAsBookedRequest(
    BookingType BookingType,
    int? UnitId = null,
    List<int>? SubUnitIds = null,
    int BookingId = 0,
    DateTime CheckInDate = default,
    DateTime CheckOutDate = default
);

// ============================================================================
// AVAILABILITY RESPONSES
// ============================================================================

//public record AvailabilityResponse(
//    int Id,
//    DateTime StartDate,
//    DateTime EndDate,
//    bool IsAvailable,
//    string? Reason,
//    decimal? SpecialPrice,
//    decimal? WeekendPrice
//);

/// <summary>
/// UPDATED: Unit availability status - now includes standalone unit flag
/// </summary>
public record UnitAvailabilityStatus
{
    public bool IsAvailable { get; init; }
    public string? Reason { get; init; }
    public bool HasManualBlock { get; init; }
    public bool HasActiveBooking { get; init; }

    /// <summary>
    /// NEW: Indicates if this is a standalone unit (no subunits)
    /// TRUE = Standalone unit (garden, pool, event hall) - rented as whole
    /// FALSE = Unit with subunits (hotel, apartment building) - can rent individual rooms
    /// </summary>
    public bool IsStandaloneUnit { get; init; }

    /// <summary>
    /// For units with subunits: number of available subunits
    /// For standalone units: always 0
    /// </summary>
    public int AvailableSubUnitsCount { get; init; }

    /// <summary>
    /// For units with subunits: total number of subunits
    /// For standalone units: always 0
    /// </summary>
    public int TotalSubUnitsCount { get; init; }
}

public record SubUnitAvailabilityStatus
{
    public bool IsAvailable { get; init; }
    public string? Reason { get; init; }
    public bool HasManualBlock { get; init; }
    public bool HasActiveBooking { get; init; }
    public decimal? CurrentPrice { get; init; }
    public decimal? SpecialPrice { get; init; }
}

/// <summary>
/// UPDATED: Day availability for units - now includes standalone flag
/// </summary>
public record UnitDayAvailability
{
    public DateTime Date { get; init; }
    public bool IsAvailable { get; init; }
    public bool IsBooked { get; init; }
    public bool IsManuallyBlocked { get; init; }

    /// <summary>
    /// NEW: Indicates if this is a standalone unit
    /// </summary>
    public bool IsStandaloneUnit { get; init; }

    /// <summary>
    /// For units with subunits: number of available subunits on this date
    /// For standalone units: always 0
    /// </summary>
    public int AvailableSubUnits { get; init; }

    /// <summary>
    /// For units with subunits: total subunits
    /// For standalone units: always 0
    /// </summary>
    public int TotalSubUnits { get; init; }

    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public UnavailabilityReason? UnavailabilityReason { get; init; }
}

public record SubUnitDayAvailability
{
    public DateTime Date { get; init; }
    public bool IsAvailable { get; init; }
    public bool IsBooked { get; init; }
    public bool IsManuallyBlocked { get; init; }
    public decimal Price { get; init; }
    public bool IsWeekend { get; init; }
    public decimal? SpecialPrice { get; init; }
    public UnavailabilityReason? UnavailabilityReason { get; init; }
}

// ============================================================================
// ENUMS (if not already defined elsewhere)
// ============================================================================

public enum CouponType
{
    Percentage = 1,
    FixedAmount = 2
}

//public enum BookingStatus
//{
//    Pending = 1,
//    Confirmed = 2,
//    CheckedIn = 3,
//    Completed = 4,
//    Cancelled = 5
//}

//public enum PaymentStatus
//{
//    Pending = 1,
//    PartiallyPaid = 2,
//    Paid = 3,
//    Refunded = 4
//}

//public enum PaymentMethod
//{
//    CreditCard = 1,
//    DebitCard = 2,
//    Cash = 3,
//    BankTransfer = 4
//}

public enum LoyaltyTier
{
    Bronze = 1,
    Silver = 2,
    Gold = 3,
    Platinum = 4
}

public enum NotificationType
{
    BookingConfirmation = 1,
    PaymentReceived = 2,
    BookingCancelled = 3,
    SystemAlert = 4,
    Promotion = 5
}

public enum NotificationPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Urgent = 4
}

public enum NotificationTarget
{
    AllUsers = 1,
    SpecificUser = 2,
    DepartmentUsers = 3,
    UnitAdmins = 4,
    AllAdmins = 5
}

public enum PricingRuleType
{
    EarlyBird = 1,
    LastMinute = 2,
    LongStay = 3,
    Weekend = 4,
    Seasonal = 5
}

public enum PricingAdjustmentType
{
    Percentage = 1,
    FixedAmount = 2
}

public enum GeneralPolicyCategory
{
    HouseRules = 1,
    CheckInOut = 2,
    Cancellation = 3,
    Payment = 4,
    Safety = 5,
    Other = 6
}

public enum GeneralPolicyName
{
    CheckInTime = 1,
    CheckOutTime = 2,
    SmokingPolicy = 3,
    PetPolicy = 4,
    PartyPolicy = 5,
    ChildrenPolicy = 6,
    CancellationPolicy = 7,
    PaymentPolicy = 8,
    Custom = 99
}