using Application.Abstraction;
using Application.Contracts.AD;
using Application.Contracts.Unit;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Service.UnitType;

public class UnitTypeService(
    ApplicationDbcontext context,
    ILogger<UnitTypeService> logger) : IUnitTypeService
{
    private readonly ApplicationDbcontext _context = context;
    private readonly ILogger<UnitTypeService> _logger = logger;

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
       int pageSize = 10)
    {
        var query = _context.UnitTypes
            .Where(ut => ut.IsActive)
            .AsQueryable();

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
            _context.Units.Count(u => u.UnitTypeId == ut.Id && !u.IsDeleted)
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
                IsActive = request.IsActive
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
            TotalUnits: totalUnits
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
}