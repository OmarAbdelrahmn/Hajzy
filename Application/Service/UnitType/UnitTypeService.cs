using Application.Abstraction;
using Application.Contracts.AD;
using Application.Contracts.Options;
using Application.Contracts.Unit;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Service.UnitType;

public class UnitTypeService(
    ApplicationDbcontext context,
    ILogger<UnitTypeService> logger) : IUnitTypeService
{
    private readonly ApplicationDbcontext _context = context;
    private readonly ILogger<UnitTypeService> _logger = logger;
    private static bool UTOptionRequiresSelections(OptionInputType t) =>
    t is OptionInputType.Select or OptionInputType.MultiSelect;

    #region CRUD Operations

    public async Task<Result<UnitTypeResponse>> GetByIdAsync(int unitTypeId)
    {
        var unitType = await _context.UnitTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(ut => ut.Id == unitTypeId);

        if (unitType == null)
            return Result.Failure<UnitTypeResponse>(
                new Error("NotFound", "Unit type not found", 404));

        var totalUnits = await _context.Units
            .CountAsync(u => u.UnitTypeId == unitTypeId && !u.IsDeleted);

        var response = MapToResponse(unitType, totalUnits);
        return Result.Success(response);
    }
    public async Task<Result<PaginatedResponse<UnitTypeResponse>>> GetAllUnitTypesAsync(
       int page = 1,
       int pageSize = 10,
       string? searchTerm = null)
    {
        var query = _context.UnitTypes
            .Where(ut => ut.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(ut =>
                ut.Name.ToLower().Contains(term) ||
                (ut.Description != null && ut.Description.ToLower().Contains(term))
            );
        }

        var totalCount = await query.CountAsync();

        var unitTypes = await query
            .OrderBy(ut => ut.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        var responses = unitTypes.Select(ut => new UnitTypeResponse(
            ut.Id,
            ut.Name,
            ut.Description,
            ut.IsActive,
            _context.Units.Count(u => u.UnitTypeId == ut.Id && !u.IsDeleted),
            ut.IsStandalone
        )).ToList();

        var paginatedResult = CreatePaginatedResponse(
            responses, totalCount, page, pageSize);

        return Result.Success(paginatedResult);
    }

    private PaginatedResponse<T> CreatePaginatedResponse<T>(
        IEnumerable<T> items,
        int totalCount,
        int page,
        int pageSize)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PaginatedResponse<T>
        {
            Items = items,
            TotalCount = totalCount,
            TotalPages = totalPages,
            CurrentPage = page,
            NextPage = page < totalPages ? page + 1 : null,
            PrevPage = page > 1 ? page - 1 : null
        };
    }

    public async Task<Result<UnitTypeDetailsResponse>> GetDetailsAsync(int unitTypeId)
    {
        var unitType = await _context.UnitTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(ut => ut.Id == unitTypeId);

        if (unitType == null)
            return Result.Failure<UnitTypeDetailsResponse>(
                new Error("NotFound", "Unit type not found", 404));

        var units = await _context.Units
            .Where(u => u.UnitTypeId == unitTypeId && !u.IsDeleted)
            .Select(u => new UnitBasicInfo(
                u.Id,
                u.Name,
                u.IsActive,
                u.IsVerified
            ))
            .ToListAsync();

        var response = new UnitTypeDetailsResponse
        {
            Id = unitType.Id,
            Name = unitType.Name,
            Description = unitType.Description,
            IsActive = unitType.IsActive,
            TotalUnits = units.Count,
            ActiveUnits = units.Count(u => u.IsActive),
            InactiveUnits = units.Count(u => !u.IsActive),
            Units = units
        };

        return Result.Success(response);
    }

    public async Task<Result<IEnumerable<UnitTypeResponse>>> GetAllAsync(bool includeInactive = false)
    {
        var query = _context.UnitTypes.AsQueryable();

        if (!includeInactive)
            query = query.Where(ut => ut.IsActive);

        var unitTypes = await query
            .AsNoTracking()
            .ToListAsync();

        var responses = new List<UnitTypeResponse>();

        foreach (var unitType in unitTypes)
        {
            var totalUnits = await _context.Units
                .CountAsync(u => u.UnitTypeId == unitType.Id && !u.IsDeleted);

            responses.Add(MapToResponse(unitType, totalUnits));
        }

        return Result.Success<IEnumerable<UnitTypeResponse>>(responses);
    }

    public async Task<Result<UnitTypeResponse>> CreateAsync(CreateUnitTypeRequest request)
    {
        try
        {
            // Check for duplicate name
            var exists = await _context.UnitTypes
                .AnyAsync(ut => ut.Name.ToLower() == request.Name.ToLower());

            if (exists)
                return Result.Failure<UnitTypeResponse>(
                    new Error("DuplicateName", "A unit type with this name already exists", 400));

            var unitType = new Domain.Entities.UnitType
            {
                Name = request.Name,
                Description = request.Description,
                IsActive = request.IsActive,
                IsStandalone = request.IsStandalone
            };

            await _context.UnitTypes.AddAsync(unitType);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Unit type {UnitTypeId} created: {Name}", unitType.Id, unitType.Name);

            var response = MapToResponse(unitType, 0);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating unit type");
            return Result.Failure<UnitTypeResponse>(
                new Error("CreateFailed", "Failed to create unit type", 500));
        }
    }

    public async Task<Result<UnitTypeResponse>> UpdateAsync(
        int unitTypeId,
        UpdateUnitTypeRequest request)
    {
        try
        {
            var unitType = await _context.UnitTypes
                .FirstOrDefaultAsync(ut => ut.Id == unitTypeId);

            if (unitType == null)
                return Result.Failure<UnitTypeResponse>(
                    new Error("NotFound", "Unit type not found", 404));

            // Check for duplicate name if changing name
            if (request.Name != null && request.Name != unitType.Name)
            {
                var duplicate = await _context.UnitTypes
                    .AnyAsync(ut => ut.Id != unitTypeId &&
                                   ut.Name.ToLower() == request.Name.ToLower());

                if (duplicate)
                    return Result.Failure<UnitTypeResponse>(
                        new Error("DuplicateName", "A unit type with this name already exists", 400));

                unitType.Name = request.Name;
            }

            if (request.Description != null)
                unitType.Description = request.Description;

            if (request.IsActive.HasValue)
                unitType.IsActive = request.IsActive.Value;
            
            if (request.IsStandalone.HasValue)
                unitType.IsStandalone = request.IsStandalone.Value;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Unit type {UnitTypeId} updated", unitTypeId);

            var totalUnits = await _context.Units
                .CountAsync(u => u.UnitTypeId == unitTypeId && !u.IsDeleted);

            var response = MapToResponse(unitType, totalUnits);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating unit type {UnitTypeId}", unitTypeId);
            return Result.Failure<UnitTypeResponse>(
                new Error("UpdateFailed", "Failed to update unit type", 500));
        }
    }

    public async Task<Result> DeleteAsync(int unitTypeId)
    {
        try
        {
            var unitType = await _context.UnitTypes
                .FirstOrDefaultAsync(ut => ut.Id == unitTypeId);

            if (unitType == null)
                return Result.Failure(
                    new Error("NotFound", "Unit type not found", 404));

            // Check if unit type is in use
            var hasUnits = await _context.Units
                .AnyAsync(u => u.UnitTypeId == unitTypeId && !u.IsDeleted);

            if (hasUnits)
                return Result.Failure(
                    new Error("InUse", "Cannot delete unit type that is in use by units", 400));

            _context.UnitTypes.Remove(unitType);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Unit type {UnitTypeId} deleted", unitTypeId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting unit type {UnitTypeId}", unitTypeId);
            return Result.Failure(
                new Error("DeleteFailed", "Failed to delete unit type", 500));
        }
    }

    #endregion

    #region Status Management

    public async Task<Result> ToggleActiveAsync(int unitTypeId)
    {
        try
        {
            var unitType = await _context.UnitTypes
                .FirstOrDefaultAsync(ut => ut.Id == unitTypeId);

            if (unitType == null)
                return Result.Failure(
                    new Error("NotFound", "Unit type not found", 404));

            unitType.IsActive = !unitType.IsActive;
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Unit type {UnitTypeId} status toggled to {IsActive}",
                unitTypeId, unitType.IsActive);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling unit type {UnitTypeId} status", unitTypeId);
            return Result.Failure(
                new Error("ToggleFailed", "Failed to toggle unit type status", 500));
        }
    }

    #endregion

    #region Filtering & Search

    public async Task<Result<IEnumerable<UnitTypeResponse>>> FilterAsync(UnitTypeFilter filter)
    {
        var query = _context.UnitTypes.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(ut => ut.Name.Contains(filter.Name));

        if (filter.IsActive.HasValue)
            query = query.Where(ut => ut.IsActive == filter.IsActive.Value);

        // Apply sorting
        query = ApplySorting(query, filter.SortBy, filter.SortDirection);

        // Pagination
        var skip = (filter.Page - 1) * filter.PageSize;

        var unitTypes = await query
            .Skip(skip)
            .Take(filter.PageSize)
            .AsNoTracking()
            .ToListAsync();

        var responses = new List<UnitTypeResponse>();

        foreach (var unitType in unitTypes)
        {
            var totalUnits = await _context.Units
                .CountAsync(u => u.UnitTypeId == unitType.Id && !u.IsDeleted);

            // Apply unit count filters
            if (filter.MinUnits.HasValue && totalUnits < filter.MinUnits.Value)
                continue;

            if (filter.MaxUnits.HasValue && totalUnits > filter.MaxUnits.Value)
                continue;

            responses.Add(MapToResponse(unitType, totalUnits));
        }

        return Result.Success<IEnumerable<UnitTypeResponse>>(responses);
    }

    public async Task<Result<IEnumerable<UnitTypeResponse>>> SearchAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return await GetAllAsync();

        keyword = keyword.ToLower();

        var unitTypes = await _context.UnitTypes
            .Where(ut => ut.Name.ToLower().Contains(keyword) ||
                        (ut.Description != null && ut.Description.ToLower().Contains(keyword)))
            .AsNoTracking()
            .ToListAsync();

        var responses = new List<UnitTypeResponse>();

        foreach (var unitType in unitTypes)
        {
            var totalUnits = await _context.Units
                .CountAsync(u => u.UnitTypeId == unitType.Id && !u.IsDeleted);

            responses.Add(MapToResponse(unitType, totalUnits));
        }

        return Result.Success<IEnumerable<UnitTypeResponse>>(responses);
    }

    #endregion

    #region Statistics

    public async Task<Result<UnitTypeStatisticsResponse>> GetStatisticsAsync()
    {
        var unitTypes = await _context.UnitTypes
            .AsNoTracking()
            .ToListAsync();

        var unitCountByType = new Dictionary<string, int>();
        var totalUnits = 0;

        foreach (var unitType in unitTypes)
        {
            var count = await _context.Units
                .CountAsync(u => u.UnitTypeId == unitType.Id && !u.IsDeleted);

            unitCountByType[unitType.Name] = count;
            totalUnits += count;
        }

        var response = new UnitTypeStatisticsResponse(
            TotalTypes: unitTypes.Count,
            ActiveTypes: unitTypes.Count(ut => ut.IsActive),
            InactiveTypes: unitTypes.Count(ut => !ut.IsActive),
            TotalUnitsAcrossAllTypes: totalUnits,
            UnitCountByType: unitCountByType
        );

        return Result.Success(response);
    }

    #endregion

    #region Validation

    public async Task<Result<bool>> IsNameUniqueAsync(string name, int? excludeId = null)
    {
        var query = _context.UnitTypes
            .Where(ut => ut.Name.ToLower() == name.ToLower());

        if (excludeId.HasValue)
            query = query.Where(ut => ut.Id != excludeId.Value);

        var exists = await query.AnyAsync();

        return Result.Success(!exists);
    }

    public async Task<Result<bool>> CanDeleteAsync(int unitTypeId)
    {
        var hasUnits = await _context.Units
            .AnyAsync(u => u.UnitTypeId == unitTypeId && !u.IsDeleted);

        return Result.Success(!hasUnits);
    }

    #endregion

    #region Private Helper Methods

    private static UnitTypeResponse MapToResponse(Domain.Entities.UnitType unitType, int totalUnits)
    {
        return new UnitTypeResponse(
            Id: unitType.Id,
            Name: unitType.Name,
            Description: unitType.Description,
            IsActive: unitType.IsActive,
            TotalUnits: totalUnits,
            unitType.IsStandalone
        );
    }

    private static IQueryable<Domain.Entities.UnitType> ApplySorting(
        IQueryable<Domain.Entities.UnitType> query,
        string? sortBy,
        string? sortDirection)
    {
        var descending = sortDirection?.ToUpper() == "DESC";

        return sortBy switch
        {
            "Name" => descending
                ? query.OrderByDescending(ut => ut.Name)
                : query.OrderBy(ut => ut.Name),
            _ => query.OrderBy(ut => ut.Name)
        };
    }

    #endregion

    #region options

    private static UnitTypeOptionResponse MapUTOption(UnitTypeOption opt) => new()
    {
        Id = opt.Id,
        UnitTypeId = opt.UnitTypeId,
        Name = opt.Name,
        InputType = opt.InputType.ToString(),
        IsRequired = opt.IsRequired,
        DisplayOrder = opt.DisplayOrder,
        IsActive = opt.IsActive,
        CreatedAt = opt.CreatedAt,
        UpdatedAt = opt.UpdatedAt,
        Selections = UTOptionRequiresSelections(opt.InputType)
        ? opt.Selections.OrderBy(s => s.DisplayOrder)
              .Select(s => new TypeOptionSelectionDto
              { Id = s.Id, Value = s.Value, DisplayOrder = s.DisplayOrder }).ToList()
        : null
    };

    public async Task<Result<IEnumerable<UnitTypeOptionResponse>>> GetOptionsAsync(int unitTypeId)
    {
        try
        {
            var opts = await _context.Set<UnitTypeOption>()
                .Include(o => o.Selections.OrderBy(s => s.DisplayOrder))
                .Where(o => o.UnitTypeId == unitTypeId && o.IsActive)
                .OrderBy(o => o.DisplayOrder).ThenBy(o => o.CreatedAt)
                .AsNoTracking().ToListAsync();
            return Result.Success(opts.Select(MapUTOption));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting options for unit type {Id}", unitTypeId);
            return Result.Failure<IEnumerable<UnitTypeOptionResponse>>(
                new Error("GetOptionsFailed", "Failed to retrieve unit type options", 500));
        }
    }

    public async Task<Result<UnitTypeOptionResponse>> GetOptionByIdAsync(int optionId)
    {
        try
        {
            var opt = await _context.Set<UnitTypeOption>()
                .Include(o => o.Selections.OrderBy(s => s.DisplayOrder))
                .FirstOrDefaultAsync(o => o.Id == optionId);
            if (opt is null)
                return Result.Failure<UnitTypeOptionResponse>(
                    new Error("NotFound", "Unit type option not found", 404));
            return Result.Success(MapUTOption(opt));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unit type option {Id}", optionId);
            return Result.Failure<UnitTypeOptionResponse>(
                new Error("GetOptionFailed", "Failed to retrieve unit type option", 500));
        }
    }

    public async Task<Result<UnitTypeOptionResponse>> CreateOptionAsync(
        int unitTypeId, CreateUnitTypeOptionRequest request)
    {
        try
        {
            if (!await _context.UnitTypes.AnyAsync(t => t.Id == unitTypeId))
                return Result.Failure<UnitTypeOptionResponse>(
                    new Error("NotFound", "Unit type not found", 404));

            if (UTOptionRequiresSelections(request.InputType) &&
                (request.Selections is null || request.Selections.Count == 0))
                return Result.Failure<UnitTypeOptionResponse>(
                    new Error("SelectionsRequired",
                        "Selections are required for Select and MultiSelect input types", 400));

            if (!UTOptionRequiresSelections(request.InputType) && request.Selections?.Count > 0)
                return Result.Failure<UnitTypeOptionResponse>(
                    new Error("SelectionsNotAllowed",
                        "Selections are only allowed for Select and MultiSelect input types", 400));

            var opt = new UnitTypeOption
            {
                UnitTypeId = unitTypeId,
                Name = request.Name,
                InputType = request.InputType,
                IsRequired = request.IsRequired,
                DisplayOrder = request.DisplayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddHours(3)
            };

            if (UTOptionRequiresSelections(request.InputType) && request.Selections is not null)
                opt.Selections = request.Selections
                    .Select((s, i) => new UnitTypeOptionSelection
                    { Value = s.Value, DisplayOrder = s.DisplayOrder == 0 ? i : s.DisplayOrder })
                    .ToList();

            _context.Set<UnitTypeOption>().Add(opt);
            await _context.SaveChangesAsync();
            await _context.Entry(opt).Collection(o => o.Selections).LoadAsync();

            _logger.LogInformation("UnitTypeOption {Id} created for unit type {TypeId}", opt.Id, unitTypeId);
            return Result.Success(MapUTOption(opt));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating option for unit type {Id}", unitTypeId);
            return Result.Failure<UnitTypeOptionResponse>(
                new Error("CreateOptionFailed", "Failed to create unit type option", 500));
        }
    }

    public async Task<Result<UnitTypeOptionResponse>> UpdateOptionAsync(
        int optionId, UpdateUnitTypeOptionRequest request)
    {
        try
        {
            var opt = await _context.Set<UnitTypeOption>()
                .Include(o => o.Selections)
                .FirstOrDefaultAsync(o => o.Id == optionId);

            if (opt is null)
                return Result.Failure<UnitTypeOptionResponse>(
                    new Error("NotFound", "Unit type option not found", 404));

            if (request.Name is not null) opt.Name = request.Name;
            if (request.IsRequired is not null) opt.IsRequired = request.IsRequired.Value;
            if (request.DisplayOrder is not null) opt.DisplayOrder = request.DisplayOrder.Value;
            if (request.IsActive is not null) opt.IsActive = request.IsActive.Value;

            var effectiveType = request.InputType ?? opt.InputType;
            if (request.InputType.HasValue) opt.InputType = request.InputType.Value;

            if (request.Selections is not null)
            {
                if (UTOptionRequiresSelections(effectiveType) && request.Selections.Count == 0)
                    return Result.Failure<UnitTypeOptionResponse>(new Error("SelectionsRequired",
                        "Selections cannot be empty for Select/MultiSelect types", 400));

                if (!UTOptionRequiresSelections(effectiveType) && request.Selections.Count > 0)
                    return Result.Failure<UnitTypeOptionResponse>(new Error("SelectionsNotAllowed",
                        "Selections are only allowed for Select and MultiSelect types", 400));

                _context.Set<UnitTypeOptionSelection>().RemoveRange(opt.Selections);
                opt.Selections = request.Selections
                    .Select((s, i) => new UnitTypeOptionSelection
                    { Value = s.Value, DisplayOrder = s.DisplayOrder == 0 ? i : s.DisplayOrder })
                    .ToList();
            }
            else if (!UTOptionRequiresSelections(effectiveType) && opt.Selections.Count > 0)
            {
                _context.Set<UnitTypeOptionSelection>().RemoveRange(opt.Selections);
                opt.Selections.Clear();
            }

            opt.UpdatedAt = DateTime.UtcNow.AddHours(3);
            await _context.SaveChangesAsync();
            _logger.LogInformation("UnitTypeOption {Id} updated", optionId);
            return Result.Success(MapUTOption(opt));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating unit type option {Id}", optionId);
            return Result.Failure<UnitTypeOptionResponse>(
                new Error("UpdateOptionFailed", "Failed to update unit type option", 500));
        }
    }

    public async Task<Result> DeleteOptionAsync(int optionId)
    {
        try
        {
            var opt = await _context.Set<UnitTypeOption>()
                .Include(o => o.Selections).FirstOrDefaultAsync(o => o.Id == optionId);

            if (opt is null)
                return Result.Failure(new Error("NotFound", "Unit type option not found", 404));

            _context.Set<UnitTypeOptionSelection>().RemoveRange(opt.Selections);
            _context.Set<UnitTypeOption>().Remove(opt);
            await _context.SaveChangesAsync();
            _logger.LogInformation("UnitTypeOption {Id} deleted", optionId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting unit type option {Id}", optionId);
            return Result.Failure(new Error("DeleteOptionFailed", "Failed to delete unit type option", 500));
        }
    }
    #endregion

    // ─────────────────────────────────────────────────────────────────────────────
    // ADD this entire #region block at the BOTTOM of UnitTypeService.cs
    // (inside the class, after the existing #region options block)
    // ─────────────────────────────────────────────────────────────────────────────

    #region Unit Option Values (platform-admin scope)

    /// <summary>
    /// Returns every active option defined on the unit's UnitType, together with
    /// whatever values have already been saved for that specific unit.
    /// No user-access gate — intended for platform admins.
    /// </summary>
    public async Task<Result<IEnumerable<UnitOptionValueResponse>>> GetUnitOptionValuesAsync(int unitId)
    {
        try
        {
            var unit = await _context.Units
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

            if (unit is null)
                return Result.Failure<IEnumerable<UnitOptionValueResponse>>(
                    new Error("NotFound", "Unit not found", 404));

            // Load option definitions from the UnitType
            var typeOptions = await _context.Set<UnitTypeOption>()
                .Include(o => o.Selections.OrderBy(s => s.DisplayOrder))
                .Where(o => o.UnitTypeId == unit.UnitTypeId && o.IsActive)
                .OrderBy(o => o.DisplayOrder).ThenBy(o => o.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            // Load values already saved for this unit
            var savedValues = await _context.Set<UnitOptionValue>()
                .Where(v => v.UnitId == unitId)
                .AsNoTracking()
                .ToListAsync();

            var responses = typeOptions.Select(opt =>
            {
                var vals = savedValues
                    .Where(v => v.UnitTypeOptionId == opt.Id)
                    .Select(v => v.Value)
                    .ToList();

                return new UnitOptionValueResponse
                {
                    UnitTypeOptionId = opt.Id,
                    OptionName = opt.Name,
                    InputType = opt.InputType.ToString(),
                    IsRequired = opt.IsRequired,
                    Values = vals,
                    AvailableSelections = opt.InputType is OptionInputType.Select
                                              or OptionInputType.MultiSelect
                        ? opt.Selections.Select(s => new TypeOptionSelectionDto
                        { Id = s.Id, Value = s.Value, DisplayOrder = s.DisplayOrder }).ToList()
                        : null
                };
            });

            return Result.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unit option values for unit {UnitId}", unitId);
            return Result.Failure<IEnumerable<UnitOptionValueResponse>>(
                new Error("GetValuesFailed", "Failed to retrieve unit option values", 500));
        }
    }

    /// <summary>
    /// Saves (upserts) option values for a unit.
    /// Each entry atomically replaces all existing values for that option on this unit.
    /// No user-access gate — intended for platform admins.
    /// </summary>
    public async Task<Result> SaveUnitOptionValuesAsync(int unitId, SaveUnitOptionValuesRequest request)
    {
        try
        {
            var unit = await _context.Units
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

            if (unit is null)
                return Result.Failure(new Error("NotFound", "Unit not found", 404));

            // Validate that all option IDs belong to this unit's type
            var optionIds = request.Options.Select(o => o.UnitTypeOptionId).Distinct().ToList();

            var validOptions = await _context.Set<UnitTypeOption>()
                .Where(o => optionIds.Contains(o.Id) &&
                            o.UnitTypeId == unit.UnitTypeId &&
                            o.IsActive)
                .ToListAsync();

            if (validOptions.Count != optionIds.Count)
                return Result.Failure(
                    new Error("InvalidOptions",
                        "One or more option IDs are invalid or do not belong to this unit's type", 400));

            // Validate required options are provided
            foreach (var opt in validOptions.Where(o => o.IsRequired))
            {
                var input = request.Options.FirstOrDefault(i => i.UnitTypeOptionId == opt.Id);
                if (input is null || input.Values.Count == 0 || input.Values.All(string.IsNullOrWhiteSpace))
                    return Result.Failure(
                        new Error("RequiredOptionMissing",
                            $"Option '{opt.Name}' is required and must have a value", 400));
            }

            // For each submitted option, atomically replace its values
            foreach (var input in request.Options)
            {
                var existing = await _context.Set<UnitOptionValue>()
                    .Where(v => v.UnitId == unitId && v.UnitTypeOptionId == input.UnitTypeOptionId)
                    .ToListAsync();

                _context.Set<UnitOptionValue>().RemoveRange(existing);

                var newValues = input.Values
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .Select(v => new UnitOptionValue
                    {
                        UnitId = unitId,
                        UnitTypeOptionId = input.UnitTypeOptionId,
                        Value = v,
                        CreatedAt = DateTime.UtcNow.AddHours(3)
                    })
                    .ToList();

                if (newValues.Count > 0)
                    _context.Set<UnitOptionValue>().AddRange(newValues);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Option values saved for unit {UnitId} by platform admin", unitId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving unit option values for unit {UnitId}", unitId);
            return Result.Failure(
                new Error("SaveValuesFailed", "Failed to save unit option values", 500));
        }
    }

    #endregion

    #region SubUnitType Assignments

    public async Task<Result<IEnumerable<SubUnitTypeResponse>>> GetAllowedSubUnitTypesAsync(int unitTypeId)
    {
        try
        {
            if (!await _context.UnitTypes.AnyAsync(t => t.Id == unitTypeId))
                return Result.Failure<IEnumerable<SubUnitTypeResponse>>(
                    new Error("NotFound", "Unit type not found", 404));

            var subUnitTypes = await _context.UnitTypeSubUnitTypes
                .Where(x => x.UnitTypeId == unitTypeId)
                .Select(x => x.SubUnitType)
                .AsNoTracking()
                .ToListAsync();

            var responses = subUnitTypes.Select(s => new SubUnitTypeResponse(
                s.Id, s.Name, s.Description, s.IsActive,
                _context.SubUnits.Count(su => su.SubUnitTypeId == s.Id && !su.IsDeleted)
            ));

            return Result.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting allowed subunit types for unit type {Id}", unitTypeId);
            return Result.Failure<IEnumerable<SubUnitTypeResponse>>(
                new Error("GetFailed", "Failed to retrieve allowed subunit types", 500));
        }
    }

    public async Task<Result> SetAllowedSubUnitTypesAsync(int unitTypeId, List<int> subUnitTypeIds)
    {
        try
        {
            if (!await _context.UnitTypes.AnyAsync(t => t.Id == unitTypeId))
                return Result.Failure(new Error("NotFound", "Unit type not found", 404));

            // Validate all provided IDs exist
            var validIds = await _context.SubUnitTypees
                .Where(s => subUnitTypeIds.Contains(s.Id))
                .Select(s => s.Id)
                .ToListAsync();

            if (validIds.Count != subUnitTypeIds.Distinct().Count())
                return Result.Failure(
                    new Error("InvalidIds", "One or more SubUnitType IDs do not exist", 400));

            // Atomically replace
            var existing = await _context.UnitTypeSubUnitTypes
                .Where(x => x.UnitTypeId == unitTypeId)
                .ToListAsync();

            _context.UnitTypeSubUnitTypes.RemoveRange(existing);

            var newLinks = subUnitTypeIds.Distinct().Select(id => new UnitTypeSubUnitType
            {
                UnitTypeId = unitTypeId,
                SubUnitTypeId = id
            });

            await _context.UnitTypeSubUnitTypes.AddRangeAsync(newLinks);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Set {Count} allowed subunit types for unit type {Id}", subUnitTypeIds.Count, unitTypeId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting allowed subunit types for unit type {Id}", unitTypeId);
            return Result.Failure(new Error("SetFailed", "Failed to set allowed subunit types", 500));
        }
    }

    public async Task<Result> AddAllowedSubUnitTypeAsync(int unitTypeId, int subUnitTypeId)
    {
        try
        {
            if (!await _context.UnitTypes.AnyAsync(t => t.Id == unitTypeId))
                return Result.Failure(new Error("NotFound", "Unit type not found", 404));

            if (!await _context.SubUnitTypees.AnyAsync(s => s.Id == subUnitTypeId))
                return Result.Failure(new Error("NotFound", "SubUnit type not found", 404));

            var alreadyExists = await _context.UnitTypeSubUnitTypes
                .AnyAsync(x => x.UnitTypeId == unitTypeId && x.SubUnitTypeId == subUnitTypeId);

            if (alreadyExists)
                return Result.Failure(
                    new Error("AlreadyLinked", "This SubUnitType is already allowed for this UnitType", 409));

            _context.UnitTypeSubUnitTypes.Add(new UnitTypeSubUnitType
            {
                UnitTypeId = unitTypeId,
                SubUnitTypeId = subUnitTypeId
            });

            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding subunit type {SubId} to unit type {UnitId}", subUnitTypeId, unitTypeId);
            return Result.Failure(new Error("AddFailed", "Failed to add allowed subunit type", 500));
        }
    }

    public async Task<Result> RemoveAllowedSubUnitTypeAsync(int unitTypeId, int subUnitTypeId)
    {
        try
        {
            var link = await _context.UnitTypeSubUnitTypes
                .FirstOrDefaultAsync(x => x.UnitTypeId == unitTypeId && x.SubUnitTypeId == subUnitTypeId);

            if (link is null)
                return Result.Failure(
                    new Error("NotFound", "This SubUnitType is not linked to this UnitType", 404));

            _context.UnitTypeSubUnitTypes.Remove(link);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing subunit type {SubId} from unit type {UnitId}", subUnitTypeId, unitTypeId);
            return Result.Failure(new Error("RemoveFailed", "Failed to remove allowed subunit type", 500));
        }
    }

    #endregion
}