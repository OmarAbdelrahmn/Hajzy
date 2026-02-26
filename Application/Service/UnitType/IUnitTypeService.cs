using Application.Abstraction;
using Application.Contracts.AD;
using Application.Contracts.Options;
using Application.Contracts.Unit;

namespace Application.Service.UnitType;

public interface IUnitTypeService
{
    // ============= CRUD =============

    Task<Result<PaginatedResponse<UnitTypeResponse>>> GetAllUnitTypesAsync(int page = 1, int pageSize = 10, string? searchTerm = null);

    Task<Result<UnitTypeResponse>> GetByIdAsync(int unitTypeId);
    Task<Result<UnitTypeDetailsResponse>> GetDetailsAsync(int unitTypeId);
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

    Task<Result<IEnumerable<UnitTypeOptionResponse>>> GetOptionsAsync(int unitTypeId);
    Task<Result<UnitTypeOptionResponse>> GetOptionByIdAsync(int optionId);
    Task<Result<UnitTypeOptionResponse>> CreateOptionAsync(int unitTypeId, CreateUnitTypeOptionRequest request);
    Task<Result<UnitTypeOptionResponse>> UpdateOptionAsync(int optionId, UpdateUnitTypeOptionRequest request);
    Task<Result> DeleteOptionAsync(int optionId);

    /// <summary>
    /// Returns every active option defined on the unit's UnitType, together with
    /// the values already saved for that specific unit.
    /// No user-access gate — intended for platform admins.
    /// </summary>
    Task<Result<IEnumerable<UnitOptionValueResponse>>> GetUnitOptionValuesAsync(int unitId);

    /// <summary>
    /// Saves (upserts) option values for a unit.
    /// Each entry atomically replaces all existing values for that option on this unit.
    /// No user-access gate — intended for platform admins.
    /// </summary>
    Task<Result> SaveUnitOptionValuesAsync(int unitId, SaveUnitOptionValuesRequest request);
}
