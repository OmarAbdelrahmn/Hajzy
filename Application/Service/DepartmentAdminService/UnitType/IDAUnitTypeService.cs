using Application.Abstraction;
using Application.Contracts.Unit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Service.DepartmentAdminService.UnitType
{
    public  interface IDAUnitTypeService
    {
        // ============= CRUD =============
        Task<Result<UnitTypeResponse>> GetByIdAsync(int unitTypeId);
        Task<Result<UnitTypeDetailsResponse>> GetDetailsAsync(int unitTypeId);
        Task<Result<IEnumerable<UnitTypeResponse>>> GetAllAsync(bool includeInactive = false);
        Task<Result<UnitTypeResponse>> CreateAsync(CreateUnitTypeRequest request);
        Task<Result<UnitTypeResponse>> UpdateAsync(int unitTypeId, UpdateUnitTypeRequest request);
        Task<Result> DeleteAsync(int unitTypeId);

        // ============= STATUS MANAGEMENT =============
        Task<Result> ToggleActiveAsync(int unitTypeId);

        // ============= FILTERING & SEARCH =============
        Task<Result<IEnumerable<UnitTypeResponse>>> FilterAsync(UnitTypeFilter filter);
        Task<Result<IEnumerable<UnitTypeResponse>>> SearchAsync(string keyword);

        // ============= STATISTICS =============
        Task<Result<UnitTypeStatisticsResponse>> GetStatisticsAsync();

        // ============= VALIDATION =============
        Task<Result<bool>> IsNameUniqueAsync(string name, int? excludeId = null);
        Task<Result<bool>> CanDeleteAsync(int unitTypeId);
    }
}
