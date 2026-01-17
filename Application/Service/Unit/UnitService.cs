using Application.Abstraction;
using Application.Contracts.Policy;
using Application.Contracts.Unit;
using Application.Service.S3Image;
using Application.Service.UnitImage;
using Domain;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Service.Unit;

public class UnitService(
    ApplicationDbcontext context,
    IUnitImageService s3Service,
    ILogger<UnitService> logger) : IUnitService
{
    private readonly ApplicationDbcontext _context = context;
    private readonly IUnitImageService _s3Service = s3Service;
    private readonly ILogger<UnitService> _logger = logger;

    #region BASIC CRUD

    public async Task<Result<UnitResponse>> GetByIdAsync(int unitId)
    {
        var unit = await _context.Units
            .Include(u => u.City)
            .Include(u => u.UnitType)
            .Include(u => u.CancellationPolicy)
            .Include(u => u.Admins.Where(a => a.IsActive))
                .ThenInclude(a => a.User)
            .Include(u => u.Images.Where(i => !i.IsDeleted))
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

        if (unit == null)
            return Result.Failure<UnitResponse>(
                new Error("NotFound", "Unit not found", 404));

        return Result.Success(MapToResponse(unit));
    }

    public async Task<Result<UnitDetailsResponse>> GetDetailsAsync(int unitId)
    {
        var unit = await _context.Units
            .Include(u => u.City)
            .Include(u => u.UnitType)
            .Include(u => u.CancellationPolicy)
            .Include(u => u.Admins.Where(a => a.IsActive))
                .ThenInclude(a => a.User)
            .Include(u => u.Images.Where(i => !i.IsDeleted))
            .Include(u => u.Rooms.Where(r => !r.IsDeleted))
            .Include(u => u.UnitAmenities)
                .ThenInclude(ua => ua.Amenity)
            .Include(u => u.Reviews)
            .Include(u => u.Bookings)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

        if (unit == null)
            return Result.Failure<UnitDetailsResponse>(
                new Error("NotFound", "Unit not found", 404));

        return Result.Success(MapToDetailsResponse(unit));
    }

    public async Task<Result<IEnumerable<UnitComprehensiveResponse>>> GetAllComprehensiveAsync(UnitFilter filter)
    {
        var query = _context.Units
            .Include(u => u.City)
            .Include(u => u.UnitType)
            .Include(u => u.CancellationPolicy)
            .Include(u => u.Admins.Where(a => a.IsActive))
                .ThenInclude(a => a.User)

            // Unit Images
            .Include(u => u.Images.Where(i => !i.IsDeleted))

            // Unit Amenities
            .Include(u => u.UnitAmenities)
                .ThenInclude(ua => ua.Amenity)

            // SubUnits with full details
            .Include(u => u.Rooms.Where(r => !r.IsDeleted))
                .ThenInclude(r => r.SubUnitImages.Where(i => !i.IsDeleted))
            .Include(u => u.Rooms.Where(r => !r.IsDeleted))
                .ThenInclude(r => r.SubUnitAmenities)
                .ThenInclude(sa => sa.Amenity)
            .Include(u => u.Rooms.Where(r => !r.IsDeleted))
                .ThenInclude(r => r.SubUnitAvailabilities)

            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(u => u.Name.Contains(filter.Name));

        if (filter.CityId.HasValue)
            query = query.Where(u => u.CityId == filter.CityId.Value);

        if (filter.UnitTypeId.HasValue)
            query = query.Where(u => u.UnitTypeId == filter.UnitTypeId.Value);

        if (filter.IsActive.HasValue)
            query = query.Where(u => u.IsActive == filter.IsActive.Value);

        if (filter.IsVerified.HasValue)
            query = query.Where(u => u.IsVerified == filter.IsVerified.Value);

        if (filter.IsDeleted.HasValue)
            query = query.Where(u => u.IsDeleted == filter.IsDeleted.Value);
        else
            query = query.Where(u => !u.IsDeleted);

        if (filter.MinPrice.HasValue)
            query = query.Where(u => u.BasePrice >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(u => u.BasePrice <= filter.MaxPrice.Value);

        if (filter.MinRating.HasValue)
            query = query.Where(u => u.AverageRating >= filter.MinRating.Value);

        if (filter.MinGuests.HasValue)
            query = query.Where(u => u.MaxGuests >= filter.MinGuests.Value);

        // Sorting
        query = ApplySorting(query, filter.SortBy, filter.SortDirection);

        // Pagination
        //var skip = (filter.Page - 1) * filter.PageSize;

        var units = await query
          //  .Skip(skip)
            //.Take(filter.PageSize)
            .AsNoTracking()
            .ToListAsync();

        // Get general policies for each unit
        var unitIds = units.Select(u => u.Id).ToList();
        var generalPolicies = await _context.GeneralPolicies
            .Include(p => p.CancellationPolicy)
            .Where(p => unitIds.Contains(p.UnitId.Value) && p.IsActive)
            .AsNoTracking()
            .ToListAsync();

        var responses = units.Select(u => MapToComprehensiveResponse(u,
            generalPolicies.Where(p => p.UnitId == u.Id).ToList())).ToList();

        return Result.Success<IEnumerable<UnitComprehensiveResponse>>(responses);
    }

    public async Task<Result<IEnumerable<UnitComprehensiveResponse>>> GetAllByUserDepartmentAsync(
    string userId,
    UnitFilter filter)
    {
        // Find user's department
        var userDepartment = await _context.Set<DepartmentAdmin>()
            .Where(da => da.UserId == userId && da.IsActive)
            .Select(da => da.CityId)
            .FirstOrDefaultAsync();

        if (userDepartment == 0)
            return Result.Failure<IEnumerable<UnitComprehensiveResponse>>(
                new Error("NoDepartment", "User is not assigned to any department", 404));

        // Create a new filter with CityId set, preserving other properties
        var filterWithCity = new UnitFilter
        {
            Name = filter.Name,
            CityId = userDepartment,
            UnitTypeId = filter.UnitTypeId,
            IsActive = filter.IsActive,
            IsVerified = filter.IsVerified,
            IsDeleted = filter.IsDeleted,
            MinPrice = filter.MinPrice,
            MaxPrice = filter.MaxPrice,
            MinRating = filter.MinRating,
            MinGuests = filter.MinGuests,
            SortBy = filter.SortBy,
            SortDirection = filter.SortDirection,
            Page = filter.Page,
            PageSize = filter.PageSize
        };

        return await GetAllComprehensiveAsync(filterWithCity);
    }

    /// <summary>
    /// Get all units managed by a specific hotel admin
    /// </summary>
    public async Task<Result<IEnumerable<UnitComprehensiveResponse>>> GetAllByHotelAdminAsync(
        string userId,
        UnitFilter filter)
    {
        var query = _context.Units
            .Include(u => u.City)
            .Include(u => u.UnitType)
            .Include(u => u.CancellationPolicy)
            .Include(u => u.Admins.Where(a => a.IsActive))
                .ThenInclude(a => a.User)
            .Include(u => u.Images.Where(i => !i.IsDeleted))
            .Include(u => u.UnitAmenities)
                .ThenInclude(ua => ua.Amenity)
            .Include(u => u.Rooms.Where(r => !r.IsDeleted))
                .ThenInclude(r => r.SubUnitImages.Where(i => !i.IsDeleted))
            .Include(u => u.Rooms.Where(r => !r.IsDeleted))
                .ThenInclude(r => r.SubUnitAmenities)
                .ThenInclude(sa => sa.Amenity)
            .Include(u => u.Rooms.Where(r => !r.IsDeleted))
                .ThenInclude(r => r.SubUnitAvailabilities)
            .Where(u => u.Admins.Any(a => a.UserId == userId && a.IsActive))
            .AsQueryable();

        // Apply remaining filters
        if (!string.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(u => u.Name.Contains(filter.Name));

        if (filter.IsActive.HasValue)
            query = query.Where(u => u.IsActive == filter.IsActive.Value);

        if (filter.IsDeleted.HasValue)
            query = query.Where(u => u.IsDeleted == filter.IsDeleted.Value);
        else
            query = query.Where(u => !u.IsDeleted);

        query = ApplySorting(query, filter.SortBy, filter.SortDirection);

        var skip = (filter.Page - 1) * filter.PageSize;
        var units = await query
            .Skip(skip)
            .Take(filter.PageSize)
            .AsNoTracking()
            .ToListAsync();

        var unitIds = units.Select(u => u.Id).ToList();
        var generalPolicies = await _context.GeneralPolicies
            .Include(p => p.CancellationPolicy)
            .Where(p => unitIds.Contains(p.UnitId.Value) && p.IsActive)
            .AsNoTracking()
            .ToListAsync();

        var responses = units.Select(u => MapToComprehensiveResponse(u,
            generalPolicies.Where(p => p.UnitId == u.Id).ToList())).ToList();

        return Result.Success<IEnumerable<UnitComprehensiveResponse>>(responses);
    }


    private static UnitComprehensiveResponse MapToComprehensiveResponse(
    Domain.Entities.Unit unit,
    List<GeneralPolicy> generalPolicies)
    {
        // Map admins
        var admins = unit.Admins?.Select(a => new UnitAdminInfo(
            a.UserId,
            a.User?.FullName ?? "N/A",
            a.User?.Email ?? "N/A",
            a.User?.PhoneNumber,
            a.IsActive,
            a.AssignedAt
        )).ToList() ?? new List<UnitAdminInfo>();

        // Map images
        var images = unit.Images?.Where(i => !i.IsDeleted)
            .Select(i => new UnitImageDetail
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                ThumbnailUrl = i.ThumbnailUrl,
                MediumUrl = i.MediumUrl,
                IsPrimary = i.IsPrimary,
                DisplayOrder = i.DisplayOrder,
                Caption = i.Caption,
                AltText = i.AltText,
                ImageType = i.ImageType.ToString(),
                Width = i.Width,
                Height = i.Height,
                FileSizeBytes = i.FileSizeBytes
            })
            .OrderBy(i => i.DisplayOrder)
            .ToList() ?? new List<UnitImageDetail>();

        // Map amenities
        var amenities = unit.UnitAmenities?.Select(ua => new AmenityDetail
        {
            Id = ua.AmenityId,
            Name = ua.Amenity?.Name.ToString() ?? "",
            Description = ua.Amenity?.Description,
            Category = ua.Amenity?.Category.ToString() ?? "",
            IsAvailable = ua.IsAvailable
        }).ToList() ?? new List<AmenityDetail>();

        // Map general policies
        var policies = generalPolicies.Select(p => new GeneralPolicyDetail
        {
            Id = p.Id,
            Title = p.Title,
            Description = p.Description,
            PolicyType = p.PolicyType.ToString(),
            PolicyCategory = p.PolicyCategory?.ToString(),
            CustomPolicyName = p.CustomPolicyName,
            IsMandatory = p.IsMandatory,
            IsHighlighted = p.IsHighlighted,
            CancellationPolicyName = p.CancellationPolicy?.Name
        }).ToList();

        // Map cancellation policy
        CancellationPolicyDetail? cancellationPolicy = null;
        if (unit.CancellationPolicy != null)
        {
            cancellationPolicy = new CancellationPolicyDetail
            {
                Id = unit.CancellationPolicy.Id,
                Name = unit.CancellationPolicy.Name,
                Description = unit.CancellationPolicy.Description,
                FullRefundDays = unit.CancellationPolicy.FullRefundDays,
                PartialRefundDays = unit.CancellationPolicy.PartialRefundDays,
                PartialRefundPercentage = unit.CancellationPolicy.PartialRefundPercentage
            };
        }

        // Map subunits
        var subUnits = unit.Rooms?.Where(r => !r.IsDeleted).Select(r => new SubUnitComprehensiveDetail
        {
            Id = r.Id,
            RoomNumber = r.RoomNumber,
            TypeId = r.SubUnitTypeId,
            PricePerNight = r.PricePerNight,
            MaxOccupancy = r.MaxOccupancy,
            Bedrooms = r.Bedrooms,
            Bathrooms = r.Bathrooms,
            Size = r.Size,
            Description = r.Description,
            IsAvailable = r.IsAvailable,

            Images = r.SubUnitImages?.Where(i => !i.IsDeleted)
                .Select(i => new SubUnitImageDetail
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    ThumbnailUrl = i.ThumbnailUrl,
                    SmallUrl = i.ImageUrl,
                    MediumUrl = i.MediumUrl,
                    IsPrimary = i.IsPrimary,
                    DisplayOrder = i.DisplayOrder,
                    Caption = i.Caption,
                    ImageType = i.ImageType.ToString()
                })
                .OrderBy(i => i.DisplayOrder)
                .ToList() ?? new List<SubUnitImageDetail>(),

            Amenities = r.SubUnitAmenities?.Select(sa => new AmenityDetail
            {
                Id = sa.AmenityId,
                Name = sa.Amenity?.Name.ToString() ?? "",
                Description = sa.Amenity?.Description,
                Category = sa.Amenity?.Category.ToString() ?? "",
                IsAvailable = sa.IsAvailable
            }).ToList() ?? new List<AmenityDetail>(),

            CurrentAvailability = r.SubUnitAvailabilities?
                .Where(a => a.StartDate <= DateTime.UtcNow && a.EndDate >= DateTime.UtcNow)
                .Select(a => new AvailabilityInfo
                {
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    IsAvailable = a.IsAvailable,
                    Reason = a.Reason?.ToString(),
                    SpecialPrice = a.SpecialPrice,
                    WeekendPrice = a.WeekendPrice
                })
                .FirstOrDefault()

        }).ToList() ?? new List<SubUnitComprehensiveDetail>();

        return new UnitComprehensiveResponse
        {
            Id = unit.Id,
            Name = unit.Name,
            Description = unit.Description,
            Address = unit.Address,
            Latitude = unit.Latitude,
            Longitude = unit.Longitude,

            CityId = unit.CityId,
            CityName = unit.City?.Name ?? "",
            CityCountry = unit.City?.Country ?? "",

            UnitTypeId = unit.UnitTypeId,
            UnitTypeName = unit.UnitType?.Name ?? "",
            UnitTypeDescription = unit.UnitType?.Description,

            BasePrice = unit.BasePrice,
            MaxGuests = unit.MaxGuests,
            Bedrooms = unit.Bedrooms,
            Bathrooms = unit.Bathrooms,

            IsActive = unit.IsActive,
            IsVerified = unit.IsVerified,

            IsFeatuered = unit.IsFeatured,

            AverageRating = unit.AverageRating,
            TotalReviews = unit.TotalReviews,

            Admins = admins,
            Images = images,
            Amenities = amenities,
            GeneralPolicies = policies,
            CancellationPolicy = cancellationPolicy,
            SubUnits = subUnits,

            TotalSubUnits = subUnits.Count,
            AvailableSubUnits = subUnits.Count(s => s.IsAvailable),

            CreatedAt = unit.CreatedAt,
            UpdatedAt = unit.UpdatedAt
        };
    }

    public async Task<Result<UnitResponse>> UpdateAsync(int unitId, UpdateUnitRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var unit = await _context.Units
                .Include(u => u.City)
                .Include(u => u.UnitType)
                .Include(u => u.CancellationPolicy)
                .Include(u => u.Admins)
                .Include(u => u.Images.Where(i => !i.IsDeleted))
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

            if (unit == null)
                return Result.Failure<UnitResponse>(
                    new Error("NotFound", "Unit not found", 404));

            // Update properties
            if (request.Name != null) unit.Name = request.Name;
            if (request.Description != null) unit.Description = request.Description;
            if (request.Address != null) unit.Address = request.Address;
            if (request.Latitude.HasValue) unit.Latitude = request.Latitude.Value;
            if (request.Longitude.HasValue) unit.Longitude = request.Longitude.Value;
            if (request.BasePrice.HasValue) unit.BasePrice = request.BasePrice.Value;
            if (request.MaxGuests.HasValue) unit.MaxGuests = request.MaxGuests.Value;
            if (request.Bedrooms.HasValue) unit.Bedrooms = request.Bedrooms.Value;
            if (request.Bathrooms.HasValue) unit.Bathrooms = request.Bathrooms.Value;
            if (request.IsActive.HasValue) unit.IsActive = request.IsActive.Value;

            // Update city/type if needed
            if (request.CityId.HasValue && request.CityId.Value != unit.CityId)
            {
                var cityExists = await _context.Departments
                    .AnyAsync(d => d.Id == request.CityId.Value && !d.IsDeleted);
                if (!cityExists)
                    return Result.Failure<UnitResponse>(
                        new Error("InvalidCity", "City not found", 404));

                unit.CityId = request.CityId.Value;
            }

            if (request.UnitTypeId.HasValue && request.UnitTypeId.Value != unit.UnitTypeId)
            {
                var typeExists = await _context.UnitTypes
                    .AnyAsync(t => t.Id == request.UnitTypeId.Value && t.IsActive);
                if (!typeExists)
                    return Result.Failure<UnitResponse>(
                        new Error("InvalidType", "Unit type not found", 404));

                unit.UnitTypeId = request.UnitTypeId.Value;
            }

            unit.UpdatedAt = DateTime.UtcNow.AddHours(3);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Result.Success(MapToResponse(unit));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating unit {UnitId}", unitId);
            return Result.Failure<UnitResponse>(
                new Error("UpdateFailed", "Failed to update unit", 500));
        }
    }

    public async Task<Result> DeleteAsync(int unitId, bool softDelete = true)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var unit = await _context.Units
                .Include(u => u.Rooms)
                .Include(u => u.Bookings)
                .Include(u => u.Admins)
                .FirstOrDefaultAsync(u => u.Id == unitId);

            if (unit == null)
                return Result.Failure(
                    new Error("NotFound", "Unit not found", 404));

            // Check for active bookings
            var hasActiveBookings = unit.Bookings.Any(b =>
                b.Status == BookingStatus.Confirmed ||
                b.Status == BookingStatus.CheckedIn);

            if (hasActiveBookings)
                return Result.Failure(
                    new Error("HasActiveBookings",
                        "Cannot delete unit with active bookings", 400));

            if (softDelete)
            {
                unit.IsDeleted = true;
                unit.DeletedAt = DateTime.UtcNow.AddHours(3);
                unit.IsActive = false;

                // Deactivate all admins
                foreach (var admin in unit.Admins)
                {
                    admin.IsActive = false;
                }
            }
            else
            {
                _context.Units.Remove(unit);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting unit {UnitId}", unitId);
            return Result.Failure(
                new Error("DeleteFailed", "Failed to delete unit", 500));
        }
    }

    public async Task<Result> RestoreAsync(int unitId)
    {
        var unit = await _context.Units
            .FirstOrDefaultAsync(u => u.Id == unitId && u.IsDeleted);

        if (unit == null)
            return Result.Failure(
                new Error("NotFound", "Deleted unit not found", 404));

        unit.IsDeleted = false;
        unit.DeletedAt = null;
        unit.IsActive = true;

        await _context.SaveChangesAsync();
        return Result.Success();
    }

    #endregion

    #region STATUS MANAGEMENT

    public async Task<Result> ToggleActiveAsync(int unitId)
    {
        var unit = await _context.Units
            .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

        if (unit == null)
            return Result.Failure(
                new Error("NotFound", "Unit not found", 404));

        unit.IsActive = !unit.IsActive;
        unit.UpdatedAt = DateTime.UtcNow.AddHours(3);

        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> VerifyUnitAsync(int unitId, string adminUserId)
    {
        var unit = await _context.Units
            .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

        if (unit == null)
            return Result.Failure(
                new Error("NotFound", "Unit not found", 404));

        unit.IsVerified = true;
        unit.UpdatedAt = DateTime.UtcNow.AddHours(3);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Unit {UnitId} verified by admin {AdminId}",
            unitId, adminUserId);

        return Result.Success();
    }

    public async Task<Result> UnverifyUnitAsync(int unitId, string adminUserId, string reason)
    {
        var unit = await _context.Units
            .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

        if (unit == null)
            return Result.Failure(
                new Error("NotFound", "Unit not found", 404));

        unit.IsVerified = false;
        unit.IsActive = false;
        unit.UpdatedAt = DateTime.UtcNow.AddHours(3);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Unit {UnitId} unverified by admin {AdminId}. Reason: {Reason}",
            unitId, adminUserId, reason);

        return Result.Success();
    }

    #endregion

    #region IMAGE MANAGEMENT

    // Application/Service/Unit/UnitService.cs
    public async Task<Result<List<UnitImageResponse>>> UploadImagesAsync(
    int unitId,
    List<IFormFile> images,
    string userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var unit = await _context.Units
                .Include(u => u.Images.Where(i => !i.IsDeleted))
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

            if (unit == null)
                return Result.Failure<List<UnitImageResponse>>(
                    new Error("NotFound", "Unit not found", 404));

            // Calculate max order before upload
            var maxOrder = unit.Images.Any()
                ? unit.Images.Max(i => i.DisplayOrder)
                : 0;

            // Upload to S3
            var uploadResult = await _s3Service.UploadUnitImagesAsync(images, unitId, userId);
            if (!uploadResult.IsSuccess)
                return Result.Failure<List<UnitImageResponse>>(uploadResult.Error);

            var s3Keys = uploadResult.Value;
            var unitImages = new List<Domain.Entities.UnitImage>();

            // Create all entities first
            foreach (var (s3Key, index) in s3Keys.Select((k, i) => (k, i)))
            {
                var unitImage = new Domain.Entities.UnitImage
                {
                    UnitId = unitId,
                    ImageUrl = _s3Service.GetCloudFrontUrl(s3Key),
                    S3Key = s3Key,
                    S3Bucket = "huzjjy-bucket",
                    DisplayOrder = maxOrder + index + 1,
                    IsPrimary = !unit.Images.Any() && index == 0,
                    UploadedByUserId = userId,
                    UploadedAt = DateTime.UtcNow.AddHours(3),
                    ProcessingStatus = ImageProcessingStatus.Completed
                };

                unitImages.Add(unitImage);
            }

            // Add all entities to context
            await _context.Set<Domain.Entities.UnitImage>().AddRangeAsync(unitImages);

            // Save once
            await _context.SaveChangesAsync();

            // Commit transaction
            await transaction.CommitAsync();

            // Map to response AFTER successful commit
            var imageResponses = unitImages.Select(MapToImageResponse).ToList();

            return Result.Success(imageResponses);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error uploading images for unit {UnitId}", unitId);
            return Result.Failure<List<UnitImageResponse>>(
                new Error("UploadFailed", "Failed to upload images. Error: " + ex.Message, 500));
        }
    }
    private string GetThumbnailKey(string originalKey)
    {
        var directory = Path.GetDirectoryName(originalKey)?.Replace("\\", "/");
        var filename = Path.GetFileNameWithoutExtension(originalKey);
        var extension = Path.GetExtension(originalKey);
        return $"{directory}/{filename}_thumbnail{extension}";
    }

    private string GetMediumKey(string originalKey)
    {
        var directory = Path.GetDirectoryName(originalKey)?.Replace("\\", "/");
        var filename = Path.GetFileNameWithoutExtension(originalKey);
        var extension = Path.GetExtension(originalKey);
        return $"{directory}/{filename}_medium{extension}";
    }

    // Application/Service/Unit/UnitService.cs
    public async Task<Result> DeleteImageAsync(int unitId, int imageId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var image = await _context.Set<Domain.Entities.UnitImage>()
                .FirstOrDefaultAsync(i => i.Id == imageId &&
                                         i.UnitId == unitId &&
                                         !i.IsDeleted);

            if (image == null)
                return Result.Failure(
                    new Error("NotFound", "Image not found", 404));

            // Soft delete in DB
            image.IsDeleted = true;
            image.DeletedAt = DateTime.UtcNow.AddHours(3);

            // If it was primary, set another as primary
            if (image.IsPrimary)
            {
                var nextPrimary = await _context.Set<Domain.Entities.UnitImage>()
                    .Where(i => i.UnitId == unitId &&
                               i.Id != imageId &&
                               !i.IsDeleted)
                    .OrderBy(i => i.DisplayOrder)
                    .FirstOrDefaultAsync();

                if (nextPrimary != null)
                {
                    nextPrimary.IsPrimary = true;
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // ACTUALLY DELETE from S3 (all three sizes)
            var deleteResult = await _s3Service.DeleteImagesAsync(new List<string> { image.S3Key });

            if (!deleteResult.IsSuccess)
            {
                _logger.LogWarning(
                    "Image {ImageId} marked as deleted in DB but failed to delete from S3: {Error}",
                    imageId, deleteResult.Error.Description);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting image {ImageId}", imageId);
            return Result.Failure(
                new Error("DeleteFailed", "Failed to delete image", 500));
        }
    }
    public async Task<Result> SetPrimaryImageAsync(int unitId, int imageId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var images = await _context.Set<Domain.Entities.UnitImage>()
                .Where(i => i.UnitId == unitId && !i.IsDeleted)
                .ToListAsync();

            var targetImage = images.FirstOrDefault(i => i.Id == imageId);
            if (targetImage == null)
                return Result.Failure(
                    new Error("NotFound", "Image not found", 404));

            // Remove primary from all others
            foreach (var img in images)
            {
                img.IsPrimary = img.Id == imageId;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error setting primary image");
            return Result.Failure(
                new Error("UpdateFailed", "Failed to set primary image", 500));
        }
    }

    public async Task<Result> ReorderImagesAsync(int unitId, List<int> imageIds)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var images = await _context.Set<Domain.Entities.UnitImage>()
                .Where(i => i.UnitId == unitId && !i.IsDeleted)
                .ToListAsync();

            foreach (var (imageId, order) in imageIds.Select((id, idx) => (id, idx)))
            {
                var image = images.FirstOrDefault(i => i.Id == imageId);
                if (image != null)
                {
                    image.DisplayOrder = order;
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error reordering images");
            return Result.Failure(
                new Error("UpdateFailed", "Failed to reorder images", 500));
        }
    }

    #endregion

    #region ADMIN MANAGEMENT

    public async Task<Result> AttachAdminAsync(int unitId, string userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var unit = await _context.Units
                .Include(u => u.Admins)
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

            if (unit == null)
                return Result.Failure(
                    new Error("NotFound", "Unit not found", 404));

            // Check if already attached
            var existing = unit.Admins.FirstOrDefault(a => a.UserId == userId);
            if (existing != null && existing.IsActive)
                return Result.Failure(
                    new Error("AlreadyAttached", "Admin already attached to this unit", 400));

            if (existing != null)
            {
                // Reactivate
                existing.IsActive = true;
                existing.AssignedAt = DateTime.UtcNow.AddHours(3);
            }
            else
            {
                // Create new
                var newAdmin = new UniteAdmin
                {
                    UnitId = unitId,
                    UserId = userId,
                    IsActive = true,
                    AssignedAt = DateTime.UtcNow.AddHours(3)
                };
                await _context.Set<UniteAdmin>().AddAsync(newAdmin);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error attaching admin to unit");
            return Result.Failure(
                new Error("AttachFailed", "Failed to attach admin", 500));
        }
    }

    public async Task<Result> RemoveAdminAsync(int unitId, string userId)
    {
        var admin = await _context.Set<UniteAdmin>()
            .FirstOrDefaultAsync(a => a.UnitId == unitId && a.UserId == userId);

        if (admin == null)
            return Result.Failure(
                new Error("NotFound", "Admin not found", 404));

        _context.Set<UniteAdmin>().Remove(admin);
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<UnitAdminsResponse>> GetUnitAdminsAsync(int unitId)
    {
        var unit = await _context.Units
            .Include(u => u.Admins.Where(a => a.IsActive))
                .ThenInclude(a => a.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

        if (unit == null)
            return Result.Failure<UnitAdminsResponse>(
                new Error("NotFound", "Unit not found", 404));

        var admins = unit.Admins.Select(a => new UnitAdminInfo(
            a.UserId,
            a.User?.FullName ?? "N/A",
            a.User?.Email ?? "N/A",
            a.User?.PhoneNumber,
            a.IsActive,
            a.AssignedAt
        )).ToList();

        var response = new UnitAdminsResponse(
            unit.Id,
            unit.Name,
            admins.Count,
            admins
        );

        return Result.Success(response);
    }

    public async Task<Result<IEnumerable<UnitResponse>>> GetAdminUnitsAsync(string userId)
    {
        var units = await _context.Units
            .Include(u => u.City)
            .Include(u => u.UnitType)
            .Include(u => u.Admins.Where(a => a.UserId == userId && a.IsActive))
            .Include(u => u.Images.Where(i => !i.IsDeleted && i.IsPrimary))
            .Where(u => !u.IsDeleted && u.Admins.Any(a => a.UserId == userId && a.IsActive))
            .AsNoTracking()
            .ToListAsync();

        var responses = units.Select(MapToResponse).ToList();
        return Result.Success<IEnumerable<UnitResponse>>(responses);
    }

    #endregion

    #region POLICY MANAGEMENT

    public async Task<Result> AttachCancellationPolicyAsync(int unitId, int policyId)
    {
        var unit = await _context.Units
            .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

        if (unit == null)
            return Result.Failure(
                new Error("NotFound", "Unit not found", 404));

        var policyExists = await _context.CancellationPolicies
            .AnyAsync(p => p.Id == policyId && p.IsActive);

        if (!policyExists)
            return Result.Failure(
                new Error("PolicyNotFound", "Cancellation policy not found", 404));

        unit.CancellationPolicyId = policyId;
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> RemoveCancellationPolicyAsync(int unitId)
    {
        var unit = await _context.Units
            .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

        if (unit == null)
            return Result.Failure(
                new Error("NotFound", "Unit not found", 404));

        unit.CancellationPolicyId = null;
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> AttachGeneralPolicyAsync(int unitId, int policyId)
    {
        var unit = await _context.Units
            .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

        if (unit == null)
            return Result.Failure(
                new Error("NotFound", "Unit not found", 404));

        var policy = await _context.GeneralPolicies
            .FirstOrDefaultAsync(p => p.Id == policyId && p.IsActive);

        if (policy == null)
            return Result.Failure(
                new Error("PolicyNotFound", "General policy not found", 404));

        // Attach to unit
        policy.UnitId = unitId;
        policy.SubUnitId = null;

        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> RemoveGeneralPolicyAsync(int unitId, int policyId)
    {
        var policy = await _context.GeneralPolicies
            .FirstOrDefaultAsync(p => p.Id == policyId && p.UnitId == unitId);

        if (policy == null)
            return Result.Failure(
                new Error("NotFound", "Policy not found", 404));

        policy.UnitId = null;
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<IEnumerable<GeneralPolicyResponse>>> GetUnitPoliciesAsync(int unitId)
    {
        var policies = await _context.GeneralPolicies
            .Include(p => p.CancellationPolicy)
            .Where(p => p.UnitId == unitId && p.IsActive)
            .AsNoTracking()
            .ToListAsync();

        var responses = policies.Select(MapToPolicyResponse).ToList();
        return Result.Success<IEnumerable<GeneralPolicyResponse>>(responses);
    }

    #endregion

    #region STATISTICS & SEARCH

    public async Task<Result<UnitStatisticsResponse>> GetStatisticsAsync(int unitId)
    {
        var unit = await _context.Units
            .Include(u => u.Bookings)
            .Include(u => u.Reviews)
            .Include(u => u.Rooms)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

        if (unit == null)
            return Result.Failure<UnitStatisticsResponse>(
                new Error("NotFound", "Unit not found", 404));

        var totalBookings = unit.Bookings.Count;
        var completedBookings = unit.Bookings.Count(b => b.Status == BookingStatus.Completed);
        var totalRevenue = unit.Bookings
            .Where(b => b.Status == BookingStatus.Completed)
            .Sum(b => b.TotalPrice);

        var occupancyRate = totalBookings > 0
            ? (decimal)completedBookings / totalBookings * 100
            : 0;

        var response = new UnitStatisticsResponse(
            unitId,
            unit.Name,
            totalBookings,
            completedBookings,
            totalRevenue,
            unit.AverageRating,
            unit.TotalReviews,
            unit.Rooms.Count(r => !r.IsDeleted),
            occupancyRate
        );

        return Result.Success(response);
    }

    public async Task<Result<IEnumerable<UnitResponse>>> SearchAsync(string keyword)
    {
        keyword = keyword.ToLower();

        var units = await _context.Units
            .Include(u => u.City)
            .Include(u => u.UnitType)
            .Include(u => u.Images.Where(i => !i.IsDeleted && i.IsPrimary))
            .Where(u => !u.IsDeleted &&
                       (u.Name.ToLower().Contains(keyword) ||
                        u.Description.ToLower().Contains(keyword) ||
                        u.Address.ToLower().Contains(keyword)))
            .AsNoTracking()
            .ToListAsync();

        var responses = units.Select(MapToResponse).ToList();
        return Result.Success<IEnumerable<UnitResponse>>(responses);
    }

    public async Task<Result<IEnumerable<UnitResponse>>> GetByDepartmentAsync(int departmentId)
    {
        var units = await _context.Units
            .Include(u => u.City)
            .Include(u => u.UnitType)
            .Include(u => u.Images.Where(i => !i.IsDeleted && i.IsPrimary))
            .Where(u => u.CityId == departmentId && !u.IsDeleted && u.IsActive)
            .AsNoTracking()
            .ToListAsync();

        var responses = units.Select(MapToResponse).ToList();
        return Result.Success<IEnumerable<UnitResponse>>(responses);
    }

    public async Task<Result<IEnumerable<UnitResponse>>> GetByTypeAsync(int typeId)
    {
        var units = await _context.Units
            .Include(u => u.City)
            .Include(u => u.UnitType)
            .Include(u => u.Images.Where(i => !i.IsDeleted && i.IsPrimary))
            .Where(u => u.UnitTypeId == typeId && !u.IsDeleted && u.IsActive)
            .AsNoTracking()
            .ToListAsync();

        var responses = units.Select(MapToResponse).ToList();
        return Result.Success<IEnumerable<UnitResponse>>(responses);
    }

    public async Task<Result<IEnumerable<UnitResponse>>> GetNearbyAsync(
        decimal latitude,
        decimal longitude,
        int radiusKm)
    {
        // Simplified distance calculation (Haversine formula)
        var units = await _context.Units
            .Include(u => u.City)
            .Include(u => u.UnitType)
            .Include(u => u.Images.Where(i => !i.IsDeleted && i.IsPrimary))
            .Where(u => !u.IsDeleted && u.IsActive)
            .AsNoTracking()
            .ToListAsync();

        var nearby = units.Where(u =>
        {
            var distance = CalculateDistance(latitude, longitude, u.Latitude, u.Longitude);
            return distance <= radiusKm;
        }).ToList();

        var responses = nearby.Select(MapToResponse).ToList();
        return Result.Success<IEnumerable<UnitResponse>>(responses);
    }

    #endregion

    #region VALIDATION

    public async Task<Result<bool>> CanUserManageUnitAsync(string userId, int unitId)
    {
        var isAdmin = await _context.Set<UniteAdmin>()
            .AnyAsync(a => a.UnitId == unitId && a.UserId == userId && a.IsActive);

        return Result.Success(isAdmin);
    }

    public async Task<Result<bool>> IsUnitAvailableAsync(
        int unitId,
        DateTime checkIn,
        DateTime checkOut)
    {
        var hasAvailableRooms = await _context.SubUnits
            .Where(r => r.UnitId == unitId && !r.IsDeleted && r.IsAvailable)
            .AnyAsync();

        if (!hasAvailableRooms)
            return Result.Success(false);

        // Check if any rooms are booked for these dates
        var hasConflict = await _context.BookingRooms
            .Include(br => br.Booking)
            .AnyAsync(br => br.Room.UnitId == unitId &&
                           br.Booking.CheckInDate < checkOut &&
                           br.Booking.CheckOutDate > checkIn &&
                           br.Booking.Status != BookingStatus.Cancelled);

        return Result.Success(!hasConflict);
    }

    #endregion

    #region HELPER METHODS

    private static UnitResponse MapToResponse(Domain.Entities.Unit unit)
    {
        return new UnitResponse
        {
            Id = unit.Id,
            Name = unit.Name,
            Description = unit.Description,
            Address = unit.Address,
            Latitude = unit.Latitude,
            Longitude = unit.Longitude,
            CityId = unit.CityId,
            CityName = unit.City?.Name ?? "",
            UnitTypeId = unit.UnitTypeId,
            UnitTypeName = unit.UnitType?.Name ?? "",
            BasePrice = unit.BasePrice,
            MaxGuests = unit.MaxGuests,
            Bedrooms = unit.Bedrooms,
            Bathrooms = unit.Bathrooms,
            IsActive = unit.IsActive,
            IsVerified = unit.IsVerified,
            AverageRating = unit.AverageRating,
            TotalReviews = unit.TotalReviews,
            PrimaryImageUrl = unit.Images?.FirstOrDefault(i => i.IsPrimary)?.ImageUrl,
            AdminCount = unit.Admins?.Count(a => a.IsActive) ?? 0,
            CreatedAt = unit.CreatedAt,
            UpdatedAt = unit.UpdatedAt
        };
    }

    private static UnitDetailsResponse MapToDetailsResponse(Domain.Entities.Unit unit)
    {
        var admins = unit.Admins?.Select(a => new UnitAdminInfo(
            a.UserId,
            a.User?.FullName ?? "N/A",
            a.User?.Email ?? "N/A",
            a.User?.PhoneNumber,
            a.IsActive,
            a.AssignedAt
        )).ToList() ?? new List<UnitAdminInfo>();

        var images = unit.Images?.Where(i => !i.IsDeleted)
            .Select(i => new UnitImageResponse
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                ThumbnailUrl = i.ThumbnailUrl,
                IsPrimary = i.IsPrimary,
                DisplayOrder = i.DisplayOrder,
                Caption = i.Caption
            })
            .OrderBy(i => i.DisplayOrder)
            .ToList() ?? new List<UnitImageResponse>();

        var amenities = unit.UnitAmenities?.Select(ua => new AmenityInfo(
            ua.AmenityId,
            ua.Amenity?.Name.ToString() ?? "",
            ua.Amenity?.Category.ToString() ?? "",
            ua.IsAvailable
        )).ToList() ?? new List<AmenityInfo>();

        var rooms = unit.Rooms?.Where(r => !r.IsDeleted).Select(r => new SubUnitSummary(
            r.Id,
            r.RoomNumber,
            r.SubUnitTypeId,
            r.PricePerNight,
            r.MaxOccupancy,
            r.IsAvailable
        )).ToList() ?? new List<SubUnitSummary>();

        return new UnitDetailsResponse
        {
            Id = unit.Id,
            Name = unit.Name,
            Description = unit.Description,
            Address = unit.Address,
            Latitude = unit.Latitude,
            Longitude = unit.Longitude,
            CityId = unit.CityId,
            CityName = unit.City?.Name ?? "",
            UnitTypeId = unit.UnitTypeId,
            UnitTypeName = unit.UnitType?.Name ?? "",
            BasePrice = unit.BasePrice,
            MaxGuests = unit.MaxGuests,
            Bedrooms = unit.Bedrooms,
            Bathrooms = unit.Bathrooms,
            IsActive = unit.IsActive,
            IsVerified = unit.IsVerified,
            AverageRating = unit.AverageRating,
            TotalReviews = unit.TotalReviews,
            CancellationPolicyId = unit.CancellationPolicyId,
            CancellationPolicyName = unit.CancellationPolicy?.Name,
            Admins = admins,
            Images = images,
            Amenities = amenities,
            Rooms = rooms,
            CreatedAt = unit.CreatedAt,
            UpdatedAt = unit.UpdatedAt
        };
    }

    private static UnitImageResponse MapToImageResponse(Domain.Entities.UnitImage image)
    {
        return new UnitImageResponse
        {
            Id = image.Id,
            ImageUrl = image.ImageUrl,
            ThumbnailUrl = image.ThumbnailUrl,
            IsPrimary = image.IsPrimary,
            DisplayOrder = image.DisplayOrder,
            Caption = image.Caption
        };
    }

    private static GeneralPolicyResponse MapToPolicyResponse(GeneralPolicy policy)
    {
        return new GeneralPolicyResponse(
            policy.Id,
            policy.Title,
            policy.Description,
            policy.PolicyType,
            policy.PolicyCategory,
            policy.CustomPolicyName,
            policy.CancellationPolicyId,
            policy.CancellationPolicy?.Name,
            policy.IsMandatory,
            policy.IsHighlighted,
            policy.IsActive,
            policy.UnitId,
            policy.Unit?.Name,
            policy.SubUnitId,
            policy.SubUnit?.RoomNumber,
            "Unit"
        );
    }

    private static IQueryable<Domain.Entities.Unit> ApplySorting(
        IQueryable<Domain.Entities.Unit> query,
        string? sortBy,
        string? sortDirection)
    {
        var descending = sortDirection?.ToUpper() == "DESC";

        return sortBy switch
        {
            "Name" => descending ? query.OrderByDescending(u => u.Name) : query.OrderBy(u => u.Name),
            "Price" => descending ? query.OrderByDescending(u => u.BasePrice) : query.OrderBy(u => u.BasePrice),
            "Rating" => descending ? query.OrderByDescending(u => u.AverageRating) : query.OrderBy(u => u.AverageRating),
            "CreatedAt" => descending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
            _ => query.OrderBy(u => u.Name)
        };
    }

    private static double CalculateDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
    {
        const double R = 6371; // Earth radius in km
        var dLat = ToRadians((double)(lat2 - lat1));
        var dLon = ToRadians((double)(lon2 - lon1));

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians((double)lat1)) * Math.Cos(ToRadians((double)lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;

    #endregion


    #region FEATURED UNITS

    public async Task<Result> ToggleFeaturedAsync(int unitId)
    {
        var unit = await _context.Units
            .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

        if (unit == null)
            return Result.Failure(
                new Error("NotFound", "Unit not found", 404));

        unit.IsFeatured = !unit.IsFeatured;
        unit.UpdatedAt = DateTime.UtcNow.AddHours(3);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Unit {UnitId} featured status toggled to {IsFeatured}",
            unitId, unit.IsFeatured);

        return Result.Success();
    }

    public async Task<Result> SetFeaturedAsync(int unitId, bool isFeatured)
    {
        var unit = await _context.Units
            .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

        if (unit == null)
            return Result.Failure(
                new Error("NotFound", "Unit not found", 404));

        unit.IsFeatured = isFeatured;
        unit.UpdatedAt = DateTime.UtcNow.AddHours(3);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Unit {UnitId} featured status set to {IsFeatured}",
            unitId, isFeatured);

        return Result.Success();
    }

    public async Task<Result<IEnumerable<UnitComprehensiveResponse>>> GetFeaturedUnitsAsync(
        UnitFilter filter)
    {
        var query = _context.Units
            .Include(u => u.City)
            .Include(u => u.UnitType)
            .Include(u => u.CancellationPolicy)
            .Include(u => u.Admins.Where(a => a.IsActive))
                .ThenInclude(a => a.User)
            .Include(u => u.Images.Where(i => !i.IsDeleted))
            .Include(u => u.UnitAmenities)
                .ThenInclude(ua => ua.Amenity)
            .Include(u => u.Rooms.Where(r => !r.IsDeleted))
                .ThenInclude(r => r.SubUnitImages.Where(i => !i.IsDeleted))
            .Include(u => u.Rooms.Where(r => !r.IsDeleted))
                .ThenInclude(r => r.SubUnitAmenities)
                .ThenInclude(sa => sa.Amenity)
            .Include(u => u.Rooms.Where(r => !r.IsDeleted))
                .ThenInclude(r => r.SubUnitAvailabilities)
            .Where(u => !u.IsDeleted && u.IsFeatured && u.IsActive && u.IsVerified)
            .AsQueryable();

        // Apply additional filters
        if (!string.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(u => u.Name.Contains(filter.Name));

        if (filter.CityId.HasValue)
            query = query.Where(u => u.CityId == filter.CityId.Value);

        if (filter.UnitTypeId.HasValue)
            query = query.Where(u => u.UnitTypeId == filter.UnitTypeId.Value);

        if (filter.MinPrice.HasValue)
            query = query.Where(u => u.BasePrice >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(u => u.BasePrice <= filter.MaxPrice.Value);

        if (filter.MinRating.HasValue)
            query = query.Where(u => u.AverageRating >= filter.MinRating.Value);

        // Sorting (default by rating for featured units)
        query = ApplySorting(query, filter.SortBy ?? "Rating", filter.SortDirection ?? "DESC");

        // Pagination
        var skip = (filter.Page - 1) * filter.PageSize;

        var units = await query
            .Skip(skip)
            .Take(filter.PageSize)
            .AsNoTracking()
            .ToListAsync();

        // Get general policies
        var unitIds = units.Select(u => u.Id).ToList();
        var generalPolicies = await _context.GeneralPolicies
            .Include(p => p.CancellationPolicy)
            .Where(p => unitIds.Contains(p.UnitId.Value) && p.IsActive)
            .AsNoTracking()
            .ToListAsync();

        var responses = units.Select(u => MapToComprehensiveResponse(u,
            generalPolicies.Where(p => p.UnitId == u.Id).ToList())).ToList();

        return Result.Success<IEnumerable<UnitComprehensiveResponse>>(responses);
    }

    #endregion
}