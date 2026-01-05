using Application.Abstraction;
using Application.Contracts.publicuser;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Service.publicuser;
    public interface IPublicServise
{
    // <summary>
    /// Get all units with public information only
    /// </summary>
    Task<Result<IEnumerable<PublicUnitResponse>>> GetAllUnitsAsync(PublicUnitFilter filter);

    /// <summary>
    /// Get single unit details (public view)
    /// </summary>
    Task<Result<PublicUnitDetailsResponse>> GetUnitDetailsAsync(int unitId);

    /// <summary>
    /// Search units by keyword
    /// </summary>
    Task<Result<IEnumerable<PublicUnitResponse>>> SearchUnitsAsync(PublicSearchRequest request);

    /// <summary>
    /// Get featured units (top rated, most booked, etc.)
    /// </summary>
    Task<Result<FeaturedUnitsResponse>> GetFeaturedUnitsAsync(int count = 10);

    // ============= SUBUNITS =============

    /// <summary>
    /// Get subunit details (public view)
    /// </summary>
    Task<Result<PublicSubUnitDetailsResponse>> GetSubUnitDetailsAsync(int subUnitId);

    /// <summary>
    /// Get available subunits for a unit
    /// </summary>
    Task<Result<IEnumerable<PublicSubUnitSummary>>> GetAvailableSubUnitsAsync(
        int unitId,
        DateTime checkIn,
        DateTime checkOut);

    // ============= CITIES/DEPARTMENTS =============

    /// <summary>
    /// Get all cities
    /// </summary>
    Task<Result<IEnumerable<PublicCityResponse>>> GetAllCitiesAsync();

    /// <summary>
    /// Get single city details
    /// </summary>
    Task<Result<PublicCityResponse>> GetCityDetailsAsync(int cityId);

    /// <summary>
    /// Get units by city
    /// </summary>
    Task<Result<IEnumerable<PublicUnitResponse>>> GetUnitsByCityAsync(
        int cityId,
        PublicUnitFilter? filter = null);

    // ============= AVAILABILITY =============

    /// <summary>
    /// Check availability and get price estimate
    /// </summary>
    Task<Result<AvailabilityCheckResponse>> CheckAvailabilityAsync(CheckAvailabilityRequest request);

    // ============= FAVORITES (for anonymous users) =============

    /// <summary>
    /// Get favorite units for a session
    /// </summary>
    //Task<Result<FavoritesResponse>> GetFavoritesAsync(string sessionId);

    //// ============= NEARBY UNITS =============

    ///// <summary>
    /// Get units near a location
    /// </summary>
    Task<Result<IEnumerable<PublicUnitResponse>>> GetNearbyUnitsAsync(
        decimal latitude,
        decimal longitude,
        int radiusKm = 50);
}

