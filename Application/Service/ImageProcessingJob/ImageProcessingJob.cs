using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Application.Abstraction;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System.Text.Json;

namespace Application.Service.ImageProcessingJob;

public class ImageProcessingJob(
    IAmazonS3 s3Client,
    ApplicationDbcontext context,
    ILogger<ImageProcessingJob> logger,
    IConfiguration configuration)
{
    private readonly IAmazonS3 _s3Client = s3Client;
    private readonly ApplicationDbcontext _context = context;
    private readonly ILogger<ImageProcessingJob> _logger = logger;
    private readonly string _bucketName = configuration["AWS:S3:BucketName"]
            ?? throw new InvalidOperationException("S3 bucket name not configured");

    /// <summary>
    /// Background job to convert uploaded images to WebP format
    /// </summary>
    /// 
    public async Task ProcessRegistrationImagesAsync(int requestId, List<string> originalS3Keys)
    {
        _logger.LogInformation(
            "Starting WebP conversion for {Count} images, request {RequestId}",
            originalS3Keys.Count, requestId);

        var processedKeys = new List<string>();
        var successfulOriginals = new List<string>();
        var semaphore = new SemaphoreSlim(3, 3); // Reduce concurrency for CPU-intensive work

        try
        {
            var tasks = originalS3Keys.Select(async originalKey =>
            {
                await semaphore.WaitAsync();
                try
                {
                    // ⏰ Add small random delay to avoid thundering herd on S3
                    await Task.Delay(Random.Shared.Next(100, 500));

                    var webpKey = await ConvertImageToWebPAsync(originalKey, requestId);
                    if (webpKey != null)
                    {
                        successfulOriginals.Add(originalKey);
                    }
                    return webpKey;
                }
                finally
                {
                    semaphore.Release();
                }
            });

            var results = await Task.WhenAll(tasks);
            processedKeys = results.Where(key => key != null).ToList()!;
            var failedCount = results.Count(key => key == null);

            // Update database with final results
            var request = await _context.Set<UnitRegistrationRequest>()
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request != null)
            {
                request.ImageS3Keys = JsonSerializer.Serialize(processedKeys);
                request.ImageCount = processedKeys.Count;
                request.ImagesProcessedAt = DateTime.UtcNow.AddHours(3);

                if (failedCount == 0)
                {
                    request.ImageProcessingStatus = ImageProcessingStatus.Completed;
                    _logger.LogInformation(
                        "Successfully processed all {Count} images for request {RequestId}",
                        processedKeys.Count, requestId);
                }
                else if (processedKeys.Any())
                {
                    request.ImageProcessingStatus = ImageProcessingStatus.Completed; // Partial success
                    request.ImageProcessingError = $"{failedCount} of {originalS3Keys.Count} images failed";
                    _logger.LogWarning(
                        "Partially processed images for request {RequestId}: {Success} succeeded, {Failed} failed",
                        requestId, processedKeys.Count, failedCount);
                }
                else
                {
                    request.ImageProcessingStatus = ImageProcessingStatus.Failed;
                    request.ImageProcessingError = "All WebP conversions failed";
                    _logger.LogError("All conversions failed for request {RequestId}", requestId);
                }

                await _context.SaveChangesAsync();
            }

            // Clean up successful originals
            if (successfulOriginals.Any())
            {
                await DeleteOriginalImagesAsync(successfulOriginals);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process images for request {RequestId}", requestId);
            await UpdateProcessingStatusAsync(requestId, ImageProcessingStatus.Failed, ex.Message);

            // Clean up on complete failure
            if (processedKeys.Any())
            {
                await DeleteImagesAsync(processedKeys);
            }

            throw; // Let Hangfire retry
        }
    }

    private async Task<string?> ConvertImageToWebPAsync(string originalKey, int requestId)
    {
        var maxRetries = 3;
        var currentRetry = 0;

        while (currentRetry < maxRetries)
        {
            try
            {
                _logger.LogDebug("Converting image to WebP (attempt {Retry}): {Key}",
                    currentRetry + 1, originalKey);

                // Download with timeout
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

                var getRequest = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = originalKey
                };

                using var response = await _s3Client.GetObjectAsync(getRequest, cts.Token);
                using var responseStream = response.ResponseStream;
                using var memoryStream = new MemoryStream();

                await responseStream.CopyToAsync(memoryStream, cts.Token);
                memoryStream.Position = 0;

                // Convert to WebP
                using var image = await Image.LoadAsync(memoryStream, cts.Token);
                using var webpStream = new MemoryStream();

                var encoder = new WebpEncoder
                {
                    Quality = 75,
                    Method = WebpEncodingMethod.Fastest,
                    SkipMetadata = true
                };

                await image.SaveAsync(webpStream, encoder, cts.Token);
                webpStream.Position = 0;

                // Upload WebP
                var fileName = Path.GetFileNameWithoutExtension(originalKey);
                var webpKey = $"registrations/temp/request-{requestId}/{fileName}.webp";

                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = webpStream,
                    Key = webpKey,
                    BucketName = _bucketName,
                    ContentType = "image/webp",
                    CannedACL = S3CannedACL.Private,
                    Metadata =
                {
                    ["original-key"] = originalKey,
                    ["processed-at"] = DateTime.UtcNow.ToString("o"),
                    ["request-id"] = requestId.ToString()
                }
                };

                var transferUtility = new TransferUtility(_s3Client);
                await transferUtility.UploadAsync(uploadRequest, cts.Token);

                _logger.LogDebug("Successfully converted to WebP: {Key}", webpKey);
                return webpKey;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Timeout converting image {Key}, attempt {Retry}",
                    originalKey, currentRetry + 1);
                currentRetry++;

                if (currentRetry < maxRetries)
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, currentRetry))); // Exponential backoff
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Image not found in S3 (eventual consistency?): {Key}, attempt {Retry}",
                    originalKey, currentRetry + 1);
                currentRetry++;

                if (currentRetry < maxRetries)
                    await Task.Delay(TimeSpan.FromSeconds(2 * currentRetry)); // Wait for S3 consistency
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to convert image {Key}, attempt {Retry}",
                    originalKey, currentRetry + 1);
                currentRetry++;

                if (currentRetry < maxRetries)
                    await Task.Delay(TimeSpan.FromSeconds(currentRetry));
            }
        }

        _logger.LogError("Failed to convert image after {MaxRetries} attempts: {Key}",
            maxRetries, originalKey);
        return null;
    }
    //public async Task ProcessRegistrationImagesAsync(int requestId, List<string> originalS3Keys)
    //{
    //    _logger.LogInformation(
    //        "Starting image processing for registration request {RequestId}. Images: {Count}",
    //        requestId, originalS3Keys.Count);

    //    var processedKeys = new List<string>();
    //    var successfulOriginals = new List<string>(); // Track which originals succeeded
    //    var semaphore = new SemaphoreSlim(5, 5);

    //    try
    //    {
    //        await UpdateProcessingStatusAsync(requestId, ImageProcessingStatus.Processing);

    //        var tasks = originalS3Keys.Select(async originalKey =>
    //        {
    //            await semaphore.WaitAsync();
    //            try
    //            {
    //                var webpKey = await ConvertImageToWebPAsync(originalKey, requestId);
    //                if (webpKey != null)
    //                {
    //                    successfulOriginals.Add(originalKey); // ✅ Track success
    //                }
    //                return webpKey;
    //            }
    //            finally
    //            {
    //                semaphore.Release();
    //            }
    //        });

    //        var results = await Task.WhenAll(tasks);
    //        processedKeys = results.Where(key => key != null).ToList()!;
    //        var failedCount = results.Count(key => key == null);

    //        // Update database with processed keys
    //        var request = await _context.Set<UnitRegistrationRequest>()
    //            .FirstOrDefaultAsync(r => r.Id == requestId);

    //        if (request != null)
    //        {
    //            request.ImageS3Keys = System.Text.Json.JsonSerializer.Serialize(processedKeys);
    //            request.ImageCount = processedKeys.Count;
    //            request.ImagesProcessedAt = DateTime.UtcNow.AddHours(3);

    //            if (failedCount == 0)
    //            {
    //                request.ImageProcessingStatus = ImageProcessingStatus.Completed;
    //                _logger.LogInformation(
    //                    "Successfully processed all {Count} images for request {RequestId}",
    //                    processedKeys.Count, requestId);
    //            }
    //            else if (processedKeys.Any())
    //            {
    //                request.ImageProcessingStatus = ImageProcessingStatus.Processing; // Consider adding this status
    //                request.ImageProcessingError = $"{failedCount} of {originalS3Keys.Count} images failed to process";
    //                _logger.LogWarning(
    //                    "Partially processed images for request {RequestId}: {Success} succeeded, {Failed} failed",
    //                    requestId, processedKeys.Count, failedCount);
    //            }
    //            else
    //            {
    //                request.ImageProcessingStatus = ImageProcessingStatus.Failed;
    //                request.ImageProcessingError = "All images failed to process";
    //                _logger.LogError(
    //                    "All images failed to process for request {RequestId}",
    //                    requestId);
    //            }

    //            await _context.SaveChangesAsync();
    //        }

    //        // ✅ Only delete originals that were successfully converted
    //        if (successfulOriginals.Any())
    //        {
    //            _logger.LogInformation(
    //                "Deleting {Count} successfully converted original images",
    //                successfulOriginals.Count);
    //            await DeleteOriginalImagesAsync(successfulOriginals);
    //        }

    //        // Keep failed originals for debugging/retry
    //        if (failedCount > 0)
    //        {
    //            _logger.LogWarning(
    //                "Keeping {Count} original images that failed to convert for potential retry",
    //                failedCount);
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex,
    //            "Failed to process images for request {RequestId}", requestId);

    //        await UpdateProcessingStatusAsync(
    //            requestId,
    //            ImageProcessingStatus.Failed,
    //            ex.Message);

    //        // Clean up any processed images on complete failure
    //        if (processedKeys.Any())
    //        {
    //            await DeleteImagesAsync(processedKeys);
    //        }

    //        throw;
    //    }
    //}

    private async Task UpdateProcessingStatusAsync(
        int requestId,
        ImageProcessingStatus status,
        string? error = null)
    {
        try
        {
            var request = await _context.Set<UnitRegistrationRequest>()
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request != null)
            {
                request.ImageProcessingStatus = status;
                if (error != null)
                {
                    request.ImageProcessingError = error;
                }
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to update processing status for request {RequestId}",
                requestId);
        }
    }

    //private async Task<string?> ConvertImageToWebPAsync(string originalKey, int requestId)
    //{
    //    try
    //    {
    //        _logger.LogDebug("Converting image to WebP: {Key}", originalKey);

    //        // Download original image from S3
    //        var getRequest = new GetObjectRequest
    //        {
    //            BucketName = _bucketName,
    //            Key = originalKey
    //        };

    //        using var response = await _s3Client.GetObjectAsync(getRequest);
    //        using var responseStream = response.ResponseStream;
    //        using var memoryStream = new MemoryStream();
    //        await responseStream.CopyToAsync(memoryStream);
    //        memoryStream.Position = 0;

    //        // Convert to WebP
    //        using var image = await Image.LoadAsync(memoryStream);
    //        using var webpStream = new MemoryStream();

    //        var encoder = new WebpEncoder
    //        {
    //            Quality = 75,
    //            Method = WebpEncodingMethod.Fastest,
    //            SkipMetadata = true
    //        };

    //        await image.SaveAsync(webpStream, encoder);
    //        webpStream.Position = 0;

    //        // Generate new key for WebP image
    //        var fileName = Path.GetFileNameWithoutExtension(originalKey);
    //        var webpKey = $"registrations/temp/request-{requestId}/{fileName}.webp";

    //        // Upload WebP to S3
    //        var uploadRequest = new TransferUtilityUploadRequest
    //        {
    //            InputStream = webpStream,
    //            Key = webpKey,
    //            BucketName = _bucketName,
    //            ContentType = "image/webp",
    //            CannedACL = S3CannedACL.Private,
    //            Metadata =
    //            {
    //                ["original-key"] = originalKey,
    //                ["processed-at"] = DateTime.UtcNow.ToString("o"),
    //                ["request-id"] = requestId.ToString()
    //            }
    //        };

    //        var transferUtility = new TransferUtility(_s3Client);
    //        await transferUtility.UploadAsync(uploadRequest);

    //        _logger.LogDebug("Successfully converted to WebP: {Key}", webpKey);
    //        return webpKey;
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Failed to convert image {Key} to WebP", originalKey);
    //        return null; // Return null for failed conversions
    //    }
    //}

    private async Task DeleteOriginalImagesAsync(List<string> keys)
    {
        try
        {
            if (!keys.Any()) return;

            var deleteRequest = new DeleteObjectsRequest
            {
                BucketName = _bucketName,
                Objects = keys.Select(key => new KeyVersion { Key = key }).ToList()
            };

            await _s3Client.DeleteObjectsAsync(deleteRequest);
            _logger.LogInformation("Deleted {Count} original images", keys.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete original images");
            // Don't throw - this is cleanup, not critical
        }
    }

    private async Task DeleteImagesAsync(List<string> keys)
    {
        try
        {
            if (!keys.Any()) return;

            var deleteRequest = new DeleteObjectsRequest
            {
                BucketName = _bucketName,
                Objects = keys.Select(key => new KeyVersion { Key = key }).ToList()
            };

            await _s3Client.DeleteObjectsAsync(deleteRequest);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete images during cleanup");
        }
    }
}