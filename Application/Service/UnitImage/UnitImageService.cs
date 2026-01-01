using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Application.Abstraction;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Application.Service.UnitImage;

public class UnitImageService(
    IAmazonS3 s3Client,
    IConfiguration configuration) : IUnitImageService
{
    private readonly IAmazonS3 _s3Client = s3Client;
    private readonly string _bucketName = configuration["AWS:S3:BucketName"]
        ?? throw new InvalidOperationException("S3 bucket name not configured");
    private readonly string _cloudFrontDomain = configuration["AWS:S3:Domain"] ?? "";

    public async Task<Result<List<string>>> UploadUnitImagesAsync(
        List<IFormFile> images,
        int unitId,
        string userId)
    {
        var uploadedKeys = new List<string>();

        try
        {
            var validationResult = ValidateImages(images);
            if (!validationResult.IsSuccess)
                return Result.Failure<List<string>>(validationResult.Error);

            var transferUtility = new TransferUtility(_s3Client);

            foreach (var (image, index) in images.Select((img, i) => (img, i)))
            {
                var fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();
                var s3Key = $"units/{unitId}/images/{Guid.NewGuid()}{fileExtension}";

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
                        ["uploaded-by"] = userId,
                        ["unit-id"] = unitId.ToString(),
                        ["display-order"] = index.ToString()
                    }
                };

                await transferUtility.UploadAsync(uploadRequest);
                uploadedKeys.Add(s3Key);

                // Generate thumbnails asynchronously
                _ = GenerateThumbnailsAsync(image, s3Key);
            }

            return Result.Success(uploadedKeys);
        }
        catch (Exception ex)
        {
            // Cleanup on failure
            if (uploadedKeys.Any())
                await DeleteImagesAsync(uploadedKeys);

            return Result.Failure<List<string>>(
                new Error("UploadFailed", $"Failed to upload unit images: {ex.Message}", 500));
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
                var fileName = Path.GetFileName(tempKey);
                var newKey = $"units/{unitId}/images/{fileName}";

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

                // Copy thumbnails
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

            // Delete thumbnails
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

            var url = await _s3Client.GetPreSignedURLAsync(request);
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

    public async Task<Result> ReorderImagesAsync(int unitId, List<string> s3KeysInOrder)
    {
        try
        {
            foreach (var (s3Key, order) in s3KeysInOrder.Select((key, idx) => (key, idx)))
            {
                var copyRequest = new CopyObjectRequest
                {
                    SourceBucket = _bucketName,
                    SourceKey = s3Key,
                    DestinationBucket = _bucketName,
                    DestinationKey = s3Key,
                    MetadataDirective = S3MetadataDirective.REPLACE,
                    CannedACL = S3CannedACL.Private
                };

                copyRequest.Metadata.Add("display-order", order.ToString());
                await _s3Client.CopyObjectAsync(copyRequest);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(
                new Error("ReorderFailed", $"Failed to reorder images: {ex.Message}", 500));
        }
    }

    // ============= PRIVATE HELPERS =============

    private Result ValidateImages(List<IFormFile> images)
    {
        if (images.Count < 1 || images.Count > 20)
            return Result.Failure(
                new Error("InvalidImageCount", "Between 1 and 20 images required", 400));

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
                ["medium"] = 800,
                ["large"] = 1920
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
            // Thumbnail generation is optional
        }
    }

    private async Task CopyThumbnailsAsync(string sourceKey, string destKey)
    {
        var suffixes = new[] { "thumbnail", "small", "medium", "large" };

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
        var directory = Path.GetDirectoryName(originalKey)?.Replace("\\", "/");
        var filename = Path.GetFileNameWithoutExtension(originalKey);
        var extension = Path.GetExtension(originalKey);

        return $"{directory}/{filename}_{suffix}{extension}";
    }

    private List<string> GetThumbnailKeys(string originalKey)
    {
        return new[] { "thumbnail", "small", "medium", "large" }
            .Select(suffix => GetThumbnailKey(originalKey, suffix))
            .ToList();
    }
}