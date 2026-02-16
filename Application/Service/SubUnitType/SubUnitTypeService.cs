using Application.Abstraction;
using Application.Contracts.AD;
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
}