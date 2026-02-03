using Application.Abstraction;
using Application.Contracts.Aminety;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Service.Amenity;

public class AmenityService(
    ApplicationDbcontext context,
    ILogger<AmenityService> logger) : IAmenityService
{
    private readonly ApplicationDbcontext _context = context;
    private readonly ILogger<AmenityService> _logger = logger;

    #region CRUD

    public async Task<Result<PaginatedResponse<AmenityResponse>>> GetAllAmenitiesAsync(
        )
    {
        var query = _context.Set<Domain.Entities.Amenity>()
            .AsNoTracking();

        var totalCount = await query.CountAsync();
        int page = 1;
        int pageSize = 10;

        var amenities = await query
            .OrderBy(a => a.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var responses = amenities.Select(a => new AmenityResponse(
            a.Id,
            a.Name,
            a.Description,
            a.Category
        )).ToList();

        var paginatedResult = CreatePaginatedResponse(
            responses, totalCount, page, pageSize);

        return Result.Success(paginatedResult);
    }

    public async Task<Result<AmenityDetailsResponse>> GetByIdAsync(int amenityId)
    {
        var amenity = await _context.Set<Domain.Entities.Amenity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == amenityId);

        if (amenity == null)
            return Result.Failure<AmenityDetailsResponse>(
                new Error("NotFound", "Amenity not found", 404));

        // Count units and subunits using this amenity
        var unitCount = await _context.UnitAmenities
            .CountAsync(ua => ua.AmenityId == amenityId);

        var subUnitCount = await _context.Set<SubUniteAmenity>()
            .CountAsync(sa => sa.AmenityId == amenityId);

        var response = new AmenityDetailsResponse
        {
            Id = amenity.Id,
            Name = amenity.Name ,
            Description = amenity.Description,
            Category = amenity.Category ,
            TotalUnitsUsing = unitCount,
            TotalSubUnitsUsing = subUnitCount,
            CreatedAt = DateTime.UtcNow, // You might want to add this to your entity
            UpdatedAt = null
        };

        return Result.Success(response);
    }

    public async Task<Result<IEnumerable<AmenityResponse>>> GetByCategoryAsync(string category)
    {
        var amenities = await _context.Set<Domain.Entities.Amenity>()
            .Where(a => a.Category  == category)
            .AsNoTracking()
            .ToListAsync();

        var responses = amenities.Select(a => new AmenityResponse(
            a.Id,
            a.Name ,
            a.Description,
            a.Category 
        )).ToList();

        return Result.Success<IEnumerable<AmenityResponse>>(responses);
    }

    public async Task<Result<AmenityResponse>> CreateAsync(CreateAmenityRequest request)
    {
        try
        {
            // Check for duplicate name
            var exists = await _context.Set<Domain.Entities.Amenity>()
                .AnyAsync(a => a.Name  == request.Name);

            if (exists)
                return Result.Failure<AmenityResponse>(
                    new Error("DuplicateAmenity", "An amenity with this name already exists", 400));

            var amenity = new Domain.Entities.Amenity
            {
                // You'll need to parse the enum values
                 Name = request.Name,
                 Category = request.Category,
                Description = request.Description
            };

            await _context.Set<Domain.Entities.Amenity>().AddAsync(amenity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Amenity {AmenityId} created successfully", amenity.Id);

            var response = new AmenityResponse(
                amenity.Id,
                amenity.Name ,
                amenity.Description,
                amenity.Category 
            );

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating amenity");
            return Result.Failure<AmenityResponse>(
                new Error("CreateFailed", "Failed to create amenity", 500));
        }
    }

    public async Task<Result<AmenityResponse>> UpdateAsync(int amenityId, UpdateAmenityRequest request)
    {
        var amenity = await _context.Set<Domain.Entities.Amenity>()
            .FirstOrDefaultAsync(a => a.Id == amenityId);

        if (amenity == null)
            return Result.Failure<AmenityResponse>(
                new Error("NotFound", "Amenity not found", 404));

        try
        {
            // Update properties if provided
            if (!string.IsNullOrEmpty(request.Name))
            {
                // Check for duplicate
                var duplicate = await _context.Set<Domain.Entities.Amenity>()
                    .AnyAsync(a => a.Id != amenityId && a.Name  == request.Name);

                if (duplicate)
                    return Result.Failure<AmenityResponse>(
                        new Error("DuplicateAmenity", "An amenity with this name already exists", 400));

                 amenity.Name = request.Name;
            }

            if (!string.IsNullOrEmpty(request.Category))
            {
                 amenity.Category = request.Category;
            }

            if (request.Description != null)
            {
                amenity.Description = request.Description;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Amenity {AmenityId} updated successfully", amenityId);

            var response = new AmenityResponse(
                amenity.Id,
                amenity.Name ,
                amenity.Description,
                amenity.Category 
            );

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating amenity {AmenityId}", amenityId);
            return Result.Failure<AmenityResponse>(
                new Error("UpdateFailed", "Failed to update amenity", 500));
        }
    }

    public async Task<Result> DeleteAsync(int amenityId)
    {
        var amenity = await _context.Set<Domain.Entities.Amenity>()
            .FirstOrDefaultAsync(a => a.Id == amenityId);

        if (amenity == null)
            return Result.Failure(
                new Error("NotFound", "Amenity not found", 404));

        // Check if amenity is in use
        var isInUse = await _context.UnitAmenities
            .AnyAsync(ua => ua.AmenityId == amenityId) ||
            await _context.Set<SubUniteAmenity>()
            .AnyAsync(sa => sa.AmenityId == amenityId);

        if (isInUse)
            return Result.Failure(
                new Error("AmenityInUse", "Cannot delete amenity that is currently in use", 400));

        try
        {
            _context.Set<Domain.Entities.Amenity>().Remove(amenity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Amenity {AmenityId} deleted successfully", amenityId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting amenity {AmenityId}", amenityId);
            return Result.Failure(
                new Error("DeleteFailed", "Failed to delete amenity", 500));
        }
    }

    #endregion

    #region FILTERING

    public async Task<Result<IEnumerable<AmenityResponse>>> FilterAmenitiesAsync(AmenityFilter filter)
    {
        var query = _context.Set<Domain.Entities.Amenity>().AsQueryable();

        // Apply category filter
        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            query = query.Where(a => a.Category  == filter.Category);
        }

        // Apply search term filter
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.ToLower();
            query = query.Where(a =>
                a.Name .ToLower().Contains(searchTerm) ||
                (a.Description != null && a.Description.ToLower().Contains(searchTerm)));
        }

        // Pagination
        var totalCount = await query.CountAsync();
        var skip = (filter.Page - 1) * filter.PageSize;

        var amenities = await query
            .Skip(skip)
            .Take(filter.PageSize)
            .AsNoTracking()
            .ToListAsync();

        var responses = amenities.Select(a => new AmenityResponse(
            a.Id,
            a.Name ,
            a.Description,
            a.Category 
        )).ToList();

        return Result.Success<IEnumerable<AmenityResponse>>(responses);
    }

    #endregion

    #region STATISTICS

    //public async Task<Result<AmenityDetailsResponse>> GetAmenityStatisticsAsync(int amenityId)
    //{
    //    return await GetByIdAsync(amenityId);
    //}

    public async Task<Result<IEnumerable<string>>> GetCategoriesAsync()
    {
        var categories = await _context.Set<Domain.Entities.Amenity>()
            .Select(a => a.Category )
            .Distinct()
            .AsNoTracking()
            .ToListAsync();

        return Result.Success<IEnumerable<string>>(categories);
    }

    #endregion


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

    
}