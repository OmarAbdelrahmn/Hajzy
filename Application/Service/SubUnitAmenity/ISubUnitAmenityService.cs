using Application.Abstraction;
using Application.Contracts.Aminety;

namespace Application.Service.SubUnitAmenity;

public interface ISubUnitAmenityService
{
    Task<Result<IEnumerable<AmenityResponse>>> GetSubUnitAmenitiesAsync(int subUnitId);
    Task<Result> AttachAmenityAsync(int subUnitId, int amenityId);
    Task<Result> AttachAmenitiesAsync(int subUnitId, List<int> amenityIds);
    Task<Result> RemoveAmenityAsync(int subUnitId, int amenityId);
    Task<Result> RemoveAllAmenitiesAsync(int subUnitId);
    Task<Result> ToggleAvailabilityAsync(int subUnitId, int amenityId);
    Task<Result> UpdateAmenitiesAsync(int subUnitId, UpdateSubUnitAmenitiesRequest request);
    Task<Result> CopyFromUnitAsync(int subUnitId);

}