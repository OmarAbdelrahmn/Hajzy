
using Application.Abstraction;
using Application.Contracts.Aminety;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace Application.Service.UnitAmenity;

public class UnitAmenityService(
    ApplicationDbcontext context,
    ILogger<UnitAmenityService> logger) : IUnitAmenityService
{
    private readonly ApplicationDbcontext _context = context;
    private readonly ILogger<UnitAmenityService> _logger = logger;

    public async Task<Result<IEnumerable<AmenityResponse>>> GetAllAmenitiesAsync()
    {
        var amenities = await _context.Set<Domain.Entities.Amenity>()
            .AsNoTracking()
            .ToListAsync();

        var responses = amenities.Select(a => new AmenityResponse(
            a.Id,
            a.Name.ToString(),
            a.Description,
            a.Category.ToString()
        )).ToList();

        return Result.Success<IEnumerable<AmenityResponse>>(responses);
    }

    public async Task<Result<IEnumerable<AmenityResponse>>> GetUnitAmenitiesAsync(int unitId)
    {
        var unitExists = await _context.Units
            .AnyAsync(u => u.Id == unitId && !u.IsDeleted);

        if (!unitExists)
            return Result.Failure<IEnumerable<AmenityResponse>>(
                new Error("UnitNotFound", "Unit not found", 404));

        var unitAmenities = await _context.UnitAmenities
            .Include(ua => ua.Amenity)
            .Where(ua => ua.UnitId == unitId)
            .AsNoTracking()
            .ToListAsync();

        var responses = unitAmenities.Select(ua => new AmenityResponse(
            ua.AmenityId,
            ua.Amenity.Name.ToString(),
            ua.Amenity.Description,
            ua.Amenity.Category.ToString(),
            ua.IsAvailable
        )).ToList();

        return Result.Success<IEnumerable<AmenityResponse>>(responses);
    }

    public async Task<Result> AttachAmenityAsync(int unitId, int amenityId)
    {
        var unit = await _context.Units
            .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

        if (unit == null)
            return Result.Failure(
                new Error("UnitNotFound", "Unit not found", 404));

        var amenity = await _context.Set<Domain.Entities.Amenity>()
            .FindAsync(amenityId);

        if (amenity == null)
            return Result.Failure(
                new Error("AmenityNotFound", "Amenity not found", 404));

        // Check if already attached
        var exists = await _context.UnitAmenities
            .AnyAsync(ua => ua.UnitId == unitId && ua.AmenityId == amenityId);

        if (exists)
            return Result.Failure(
                new Error("AlreadyAttached", "Amenity already attached to this unit", 400));

        var unitAmenity = new Domain.Entities.UnitAmenity
        {
            UnitId = unitId,
            AmenityId = amenityId,
            IsAvailable = true
        };

        await _context.UnitAmenities.AddAsync(unitAmenity);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Amenity {AmenityId} attached to unit {UnitId}",
            amenityId, unitId);

        return Result.Success();
    }

    public async Task<Result> AttachAmenitiesAsync(int unitId, List<int> amenityIds)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var unit = await _context.Units
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

            if (unit == null)
                return Result.Failure(
                    new Error("UnitNotFound", "Unit not found", 404));

            // Validate all amenities exist
            var amenities = await _context.Set<Domain.Entities.Amenity>()
                .Where(a => amenityIds.Contains(a.Id))
                .Select(a => a.Id)
                .ToListAsync();

            var invalidIds = amenityIds.Except(amenities).ToList();
            if (invalidIds.Any())
                return Result.Failure(
                    new Error("InvalidAmenities",
                        $"Invalid amenity IDs: {string.Join(", ", invalidIds)}", 400));

            // Get existing amenities
            var existing = await _context.UnitAmenities
                .Where(ua => ua.UnitId == unitId)
                .Select(ua => ua.AmenityId)
                .ToListAsync();

            // Add only new amenities
            var toAdd = amenityIds.Except(existing).ToList();

            foreach (var amenityId in toAdd)
            {
                var unitAmenity = new Domain.Entities.UnitAmenity
                {
                    UnitId = unitId,
                    AmenityId = amenityId,
                    IsAvailable = true
                };
                await _context.UnitAmenities.AddAsync(unitAmenity);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "{Count} amenities attached to unit {UnitId}",
                toAdd.Count, unitId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error attaching amenities to unit {UnitId}", unitId);
            return Result.Failure(
                new Error("AttachFailed", "Failed to attach amenities", 500));
        }
    }

    public async Task<Result> RemoveAmenityAsync(int unitId, int amenityId)
    {
        var unitAmenity = await _context.UnitAmenities
            .FirstOrDefaultAsync(ua => ua.UnitId == unitId && ua.AmenityId == amenityId);

        if (unitAmenity == null)
            return Result.Failure(
                new Error("NotFound", "Amenity not attached to this unit", 404));

        _context.UnitAmenities.Remove(unitAmenity);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Amenity {AmenityId} removed from unit {UnitId}",
            amenityId, unitId);

        return Result.Success();
    }

    public async Task<Result> RemoveAllAmenitiesAsync(int unitId)
    {
        var unitAmenities = await _context.UnitAmenities
            .Where(ua => ua.UnitId == unitId)
            .ToListAsync();

        if (!unitAmenities.Any())
            return Result.Success();

        _context.UnitAmenities.RemoveRange(unitAmenities);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "All amenities removed from unit {UnitId}", unitId);

        return Result.Success();
    }

    public async Task<Result> ToggleAvailabilityAsync(int unitId, int amenityId)
    {
        var unitAmenity = await _context.UnitAmenities
            .FirstOrDefaultAsync(ua => ua.UnitId == unitId && ua.AmenityId == amenityId);

        if (unitAmenity == null)
            return Result.Failure(
                new Error("NotFound", "Amenity not found", 404));

        unitAmenity.IsAvailable = !unitAmenity.IsAvailable;
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> UpdateAmenitiesAsync(
        int unitId,
        UpdateUnitAmenitiesRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var unit = await _context.Units
                .Include(u => u.UnitAmenities)
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

            if (unit == null)
                return Result.Failure(
                    new Error("UnitNotFound", "Unit not found", 404));

            // Validate amenities exist
            var amenities = await _context.Set<Domain.Entities.Amenity>()
                .Where(a => request.AmenityIds.Contains(a.Id))
                .Select(a => a.Id)
                .ToListAsync();

            var invalidIds = request.AmenityIds.Except(amenities).ToList();
            if (invalidIds.Any())
                return Result.Failure(
                    new Error("InvalidAmenities",
                        $"Invalid amenity IDs: {string.Join(", ", invalidIds)}", 400));

            // Remove all existing
            _context.UnitAmenities.RemoveRange(unit.UnitAmenities);

            // Add new ones
            foreach (var amenityId in request.AmenityIds)
            {
                var unitAmenity = new Domain.Entities.UnitAmenity
                {
                    UnitId = unitId,
                    AmenityId = amenityId,
                    IsAvailable = true
                };
                await _context.UnitAmenities.AddAsync(unitAmenity);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Amenities updated for unit {UnitId}. Total: {Count}",
                unitId, request.AmenityIds.Count);

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating amenities for unit {UnitId}", unitId);
            return Result.Failure(
                new Error("UpdateFailed", "Failed to update amenities", 500));
        }
    }
}