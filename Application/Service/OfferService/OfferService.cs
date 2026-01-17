using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Application.Abstraction;
using Application.Contracts.Offer;
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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Application.Service.OfferService;

public class OfferService(
    ApplicationDbcontext context,
    IAmazonS3 s3Client,
    ILogger<OfferService> logger) : IOfferService
{
    private readonly ApplicationDbcontext _context = context;
    private readonly IAmazonS3 _s3Client = s3Client;
    private readonly ILogger<OfferService> _logger = logger;
    private const string CloudFrontUrl = "";
    private const string BucketName = "hujjzy-bucket";

    public async Task<Result<OfferResponse>> CreateOfferAsync(CreateOfferRequest request, string userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            if (request.UnitId.HasValue)
            {
                var unitExists = await _context.Units
                    .AnyAsync(u => u.Id == request.UnitId.Value && !u.IsDeleted);

                if (!unitExists)
                    return Result.Failure<OfferResponse>(
                        new Error("UnitNotFound", "Unit not found", 404));
            }

            if (request.EndDate <= request.StartDate)
                return Result.Failure<OfferResponse>(
                    new Error("InvalidDates", "End date must be after start date", 400));

            var imageResult = await UploadOfferImageAsync(request.Image, userId);
            if (!imageResult.IsSuccess)
                return Result.Failure<OfferResponse>(imageResult.Error);

            var originalKey = imageResult.Value;

            var offer = new Offer
            {
                Title = request.Title,
                Description = request.Description,
                ImageUrl = GetCloudFrontUrl(originalKey),
                S3Key = originalKey,
                UnitId = request.UnitId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                DiscountPercentage = request.DiscountPercentage,
                DiscountAmount = request.DiscountAmount,
                IsActive = request.IsActive,
                UserId = userId,
                CreatedAt = DateTime.UtcNow.AddHours(3),
                IsFeatured = request.IsFeatured,
            };

            await _context.Set<Offer>().AddAsync(offer);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var createdOffer = await _context.Set<Offer>()
                .Include(o => o.Unit)
                .Include(o => o.User)
                .FirstAsync(o => o.Id == offer.Id);

            return Result.Success(MapToResponse(createdOffer));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating offer");
            return Result.Failure<OfferResponse>(
                new Error($"{ex}", "Failed to create offer", 500));
        }
    }

    public async Task<Result<OfferResponse>> UpdateOfferAsync(int offerId, UpdateOfferRequest request, string userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var offer = await _context.Set<Offer>()
                .Include(o => o.Unit)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == offerId && !o.IsDeleted);

            if (offer == null)
                return Result.Failure<OfferResponse>(
                    new Error("NotFound", "Offer not found", 404));

            if (request.Title != null) offer.Title = request.Title;
            if (request.Description != null) offer.Description = request.Description;
            if (request.StartDate.HasValue) offer.StartDate = request.StartDate.Value;
            if (request.EndDate.HasValue) offer.EndDate = request.EndDate.Value;
            if (request.DiscountPercentage.HasValue) offer.DiscountPercentage = request.DiscountPercentage.Value;
            if (request.DiscountAmount.HasValue) offer.DiscountAmount = request.DiscountAmount.Value;
            if (request.IsActive.HasValue) offer.IsActive = request.IsActive.Value;
            if (request.IsFeatured.HasValue) offer.IsFeatured = request.IsFeatured.Value;

            if (request.UnitId.HasValue && request.UnitId.Value != offer.UnitId)
            {
                var unitExists = await _context.Units
                    .AnyAsync(u => u.Id == request.UnitId.Value && !u.IsDeleted);

                if (!unitExists)
                    return Result.Failure<OfferResponse>(
                        new Error("UnitNotFound", "Unit not found", 404));

                offer.UnitId = request.UnitId.Value;
            }

            if (request.Image != null)
            {
                var oldKeys = new List<string> {offer.S3Key}
                    .Where(k => !string.IsNullOrEmpty(k))
                    .ToList();

                await DeleteS3ImagesAsync(oldKeys);

                var imageResult = await UploadOfferImageAsync(request.Image, userId);
                if (!imageResult.IsSuccess)
                    return Result.Failure<OfferResponse>(imageResult.Error);

                var originalKey = imageResult.Value;

                offer.ImageUrl = GetCloudFrontUrl(originalKey);
                offer.S3Key = originalKey;
            }

            offer.UpdatedAt = DateTime.UtcNow.AddHours(3);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Result.Success(MapToResponse(offer));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating offer {OfferId}", offerId);
            return Result.Failure<OfferResponse>(
                new Error("UpdateFailed", "Failed to update offer", 500));
        }
    }

    public async Task<Result> DeleteOfferAsync(int offerId)
    {
        try
        {
            var offer = await _context.Set<Offer>()
                .FirstOrDefaultAsync(o => o.Id == offerId && !o.IsDeleted);

            if (offer == null)
                return Result.Failure(new Error("NotFound", "Offer not found", 404));

            offer.IsDeleted = true;
            offer.DeletedAt = DateTime.UtcNow.AddHours(3);

            await _context.SaveChangesAsync();

            var keys = new List<string> { offer.S3Key }
                .Where(k => !string.IsNullOrEmpty(k))
                .ToList();

            _ = DeleteS3ImagesAsync(keys);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting offer {OfferId}", offerId);
            return Result.Failure(new Error("DeleteFailed", "Failed to delete offer", 500));
        }
    }

    public async Task<Result<OfferResponse>> GetOfferByIdAsync(int offerId)
    {
        var offer = await _context.Set<Offer>()
            .Include(o => o.Unit)
            .Include(o => o.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == offerId && !o.IsDeleted);

        if (offer == null)
            return Result.Failure<OfferResponse>(
                new Error("NotFound", "Offer not found", 404));

        return Result.Success(MapToResponse(offer));
    }

    public async Task<Result<IEnumerable<OfferResponse>>> GetAllOffersAsync(OfferListFilter filter)
    {
        var query = _context.Set<Offer>()
            .Include(o => o.Unit)
            .Include(o => o.User)
            .Where(o => !o.IsDeleted)
            .AsQueryable();

        if (filter.IsActive.HasValue)
            query = query.Where(o => o.IsActive == filter.IsActive.Value);
     
        if (filter.IsFeatured.HasValue)
            query = query.Where(o => o.IsFeatured == filter.IsFeatured.Value);

        if (filter.IsExpired.HasValue)
        {
            var now = DateTime.UtcNow;
            query = filter.IsExpired.Value
                ? query.Where(o => o.EndDate < now)
                : query.Where(o => o.EndDate >= now);
        }

        if (filter.UnitId.HasValue)
            query = query.Where(o => o.UnitId == filter.UnitId.Value);

        if (filter.StartDateFrom.HasValue)
            query = query.Where(o => o.StartDate >= filter.StartDateFrom.Value);

        if (filter.StartDateTo.HasValue)
            query = query.Where(o => o.StartDate <= filter.StartDateTo.Value);

        var offers = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .AsNoTracking()
            .ToListAsync();

        var responses = offers.Select(MapToResponse).ToList();
        return Result.Success<IEnumerable<OfferResponse>>(responses);
    }

    public async Task<Result<IEnumerable<OfferResponse>>> GetCurrentActiveOffersAsync()
    {
        var now = DateTime.UtcNow;

        var offers = await _context.Set<Offer>()
            .Include(o => o.Unit)
            .Include(o => o.User)
            .Where(o => !o.IsDeleted &&
                       o.IsActive &&
                       o.StartDate <= now &&
                       o.EndDate >= now)
            .OrderByDescending(o => o.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        var responses = offers.Select(MapToResponse).ToList();
        return Result.Success<IEnumerable<OfferResponse>>(responses);
    }

    public async Task<Result<IEnumerable<OfferResponse>>> GetInactiveOffersAsync()
    {
        var now = DateTime.UtcNow;

        var offers = await _context.Set<Offer>()
            .Include(o => o.Unit)
            .Include(o => o.User)
            .Where(o => !o.IsDeleted &&
                       (!o.IsActive || o.EndDate < now))
            .OrderByDescending(o => o.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        var responses = offers.Select(MapToResponse).ToList();
        return Result.Success<IEnumerable<OfferResponse>>(responses);
    }

    public async Task<Result> DeactivateExpiredOffersAsync()
    {
        try
        {
            var now = DateTime.UtcNow;

            var expiredOffers = await _context.Set<Offer>()
                .Where(o => !o.IsDeleted &&
                           o.IsActive &&
                           o.EndDate < now)
                .ToListAsync();

            foreach (var offer in expiredOffers)
            {
                offer.IsActive = false;
                offer.UpdatedAt = DateTime.UtcNow.AddHours(3);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Deactivated {Count} expired offers", expiredOffers.Count);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating expired offers");
            return Result.Failure(
                new Error("DeactivationFailed", "Failed to deactivate expired offers", 500));
        }
    }

    // ============= HELPER METHODS (same as AdService) =============

    private async Task<Result<string>> UploadOfferImageAsync(
        Microsoft.AspNetCore.Http.IFormFile image,
        string userId)
    {
        try
        {
            var transferUtility = new TransferUtility(_s3Client);
            var timestamp = DateTime.UtcNow.Ticks;

            var originalKey = $"offers/{userId}/{timestamp}.webp";
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

          
            return Result.Success((originalKey));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading offer image");
            return Result.Failure<string>(
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
    private OfferResponse MapToResponse(Offer offer)
    {
        return new OfferResponse(
            offer.Id,
            offer.Title,
            offer.Description,
            offer.ImageUrl,
            offer.UnitId,
            offer.Unit?.Name,
            offer.StartDate,
            offer.EndDate,
            offer.IsFeatured,
            offer.DiscountPercentage,
            offer.DiscountAmount,
            offer.IsActive,
            offer.IsExpired,
            offer.UserId,
            offer.User?.FullName,
            offer.CreatedAt
        );
    }

    public async Task<Result> ToggleFeatured(int OfferId)
    {
        var offer = await _context.Offers.FirstOrDefaultAsync(o => o.Id == OfferId && !o.IsDeleted);

        if (offer == null)
            return Result.Failure(new Error("NotFound", "Offer not found", 404));

        offer.IsFeatured = !offer.IsFeatured;
        offer.UpdatedAt = DateTime.UtcNow.AddHours(3);

        await _context.SaveChangesAsync();
        return Result.Success();

    }

    public async Task<Result<IEnumerable<OfferResponse>>> GetFeaturedOffersAsync()
    { 
        var query = await _context.Set<Offer>()
            .Include(o => o.Unit)
            .Include(o => o.User)
            .Where(o => !o.IsDeleted && o.IsFeatured)
            .AsNoTracking()
            .ToListAsync();


        var responses = query.Select(MapToResponse).ToList();
        return Result.Success<IEnumerable<OfferResponse>>(responses);
    }
}