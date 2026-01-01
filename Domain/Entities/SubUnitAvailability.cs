using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Domain.Entities;

public class SubUnitAvailability
{
    public int Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsAvailable { get; set; } = true;
    public UnavailabilityReason? Reason { get; set; }
    public decimal? SpecialPrice { get; set; }
    public decimal? WeekendPrice { get; set; }

    // Tracking
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    // Navigation
    public int SubUnitId { get; set; }
    public SubUnit SubUnit { get; set; } = default!;
    public ApplicationUser? UpdatedBy { get; set; }
}

public enum UnavailabilityReason
{
    Booked,
    Maintenance,
    OwnerBlocked,
    SeasonallyClosed,
    Other
}


public class PricingCalculator
{
    public decimal CalculatePrice(
        SubUnit room,
        DateTime checkIn,
        DateTime checkOut,
        SubUnitAvailability? availabilityOverride = null)
    {
        var nights = (checkOut - checkIn).Days;
        var basePrice = room.PricePerNight;

        // Check for special pricing
        if (availabilityOverride?.SpecialPrice.HasValue == true)
            return availabilityOverride.SpecialPrice.Value * nights;

        // Check for weekend pricing
        var weekendNights = CountWeekendNights(checkIn, checkOut);
        var weekdayNights = nights - weekendNights;

        if (availabilityOverride?.WeekendPrice.HasValue == true)
        {
            return (weekdayNights * basePrice) +
                   (weekendNights * availabilityOverride.WeekendPrice.Value);
        }

        return basePrice * nights;
    }

    public static int CountWeekendNights(DateTime startDate, DateTime endDate)
    {
        if (endDate <= startDate)
            return 0;

        int weekendNights = 0;

        for (var date = startDate.Date; date < endDate.Date; date = date.AddDays(1))
        {
            // Friday night or Saturday night
            if (date.DayOfWeek == DayOfWeek.Friday ||
                date.DayOfWeek == DayOfWeek.Saturday)
            {
                weekendNights++;
            }
        }

        return weekendNights;
    }

}