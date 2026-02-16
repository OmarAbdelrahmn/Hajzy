using Application.Abstraction;
using Application.Contracts.Aminety;
using Application.Contracts.other;
using Application.Contracts.publicuser;
using Application.Contracts.Unit;
using static Application.Service.publicuser.PublicService;

namespace Application.Service.publicuser;

public interface IPublicServise
{
    // <summary>
    /// Get all units with public information only
    /// </summary>
    Task<Result<PaginatedResponse<PublicUnitResponse>>> GetAllUnitsAsync(PublicUnitFilter filter);

    /// <summary>
    /// Get single unit details (public view)
    /// </summary>
    Task<Result<PublicUnitDetailsResponses>> GetUnitDetailsAsync(int unitId);

    /// <summary>
    /// Search units by keyword
    /// </summary>
    Task<Result<IEnumerable<PublicUnitResponse>>> SearchUnitsAsync(PublicSearchRequest request);

    /// <summary>
    /// Get featured units (top rated, most booked, etc.)
    /// </summary>
    Task<Result<List<PublicUnitResponse>>> GetFeaturedUnitsAsync();

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
    Task<Result<PaginatedResponse<PublicUnitResponse>>> GetUnitsByCityAsync(
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

    Task<Result<IEnumerable<PublicAdResponse>>> GetActiveAdsAsync();


    Task<Result<IEnumerable<PublicOfferResponse>>> GetActiveOffersAsync();
    Task<Result<IEnumerable<PaymentMethodDto>>> GetPaymentMethodesAsync();
    Task<Result<IEnumerable<UnitTypeResponse>>> GetUnitTypesAsync();
    Task<Result<IEnumerable<AmenityResponse>>> GetAminitiesAsync();

}


public record PublicAdResponse
{
    public int Id { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public int? UnitId { get; init; }
    public string? UnitName { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public DateTime CreatedAt { get; init; }
}

// ============= PUBLIC OFFER DTOs =============

public record PublicOfferResponse
{
    public int Id { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public bool? IsFeatured { get; init; }
    public int? UnitId { get; init; }
    public string? UnitName { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public decimal? DiscountPercentage { get; init; }
    public decimal? DiscountAmount { get; init; }
    public DateTime CreatedAt { get; init; }
}


