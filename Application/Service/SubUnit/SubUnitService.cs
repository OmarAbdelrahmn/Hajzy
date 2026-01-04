using Application.Abstraction;
using Application.Contracts.SubUnit;
using Application.Contracts.Unit;
using Application.Service.Availability;
using Application.Service.S3Image;
using Application.Service.SubUnitImage;
using Domain;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Service.SubUnit;

public class SubUnitService(
    ApplicationDbcontext context,
    ISubUnitImageService s3Service,
    ILogger<SubUnitService> logger,
    IAvailabilityService service) : ISubUnitService
{
    private readonly ApplicationDbcontext _context = context;
    private readonly ISubUnitImageService _s3Service = s3Service;
    private readonly ILogger<SubUnitService> _logger = logger;
    private readonly IAvailabilityService service = service;

    #region CRUD

    public async Task<Result<SubUnitResponse>> GetByIdAsync(int subUnitId)
    {
        var subUnit = await _context.SubUnits
            .Include(s => s.Unit)
            .Include(s => s.SubUnitImages.Where(i => !i.IsDeleted))
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

        if (subUnit == null)
            return Result.Failure<SubUnitResponse>(
                new Error("NotFound", "SubUnit not found", 404));

        return Result.Success(MapToResponse(subUnit));
    }

    public async Task<Result<SubUnitDetailsResponse>> GetDetailsAsync(int subUnitId)
    {
        var subUnit = await _context.SubUnits
            .Include(s => s.Unit)
            .Include(s => s.SubUnitImages.Where(i => !i.IsDeleted))
            .Include(s => s.SubUnitAmenities)
                .ThenInclude(sa => sa.Amenity)
            .Include(s => s.SubUnitAvailabilities)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

        if (subUnit == null)
            return Result.Failure<SubUnitDetailsResponse>(
                new Error("NotFound", "SubUnit not found", 404));

        return Result.Success(MapToDetailsResponse(subUnit));
    }

    public async Task<Result<IEnumerable<SubUnitResponse>>> GetByUnitAsync(int unitId)
    {
        var subUnits = await _context.SubUnits
            .Include(s => s.Unit)
            .Include(s => s.SubUnitImages.Where(i => !i.IsDeleted && i.IsPrimary))
            .Where(s => s.UnitId == unitId && !s.IsDeleted)
            .AsNoTracking()
            .ToListAsync();

        var responses = subUnits.Select(MapToResponse).ToList();
        return Result.Success<IEnumerable<SubUnitResponse>>(responses);
    }

    public async Task<Result<SubUnitResponse>> CreateAsync(CreateSubUnitRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Validate unit exists
            var unitExists = await _context.Units
                .AnyAsync(u => u.Id == request.UnitId && !u.IsDeleted);

            if (!unitExists)
                return Result.Failure<SubUnitResponse>(
                    new Error("UnitNotFound", "Unit not found", 404));

            // Check for duplicate room number in same unit
            var duplicate = await _context.SubUnits
                .AnyAsync(s => s.UnitId == request.UnitId &&
                              s.RoomNumber == request.RoomNumber &&
                              !s.IsDeleted);

            if (duplicate)
                return Result.Failure<SubUnitResponse>(
                    new Error("DuplicateRoom",
                        "A room with this number already exists in this unit", 400));

            var subUnit = new Domain.Entities.SubUnit
            {
                UnitId = request.UnitId,
                RoomNumber = request.RoomNumber,
                Type = request.Type,
                PricePerNight = request.PricePerNight,
                MaxOccupancy = request.MaxOccupancy,
                Bedrooms = request.Bedrooms,
                Bathrooms = request.Bathrooms,
                Size = request.Size,
                Description = request.Description,
                IsAvailable = request.IsAvailable
            };

            await _context.SubUnits.AddAsync(subUnit);
            var availabilityInit = await service.InitializeDefaultAvailabilityAsync(subUnit.Id, 365);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Reload with navigation properties
            await _context.Entry(subUnit)
                .Reference(s => s.Unit)
                .LoadAsync();

            return Result.Success(MapToResponse(subUnit));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating subunit");
            return Result.Failure<SubUnitResponse>(
                new Error("CreateFailed", "Failed to create subunit", 500));
        }
    }

    public async Task<Result<SubUnitResponse>> UpdateAsync(
        int subUnitId,
        UpdateSubUnitRequest request)
    {
        var subUnit = await _context.SubUnits
            .Include(s => s.Unit)
            .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

        if (subUnit == null)
            return Result.Failure<SubUnitResponse>(
                new Error("NotFound", "SubUnit not found", 404));

        // Update properties
        if (request.RoomNumber != null) subUnit.RoomNumber = request.RoomNumber;
        if (request.Type.HasValue) subUnit.Type = request.Type.Value;
        if (request.PricePerNight.HasValue) subUnit.PricePerNight = request.PricePerNight.Value;
        if (request.MaxOccupancy.HasValue) subUnit.MaxOccupancy = request.MaxOccupancy.Value;
        if (request.Bedrooms.HasValue) subUnit.Bedrooms = request.Bedrooms;
        if (request.Bathrooms.HasValue) subUnit.Bathrooms = request.Bathrooms;
        if (request.Size.HasValue) subUnit.Size = request.Size;
        if (request.Description != null) subUnit.Description = request.Description;
        if (request.IsAvailable.HasValue) subUnit.IsAvailable = request.IsAvailable.Value;

        await _context.SaveChangesAsync();

        return Result.Success(MapToResponse(subUnit));
    }

    public async Task<Result> DeleteAsync(int subUnitId, bool softDelete = true)
    {
        var subUnit = await _context.SubUnits
            .Include(s => s.BookingRooms)
            .FirstOrDefaultAsync(s => s.Id == subUnitId);

        if (subUnit == null)
            return Result.Failure(
                new Error("NotFound", "SubUnit not found", 404));

        // Check for active bookings
        var hasActiveBookings = subUnit.BookingRooms.Any(br =>
            br.Booking.Status == BookingStatus.Confirmed ||
            br.Booking.Status == BookingStatus.CheckedIn);

        if (hasActiveBookings)
            return Result.Failure(
                new Error("HasActiveBookings",
                    "Cannot delete subunit with active bookings", 400));

        if (softDelete)
        {
            subUnit.IsDeleted = true;
            subUnit.DeletedAt = DateTime.UtcNow.AddHours(3);
            subUnit.IsAvailable = false;
        }
        else
        {
            _context.SubUnits.Remove(subUnit);
        }

        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> RestoreAsync(int subUnitId)
    {
        var subUnit = await _context.SubUnits
            .FirstOrDefaultAsync(s => s.Id == subUnitId && s.IsDeleted);

        if (subUnit == null)
            return Result.Failure(
                new Error("NotFound", "Deleted subunit not found", 404));

        subUnit.IsDeleted = false;
        subUnit.DeletedAt = null;
        subUnit.IsAvailable = true;

        await _context.SaveChangesAsync();
        return Result.Success();
    }

    #endregion

    #region AVAILABILITY

    public async Task<Result> SetAvailabilityAsync(
        int subUnitId,
        SetAvailabilityRequest request)
    {
        var subUnit = await _context.SubUnits
            .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

        if (subUnit == null)
            return Result.Failure(
                new Error("NotFound", "SubUnit not found", 404));

        // Check for existing availability record
        var existing = await _context.Set<SubUnitAvailability>()
            .FirstOrDefaultAsync(a => a.SubUnitId == subUnitId &&
                                     a.StartDate == request.StartDate &&
                                     a.EndDate == request.EndDate);

        if (existing != null)
        {
            // Update existing
            existing.IsAvailable = request.IsAvailable;
            existing.Reason = request.Reason;
            existing.SpecialPrice = request.SpecialPrice;
            existing.WeekendPrice = request.WeekendPrice;
            existing.UpdatedAt = DateTime.UtcNow.AddHours(3);
            existing.UpdatedByUserId = request.UpdatedByUserId;
        }
        else
        {
            // Create new
            var availability = new SubUnitAvailability
            {
                SubUnitId = subUnitId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsAvailable = request.IsAvailable,
                Reason = request.Reason,
                SpecialPrice = request.SpecialPrice,
                WeekendPrice = request.WeekendPrice,
                UpdatedByUserId = request.UpdatedByUserId,
                CreatedAt = DateTime.UtcNow.AddHours(3)
            };

            await _context.Set<SubUnitAvailability>().AddAsync(availability);
        }

        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> ToggleAvailableAsync(int subUnitId)
    {
        var subUnit = await _context.SubUnits
            .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

        if (subUnit == null)
            return Result.Failure(
                new Error("NotFound", "SubUnit not found", 404));

        subUnit.IsAvailable = !subUnit.IsAvailable;
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<IEnumerable<AvailabilityResponse>>> GetAvailabilityAsync(
        int subUnitId,
        DateTime startDate,
        DateTime endDate)
    {
        var availabilities = await _context.Set<SubUnitAvailability>()
            .Where(a => a.SubUnitId == subUnitId &&
                       a.StartDate >= startDate &&
                       a.EndDate <= endDate)
            .AsNoTracking()
            .ToListAsync();

        var responses = availabilities.Select(a => new AvailabilityResponse(
            a.Id,
            a.StartDate,
            a.EndDate,
            a.IsAvailable,
            a.Reason?.ToString(),
            a.SpecialPrice,
            a.WeekendPrice
        )).ToList();

        return Result.Success<IEnumerable<AvailabilityResponse>>(responses);
    }

    public async Task<Result<List<SubUnitResponse>>> GetAvailableSubUnitsAsync(
        int unitId,
        DateTime checkIn,
        DateTime checkOut)
    {
        var subUnits = await _context.SubUnits
            .Include(s => s.Unit)
            .Include(s => s.SubUnitImages.Where(i => !i.IsDeleted && i.IsPrimary))
            .Where(s => s.UnitId == unitId &&
                       !s.IsDeleted &&
                       s.IsAvailable)
            .AsNoTracking()
            .ToListAsync();

        // Filter out rooms with bookings during the requested dates
        var availableRoomIds = await _context.BookingRooms
            .Include(br => br.Booking)
            .Where(br => br.Room.UnitId == unitId &&
                        br.Booking.CheckInDate < checkOut &&
                        br.Booking.CheckOutDate > checkIn &&
                        br.Booking.Status != BookingStatus.Cancelled)
            .Select(br => br.RoomId)
            .ToListAsync();

        var available = subUnits
            .Where(s => !availableRoomIds.Contains(s.Id))
            .Select(MapToResponse)
            .ToList();

        return Result.Success(available);
    }

    #endregion

    #region IMAGE MANAGEMENT


        public async Task<Result<List<SubUnitImageResponses>>> UploadImagesAsync(
            int subUnitId,
            List<IFormFile> images,
            string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var subUnit = await _context.SubUnits
                    .Include(s => s.SubUnitImages.Where(i => !i.IsDeleted))
                    .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

                if (subUnit == null)
                    return Result.Failure<List<SubUnitImageResponses>>(
                        new Error("NotFound", "SubUnit not found", 404));

                // Upload to S3 (creates original + thumbnail + medium)
                var uploadResult = await _s3Service.UploadSubUnitImagesAsync(images, subUnitId, userId);
                if (!uploadResult.IsSuccess)
                    return Result.Failure<List<SubUnitImageResponses>>(uploadResult.Error);

                var s3Keys = uploadResult.Value;
                var imageResponses = new List<SubUnitImageResponses>();

                var maxOrder = subUnit.SubUnitImages.Any()
                    ? subUnit.SubUnitImages.Max(i => i.DisplayOrder)
                    : 0;

                foreach (var (s3Key, index) in s3Keys.Select((k, i) => (k, i)))
                {
                    var subUnitImage = new Domain.Entities.SubUnitImage
                    {
                        SubUnitId = subUnitId,

                        // ORIGINAL (Full Size)
                        ImageUrl = _s3Service.GetCloudFrontUrl(s3Key),
                        S3Key = s3Key,
                        S3Bucket = GetBucketNameFromConfig(), // Get from configuration

                        // THUMBNAIL (150x150)
                        ThumbnailUrl = _s3Service.GetCloudFrontUrl(GetThumbnailKey(s3Key)),
                        ThumbnailS3Key = GetThumbnailKey(s3Key),

                        // MEDIUM (800x800)
                        MediumUrl = _s3Service.GetCloudFrontUrl(GetMediumKey(s3Key)),
                        MediumS3Key = GetMediumKey(s3Key),

                        // Display Properties
                        DisplayOrder = maxOrder + index + 1,
                        IsPrimary = !subUnit.SubUnitImages.Any() && index == 0,

                        // Tracking
                        UploadedByUserId = userId,
                        UploadedAt = DateTime.UtcNow.AddHours(3),
                        ProcessingStatus = ImageProcessingStatus.Completed
                    };

                    await _context.Set<Domain.Entities.SubUnitImage>().AddAsync(subUnitImage);
                    imageResponses.Add(MapToImageResponse(subUnitImage));
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Uploaded {Count} images for SubUnit {SubUnitId}",
                    images.Count, subUnitId);

                return Result.Success(imageResponses);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error uploading images for subunit {SubUnitId}", subUnitId);
                return Result.Failure<List<SubUnitImageResponses>>(
                    new Error("UploadFailed", "Failed to upload images", 500));
            }
        }

        public async Task<Result> DeleteImageAsync(int subUnitId, int imageId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var image = await _context.Set<Domain.Entities.SubUnitImage>()
                    .FirstOrDefaultAsync(i => i.Id == imageId &&
                                             i.SubUnitId == subUnitId &&
                                             !i.IsDeleted);

                if (image == null)
                    return Result.Failure(
                        new Error("NotFound", "Image not found", 404));

                // Mark as soft deleted in database
                image.IsDeleted = true;
                image.DeletedAt = DateTime.UtcNow.AddHours(3);

                // If it was primary, set another image as primary
                if (image.IsPrimary)
                {
                    var nextPrimary = await _context.Set<Domain.Entities.SubUnitImage>()
                        .Where(i => i.SubUnitId == subUnitId &&
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

                // ACTUALLY DELETE from S3 (all three sizes: original + thumbnail + medium)
                var deleteResult = await _s3Service.DeleteImagesAsync(new List<string> { image.S3Key });

                if (!deleteResult.IsSuccess)
                {
                    _logger.LogWarning(
                        "Image {ImageId} marked as deleted in DB but failed to delete from S3: {Error}",
                        imageId, deleteResult.Error.Description);
                }
                else
                {
                    _logger.LogInformation(
                        "Successfully deleted image {ImageId} from SubUnit {SubUnitId} (DB + S3)",
                        imageId, subUnitId);
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

        public async Task<Result> SetPrimaryImageAsync(int subUnitId, int imageId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var images = await _context.Set<Domain.Entities.SubUnitImage>()
                    .Where(i => i.SubUnitId == subUnitId && !i.IsDeleted)
                    .ToListAsync();

                var targetImage = images.FirstOrDefault(i => i.Id == imageId);
                if (targetImage == null)
                    return Result.Failure(
                        new Error("NotFound", "Image not found", 404));

                // Remove primary flag from all images
                foreach (var img in images)
                {
                    img.IsPrimary = img.Id == imageId;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Set image {ImageId} as primary for SubUnit {SubUnitId}",
                    imageId, subUnitId);

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

        #endregion

    #region HELPER METHODS

        private static SubUnitImageResponses MapToImageResponse(Domain.Entities.SubUnitImage image)
        {
            return new SubUnitImageResponses
            {
                Id = image.Id,
                ImageUrl = image.ImageUrl,              // Original (full size)
                ThumbnailUrl = image.ThumbnailUrl,      // 150x150
                MediumUrl = image.MediumUrl,            // 800x800
                IsPrimary = image.IsPrimary,
                DisplayOrder = image.DisplayOrder,
                Caption = image.Caption ?? "no caption"
            };
        }

        /// <summary>
        /// Get the S3 key for the thumbnail version of an image
        /// </summary>
        private static string GetThumbnailKey(string originalKey)
        {
            var directory = Path.GetDirectoryName(originalKey)?.Replace("\\", "/");
            var filename = Path.GetFileNameWithoutExtension(originalKey);
            var extension = Path.GetExtension(originalKey);
            return $"{directory}/{filename}_thumbnail{extension}";
        }

        /// <summary>
        /// Get the S3 key for the medium version of an image
        /// </summary>
        private static string GetMediumKey(string originalKey)
        {
            var directory = Path.GetDirectoryName(originalKey)?.Replace("\\", "/");
            var filename = Path.GetFileNameWithoutExtension(originalKey);
            var extension = Path.GetExtension(originalKey);
            return $"{directory}/{filename}_medium{extension}";
        }

        /// <summary>
        /// Get bucket name from configuration (helper method)
        /// </summary>
        private string GetBucketNameFromConfig()
        {
            // You can inject IConfiguration if needed, or return a constant
            return "your-bucket-name"; // Replace with actual bucket name from config
        }

        #endregion
    

    #region PRICING

    public async Task<Result> SetSpecialPricingAsync(
        int subUnitId,
        SetSpecialPricingRequestsss request)
    {
        var availability = await _context.Set<SubUnitAvailability>()
            .FirstOrDefaultAsync(a => a.SubUnitId == subUnitId &&
                                     a.StartDate == request.StartDate &&
                                     a.EndDate == request.EndDate);

        if (availability == null)
        {
            // Create new availability record with special pricing
            availability = new SubUnitAvailability
            {
                SubUnitId = subUnitId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsAvailable = true,
                SpecialPrice = request.SpecialPrice,
                WeekendPrice = request.WeekendPrice,
                UpdatedByUserId = request.UpdatedByUserId,
                CreatedAt = DateTime.UtcNow.AddHours(3)
            };

            await _context.Set<SubUnitAvailability>().AddAsync(availability);
        }
        else
        {
            availability.SpecialPrice = request.SpecialPrice;
            availability.WeekendPrice = request.WeekendPrice;
            availability.UpdatedAt = DateTime.UtcNow.AddHours(3);
            availability.UpdatedByUserId = request.UpdatedByUserId;
        }

        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<decimal>> CalculatePriceAsync(
        int subUnitId,
        DateTime checkIn,
        DateTime checkOut)
    {
        var subUnit = await _context.SubUnits
            .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

        if (subUnit == null)
            return Result.Failure<decimal>(
                new Error("NotFound", "SubUnit not found", 404));

        // Get availability with pricing info
        var availability = await _context.Set<SubUnitAvailability>()
            .FirstOrDefaultAsync(a => a.SubUnitId == subUnitId &&
                                     checkIn >= a.StartDate &&
                                     checkOut <= a.EndDate);

        var calculator = new PricingCalculator();
        var price = calculator.CalculatePrice(subUnit, checkIn, checkOut, availability);

        return Result.Success(price);
    }

    #endregion

    #region POLICY MANAGEMENT

    public async Task<Result> AttachGeneralPolicyAsync(int subUnitId, int policyId)
    {
        var subUnit = await _context.SubUnits
            .FirstOrDefaultAsync(s => s.Id == subUnitId && !s.IsDeleted);

        if (subUnit == null)
            return Result.Failure(
                new Error("NotFound", "SubUnit not found", 404));

        var policy = await _context.GeneralPolicies
            .FirstOrDefaultAsync(p => p.Id == policyId && p.IsActive);

        if (policy == null)
            return Result.Failure(
                new Error("PolicyNotFound", "General policy not found", 404));

        policy.SubUnitId = subUnitId;
        policy.UnitId = null;

        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> RemoveGeneralPolicyAsync(int subUnitId, int policyId)
    {
        var policy = await _context.GeneralPolicies
            .FirstOrDefaultAsync(p => p.Id == policyId && p.SubUnitId == subUnitId);

        if (policy == null)
            return Result.Failure(
                new Error("NotFound", "Policy not found", 404));

        policy.SubUnitId = null;
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    #endregion

    #region HELPER METHODS

    private static SubUnitResponse MapToResponse(Domain.Entities.SubUnit subUnit)
    {
        return new SubUnitResponse
        {
            Id = subUnit.Id,
            UnitId = subUnit.UnitId,
            UnitName = subUnit.Unit?.Name ?? "",
            RoomNumber = subUnit.RoomNumber,
            Type = subUnit.Type.ToString(),
            PricePerNight = subUnit.PricePerNight,
            MaxOccupancy = subUnit.MaxOccupancy,
            Bedrooms = subUnit.Bedrooms,
            Bathrooms = subUnit.Bathrooms,
            Size = subUnit.Size,
            Description = subUnit.Description,
            IsAvailable = subUnit.IsAvailable,
            PrimaryImageUrl = subUnit.SubUnitImages?
                .FirstOrDefault(i => i.IsPrimary)?.ImageUrl
        };
    }

    private static SubUnitDetailsResponse MapToDetailsResponse(Domain.Entities.SubUnit subUnit)
    {
        var images = subUnit.SubUnitImages?.Where(i => !i.IsDeleted)
            .Select(i => new SubUnitImageResponse
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                ThumbnailUrl = i.ThumbnailUrl,
                IsPrimary = i.IsPrimary,
                DisplayOrder = i.DisplayOrder
            })
            .OrderBy(i => i.DisplayOrder)
            .ToList() ?? new List<SubUnitImageResponse>();

        var amenities = subUnit.SubUnitAmenities?.Select(sa => new AmenityInfo(
            sa.AmenityId,
            sa.Amenity?.Name.ToString() ?? "",
            sa.Amenity?.Category.ToString() ?? "",
            sa.IsAvailable
        )).ToList() ?? new List<AmenityInfo>();

        return new SubUnitDetailsResponse
        {
            Id = subUnit.Id,
            UnitId = subUnit.UnitId,
            UnitName = subUnit.Unit?.Name ?? "",
            RoomNumber = subUnit.RoomNumber,
            Type = subUnit.Type.ToString(),
            PricePerNight = subUnit.PricePerNight,
            MaxOccupancy = subUnit.MaxOccupancy,
            Bedrooms = subUnit.Bedrooms,
            Bathrooms = subUnit.Bathrooms,
            Size = subUnit.Size,
            Description = subUnit.Description,
            IsAvailable = subUnit.IsAvailable,
            Images = images,
            Amenities = amenities
        };
    }

    //private static SubUnitImageResponse MapToImageResponse(Domain.Entities.SubUnitImage image)
    //{
    //    return new SubUnitImageResponse
    //    {
    //        Id = image.Id,
    //        ImageUrl = image.ImageUrl,
    //        ThumbnailUrl = image.ThumbnailUrl,
    //        IsPrimary = image.IsPrimary,
    //        DisplayOrder = image.DisplayOrder
    //    };
    //}

    #endregion
}