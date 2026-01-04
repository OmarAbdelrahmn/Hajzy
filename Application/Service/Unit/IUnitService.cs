using Application.Abstraction;
using Application.Contracts.Policy;
using Application.Contracts.Unit;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Service.Unit;

public interface IUnitService
{
    Task<Result<UnitResponse>> GetByIdAsync(int unitId);
    Task<Result<UnitDetailsResponse>> GetDetailsAsync(int unitId);
  //  Task<Result<IEnumerable<UnitResponse>>> GetAllAsync(UnitFilter filter);

    Task<Result<IEnumerable<UnitComprehensiveResponse>>> GetAllComprehensiveAsync(UnitFilter filter);

    /// <summary>
    /// Get all units for a specific user's department (for CityAdmin role)
    /// </summary>
    Task<Result<IEnumerable<UnitComprehensiveResponse>>> GetAllByUserDepartmentAsync(string userId, UnitFilter filter);

    /// <summary>
    /// Get all units managed by a specific hotel admin
    /// </summary>
    Task<Result<IEnumerable<UnitComprehensiveResponse>>> GetAllByHotelAdminAsync(string userId, UnitFilter filter);


    Task<Result<UnitResponse>> UpdateAsync(int unitId, UpdateUnitRequest request);
    Task<Result> DeleteAsync(int unitId, bool softDelete = true);
    Task<Result> RestoreAsync(int unitId);

    // ============= STATUS MANAGEMENT =============
    Task<Result> ToggleActiveAsync(int unitId);
    Task<Result> VerifyUnitAsync(int unitId, string adminUserId);
    Task<Result> UnverifyUnitAsync(int unitId, string adminUserId, string reason);

    // ============= IMAGE MANAGEMENT =============
    Task<Result<List<UnitImageResponse>>> UploadImagesAsync(int unitId, List<IFormFile> images, string userId);
    Task<Result> DeleteImageAsync(int unitId, int imageId);
    Task<Result> SetPrimaryImageAsync(int unitId, int imageId);
    Task<Result> ReorderImagesAsync(int unitId, List<int> imageIds);

    // ============= ADMIN MANAGEMENT =============
    Task<Result> AttachAdminAsync(int unitId, string userId);
    Task<Result> RemoveAdminAsync(int unitId, string userId);
    Task<Result<UnitAdminsResponse>> GetUnitAdminsAsync(int unitId);
    Task<Result<IEnumerable<UnitResponse>>> GetAdminUnitsAsync(string userId);

    // ============= POLICY MANAGEMENT =============
    Task<Result> AttachCancellationPolicyAsync(int unitId, int policyId);
    Task<Result> RemoveCancellationPolicyAsync(int unitId);
    Task<Result> AttachGeneralPolicyAsync(int unitId, int policyId);
    Task<Result> RemoveGeneralPolicyAsync(int unitId, int policyId);
    Task<Result<IEnumerable<GeneralPolicyResponse>>> GetUnitPoliciesAsync(int unitId);

    // ============= STATISTICS & SEARCH =============
    Task<Result<UnitStatisticsResponse>> GetStatisticsAsync(int unitId);
    Task<Result<IEnumerable<UnitResponse>>> SearchAsync(string keyword);
    Task<Result<IEnumerable<UnitResponse>>> GetByDepartmentAsync(int departmentId);
    Task<Result<IEnumerable<UnitResponse>>> GetByTypeAsync(int typeId);
    Task<Result<IEnumerable<UnitResponse>>> GetNearbyAsync(decimal latitude, decimal longitude, int radiusKm);

    // ============= VALIDATION =============
    Task<Result<bool>> CanUserManageUnitAsync(string userId, int unitId);
    Task<Result<bool>> IsUnitAvailableAsync(int unitId, DateTime checkIn, DateTime checkOut);

}
