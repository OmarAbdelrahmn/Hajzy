using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Application.Abstraction;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Application.Service.S3Image;

public class S3ImageService : IS3ImageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _cloudFrontDomain;
    private readonly IConfiguration _configuration;

    public S3ImageService(
        IAmazonS3 s3Client,
        IConfiguration configuration)
    {
        _s3Client = s3Client;
        _bucketName = configuration["AWS:S3:BucketName"]
            ?? throw new InvalidOperationException("S3 bucket name not configured");
        _cloudFrontDomain = configuration["AWS:S3:Domain"] ?? "";
        _configuration = configuration;
    }

    /// <summary>
    /// Upload and convert images to WebP in parallel - faster than sequential
    /// </summary>
    public async Task<Result<List<string>>> UploadRegistrationImagesAsync(
        List<IFormFile> images,
        int requestId)
    {
        var uploadedKeys = new List<string>();

        try
        {
            var validationResult = ValidateImages(images);
            if (!validationResult.IsSuccess)
                return Result.Failure<List<string>>(validationResult.Error);

            var transferUtility = new TransferUtility(_s3Client);
            var semaphore = new SemaphoreSlim(5, 5); // Process 5 images concurrently

            // Upload and convert to WebP in parallel
            var uploadTasks = images.Select(async (image, index) =>
            {
                await semaphore.WaitAsync();
                try
                {
                    return await UploadAndConvertToWebPAsync(image, requestId, index, transferUtility);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            uploadedKeys = (await Task.WhenAll(uploadTasks)).ToList();

            return Result.Success(uploadedKeys);
        }
        catch (Exception ex)
        {
            // Cleanup on failure
            if (uploadedKeys.Any())
                await DeleteImagesAsync(uploadedKeys);

            return Result.Failure<List<string>>(
                new Error("UploadFailed", $"Failed to upload images: {ex.Message}", 500));
        }
    }

    /// <summary>
    /// Upload with WebP conversion - happens in parallel for speed
    /// </summary>
    /// 
    /// <summary>
    /// Quick upload of original images - NO processing, just raw upload
    /// Background job will handle conversion later
    /// </summary>
    public async Task<Result<List<string>>> UploadRegistrationImagesQuickAsync(
        List<IFormFile> images,
        int requestId)
    {
        var uploadedKeys = new List<string>();

        try
        {
            var validationResult = ValidateImages(images);
            if (!validationResult.IsSuccess)
                return Result.Failure<List<string>>(validationResult.Error);

            var transferUtility = new TransferUtility(_s3Client);
            var semaphore = new SemaphoreSlim(5, 5); // 5 concurrent uploads

            // Upload originals in parallel - FAST
            var uploadTasks = images.Select(async (image, index) =>
            {
                await semaphore.WaitAsync();
                try
                {
                    return await UploadOriginalAsync(image, requestId, index, transferUtility);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            uploadedKeys = (await Task.WhenAll(uploadTasks)).ToList();

            return Result.Success(uploadedKeys);
        }
        catch (Exception ex)
        {
            if (uploadedKeys.Any())
                await DeleteImagesAsync(uploadedKeys);

            return Result.Failure<List<string>>(
                new Error("UploadFailed", $"Failed to upload images: {ex.Message}", 500));
        }
    }

    /// <summary>
    /// Upload original image without any processing - FAST
    /// </summary>
    private async Task<string> UploadOriginalAsync(
        IFormFile image,
        int requestId,
        int index,
        TransferUtility transferUtility)
    {
        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        var s3Key = $"registrations/temp/request-{requestId}/{Guid.NewGuid()}{extension}";

        using var stream = image.OpenReadStream();

        var uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = stream,
            Key = s3Key,
            BucketName = _bucketName,
            ContentType = image.ContentType,
            CannedACL = S3CannedACL.Private,
            Metadata =
        {
            ["original-filename"] = image.FileName,
            ["uploaded-at"] = DateTime.UtcNow.ToString("o"),
            ["request-id"] = requestId.ToString(),
            ["index"] = index.ToString()
        }
        };

        await transferUtility.UploadAsync(uploadRequest);
        return s3Key;
    }
    private async Task<string> UploadAndConvertToWebPAsync(
        IFormFile image,
        int requestId,
        int index,
        TransferUtility transferUtility)
    {
        var s3Key = $"registrations/temp/request-{requestId}/{Guid.NewGuid()}.webp";

        // Convert to WebP in background thread (CPU-bound work)
        using var inputStream = image.OpenReadStream();
        using var webpStream = new MemoryStream();

        await Task.Run(() => ConvertToWebp(inputStream, webpStream));
        webpStream.Position = 0;

        var uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = webpStream,
            Key = s3Key,
            BucketName = _bucketName,
            ContentType = "image/webp",
            CannedACL = S3CannedACL.Private,
            Metadata =
            {
                ["original-filename"] = image.FileName,
                ["uploaded-at"] = DateTime.UtcNow.ToString("o"),
                ["request-id"] = requestId.ToString(),
                ["index"] = index.ToString()
            }
        };

        await transferUtility.UploadAsync(uploadRequest);
        return s3Key;
    }

 
    private static void ConvertToWebp(Stream input, Stream output)
    {
        using var image = Image.Load(input);

        var encoder = new WebpEncoder
        {
            Quality = 75 // Just quality setting, use defaults for rest
        };

        image.Save(output, encoder);
    }
    // Keep existing methods...
    public async Task<Result<List<string>>> MoveImagesToUnitAsync(
        List<string> tempS3Keys,
        int unitId)
    {
        var movedKeys = new List<string>();

        try
        {
            if (!tempS3Keys.Any())
                return Result.Failure<List<string>>(
                    new Error("NoImages", "No images to move", 400));

            foreach (var tempKey in tempS3Keys)
            {
                var fileName = Path.GetFileName(tempKey);
                var newKey = $"units/{unitId}/images/{fileName}";

                var copyRequest = new CopyObjectRequest
                {
                    SourceBucket = _bucketName,
                    SourceKey = tempKey,
                    DestinationBucket = _bucketName,
                    DestinationKey = newKey,
                    CannedACL = S3CannedACL.Private,
                    MetadataDirective = S3MetadataDirective.COPY
                };

                var copyResponse = await _s3Client.CopyObjectAsync(copyRequest);

                if (copyResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception($"Failed to copy {tempKey} to {newKey}");
                }

                movedKeys.Add(newKey);
            }

            var deleteResult = await DeleteImagesAsync(tempS3Keys);
            if (!deleteResult.IsSuccess)
            {
                Console.WriteLine($"Warning: Failed to delete temp files: {deleteResult.Error?.Description}");
            }

            return Result.Success(movedKeys);
        }
        catch (Exception ex)
        {
            if (movedKeys.Any())
            {
                try
                {
                    await DeleteImagesAsync(movedKeys);
                }
                catch (Exception cleanupEx)
                {
                    Console.WriteLine($"Cleanup failed: {cleanupEx.Message}");
                }
            }

            return Result.Failure<List<string>>(
                new Error("MoveFailed", $"Failed to move images: {ex.Message}", 500));
        }
    }

    public async Task<Result> DeleteImagesAsync(List<string> s3Keys)
    {
        try
        {
            if (!s3Keys.Any())
                return Result.Success();

            var deleteRequest = new DeleteObjectsRequest
            {
                BucketName = _bucketName,
                Objects = s3Keys.Select(key => new KeyVersion { Key = key }).ToList()
            };

            await _s3Client.DeleteObjectsAsync(deleteRequest);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(
                new Error("DeleteFailed", $"Failed to delete images: {ex.Message}", 500));
        }
    }

    public async Task<Result<string>> GetPresignedUrlAsync(string s3Key, int expirationMinutes = 60)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = s3Key,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes)
            };

            var url = _s3Client.GetPreSignedURL(request);
            return Result.Success(url);
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(
                new Error("UrlFailed", $"Failed to generate URL: {ex.Message}", 500));
        }
    }

    public string GetCloudFrontUrl(string s3Key)
    {
        if (string.IsNullOrEmpty(s3Key))
            return string.Empty;

        if (string.IsNullOrEmpty(_cloudFrontDomain))
            return $"https://{_bucketName}.s3.amazonaws.com/{s3Key}";

        return $"https://{_cloudFrontDomain}/{s3Key}";
    }

    private Result ValidateImages(List<IFormFile> images)
    {
        if (images.Count < 1 || images.Count > 15)
            return Result.Failure(
                new Error("InvalidImageCount", "Between 1 and 15 images required", 400));

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var maxFileSize = 10 * 1024 * 1024; // 10MB

        foreach (var image in images)
        {
            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return Result.Failure(
                    new Error("InvalidFormat", $"Invalid image format: {extension}", 400));

            if (image.Length > maxFileSize)
                return Result.Failure(
                    new Error("FileTooLarge", "Image size must be less than 10MB", 400));

            if (image.Length == 0)
                return Result.Failure(
                    new Error("EmptyFile", "Empty image file detected", 400));
        }

        return Result.Success();
    }
}