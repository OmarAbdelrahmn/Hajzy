using Application.Abstraction;
using Application.Contracts.Offer;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Service.OfferService;

public interface IOfferService
{
    /// <summary>
    /// Create a new offer with image
    /// </summary>
    Task<Result<OfferResponse>> CreateOfferAsync(CreateOfferRequest request, string userId);

    /// <summary>
    /// Update an existing offer
    /// </summary>
    Task<Result<OfferResponse>> UpdateOfferAsync(int offerId, UpdateOfferRequest request, string userId);

    /// <summary>
    /// Delete an offer (soft delete)
    /// </summary>
    Task<Result> DeleteOfferAsync(int offerId);

    /// <summary>
    /// Get offer by ID
    /// </summary>
    Task<Result<OfferResponse>> GetOfferByIdAsync(int offerId);

    /// <summary>
    /// Get all offers with filtering
    /// </summary>
    Task<Result<IEnumerable<OfferResponse>>> GetAllOffersAsync(OfferListFilter filter);

    /// <summary>
    /// Get current active offers (not expired, IsActive = true)
    /// </summary>
    Task<Result<IEnumerable<OfferResponse>>> GetCurrentActiveOffersAsync();

    /// <summary>
    /// Get inactive/expired offers
    /// </summary>
    Task<Result<IEnumerable<OfferResponse>>> GetInactiveOffersAsync();

    /// <summary>
    /// Deactivate expired offers (background job)
    /// </summary>
    Task<Result> DeactivateExpiredOffersAsync();
}