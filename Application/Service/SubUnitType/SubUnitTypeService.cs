using Application.Abstraction;
using Application.Contracts.AD;
using Application.Contracts.Options;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Service.SubUnitType;

public class SubUnitTypeService(
    ApplicationDbcontext context,
    ILogger<SubUnitTypeService> logger) : ISubUnitTypeService
{
    private readonly ApplicationDbcontext _context = context;
    private readonly ILogger<SubUnitTypeService> _logger = logger;
    private static bool SUTOptionRequiresSelections(OptionInputType t) =>
      t is OptionInputType.Select or OptionInputType.MultiSelect;


    #region CRUD Operations

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

    public async Task<Result<PaginatedResponse<SubUnitTypeResponse>>> GetAllSubUnitTypesAsync(
       int page = 1,
       int pageSize = 10)
    {
        var query = _context.Set<SubUnitTypee>()
            .Where(sut => sut.IsActive)
            .AsQueryable();

        var totalCount = await query.CountAsync();

        var subUnitTypes = await query
            .OrderBy(sut => sut.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        var responses = subUnitTypes.Select(sut => new SubUnitTypeResponse(
            sut.Id,
            sut.Name,
            sut.Description,
            sut.IsActive,
            _context.SubUnits.Count(su => su.SubUnitTypeId == sut.Id && !su.IsDeleted)
        )).ToList();

        var paginatedResult = CreatePaginatedResponse(
            responses, totalCount, page, pageSize);

        return Result.Success(paginatedResult);
    }

    public async Task<Result<SubUnitTypeResponse>> GetByIdAsync(int subUnitTypeId)
    {
        var subUnitType = await _context.SubUnitTypees
            .AsNoTracking()
            .FirstOrDefaultAsync(st => st.Id == subUnitTypeId);

        if (subUnitType == null)
            return Result.Failure<SubUnitTypeResponse>(
                new Error("NotFound", "Sub unit type not found", 404));

        var totalSubUnits = await _context.SubUnits
            .CountAsync(s => s.SubUnitTypeId == subUnitTypeId && !s.IsDeleted);

        var response = MapToResponse(subUnitType, totalSubUnits);
        return Result.Success(response);
    }

    public async Task<Result<SubUnitTypeDetailsResponse>> GetDetailsAsync(int subUnitTypeId)
    {
        var subUnitType = await _context.SubUnitTypees
            .AsNoTracking()
            .FirstOrDefaultAsync(st => st.Id == subUnitTypeId);

        if (subUnitType == null)
            return Result.Failure<SubUnitTypeDetailsResponse>(
                new Error("NotFound", "Sub unit type not found", 404));

        var subUnits = await _context.SubUnits
            .Where(s => s.SubUnitTypeId == subUnitTypeId && !s.IsDeleted)
            .Select(s => new SubUnitBasicInfo(
                s.Id,
                s.RoomNumber,
                s.IsAvailable,
                s.UnitId
            ))
            .ToListAsync();

        var response = new SubUnitTypeDetailsResponse
        {
            Id = subUnitType.Id,
            Name = subUnitType.Name,
            Description = subUnitType.Description,
            IsActive = subUnitType.IsActive,
            TotalSubUnits = subUnits.Count,
            ActiveSubUnits = subUnits.Count(s => s.IsAvailable),
            InactiveSubUnits = subUnits.Count(s => !s.IsAvailable),
            SubUnits = subUnits
        };

        return Result.Success(response);
    }

    public async Task<Result<IEnumerable<SubUnitTypeResponse>>> GetAllAsync(bool includeInactive = false)
    {
        var query = _context.SubUnitTypees.AsQueryable();

        if (!includeInactive)
            query = query.Where(st => st.IsActive);

        var SubUnitTypees = await query
            .AsNoTracking()
            .ToListAsync();

        var responses = new List<SubUnitTypeResponse>();

        foreach (var subUnitType in SubUnitTypees)
        {
            var totalSubUnits = await _context.SubUnits
                .CountAsync(s => s.SubUnitTypeId == subUnitType.Id && !s.IsDeleted);

            responses.Add(MapToResponse(subUnitType, totalSubUnits));
        }

        return Result.Success<IEnumerable<SubUnitTypeResponse>>(responses);
    }

    public async Task<Result<SubUnitTypeResponse>> CreateAsync(CreateSubUnitTypeRequest request)
    {
        try
        {
            // Check for duplicate name
            var exists = await _context.SubUnitTypees
                .AnyAsync(st => st.Name.ToLower() == request.Name.ToLower());

            if (exists)
                return Result.Failure<SubUnitTypeResponse>(
                    new Error("DuplicateName", "A sub unit type with this name already exists", 400));

            var subUnitType = new Domain.Entities.SubUnitTypee
            {
                Name = request.Name,
                Description = request.Description,
                IsActive = request.IsActive
            };

            await _context.SubUnitTypees.AddAsync(subUnitType);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Sub unit type {SubUnitTypeId} created: {Name}", subUnitType.Id, subUnitType.Name);

            var response = MapToResponse(subUnitType, 0);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sub unit type");
            return Result.Failure<SubUnitTypeResponse>(
                new Error("CreateFailed", "Failed to create sub unit type", 500));
        }
    }

    public async Task<Result<SubUnitTypeResponse>> UpdateAsync(
        int subUnitTypeId,
        UpdateSubUnitTypeRequest request)
    {
        try
        {
            var subUnitType = await _context.SubUnitTypees
                .FirstOrDefaultAsync(st => st.Id == subUnitTypeId);

            if (subUnitType == null)
                return Result.Failure<SubUnitTypeResponse>(
                    new Error("NotFound", "Sub unit type not found", 404));

            // Check for duplicate name if changing name
            if (request.Name != null && request.Name != subUnitType.Name)
            {
                var duplicate = await _context.SubUnitTypees
                    .AnyAsync(st => st.Id != subUnitTypeId &&
                                   st.Name.ToLower() == request.Name.ToLower());

                if (duplicate)
                    return Result.Failure<SubUnitTypeResponse>(
                        new Error("DuplicateName", "A sub unit type with this name already exists", 400));

                subUnitType.Name = request.Name;
            }

            if (request.Description != null)
                subUnitType.Description = request.Description;

            if (request.IsActive.HasValue)
                subUnitType.IsActive = request.IsActive.Value;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Sub unit type {SubUnitTypeId} updated", subUnitTypeId);

            var totalSubUnits = await _context.SubUnits
                .CountAsync(s => s.SubUnitTypeId == subUnitTypeId && !s.IsDeleted);

            var response = MapToResponse(subUnitType, totalSubUnits);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating sub unit type {SubUnitTypeId}", subUnitTypeId);
            return Result.Failure<SubUnitTypeResponse>(
                new Error("UpdateFailed", "Failed to update sub unit type", 500));
        }
    }

    public async Task<Result> DeleteAsync(int subUnitTypeId)
    {
        try
        {
            var subUnitType = await _context.SubUnitTypees
                .FirstOrDefaultAsync(st => st.Id == subUnitTypeId);

            if (subUnitType == null)
                return Result.Failure(
                    new Error("NotFound", "Sub unit type not found", 404));

            // Check if sub unit type is in use
            var hasSubUnits = await _context.SubUnits
                .AnyAsync(s => s.SubUnitTypeId == subUnitTypeId && !s.IsDeleted);

            if (hasSubUnits)
                return Result.Failure(
                    new Error("InUse", "Cannot delete sub unit type that is in use by sub units", 400));

            _context.SubUnitTypees.Remove(subUnitType);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Sub unit type {SubUnitTypeId} deleted", subUnitTypeId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting sub unit type {SubUnitTypeId}", subUnitTypeId);
            return Result.Failure(
                new Error("DeleteFailed", "Failed to delete sub unit type", 500));
        }
    }

    #endregion

    #region Status Management

    public async Task<Result> ToggleActiveAsync(int subUnitTypeId)
    {
        try
        {
            var subUnitType = await _context.SubUnitTypees
                .FirstOrDefaultAsync(st => st.Id == subUnitTypeId);

            if (subUnitType == null)
                return Result.Failure(
                    new Error("NotFound", "Sub unit type not found", 404));

            subUnitType.IsActive = !subUnitType.IsActive;
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Sub unit type {SubUnitTypeId} status toggled to {IsActive}",
                subUnitTypeId, subUnitType.IsActive);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling sub unit type {SubUnitTypeId} status", subUnitTypeId);
            return Result.Failure(
                new Error("ToggleFailed", "Failed to toggle sub unit type status", 500));
        }
    }

    #endregion

    #region Filtering & Search

    public async Task<Result<IEnumerable<SubUnitTypeResponse>>> FilterAsync(SubUnitTypeFilter filter)
    {
        var query = _context.SubUnitTypees.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(st => st.Name.Contains(filter.Name));

        if (filter.IsActive.HasValue)
            query = query.Where(st => st.IsActive == filter.IsActive.Value);

        // Apply sorting
        query = ApplySorting(query, filter.SortBy, filter.SortDirection);

        // Pagination
        var skip = (filter.Page - 1) * filter.PageSize;

        var SubUnitTypees = await query
            .Skip(skip)
            .Take(filter.PageSize)
            .AsNoTracking()
            .ToListAsync();

        var responses = new List<SubUnitTypeResponse>();

        foreach (var subUnitType in SubUnitTypees)
        {
            var totalSubUnits = await _context.SubUnits
                .CountAsync(s => s.SubUnitTypeId == subUnitType.Id && !s.IsDeleted);

            // Apply sub unit count filters
            if (filter.MinSubUnits.HasValue && totalSubUnits < filter.MinSubUnits.Value)
                continue;

            if (filter.MaxSubUnits.HasValue && totalSubUnits > filter.MaxSubUnits.Value)
                continue;

            responses.Add(MapToResponse(subUnitType, totalSubUnits));
        }

        return Result.Success<IEnumerable<SubUnitTypeResponse>>(responses);
    }

    public async Task<Result<IEnumerable<SubUnitTypeResponse>>> SearchAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return await GetAllAsync();

        keyword = keyword.ToLower();

        var SubUnitTypees = await _context.SubUnitTypees
            .Where(st => st.Name.ToLower().Contains(keyword) ||
                        (st.Description != null && st.Description.ToLower().Contains(keyword)))
            .AsNoTracking()
            .ToListAsync();

        var responses = new List<SubUnitTypeResponse>();

        foreach (var subUnitType in SubUnitTypees)
        {
            var totalSubUnits = await _context.SubUnits
                .CountAsync(s => s.SubUnitTypeId == subUnitType.Id && !s.IsDeleted);

            responses.Add(MapToResponse(subUnitType, totalSubUnits));
        }

        return Result.Success<IEnumerable<SubUnitTypeResponse>>(responses);
    }

    #endregion

    #region Statistics

    public async Task<Result<SubUnitTypeStatisticsResponse>> GetStatisticsAsync()
    {
        var subUnitTypes = await _context.SubUnitTypees
            .AsNoTracking()
            .ToListAsync();

        var subUnitCountByType = new Dictionary<string, int>();
        var totalSubUnits = 0;

        foreach (var subUnitType in subUnitTypes)
        {
            var count = await _context.SubUnits
                .CountAsync(s => s.SubUnitTypeId == subUnitType.Id && !s.IsDeleted);

            subUnitCountByType[subUnitType.Name] = count;
            totalSubUnits += count;
        }

        var response = new SubUnitTypeStatisticsResponse(
            TotalTypes: subUnitTypes.Count,
            ActiveTypes: subUnitTypes.Count(st => st.IsActive),
            InactiveTypes: subUnitTypes.Count(st => !st.IsActive),
            TotalSubUnitsAcrossAllTypes: totalSubUnits,
            SubUnitCountByType: subUnitCountByType
        );

        return Result.Success(response);
    }

    #endregion

    #region Validation

    public async Task<Result<bool>> IsNameUniqueAsync(string name, int? excludeId = null)
    {
        var query = _context.SubUnitTypees
            .Where(st => st.Name.ToLower() == name.ToLower());

        if (excludeId.HasValue)
            query = query.Where(st => st.Id != excludeId.Value);

        var exists = await query.AnyAsync();

        return Result.Success(!exists);
    }

    public async Task<Result<bool>> CanDeleteAsync(int subUnitTypeId)
    {
        var hasSubUnits = await _context.SubUnits
            .AnyAsync(s => s.SubUnitTypeId == subUnitTypeId && !s.IsDeleted);

        return Result.Success(!hasSubUnits);
    }

    #endregion

    #region Private Helper Methods

    private static SubUnitTypeResponse MapToResponse(Domain.Entities.SubUnitTypee subUnitType, int totalSubUnits)
    {
        return new SubUnitTypeResponse(
            Id: subUnitType.Id,
            Name: subUnitType.Name,
            Description: subUnitType.Description,
            IsActive: subUnitType.IsActive,
            TotalSubUnits: totalSubUnits
        );
    }

    private static IQueryable<Domain.Entities.SubUnitTypee> ApplySorting(
        IQueryable<Domain.Entities.SubUnitTypee> query,
        string? sortBy,
        string? sortDirection)
    {
        var descending = sortDirection?.ToUpper() == "DESC";

        return sortBy switch
        {
            "Name" => descending
                ? query.OrderByDescending(st => st.Name)
                : query.OrderBy(st => st.Name),
            _ => query.OrderBy(st => st.Name)
        };
    }

    #endregion

    #region Options
    private static SubUnitTypeOptionResponse MapSUTOption(SubUnitTypeOption opt) => new()
    {
        Id = opt.Id,
        SubUnitTypeId = opt.SubUnitTypeId,
        Name = opt.Name,
        InputType = opt.InputType.ToString(),
        IsRequired = opt.IsRequired,
        DisplayOrder = opt.DisplayOrder,
        IsActive = opt.IsActive,
        CreatedAt = opt.CreatedAt,
        UpdatedAt = opt.UpdatedAt,
        Selections = SUTOptionRequiresSelections(opt.InputType)
            ? opt.Selections.OrderBy(s => s.DisplayOrder)
                  .Select(s => new TypeOptionSelectionDto
                  { Id = s.Id, Value = s.Value, DisplayOrder = s.DisplayOrder }).ToList()
            : null
    };

    // ─────────────────────────────────────────────────────────────────────────
    // GET ALL OPTIONS FOR A SUBUNIT TYPE
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<Result<IEnumerable<SubUnitTypeOptionResponse>>> GetOptionsAsync(int subUnitTypeId)
    {
        try
        {
            var opts = await _context.Set<SubUnitTypeOption>()
                .Include(o => o.Selections.OrderBy(s => s.DisplayOrder))
                .Where(o => o.SubUnitTypeId == subUnitTypeId && o.IsActive)
                .OrderBy(o => o.DisplayOrder).ThenBy(o => o.CreatedAt)
                .AsNoTracking().ToListAsync();

            return Result.Success(opts.Select(MapSUTOption));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting options for subunit type {Id}", subUnitTypeId);
            return Result.Failure<IEnumerable<SubUnitTypeOptionResponse>>(
                new Error("GetOptionsFailed", "Failed to retrieve subunit type options", 500));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET SINGLE OPTION
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<Result<SubUnitTypeOptionResponse>> GetOptionByIdAsync(int optionId)
    {
        try
        {
            var opt = await _context.Set<SubUnitTypeOption>()
                .Include(o => o.Selections.OrderBy(s => s.DisplayOrder))
                .FirstOrDefaultAsync(o => o.Id == optionId);

            if (opt is null)
                return Result.Failure<SubUnitTypeOptionResponse>(
                    new Error("NotFound", "SubUnit type option not found", 404));

            return Result.Success(MapSUTOption(opt));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subunit type option {Id}", optionId);
            return Result.Failure<SubUnitTypeOptionResponse>(
                new Error("GetOptionFailed", "Failed to retrieve subunit type option", 500));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CREATE OPTION
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<Result<SubUnitTypeOptionResponse>> CreateOptionAsync(
        int subUnitTypeId, CreateSubUnitTypeOptionRequest request)
    {
        try
        {
            if (!await _context.SubUnitTypees.AnyAsync(t => t.Id == subUnitTypeId))
                return Result.Failure<SubUnitTypeOptionResponse>(
                    new Error("NotFound", "SubUnit type not found", 404));

            if (SUTOptionRequiresSelections(request.InputType) &&
                (request.Selections is null || request.Selections.Count == 0))
                return Result.Failure<SubUnitTypeOptionResponse>(
                    new Error("SelectionsRequired",
                        "Selections are required for Select and MultiSelect input types", 400));

            if (!SUTOptionRequiresSelections(request.InputType) && request.Selections?.Count > 0)
                return Result.Failure<SubUnitTypeOptionResponse>(
                    new Error("SelectionsNotAllowed",
                        "Selections are only allowed for Select and MultiSelect input types", 400));

            var opt = new SubUnitTypeOption
            {
                SubUnitTypeId = subUnitTypeId,
                Name = request.Name,
                InputType = request.InputType,
                IsRequired = request.IsRequired,
                DisplayOrder = request.DisplayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddHours(3)
            };

            if (SUTOptionRequiresSelections(request.InputType) && request.Selections is not null)
                opt.Selections = request.Selections
                    .Select((s, i) => new SubUnitTypeOptionSelection
                    { Value = s.Value, DisplayOrder = s.DisplayOrder == 0 ? i : s.DisplayOrder })
                    .ToList();

            _context.Set<SubUnitTypeOption>().Add(opt);
            await _context.SaveChangesAsync();
            await _context.Entry(opt).Collection(o => o.Selections).LoadAsync();

            _logger.LogInformation(
                "SubUnitTypeOption {Id} created for subunit type {TypeId}", opt.Id, subUnitTypeId);

            return Result.Success(MapSUTOption(opt));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating option for subunit type {Id}", subUnitTypeId);
            return Result.Failure<SubUnitTypeOptionResponse>(
                new Error("CreateOptionFailed", "Failed to create subunit type option", 500));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UPDATE OPTION
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<Result<SubUnitTypeOptionResponse>> UpdateOptionAsync(
        int optionId, UpdateSubUnitTypeOptionRequest request)
    {
        try
        {
            var opt = await _context.Set<SubUnitTypeOption>()
                .Include(o => o.Selections)
                .FirstOrDefaultAsync(o => o.Id == optionId);

            if (opt is null)
                return Result.Failure<SubUnitTypeOptionResponse>(
                    new Error("NotFound", "SubUnit type option not found", 404));

            if (request.Name is not null) opt.Name = request.Name;
            if (request.IsRequired is not null) opt.IsRequired = request.IsRequired.Value;
            if (request.DisplayOrder is not null) opt.DisplayOrder = request.DisplayOrder.Value;
            if (request.IsActive is not null) opt.IsActive = request.IsActive.Value;

            var effectiveType = request.InputType ?? opt.InputType;
            if (request.InputType.HasValue) opt.InputType = request.InputType.Value;

            if (request.Selections is not null)
            {
                if (SUTOptionRequiresSelections(effectiveType) && request.Selections.Count == 0)
                    return Result.Failure<SubUnitTypeOptionResponse>(
                        new Error("SelectionsRequired",
                            "Selections cannot be empty for Select/MultiSelect types", 400));

                if (!SUTOptionRequiresSelections(effectiveType) && request.Selections.Count > 0)
                    return Result.Failure<SubUnitTypeOptionResponse>(
                        new Error("SelectionsNotAllowed",
                            "Selections are only allowed for Select and MultiSelect types", 400));

                // Atomically replace selections
                _context.Set<SubUnitTypeOptionSelection>().RemoveRange(opt.Selections);
                opt.Selections = request.Selections
                    .Select((s, i) => new SubUnitTypeOptionSelection
                    { Value = s.Value, DisplayOrder = s.DisplayOrder == 0 ? i : s.DisplayOrder })
                    .ToList();
            }
            else if (!SUTOptionRequiresSelections(effectiveType) && opt.Selections.Count > 0)
            {
                // InputType changed away from Select/MultiSelect — purge stale selections
                _context.Set<SubUnitTypeOptionSelection>().RemoveRange(opt.Selections);
                opt.Selections.Clear();
            }

            opt.UpdatedAt = DateTime.UtcNow.AddHours(3);
            await _context.SaveChangesAsync();

            _logger.LogInformation("SubUnitTypeOption {Id} updated", optionId);
            return Result.Success(MapSUTOption(opt));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subunit type option {Id}", optionId);
            return Result.Failure<SubUnitTypeOptionResponse>(
                new Error("UpdateOptionFailed", "Failed to update subunit type option", 500));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DELETE OPTION  (cascades to SubUnitOptionValues via DB constraint)
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<Result> DeleteOptionAsync(int optionId)
    {
        try
        {
            var opt = await _context.Set<SubUnitTypeOption>()
                .Include(o => o.Selections)
                .FirstOrDefaultAsync(o => o.Id == optionId);

            if (opt is null)
                return Result.Failure(new Error("NotFound", "SubUnit type option not found", 404));

            // Remove selections first, then the option itself
            _context.Set<SubUnitTypeOptionSelection>().RemoveRange(opt.Selections);
            _context.Set<SubUnitTypeOption>().Remove(opt);
            await _context.SaveChangesAsync();

            _logger.LogInformation("SubUnitTypeOption {Id} deleted", optionId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting subunit type option {Id}", optionId);
            return Result.Failure(
                new Error("DeleteOptionFailed", "Failed to delete subunit type option", 500));
        }
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────────
    // ADD this entire #region block at the BOTTOM of SubUnitTypeService.cs
    // (inside the class, after the existing #region Options block)
    // ─────────────────────────────────────────────────────────────────────────────

    #region SubUnit Option Values (platform-admin scope)

    /// <summary>
    /// Returns every active option defined on the subunit's SubUnitTypee, together
    /// with whatever values have already been saved for that specific subunit.
    /// No user-access gate — intended for platform admins.
    /// </summary>
    public async Task<Result<IEnumerable<SubUnitOptionValueResponse>>> GetSubUnitOptionValuesAsync(int subUnitId)
    {
        try
        {
            var subUnit = await _context.SubUnits
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

            if (subUnit is null)
                return Result.Failure<IEnumerable<SubUnitOptionValueResponse>>(
                    new Error("NotFound", "SubUnit not found", 404));

            // Load option definitions from the SubUnitTypee
            var typeOptions = await _context.Set<SubUnitTypeOption>()
                .Include(o => o.Selections.OrderBy(s => s.DisplayOrder))
                .Where(o => o.SubUnitTypeId == subUnit.SubUnitTypeId && o.IsActive)
                .OrderBy(o => o.DisplayOrder).ThenBy(o => o.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            // Load values already saved for this subunit
            var savedValues = await _context.Set<SubUnitOptionValue>()
                .Where(v => v.SubUnitId == subUnitId)
                .AsNoTracking()
                .ToListAsync();

            var responses = typeOptions.Select(opt =>
            {
                var vals = savedValues
                    .Where(v => v.SubUnitTypeOptionId == opt.Id)
                    .Select(v => v.Value)
                    .ToList();

                return new SubUnitOptionValueResponse
                {
                    SubUnitTypeOptionId = opt.Id,
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
            _logger.LogError(ex, "Error getting subunit option values for subunit {SubUnitId}", subUnitId);
            return Result.Failure<IEnumerable<SubUnitOptionValueResponse>>(
                new Error("GetValuesFailed", "Failed to retrieve subunit option values", 500));
        }
    }

    /// <summary>
    /// Saves (upserts) option values for a subunit.
    /// Each entry atomically replaces all existing values for that option on this subunit.
    /// No user-access gate — intended for platform admins.
    /// </summary>
    public async Task<Result> SaveSubUnitOptionValuesAsync(int subUnitId, SaveSubUnitOptionValuesRequest request)
    {
        try
        {
            var subUnit = await _context.SubUnits
                .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

            if (subUnit is null)
                return Result.Failure(new Error("NotFound", "SubUnit not found", 404));

            // Validate that all option IDs belong to this subunit's type
            var optionIds = request.Options.Select(o => o.SubUnitTypeOptionId).Distinct().ToList();

            var validOptions = await _context.Set<SubUnitTypeOption>()
                .Where(o => optionIds.Contains(o.Id) &&
                            o.SubUnitTypeId == subUnit.SubUnitTypeId &&
                            o.IsActive)
                .ToListAsync();

            if (validOptions.Count != optionIds.Count)
                return Result.Failure(
                    new Error("InvalidOptions",
                        "One or more option IDs are invalid or do not belong to this subunit's type", 400));

            // Validate required options are provided
            foreach (var opt in validOptions.Where(o => o.IsRequired))
            {
                var input = request.Options.FirstOrDefault(i => i.SubUnitTypeOptionId == opt.Id);
                if (input is null || input.Values.Count == 0 || input.Values.All(string.IsNullOrWhiteSpace))
                    return Result.Failure(
                        new Error("RequiredOptionMissing",
                            $"Option '{opt.Name}' is required and must have a value", 400));
            }

            // For each submitted option, atomically replace its values
            foreach (var input in request.Options)
            {
                var existing = await _context.Set<SubUnitOptionValue>()
                    .Where(v => v.SubUnitId == subUnitId &&
                                v.SubUnitTypeOptionId == input.SubUnitTypeOptionId)
                    .ToListAsync();

                _context.Set<SubUnitOptionValue>().RemoveRange(existing);

                var newValues = input.Values
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .Select(v => new SubUnitOptionValue
                    {
                        SubUnitId = subUnitId,
                        SubUnitTypeOptionId = input.SubUnitTypeOptionId,
                        Value = v,
                        CreatedAt = DateTime.UtcNow.AddHours(3)
                    })
                    .ToList();

                if (newValues.Count > 0)
                    _context.Set<SubUnitOptionValue>().AddRange(newValues);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Option values saved for subunit {SubUnitId} by platform admin", subUnitId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving subunit option values for subunit {SubUnitId}", subUnitId);
            return Result.Failure(
                new Error("SaveValuesFailed", "Failed to save subunit option values", 500));
        }
    }

    #endregion
}