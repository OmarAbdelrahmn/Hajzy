using Application.Abstraction;
using Application.Contracts.SubUnit;
using Application.Service.Availability;
using Microsoft.AspNetCore.Http;

namespace Application.Service.SubUnit;

public interface ISubUnitService
{
    // ============= CRUD =============
    Task<Result<SubUnitResponse>> GetByIdAsync(int subUnitId);
    Task<Result<SubUnitDetailsResponse>> GetDetailsAsync(int subUnitId);
    Task<Result<IEnumerable<SubUnitResponse>>> GetByUnitAsync(int unitId);
    Task<Result<SubUnitResponse>> CreateAsync(CreateSubUnitRequest request);
    Task<Result<SubUnitResponse>> UpdateAsync(int subUnitId, UpdateSubUnitRequest request);
    Task<Result> DeleteAsync(int subUnitId, bool softDelete = true);
    Task<Result> RestoreAsync(int subUnitId);

    Task<Result> SetAvailabilityAsync(int subUnitId, SetAvailabilityRequest request);
    Task<Result> ToggleAvailableAsync(int subUnitId);
    Task<Result<IEnumerable<AvailabilityResponse>>> GetAvailabilityAsync(
        int subUnitId,
        DateTime startDate,
        DateTime endDate);
    Task<Result<List<SubUnitResponse>>> GetAvailableSubUnitsAsync(
        int unitId,
        DateTime checkIn,
        DateTime checkOut);

    // ============= IMAGE MANAGEMENT =============
    Task<Result<List<SubUnitImageResponse>>> UploadImagesAsync(
        int subUnitId,
        List<IFormFile> images,
        string userId);
    Task<Result> DeleteImageAsync(int subUnitId, int imageId);
    Task<Result> SetPrimaryImageAsync(int subUnitId, int imageId);

    // ============= PRICING =============
    Task<Result> SetSpecialPricingAsync(int subUnitId, SetSpecialPricingRequestsss request);
    Task<Result<decimal>> CalculatePriceAsync(
        int subUnitId,
        DateTime checkIn,
        DateTime checkOut);

    // ============= POLICY MANAGEMENT =============
    Task<Result> AttachGeneralPolicyAsync(int subUnitId, int policyId);
    Task<Result> RemoveGeneralPolicyAsync(int subUnitId, int policyId);
}