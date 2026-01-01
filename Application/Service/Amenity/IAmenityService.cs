using Application.Abstraction;
using Application.Contracts.Aminety;

namespace Application.Service.Amenity;

public interface IAmenityService
{
    // ============= CRUD =============
    Task<Result<IEnumerable<AmenityResponse>>> GetAllAmenitiesAsync();
    Task<Result<AmenityDetailsResponse>> GetByIdAsync(int amenityId);
    Task<Result<IEnumerable<AmenityResponse>>> GetByCategoryAsync(string category);
    Task<Result<AmenityResponse>> CreateAsync(CreateAmenityRequest request);
    Task<Result<AmenityResponse>> UpdateAsync(int amenityId, UpdateAmenityRequest request);
    Task<Result> DeleteAsync(int amenityId);

    // ============= FILTERING =============
    Task<Result<IEnumerable<AmenityResponse>>> FilterAmenitiesAsync(AmenityFilter filter);

    // ============= STATISTICS =============
    //Task<Result<AmenityDetailsResponse>> GetAmenityStatisticsAsync(int amenityId);
    Task<Result<IEnumerable<string>>> GetCategoriesAsync();
}