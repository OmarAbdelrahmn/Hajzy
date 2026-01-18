using Application.Abstraction;
using Application.Contracts.SubUnit;
using Application.Contracts.Unit;

namespace Application.Service.SubUnitType;

public interface ISubUnitTypeService
{
    // ============= CRUD =============
    Task<Result<SubUnitTypeResponse>> GetByIdAsync(int subUnitTypeId);
    Task<Result<SubUnitTypeDetailsResponse>> GetDetailsAsync(int subUnitTypeId);
    Task<Result<IEnumerable<SubUnitTypeResponse>>> GetAllAsync(bool includeInactive = false);
    Task<Result<SubUnitTypeResponse>> CreateAsync(CreateSubUnitTypeRequest request);
    Task<Result<SubUnitTypeResponse>> UpdateAsync(int subUnitTypeId, UpdateSubUnitTypeRequest request);
    Task<Result> DeleteAsync(int subUnitTypeId);

    // ============= STATUS MANAGEMENT =============
    Task<Result> ToggleActiveAsync(int subUnitTypeId);

    // ============= FILTERING & SEARCH =============
    Task<Result<IEnumerable<SubUnitTypeResponse>>> FilterAsync(SubUnitTypeFilter filter);
    Task<Result<IEnumerable<SubUnitTypeResponse>>> SearchAsync(string keyword);

    // ============= STATISTICS =============
    Task<Result<SubUnitTypeStatisticsResponse>> GetStatisticsAsync();

    // ============= VALIDATION =============
    Task<Result<bool>> IsNameUniqueAsync(string name, int? excludeId = null);
    Task<Result<bool>> CanDeleteAsync(int subUnitTypeId);
}


// ============= RESPONSES =============

public record SubUnitTypeResponse(
    int Id,
    string Name,
    string? Description,
    bool IsActive,
    int TotalSubUnits
);

public class SubUnitTypeDetailsResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int TotalSubUnits { get; set; }
    public int ActiveSubUnits { get; set; }
    public int InactiveSubUnits { get; set; }
    public List<SubUnitBasicInfo> SubUnits { get; set; } = [];
}

public record SubUnitBasicInfo(
    int Id,
    string RoomNumber,
    bool IsAvailable,
    int UnitId
);

public record SubUnitTypeStatisticsResponse(
    int TotalTypes,
    int ActiveTypes,
    int InactiveTypes,
    int TotalSubUnitsAcrossAllTypes,
    Dictionary<string, int> SubUnitCountByType
);

// ============= REQUESTS =============

public class CreateSubUnitTypeRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateSubUnitTypeRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}

// ============= FILTERS =============

public class SubUnitTypeFilter
{
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
    public int? MinSubUnits { get; set; }
    public int? MaxSubUnits { get; set; }
    public string? SortBy { get; set; } = "Name";
    public string? SortDirection { get; set; } = "ASC";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}