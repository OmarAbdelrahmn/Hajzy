using Application.Abstraction;
using Application.Contracts.Offer;

namespace Application.Service.OfferService;

public interface IOfferService
{
    /// <summary>
    /// Create a new offer with image
    /// </summary>
    /// 
    public class PaginatedResponse<T>
    {
        public IEnumerable<T> Items { get; set; } = [];
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int? NextPage { get; set; }
        public int? PrevPage { get; set; }
        public int TotalCount { get; set; }
    }
    Task<Result<PaginatedResponse<OfferResponse>>> GetAllOffersAsync(OfferListFilter filter);
    Task<Result<PaginatedResponse<OfferResponse>>> GetActiveOffersAsync(int page = 1, int pageSize = 10);
    Task<Result<PaginatedResponse<OfferResponse>>> GetInactiveOffersAsync(int page = 1, int pageSize = 10);

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
    /// Deactivate expired offers (background job)
    /// </summary>
    Task<Result> DeactivateExpiredOffersAsync();
    Task<Result<IEnumerable<OfferResponse>>> GetFeaturedOffersAsync();

    Task<Result> ToggleFeatured(int OfferId);
}