// Application/Service/SubUnitImage/SubUnitImageService.cs
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Application.Abstraction;
using Application.Service.SubUnitImage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
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

        try
        {
            var validationResult = ValidateImages(images);
            if (!validationResult.IsSuccess)
                return Result.Failure<List<string>>(validationResult.Error);

            var transferUtility = new TransferUtility(_s3Client);

            foreach (var (image, index) in images.Select((img, i) => (img, i)))
            {
                var fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();
                var s3Key = $"subunits/{subUnitId}/images/{Guid.NewGuid()}{fileExtension}";

                // Upload original
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
                        ["subunit-id"] = subUnitId.ToString(),
                        ["display-order"] = index.ToString()
                    }
                };

                await transferUtility.UploadAsync(uploadRequest);
                uploadedKeys.Add(s3Key);

                // Generate thumbnail and medium
                await GenerateThreeSizesAsync(image, s3Key);
            }

            return Result.Success(uploadedKeys);
        }
        catch (Exception ex)
        {
            if (uploadedKeys.Any())
                await DeleteImagesAsync(uploadedKeys);

            return Result.Failure<List<string>>(
                new Error("UploadFailed", $"Failed to upload subunit images: {ex.Message}", 500));
        }
    }

    public async Task<Result> DeleteImagesAsync(List<string> s3Keys)
    {
        try
        {
            if (!s3Keys.Any())
                return Result.Success();

            var allKeysToDelete = new List<string>();
            foreach (var s3Key in s3Keys)
            {
                allKeysToDelete.Add(s3Key);
                allKeysToDelete.Add(GetThumbnailKey(s3Key));
                allKeysToDelete.Add(GetMediumKey(s3Key));
            }

            allKeysToDelete = allKeysToDelete.Where(k => !string.IsNullOrEmpty(k)).ToList();

            if (allKeysToDelete.Any())
            {
                var deleteRequest = new DeleteObjectsRequest
                {
                    BucketName = _bucketName,
                    Objects = allKeysToDelete.Select(key => new KeyVersion { Key = key }).ToList()
                };

                await _s3Client.DeleteObjectsAsync(deleteRequest);
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