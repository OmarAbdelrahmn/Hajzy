// Application/Service/SubUnitImage/SubUnitImageService.cs
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Application.Abstraction;
using Application.Service.SubUnitImage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

public class SubUnitImageService(
    IAmazonS3 s3Client,
    IConfiguration configuration) : ISubUnitImageService
{
    private readonly IAmazonS3 _s3Client = s3Client;
    private readonly string _bucketName = configuration["AWS:S3:BucketName"]
        ?? throw new InvalidOperationException("S3 bucket name not configured");
    private readonly string _cloudFrontDomain = configuration["AWS:S3:Domain"] ?? "";

    public async Task<Result<List<string>>> UploadSubUnitImagesAsync(
        List<IFormFile> images,
        int subUnitId,
        string userId)
    {
        var uploadedKeys = new List<string>();
        var allS3Keys = new List<string>(); // Track ALL keys (original + thumbnail + medium)

        try
        {
            var validationResult = ValidateImages(images);
            if (!validationResult.IsSuccess)
                return Result.Failure<List<string>>(validationResult.Error);


            var transferUtility = new TransferUtility(_s3Client);

            foreach (var (image, index) in images.Select((img, i) => (img, i)))
            {
                var fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();
                var s3Key = $"subunits/{subUnitId}/images/{Guid.NewGuid()}.webp";


                // Read file into memory once to avoid stream consumption issues
                byte[] fileBytes;
                using (var inputStream = image.OpenReadStream())
                using (var webpStream = new MemoryStream())
                {
                    ConvertToWebp(inputStream, webpStream);
                    fileBytes = webpStream.ToArray();
                }

                // Upload original
                using (var uploadStream = new MemoryStream(fileBytes))
                {
                    var uploadRequest = new TransferUtilityUploadRequest
                    {
                        InputStream = uploadStream,
                        Key = s3Key,
                        BucketName = _bucketName,
                        ContentType = image.ContentType,
                        CannedACL = S3CannedACL.Private,
                        Metadata =
                    {
                        ["original-filename"] = image.FileName,
                        ["uploaded-at"] = DateTime.UtcNow.ToString("o"),
                        ["uploaded-by"] = userId,
                        ["subunit-id"] = subUnitId.ToString(),
                        ["display-order"] = index.ToString()
                    }
                    };

                    await transferUtility.UploadAsync(uploadRequest);
                    uploadedKeys.Add(s3Key);
                    allS3Keys.Add(s3Key);

                }
            }


            return Result.Success(uploadedKeys);
        }
        catch (Exception ex)
        {

            // Clean up ALL uploaded files (original + thumbnail + medium)
            if (allS3Keys.Any())
            {
                try
                {
                    await DeleteImagesAsync(allS3Keys);
                }
                catch (Exception cleanupEx)
                {

                }
            }

            return Result.Failure<List<string>>(
                new Error("UploadFailed", $"Failed to upload SubUnit images: {ex.Message}", 500));
        }
    }

    private async Task<(string thumbnailKey, string mediumKey)> GenerateThreeSizesAsync(
    byte[] imageBytes,
    string contentType,
    string originalS3Key)
    {
        var thumbnailKey = GetThumbnailKey(originalS3Key);
        var mediumKey = GetMediumKey(originalS3Key);

        try
        {

            // Generate and upload thumbnail (150x150)
            using (var thumbnailStream = new MemoryStream(imageBytes))
            {
                var thumbnail = await ResizeImageAsync(thumbnailStream, 150, 150);
                await UploadToS3Async(thumbnail, thumbnailKey, contentType);
            }


            // Generate and upload medium (800x800)
            using (var mediumStream = new MemoryStream(imageBytes))
            {
                var medium = await ResizeImageAsync(mediumStream, 800, 800);
                await UploadToS3Async(medium, mediumKey, contentType);
            }

            return (thumbnailKey, mediumKey);
        }
        catch (Exception ex)
        {
            throw;
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
    private string GetThumbnailKey(string originalKey)
    {
        var directory = Path.GetDirectoryName(originalKey)?.Replace("\\", "/");
        var fileName = Path.GetFileNameWithoutExtension(originalKey);
        var extension = Path.GetExtension(originalKey);

        return $"{directory}/thumbnails/{fileName}_thumb{extension}";
    }

    private string GetMediumKey(string originalKey)
    {
        var directory = Path.GetDirectoryName(originalKey)?.Replace("\\", "/");
        var fileName = Path.GetFileNameWithoutExtension(originalKey);
        var extension = Path.GetExtension(originalKey);

        return $"{directory}/medium/{fileName}_medium{extension}";
    }

    private string GetBucketNameFromConfig()
    {
        // Get from configuration instead of hardcoding
        return "huzjjy-bucket";
    }
    private async Task<byte[]> ResizeImageAsync(Stream imageStream, int width, int height)
    {
        try
        {
            using var image = await Image.LoadAsync(imageStream);


            // Resize with aspect ratio maintained
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(width, height),
                Mode = ResizeMode.Max, // Maintains aspect ratio, fits within dimensions
                Sampler = KnownResamplers.Lanczos3 // High quality resampling
            }));


            // Save to memory stream with quality settings
            using var outputStream = new MemoryStream();

            // Determine format and save with appropriate encoder
            var format = image.Metadata.DecodedImageFormat;

            if (format?.Name == "PNG")
            {
                await image.SaveAsPngAsync(outputStream, new PngEncoder
                {
                    CompressionLevel = PngCompressionLevel.BestCompression
                });
            }
            else if (format?.Name == "WEBP")
            {
                await image.SaveAsWebpAsync(outputStream, new WebpEncoder
                {
                    Quality = 85
                });
            }
            else
            {
                // Default to JPEG for other formats (jpg, jpeg, etc.)
                await image.SaveAsJpegAsync(outputStream, new JpegEncoder
                {
                    Quality = 85 // Good balance between quality and file size
                });
            }

            var resultBytes = outputStream.ToArray();

            return resultBytes;
        }
        catch (UnknownImageFormatException ex)
        {
            throw new Exception("Unsupported image format", ex);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private async Task UploadToS3Async(byte[] imageBytes, string s3Key, string contentType)
    {
        try
        {
            using var stream = new MemoryStream(imageBytes);

            var putRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = s3Key,
                InputStream = stream,
                ContentType = "image/webp",
                CannedACL = S3CannedACL.Private,
                Metadata =
            {
                ["uploaded-at"] = DateTime.UtcNow.ToString("o"),
                ["file-size"] = imageBytes.Length.ToString()
            }
            };


            var response = await _s3Client.PutObjectAsync(putRequest);

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"S3 upload failed with status code: {response.HttpStatusCode}");
            }

        }
        catch (AmazonS3Exception ex)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    public async Task<Result> DeleteImagesAsync(List<string> s3Keys)
    {
        try
        {
            if (!s3Keys.Any())
                return Result.Success();


            // S3 allows batch delete of up to 1000 objects at once
            var deleteRequest = new DeleteObjectsRequest
            {
                BucketName = _bucketName,
                Objects = s3Keys.Select(key => new KeyVersion { Key = key }).ToList()
            };

            var response = await _s3Client.DeleteObjectsAsync(deleteRequest);

            if (response.DeleteErrors.Any())
            {
                return Result.Failure(new Error("nothing","nothing to delete",400));
            }

  

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("DeleteFailed", ex.Message, 500));
        }
    }

    private async Task GenerateThreeSizesAsync(IFormFile originalImage, string originalS3Key)
    {
        try
        {
            using var image = await Image.LoadAsync(originalImage.OpenReadStream());
            await GenerateAndUploadSizedImageAsync(image, originalS3Key, 150, "thumbnail");
            await GenerateAndUploadSizedImageAsync(image, originalS3Key, 800, "medium");
        }
        catch { }
    }

    private async Task GenerateAndUploadSizedImageAsync(
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

        var sizedKey = GetSizedKey(originalS3Key, suffix);

        var uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = outputStream,
            Key = sizedKey,
            BucketName = _bucketName,
            ContentType = "image/jpeg",
            CannedACL = S3CannedACL.Private
        };

        var transferUtility = new TransferUtility(_s3Client);
        await transferUtility.UploadAsync(uploadRequest);
    }

    private string GetSizedKey(string originalKey, string suffix)
    {
        var directory = Path.GetDirectoryName(originalKey)?.Replace("\\", "/");
        var filename = Path.GetFileNameWithoutExtension(originalKey);
        var extension = Path.GetExtension(originalKey);
        return $"{directory}/{filename}_{suffix}{extension}";
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
        var maxFileSize = 10 * 1024 * 1024;

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

            var url = await _s3Client.GetPreSignedURLAsync(request);
            return Result.Success(url);
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(
                new Error("UrlFailed", $"Failed to generate URL: {ex.Message}", 500));
        }
    }

    //public Task<Result> MoveImagesToSubUnitAsync(List<string> tempS3Keys, int subUnitId)
    //{
    //    throw new NotImplementedException("SubUnitImageService uploads directly to subunit folder");
    //}

    //public Task<Result> ReorderImagesAsync(int subUnitId, List<string> s3KeysInOrder)
    //{
    //    return Task.FromResult(Result.Success());
    //}
}