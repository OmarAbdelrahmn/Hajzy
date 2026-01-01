using Application.Abstraction;
using Application.Contracts.Aminety;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Service.UnitAmenity;

public interface IUnitAmenityService
{
    Task<Result<IEnumerable<AmenityResponse>>> GetAllAmenitiesAsync();
    Task<Result<IEnumerable<AmenityResponse>>> GetUnitAmenitiesAsync(int unitId);
    Task<Result> AttachAmenityAsync(int unitId, int amenityId);
    Task<Result> AttachAmenitiesAsync(int unitId, List<int> amenityIds);
    Task<Result> RemoveAmenityAsync(int unitId, int amenityId);
    Task<Result> RemoveAllAmenitiesAsync(int unitId);
    Task<Result> ToggleAvailabilityAsync(int unitId, int amenityId);
    Task<Result> UpdateAmenitiesAsync(int unitId, UpdateUnitAmenitiesRequest request);
}