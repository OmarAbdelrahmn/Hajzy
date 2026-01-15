using Application.Abstraction;
using Application.Contracts.AD;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Service.AdService;

public interface IAdService
{
    /// <summary>
    /// Create a new ad with image
    /// </summary>
    Task<Result<AdResponse>> CreateAdAsync(CreateAdRequest request, string userId);

    /// <summary>
    /// Update an existing ad
    /// </summary>
    Task<Result<AdResponse>> UpdateAdAsync(int adId, UpdateAdRequest request, string userId);

    /// <summary>
    /// Delete an ad (soft delete)
    /// </summary>
    Task<Result> DeleteAdAsync(int adId);

    /// <summary>
    /// Get ad by ID
    /// </summary>
    Task<Result<AdResponse>> GetAdByIdAsync(int adId);

    /// <summary>
    /// Get all ads with filtering
    /// </summary>
    Task<Result<IEnumerable<AdResponse>>> GetAllAdsAsync(AdListFilter filter);

    /// <summary>
    /// Get current active ads (not expired, IsActive = true)
    /// </summary>
    Task<Result<IEnumerable<AdResponse>>> GetCurrentActiveAdsAsync();

    /// <summary>
    /// Get inactive/expired ads
    /// </summary>
    Task<Result<IEnumerable<AdResponse>>> GetInactiveAdsAsync();

    /// <summary>
    /// Deactivate expired ads (background job)
    /// </summary>
    Task<Result> DeactivateExpiredAdsAsync();
}