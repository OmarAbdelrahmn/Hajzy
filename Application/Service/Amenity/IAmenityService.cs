using Application.Abstraction;
using Application.Contracts.Aminety;

namespace Application.Service.Amenity;

public interface IAmenityService
{
    // ============= CRUD =============
    Task<Result<PaginatedResponse<AmenityResponse>>> GetAllAmenitiesAsync(int page, int pagesize);
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
public class PaginatedResponse<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int? NextPage { get; set; }
    public int? PrevPage { get; set; }
    public int TotalCount { get; set; }
}