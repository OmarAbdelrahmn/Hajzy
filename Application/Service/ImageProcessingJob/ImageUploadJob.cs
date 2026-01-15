using Amazon.S3;
using Amazon.S3.Transfer;
using Domain;
using Domain.Entities;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Application.Service.ImageProcessingJob;

    public class ImageUploadJob(
        IAmazonS3 s3Client,
        ApplicationDbcontext context,
        ILogger<ImageUploadJob> logger,
        IConfiguration configuration)
{
        private readonly IAmazonS3 _s3Client = s3Client;
        private readonly ApplicationDbcontext _context = context;
        private readonly ILogger<ImageUploadJob> _logger = logger;
        private readonly string _bucketName = configuration["AWS:S3:BucketName"]
                ?? throw new InvalidOperationException("S3 bucket name not configured");

    /// <summary>
    /// Upload originals, then queue WebP conversion
    /// </summary>
    public async Task UploadAndProcessImagesAsync(
            int requestId,
            List<ImageUploadDto> images)
        {
            _logger.LogInformation(
                "Starting upload for {Count} images, request {RequestId}",
                images.Count, requestId);

            try
            {
                // Update status to Processing
                await UpdateStatusAsync(requestId, ImageProcessingStatus.Processing);

                // PHASE 1: Upload originals FAST (parallel)
                var uploadedKeys = new List<string>();
                var transferUtility = new TransferUtility(_s3Client);
                var semaphore = new SemaphoreSlim(5, 5);

                var uploadTasks = images.Select(async (img, index) =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        var extension = Path.GetExtension(img.FileName).ToLowerInvariant();
                        var s3Key = $"registrations/temp/request-{requestId}/{Guid.NewGuid()}{extension}";

                        using var stream = new MemoryStream(img.Content);

                        var uploadRequest = new TransferUtilityUploadRequest
                        {
                            InputStream = stream,
                            Key = s3Key,
                            BucketName = _bucketName,
                            ContentType = img.ContentType,
                            CannedACL = S3CannedACL.Private,
                            Metadata =
                        {
                            ["original-filename"] = img.FileName,
                            ["uploaded-at"] = DateTime.UtcNow.ToString("o"),
                            ["request-id"] = requestId.ToString(),
                            ["index"] = index.ToString()
                        }
                        };

                        await transferUtility.UploadAsync(uploadRequest);

                        _logger.LogDebug("Uploaded original: {Key}", s3Key);
                        return s3Key;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to upload image {Index}", index);
                        return null;
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                var results = await Task.WhenAll(uploadTasks);
                uploadedKeys = results.Where(k => k != null).ToList()!;

                if (!uploadedKeys.Any())
                {
                    await UpdateStatusAsync(requestId,
                        ImageProcessingStatus.Failed,
                        "All image uploads failed");
                    return;
                }

                // Save uploaded originals to DB immediately
                var request = await _context.Set<UnitRegistrationRequest>()
                    .FirstOrDefaultAsync(r => r.Id == requestId);

                if (request != null)
                {
                    request.ImageS3Keys = JsonSerializer.Serialize(uploadedKeys);
                    request.ImageCount = uploadedKeys.Count;
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation(
                    "Uploaded {Success}/{Total} original images for request {RequestId}",
                    uploadedKeys.Count, images.Count, requestId);

                // PHASE 2: Queue WebP conversion (separate job with delay)
                // ⏰ Add 2-second delay to ensure S3 consistency
                BackgroundJob.Schedule<ImageProcessingJob>(
                    job => job.ProcessRegistrationImagesAsync(requestId, uploadedKeys),
                    TimeSpan.FromSeconds(2));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error in image upload job for request {RequestId}", requestId);
                await UpdateStatusAsync(requestId, ImageProcessingStatus.Failed, ex.Message);
            }
        }

        private async Task UpdateStatusAsync(
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
                        request.ImageProcessingError = error;

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update status for request {RequestId}", requestId);
            }
        }
    }

    // DTO for passing image data to background job
    public class ImageUploadDto
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public byte[] Content { get; set; } = Array.Empty<byte>();
    }
