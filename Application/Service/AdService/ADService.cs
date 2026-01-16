using Amazon.S3;
using Amazon.S3.Transfer;
using Application.Abstraction;
using Application.Contracts.AD;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Service.AdService;

public class AdService(
    ApplicationDbcontext context,
    IAmazonS3 s3Client,
    ILogger<AdService> logger) : IAdService
{
    private readonly ApplicationDbcontext _context = context;
    private readonly IAmazonS3 _s3Client = s3Client;
    private readonly ILogger<AdService> _logger = logger;
    private const string CloudFrontUrl = "";
    private const string BucketName = "hujjzy-bucket";

    public async Task<Result<AdResponse>> CreateAdAsync(CreateAdRequest request, string userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Validate unit if provided
            if (request.UnitId.HasValue)
            {
                var unitExists = await _context.Units
                    .AnyAsync(u => u.Id == request.UnitId.Value && !u.IsDeleted);

                if (!unitExists)
                    return Result.Failure<AdResponse>(
                        new Error("UnitNotFound", "Unit not found", 404));
            }

            // Validate dates
            if (request.EndDate <= request.StartDate)
                return Result.Failure<AdResponse>(
                    new Error("InvalidDates", "End date must be after start date", 400));

            // Upload and process image
            var imageResult = await UploadAdImageAsync(request.Image, userId);
            if (!imageResult.IsSuccess)
                return Result.Failure<AdResponse>(imageResult.Error);

            var (originalKey, thumbnailKey, mediumKey) = imageResult.Value;

            // Create ad entity
            var ad = new Ad
            {
                Title = request.Title,
                Description = request.Description,
                ImageUrl = GetCloudFrontUrl(originalKey),
                S3Key = originalKey,
                UnitId = request.UnitId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsActive = request.IsActive,
                UploadedByUserId = userId,
                CreatedAt = DateTime.UtcNow.AddHours(3)
            };

            await _context.Set<Ad>().AddAsync(ad);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Fetch complete entity with navigation
            var createdAd = await _context.Set<Ad>()
                .Include(a => a.Unit)
                .Include(a => a.UploadedBy)
                .FirstAsync(a => a.Id == ad.Id);

            return Result.Success(MapToResponse(createdAd));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating ad");
            return Result.Failure<AdResponse>(
                new Error("CreateFailed", "Failed to create ad", 500));
        }
    }

    public async Task<Result<AdResponse>> UpdateAdAsync(int adId, UpdateAdRequest request, string userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var ad = await _context.Set<Ad>()
                .Include(a => a.Unit)
                .Include(a => a.UploadedBy)
                .FirstOrDefaultAsync(a => a.Id == adId && !a.IsDeleted);

            if (ad == null)
                return Result.Failure<AdResponse>(
                    new Error("NotFound", "Ad not found", 404));

            // Update properties
            if (request.Title != null) ad.Title = request.Title;
            if (request.Description != null) ad.Description = request.Description;
            if (request.StartDate.HasValue) ad.StartDate = request.StartDate.Value;
            if (request.EndDate.HasValue) ad.EndDate = request.EndDate.Value;
            if (request.IsActive.HasValue) ad.IsActive = request.IsActive.Value;

            // Update unit if changed
            if (request.UnitId.HasValue && request.UnitId.Value != ad.UnitId)
            {
                var unitExists = await _context.Units
                    .AnyAsync(u => u.Id == request.UnitId.Value && !u.IsDeleted);

                if (!unitExists)
                    return Result.Failure<AdResponse>(
                        new Error("UnitNotFound", "Unit not found", 404));

                ad.UnitId = request.UnitId.Value;
            }

            // Update image if provided
            if (request.Image != null)
            {
                // Delete old images
                var oldKeys = new List<string> { ad.S3Key}
                    .Where(k => !string.IsNullOrEmpty(k))
                    .ToList();

                await DeleteS3ImagesAsync(oldKeys);

                // Upload new image
                var imageResult = await UploadAdImageAsync(request.Image, userId);
                if (!imageResult.IsSuccess)
                    return Result.Failure<AdResponse>(imageResult.Error);

                var (originalKey, thumbnailKey, mediumKey) = imageResult.Value;

                ad.ImageUrl = GetCloudFrontUrl(originalKey);
                ad.S3Key = originalKey;
            }

            ad.UpdatedAt = DateTime.UtcNow.AddHours(3);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Result.Success(MapToResponse(ad));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating ad {AdId}", adId);
            return Result.Failure<AdResponse>(
                new Error("UpdateFailed", "Failed to update ad", 500));
        }
    }

    public async Task<Result> DeleteAdAsync(int adId)
    {
        try
        {
            var ad = await _context.Set<Ad>()
                .FirstOrDefaultAsync(a => a.Id == adId && !a.IsDeleted);

            if (ad == null)
                return Result.Failure(new Error("NotFound", "Ad not found", 404));

            ad.IsDeleted = true;
            ad.DeletedAt = DateTime.UtcNow.AddHours(3);

            await _context.SaveChangesAsync();

            // Delete S3 images in background
            var keys = new List<string> { ad.S3Key }
                .Where(k => !string.IsNullOrEmpty(k))
                .ToList();

            _ = DeleteS3ImagesAsync(keys);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting ad {AdId}", adId);
            return Result.Failure(new Error("DeleteFailed", "Failed to delete ad", 500));
        }
    }

    public async Task<Result<AdResponse>> GetAdByIdAsync(int adId)
    {
        var ad = await _context.Set<Ad>()
            .Include(a => a.Unit)
            .Include(a => a.UploadedBy)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == adId && !a.IsDeleted);

        if (ad == null)
            return Result.Failure<AdResponse>(
                new Error("NotFound", "Ad not found", 404));

        return Result.Success(MapToResponse(ad));
    }

    public async Task<Result<IEnumerable<AdResponse>>> GetAllAdsAsync(AdListFilter filter)
    {
        var query = _context.Set<Ad>()
            .Include(a => a.Unit)
            .Include(a => a.UploadedBy)
            .Where(a => !a.IsDeleted)
            .AsQueryable();

        if (filter.IsActive.HasValue)
            query = query.Where(a => a.IsActive == filter.IsActive.Value);

        if (filter.IsExpired.HasValue)
        {
            var now = DateTime.UtcNow;
            query = filter.IsExpired.Value
                ? query.Where(a => a.EndDate < now)
                : query.Where(a => a.EndDate >= now);
        }

        if (filter.UnitId.HasValue)
            query = query.Where(a => a.UnitId == filter.UnitId.Value);

        if (filter.StartDateFrom.HasValue)
            query = query.Where(a => a.StartDate >= filter.StartDateFrom.Value);

        if (filter.StartDateTo.HasValue)
            query = query.Where(a => a.StartDate <= filter.StartDateTo.Value);

        var ads = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .AsNoTracking()
            .ToListAsync();

        var responses = ads.Select(MapToResponse).ToList();
        return Result.Success<IEnumerable<AdResponse>>(responses);
    }

    public async Task<Result<IEnumerable<AdResponse>>> GetCurrentActiveAdsAsync()
    {
        var now = DateTime.UtcNow;

        var ads = await _context.Set<Ad>()
            .Include(a => a.Unit)
            .Include(a => a.UploadedBy)
            .Where(a => !a.IsDeleted &&
                       a.IsActive &&
                       a.StartDate <= now &&
                       a.EndDate >= now)
            .OrderByDescending(a => a.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        var responses = ads.Select(MapToResponse).ToList();
        return Result.Success<IEnumerable<AdResponse>>(responses);
    }

    public async Task<Result<IEnumerable<AdResponse>>> GetInactiveAdsAsync()
    {
        var now = DateTime.UtcNow;

        var ads = await _context.Set<Ad>()
            .Include(a => a.Unit)
            .Include(a => a.UploadedBy)
            .Where(a => !a.IsDeleted &&
                       (!a.IsActive || a.EndDate < now))
            .OrderByDescending(a => a.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        var responses = ads.Select(MapToResponse).ToList();
        return Result.Success<IEnumerable<AdResponse>>(responses);
    }

    public async Task<Result> DeactivateExpiredAdsAsync()
    {
        try
        {
            var now = DateTime.UtcNow;

            var expiredAds = await _context.Set<Ad>()
                .Where(a => !a.IsDeleted &&
                           a.IsActive &&
                           a.EndDate < now)
                .ToListAsync();

            foreach (var ad in expiredAds)
            {
                ad.IsActive = false;
                ad.UpdatedAt = DateTime.UtcNow.AddHours(3);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Deactivated {Count} expired ads",
                expiredAds.Count);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating expired ads");
            return Result.Failure(
                new Error("DeactivationFailed", "Failed to deactivate expired ads", 500));
        }
    }

    // ============= HELPER METHODS =============

    private async Task<Result<(string original, string thumbnail, string medium)>> UploadAdImageAsync(
        Microsoft.AspNetCore.Http.IFormFile image,
        string userId)
    {
        try
        {
            var transferUtility = new TransferUtility(_s3Client);
            var timestamp = DateTime.UtcNow.Ticks;

            // Original (WebP 75% quality)
            var originalKey = $"ads/{userId}/{timestamp}_original.webp";
            using (var originalStream = new MemoryStream())
            {
                await ConvertToWebpAsync(image.OpenReadStream(), originalStream, 75);
                originalStream.Position = 0;

                await transferUtility.UploadAsync(new TransferUtilityUploadRequest
                {
                    InputStream = originalStream,
                    Key = originalKey,
                    BucketName = BucketName,
                    ContentType = "image/webp",
                    CannedACL = S3CannedACL.Private
                });
            }

            // Thumbnail (150x150)
            var thumbnailKey = $"ads/{userId}/{timestamp}_thumbnail.webp";
            using (var thumbnailStream = new MemoryStream())
            {
                await CreateThumbnailAsync(image.OpenReadStream(), thumbnailStream, 150);
                thumbnailStream.Position = 0;

                await transferUtility.UploadAsync(new TransferUtilityUploadRequest
                {
                    InputStream = thumbnailStream,
                    Key = thumbnailKey,
                    BucketName = BucketName,
                    ContentType = "image/webp",
                    CannedACL = S3CannedACL.Private
                });
            }

            // Medium (800 width)
            var mediumKey = $"ads/{userId}/{timestamp}_medium.webp";
            using (var mediumStream = new MemoryStream())
            {
                await CreateMediumAsync(image.OpenReadStream(), mediumStream, 800);
                mediumStream.Position = 0;

                await transferUtility.UploadAsync(new TransferUtilityUploadRequest
                {
                    InputStream = mediumStream,
                    Key = mediumKey,
                    BucketName = BucketName,
                    ContentType = "image/webp",
                    CannedACL = S3CannedACL.Private
                });
            }

            return Result.Success((originalKey, thumbnailKey, mediumKey));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading ad image");
            return Result.Failure<(string, string, string)>(
                new Error("UploadFailed", "Failed to upload image", 500));
        }
    }

    private async Task ConvertToWebpAsync(Stream input, Stream output, int quality)
    {
        await Task.Run(() =>
        {
            using var image = Image.Load(input);
            var encoder = new WebpEncoder
            {
                Quality = 75,
                Method = WebpEncodingMethod.Fastest,
                SkipMetadata = true
            };
            image.Save(output, encoder);
        });
    }

    private async Task CreateThumbnailAsync(Stream input, Stream output, int size)
    {
        await Task.Run(() =>
        {
            using var image = Image.Load(input);
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(size, size),
                Mode = ResizeMode.Crop
            }));

            var encoder = new WebpEncoder { Quality = 75 };
            image.Save(output, encoder);
        });
    }

    private async Task CreateMediumAsync(Stream input, Stream output, int width)
    {
        await Task.Run(() =>
        {
            using var image = Image.Load(input);
            var ratio = (double)width / image.Width;
            var height = (int)(image.Height * ratio);

            image.Mutate(x => x.Resize(width, height));

            var encoder = new WebpEncoder { Quality = 75 };
            image.Save(output, encoder);
        });
    }

    private async Task DeleteS3ImagesAsync(List<string> keys)
    {
        try
        {
            foreach (var key in keys)
            {
                await _s3Client.DeleteObjectAsync(BucketName, key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting S3 images");
        }
    }

    public string GetCloudFrontUrl(string s3Key)
    {
        if (string.IsNullOrEmpty(s3Key))
            return string.Empty;

        if (string.IsNullOrEmpty(CloudFrontUrl))
            return $"https://{BucketName}.s3.amazonaws.com/{s3Key}";

        return $"https://{CloudFrontUrl}/{s3Key}";
    }
    private AdResponse MapToResponse(Ad ad)
    {
        return new AdResponse(
            ad.Id,
            ad.Title,
            ad.Description,
            ad.ImageUrl,
            ad.UnitId,
            ad.Unit?.Name,
            ad.StartDate,
            ad.EndDate,
            ad.IsActive,
            ad.IsExpired,
            ad.UploadedByUserId,
            ad.UploadedBy?.FullName,
            ad.CreatedAt
        );
    }
}