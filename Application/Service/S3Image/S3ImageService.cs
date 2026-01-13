using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Application.Abstraction;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
            var validationResult = ValidateImages(images);
            if (!validationResult.IsSuccess)
                return Result.Failure<List<string>>(validationResult.Error);

            var transferUtility = new TransferUtility(s3Client);

            foreach (var (image, index) in images.Select((img, i) => (img, i)))
            {
                var fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();
                var s3Key = $"registrations/temp/request-{requestId}/{Guid.NewGuid()}.webp";

                // Upload original
                using var inputStream = image.OpenReadStream();
                using var webpStream = new MemoryStream();
                ConvertToWebp(inputStream, webpStream);
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
                        ["request-id"] = requestId.ToString()
                    }
                };

                await transferUtility.UploadAsync(uploadRequest);
                uploadedKeys.Add(s3Key);

                // Generate ONLY thumbnail and medium
                //await GenerateThreeSizesAsync(image, s3Key);
            }

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

    private static void ConvertToWebp(Stream input, Stream output)
    {
        using var image = Image.Load(input);

        var encoder = new WebpEncoder
        {
            Quality = 75,
            SkipMetadata = true
        };

        image.Save(output, encoder);
    }
    // Service Implementation
    public async Task<Result<List<string>>> MoveImagesToUnitAsync(
        List<string> tempS3Keys,
        int unitId)
    {
        var movedKeys = new List<string>();

        try
        {
            // Validate input
            if (!tempS3Keys.Any())
                return Result.Failure<List<string>>(
                    new Error("NoImages", "No images to move", 400));

            foreach (var tempKey in tempS3Keys)
            {
                // Generate new permanent key
                var fileName = Path.GetFileName(tempKey);
                var newKey = $"units/{unitId}/images/{fileName}";

                // Copy original image
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

                // Verify copy succeeded
                if (copyResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception($"Failed to copy {tempKey} to {newKey}");
                }

                movedKeys.Add(newKey);

                // Copy thumbnails
              //  await CopyThumbnailsAsync(tempKey, newKey);
            }

            // Delete temp files only after ALL copies succeed
            var deleteResult = await DeleteImagesAsync(tempS3Keys);
            if (!deleteResult.IsSuccess)
            {
                // Log warning but don't fail - images are already copied
                Console.WriteLine($"Warning: Failed to delete temp files: {deleteResult.Error?.Description}");
            }

            // Return the new S3 keys so they can be saved to database
            return Result.Success(movedKeys);
        }
        catch (Exception ex)
        {
            // Cleanup: try to delete any successfully moved files
            if (movedKeys.Any())
            {
                try
                {
                    await DeleteImagesAsync(movedKeys);
                }
                catch (AmazonS3Exception exe) when (exe.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Thumbnails don't exist, continue without them
                    Console.WriteLine($"No thumbnails found for {exe}");
                }
            }

            return Result.Failure<List<string>>(
                new Error("MoveFailed", $"Failed to move images: {ex.Message}", 500));
        }
    }

    private async Task<List<string>> CopyThumbnailsAsync(string sourceKey, string destKey)
    {
        var suffixes = new[] { "thumbnail", "small", "medium" };
        var copiedThumbnails = new List<string>();

        foreach (var suffix in suffixes)
        {
            try
            {
                var sourceThumbnail = GetThumbnailKey(sourceKey, suffix);
                var destThumbnail = GetThumbnailKey(destKey, suffix);

                // Check if source thumbnail exists first
                try
                {
                    await _s3Client.GetObjectMetadataAsync(_bucketName, sourceThumbnail);
                }
                catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Source thumbnail doesn't exist, skip it
                    Console.WriteLine($"Thumbnail not found: {sourceThumbnail}");
                    continue;
                }

                var copyRequest = new CopyObjectRequest
                {
                    SourceBucket = _bucketName,
                    SourceKey = sourceThumbnail,
                    DestinationBucket = _bucketName,
                    DestinationKey = destThumbnail,
                    CannedACL = S3CannedACL.Private,
                    MetadataDirective = S3MetadataDirective.COPY
                };

                var response = await _s3Client.CopyObjectAsync(copyRequest);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    copiedThumbnails.Add(destThumbnail);
                }
            }
            catch (Exception ex)
            {
                // Log but don't fail - thumbnails are optional
                Console.WriteLine($"Failed to copy thumbnail {suffix}: {ex.Message}");
            }
        }

        return copiedThumbnails;
    }

    //private string GetThumbnailKey(string originalKey, string suffix)
    //{
    //    var directory = Path.GetDirectoryName(originalKey)?.Replace("\\", "/");
    //    var filename = Path.GetFileNameWithoutExtension(originalKey);
    //    var extension = Path.GetExtension(originalKey);

    //    return $"{directory}/{filename}_{suffix}{extension}";
    //}

    public async Task<Result> DeleteImagesAsync(List<string> s3Keys)
    {
        try
        {
            if (!s3Keys.Any())
                return Result.Success();

            // Collect all keys: original + thumbnail + medium
            var allKeysToDelete = new List<string>();
            foreach (var s3Key in s3Keys)
            {
                allKeysToDelete.Add(s3Key); // Original
                allKeysToDelete.Add(GetThumbnailKey(s3Key));
                allKeysToDelete.Add(GetMediumKey(s3Key));
            }

            // Filter out any nulls or empties
            allKeysToDelete = allKeysToDelete.Where(k => !string.IsNullOrEmpty(k)).ToList();

            if (allKeysToDelete.Any())
            {
                var deleteRequest = new DeleteObjectsRequest
                {
                    BucketName = _bucketName,
                    Objects = allKeysToDelete.Select(key => new KeyVersion { Key = key }).ToList()
                };

                await s3Client.DeleteObjectsAsync(deleteRequest);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(
                new Error("DeleteFailed", $"Failed to delete images: {ex.Message}", 500));
        }
    }

    private async Task GenerateThreeSizesAsync(IFormFile originalImage, string originalS3Key)
    {
        try
        {
            using var image = await Image.LoadAsync(originalImage.OpenReadStream());

            // THUMBNAIL: 150x150
            //await GenerateAndUploadThumbnailAsync(image, originalS3Key, 150, "thumbnail");

            //// MEDIUM: 800x800
            //await GenerateAndUploadThumbnailAsync(image, originalS3Key, 800, "medium");
        }
        catch
        {
            // Thumbnail generation is optional
        }
    }

    private async Task GenerateAndUploadThumbnailAsync(
        Image image,
        string originalS3Key,
        int size,
        string suffix)
    {
        var resized = image.Clone(x => x.Resize(new ResizeOptions
        {
            Size = new Size(size, size),
            Mode = ResizeMode.Max
        }));

        using var outputStream = new MemoryStream();
        await resized.SaveAsJpegAsync(outputStream);
        outputStream.Position = 0;

        var thumbnailKey = GetSizedKey(originalS3Key, suffix);

        var uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = outputStream,
            Key = thumbnailKey,
            BucketName = _bucketName,
            ContentType = "image/jpeg",
            CannedACL = S3CannedACL.Private
        };

        var transferUtility = new TransferUtility(s3Client);
        await transferUtility.UploadAsync(uploadRequest);
    }

    private async Task CopyThreeSizesAsync(string sourceKey, string destKey)
    {
        var suffixes = new[] { "thumbnail", "medium" };

        foreach (var suffix in suffixes)
        {
            try
            {
                var sourceSized = GetSizedKey(sourceKey, suffix);
                var destSized = GetSizedKey(destKey, suffix);

                await s3Client.CopyObjectAsync(new CopyObjectRequest
                {
                    SourceBucket = _bucketName,
                    SourceKey = sourceSized,
                    DestinationBucket = _bucketName,
                    DestinationKey = destSized,
                    CannedACL = S3CannedACL.Private
                });
            }
            catch
            {
                // Ignore errors
            }
        }
    }

    private string GetSizedKey(string originalKey, string suffix)
    {
        var directory = Path.GetDirectoryName(originalKey)?.Replace("\\", "/");
        var filename = Path.GetFileNameWithoutExtension(originalKey);
        var extension = Path.GetExtension(originalKey);
        return $"{directory}/{filename}_{suffix}{extension}";
    }

    private string GetThumbnailKey(string originalKey) => GetSizedKey(originalKey, "thumbnail");
    private string GetMediumKey(string originalKey) => GetSizedKey(originalKey, "medium");

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

            var url = s3Client.GetPreSignedURL(request);
            return Result.Success(url);
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(
                new Error("UrlFailed", $"Failed to generate URL: {ex.Message}", 500));
        }
    }


  

    private string GetThumbnailKey(string originalKey, string suffix)
    {
        var directory = Path.GetDirectoryName(originalKey);
        var filename = Path.GetFileNameWithoutExtension(originalKey);
        var extension = Path.GetExtension(originalKey);

        return $"{directory}/{filename}_{suffix}{extension}";
    }



}

