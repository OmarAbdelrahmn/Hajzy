using Application.Abstraction;
using Application.Contracts.Aminety;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Service.SubUnitAmenity;


public class SubUnitAmenityService(
    ApplicationDbcontext context,
    ILogger<SubUnitAmenityService> logger) : ISubUnitAmenityService
{
    private readonly ApplicationDbcontext _context = context;
    private readonly ILogger<SubUnitAmenityService> _logger = logger;

    public async Task<Result<IEnumerable<AmenityResponse>>> GetSubUnitAmenitiesAsync(int subUnitId)
    {
        var subUnitExists = await _context.SubUnits
            .AnyAsync(s => s.Id == subUnitId && !s.IsDeleted);

        if (!subUnitExists)
            return Result.Failure<IEnumerable<AmenityResponse>>(
                new Error("SubUnitNotFound", "SubUnit not found", 404));

        var subUnitAmenities = await _context.Set<SubUniteAmenity>()
            .Include(sa => sa.Amenity)
            .Where(sa => sa.SubUnitId == subUnitId)
            .AsNoTracking()
            .ToListAsync();

        var responses = subUnitAmenities.Select(sa => new AmenityResponse(
            sa.AmenityId,
            sa.Amenity.Name.ToString(),
            sa.Amenity.Description,
            sa.Amenity.Category.ToString(),
            sa.IsAvailable
        )).ToList();

        return Result.Success<IEnumerable<AmenityResponse>>(responses);
    }

    public async Task<Result> AttachAmenityAsync(int subUnitId, int amenityId)
    {
        var subUnit = await _context.SubUnits
            .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

        if (subUnit == null)
            return Result.Failure(
                new Error("SubUnitNotFound", "SubUnit not found", 404));

        var amenity = await _context.Set<Domain.Entities.Amenity>()
            .FindAsync(amenityId);

        if (amenity == null)
            return Result.Failure(
                new Error("AmenityNotFound", "Amenity not found", 404));

        // Check if already attached
        var exists = await _context.Set<SubUniteAmenity>()
            .AnyAsync(sa => sa.SubUnitId == subUnitId && sa.AmenityId == amenityId);

        if (exists)
            return Result.Failure(
                new Error("AlreadyAttached", "Amenity already attached to this subunit", 400));

        var subUnitAmenity = new SubUniteAmenity
        {
            SubUnitId = subUnitId,
            AmenityId = amenityId,
            IsAvailable = true
        };

        await _context.Set<SubUniteAmenity>().AddAsync(subUnitAmenity);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Amenity {AmenityId} attached to subunit {SubUnitId}",
            amenityId, subUnitId);

        return Result.Success();
    }

    public async Task<Result> AttachAmenitiesAsync(int subUnitId, List<int> amenityIds)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var subUnit = await _context.SubUnits
                .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

            if (subUnit == null)
                return Result.Failure(
                    new Error("SubUnitNotFound", "SubUnit not found", 404));

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
            var existing = await _context.Set<SubUniteAmenity>()
                .Where(sa => sa.SubUnitId == subUnitId)
                .Select(sa => sa.AmenityId)
                .ToListAsync();

            // Add only new amenities
            var toAdd = amenityIds.Except(existing).ToList();

            foreach (var amenityId in toAdd)
            {
                var subUnitAmenity = new SubUniteAmenity
                {
                    SubUnitId = subUnitId,
                    AmenityId = amenityId,
                    IsAvailable = true
                };
                await _context.Set<SubUniteAmenity>().AddAsync(subUnitAmenity);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "{Count} amenities attached to subunit {SubUnitId}",
                toAdd.Count, subUnitId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error attaching amenities to subunit {SubUnitId}", subUnitId);
            return Result.Failure(
                new Error("AttachFailed", "Failed to attach amenities", 500));
        }
    }

    public async Task<Result> RemoveAmenityAsync(int subUnitId, int amenityId)
    {
        var subUnitAmenity = await _context.Set<SubUniteAmenity>()
            .FirstOrDefaultAsync(sa => sa.SubUnitId == subUnitId && sa.AmenityId == amenityId);

        if (subUnitAmenity == null)
            return Result.Failure(
                new Error("NotFound", "Amenity not attached to this subunit", 404));

        _context.Set<SubUniteAmenity>().Remove(subUnitAmenity);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Amenity {AmenityId} removed from subunit {SubUnitId}",
            amenityId, subUnitId);

        return Result.Success();
    }

    public async Task<Result> RemoveAllAmenitiesAsync(int subUnitId)
    {
        var subUnitAmenities = await _context.Set<SubUniteAmenity>()
            .Where(sa => sa.SubUnitId == subUnitId)
            .ToListAsync();

        if (!subUnitAmenities.Any())
            return Result.Success();

        _context.Set<SubUniteAmenity>().RemoveRange(subUnitAmenities);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "All amenities removed from subunit {SubUnitId}", subUnitId);

        return Result.Success();
    }

    public async Task<Result> ToggleAvailabilityAsync(int subUnitId, int amenityId)
    {
        var subUnitAmenity = await _context.Set<SubUniteAmenity>()
            .FirstOrDefaultAsync(sa => sa.SubUnitId == subUnitId && sa.AmenityId == amenityId);

        if (subUnitAmenity == null)
            return Result.Failure(
                new Error("NotFound", "Amenity not found", 404));

        subUnitAmenity.IsAvailable = !subUnitAmenity.IsAvailable;
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> UpdateAmenitiesAsync(
        int subUnitId,
        UpdateSubUnitAmenitiesRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var subUnit = await _context.SubUnits
                .Include(s => s.SubUnitAmenities)
                .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

            if (subUnit == null)
                return Result.Failure(
                    new Error("SubUnitNotFound", "SubUnit not found", 404));

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
            _context.Set<SubUniteAmenity>().RemoveRange(subUnit.SubUnitAmenities);

            // Add new ones
            foreach (var amenityId in request.AmenityIds)
            {
                var subUnitAmenity = new SubUniteAmenity
                {
                    SubUnitId = subUnitId,
                    AmenityId = amenityId,
                    IsAvailable = true
                };
                await _context.Set<SubUniteAmenity>().AddAsync(subUnitAmenity);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Amenities updated for subunit {SubUnitId}. Total: {Count}",
                subUnitId, request.AmenityIds.Count);

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating amenities for subunit {SubUnitId}", subUnitId);
            return Result.Failure(
                new Error("UpdateFailed", "Failed to update amenities", 500));
        }
    }

    public async Task<Result> CopyFromUnitAsync(int subUnitId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var subUnit = await _context.SubUnits
                .Include(s => s.SubUnitAmenities)
                .Include(s => s.Unit)
                    .ThenInclude(u => u.UnitAmenities)
                .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

            if (subUnit == null)
                return Result.Failure(
                    new Error("SubUnitNotFound", "SubUnit not found", 404));

            // Get unit amenities
            var unitAmenityIds = subUnit.Unit.UnitAmenities
                .Where(ua => ua.IsAvailable)
                .Select(ua => ua.AmenityId)
                .ToList();

            if (!unitAmenityIds.Any())
                return Result.Success(); // Nothing to copy

            // Get existing subunit amenities
            var existingIds = subUnit.SubUnitAmenities
                .Select(sa => sa.AmenityId)
                .ToList();

            // Add only new amenities
            var toAdd = unitAmenityIds.Except(existingIds).ToList();

            foreach (var amenityId in toAdd)
            {
                var subUnitAmenity = new SubUniteAmenity
                {
                    SubUnitId = subUnitId,
                    AmenityId = amenityId,
                    IsAvailable = true
                };
                await _context.Set<SubUniteAmenity>().AddAsync(subUnitAmenity);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "{Count} amenities copied from unit to subunit {SubUnitId}",
                toAdd.Count, subUnitId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error copying amenities to subunit {SubUnitId}", subUnitId);
            return Result.Failure(
                new Error("CopyFailed", "Failed to copy amenities", 500));
        }
    }
}