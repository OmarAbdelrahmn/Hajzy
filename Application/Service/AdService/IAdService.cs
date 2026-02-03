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
    // Application/Service/AdService/IAdService.cs
    Task<Result<PaginatedResponse<AdResponse>>> GetAllAdsAsync(AdListFilter filter);
    Task<Result<PaginatedResponse<AdResponse>>> GetCurrentActiveAdsAsync(int page = 1, int pageSize = 10);
    Task<Result<PaginatedResponse<AdResponse>>> GetInactiveAdsAsync(int page = 1, int pageSize = 10);

    /// <summary>
    /// Deactivate expired ads (background job)
    /// </summary>
    Task<Result> DeactivateExpiredAdsAsync();
}