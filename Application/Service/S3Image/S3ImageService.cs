using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Application.Abstraction;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Application.Service.S3Image;

public class S3ImageService(IAmazonS3 _s3Client, IConfiguration configuration) : IS3ImageService
{
    private readonly IAmazonS3 s3Client = _s3Client;
    private readonly string _bucketName =  configuration["AWS:S3:BucketName"] ?? throw new InvalidOperationException("S3 bucket name not configured");
    private readonly string _cloudFrontDomain =  configuration["AWS:S3:Domain"] ?? "";
    private readonly IConfiguration configuration = configuration;

    public async Task<Result<List<string>>> UploadRegistrationImagesAsync(
        List<IFormFile> images,
        int requestId)
    {
        var uploadedKeys = new List<string>();

        try
        {
            // Validate images
            var validationResult = ValidateImages(images);
            if (!validationResult.IsSuccess)
                return Result.Failure<List<string>>(validationResult.Error);

            var transferUtility = new TransferUtility(_s3Client);

            foreach (var (image, index) in images.Select((img, i) => (img, i)))
            {
                // Generate unique S3 key
                var fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();
                var s3Key = $"registrations/temp/request-{requestId}/image-{index + 1}-{Guid.NewGuid()}{fileExtension}";

                // Upload original image
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
                        ["request-id"] = requestId.ToString()
                    }
                };

                await transferUtility.UploadAsync(uploadRequest);
                uploadedKeys.Add(s3Key);

                // Generate thumbnails (optional - can be done async)
                await GenerateThumbnailsAsync(image, s3Key);
            }

            return Result.Success(uploadedKeys);
        }
        catch (Exception ex)
        {
            // Cleanup uploaded files on error
            if (uploadedKeys.Any())
                await DeleteImagesAsync(uploadedKeys);

            return Result.Failure<List<string>>(
                new Error("UploadFailed", $"Failed to upload images: {ex.Message}", 500));
        }
    }

    public async Task<Result> MoveImagesToUnitAsync(
        List<string> tempS3Keys,
        int unitId)
    {
        try
        {
            var movedKeys = new List<string>();

            foreach (var tempKey in tempS3Keys)
            {
                // New permanent key
                var fileName = Path.GetFileName(tempKey);
                var newKey = $"units/{unitId}/images/{fileName}";

                // Copy to new location
                var copyRequest = new CopyObjectRequest
                {
                    SourceBucket = _bucketName,
                    SourceKey = tempKey,
                    DestinationBucket = _bucketName,
                    DestinationKey = newKey,
                    CannedACL = S3CannedACL.Private
                };

                await _s3Client.CopyObjectAsync(copyRequest);
                movedKeys.Add(newKey);

                // Copy thumbnails as well
                await CopyThumbnailsAsync(tempKey, newKey);
            }

            // Delete temp files
            await DeleteImagesAsync(tempS3Keys);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(
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

            // Also delete thumbnails
            var thumbnailKeys = s3Keys.SelectMany(GetThumbnailKeys).ToList();
            if (thumbnailKeys.Any())
            {
                var thumbDeleteRequest = new DeleteObjectsRequest
                {
                    BucketName = _bucketName,
                    Objects = thumbnailKeys.Select(key => new KeyVersion { Key = key }).ToList()
                };
                await _s3Client.DeleteObjectsAsync(thumbDeleteRequest);
            }

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

    // ============= PRIVATE HELPERS =============

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

    private async Task GenerateThumbnailsAsync(IFormFile originalImage, string originalS3Key)
    {
        try
        {
            using var image = await Image.LoadAsync(originalImage.OpenReadStream());

            var sizes = new Dictionary<string, int>
            {
                ["thumbnail"] = 150,
                ["small"] = 400,
                ["medium"] = 800
            };

            foreach (var (suffix, size) in sizes)
            {
                var resized = image.Clone(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(size, size),
                    Mode = ResizeMode.Max
                }));

                using var outputStream = new MemoryStream();
                await resized.SaveAsJpegAsync(outputStream);
                outputStream.Position = 0;

                var thumbnailKey = GetThumbnailKey(originalS3Key, suffix);

                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = outputStream,
                    Key = thumbnailKey,
                    BucketName = _bucketName,
                    ContentType = "image/jpeg",
                    CannedACL = S3CannedACL.Private
                };

                var transferUtility = new TransferUtility(_s3Client);
                await transferUtility.UploadAsync(uploadRequest);
            }
        }
        catch
        {
            // Thumbnail generation is optional, don't fail the main upload
        }
    }

    private async Task CopyThumbnailsAsync(string sourceKey, string destKey)
    {
        var suffixes = new[] { "thumbnail", "small", "medium" };

        foreach (var suffix in suffixes)
        {
            try
            {
                var sourceThumbnail = GetThumbnailKey(sourceKey, suffix);
                var destThumbnail = GetThumbnailKey(destKey, suffix);

                var copyRequest = new CopyObjectRequest
                {
                    SourceBucket = _bucketName,
                    SourceKey = sourceThumbnail,
                    DestinationBucket = _bucketName,
                    DestinationKey = destThumbnail,
                    CannedACL = S3CannedACL.Private
                };

                await _s3Client.CopyObjectAsync(copyRequest);
            }
            catch
            {
                // Ignore thumbnail copy errors
            }
        }
    }

    private string GetThumbnailKey(string originalKey, string suffix)
    {
        var directory = Path.GetDirectoryName(originalKey);
        var filename = Path.GetFileNameWithoutExtension(originalKey);
        var extension = Path.GetExtension(originalKey);

        return $"{directory}/{filename}_{suffix}{extension}";
    }

    private List<string> GetThumbnailKeys(string originalKey)
    {
        return new[] { "thumbnail", "small", "medium" }
            .Select(suffix => GetThumbnailKey(originalKey, suffix))
            .ToList();
    }

    public string GetCloudFrontUrl(string s3Key)
    {
        if (string.IsNullOrEmpty(s3Key))
            return string.Empty;

        if (string.IsNullOrEmpty(_cloudFrontDomain))
            return $"https://{_bucketName}.s3.amazonaws.com/{s3Key}";

        return $"https://{_cloudFrontDomain}/{s3Key}";
    }
}

