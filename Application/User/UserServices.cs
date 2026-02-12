using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.User;
using Domain;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Application.User;

public class UserServices(
    UserManager<ApplicationUser> manager,
    RoleManager<ApplicationRole> roleManager,
    ApplicationDbcontext dbContext,
    IAmazonS3 s3Client,
    IConfiguration configuration,
    ILogger<UserServices> logger) : IUserService
{
    private readonly UserManager<ApplicationUser> _manager = manager;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly ApplicationDbcontext _dbContext = dbContext;
    private readonly IAmazonS3 _s3Client = s3Client;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<UserServices> _logger = logger;
    private readonly string _bucketName = configuration["AWS:S3:BucketName"]
        ?? throw new InvalidOperationException("S3 bucket name not configured");
    private readonly string _cloudFrontDomain = configuration["AWS:S3:Domain"] ?? "";

    #region Profile Management

    public async Task<Result<UserProfileResponse>> GetUserProfile(string id)
    {
        var user = await _manager.Users
            .Where(i => i.Id == id)
            .SingleOrDefaultAsync();

        if (user == null)
            return Result.Failure<UserProfileResponse>(UserErrors.UserNotFound);

        var response = new UserProfileResponse(
            Email: user.Email!,
            FullName: user.FullName ?? "",
            Address: user.Address ?? "",
            PhoneNumber: user.PhoneNumber ?? "",
            AvatarUrl: user.AvatarUrl,
            Bio: user.Bio,
            DateOfBirth: user.DateOfBirth,
            Nationality: user.Nationality,
            CreatedAt: user.CreatedAt,
            LastLoginAt: user.LastLoginAt,
            City: user.City,
            Country: user.Country
        );

        return Result.Success(response);
    }

    public async Task<Result> UpdateUserProfile(string id, UpdateUserProfileRequest request)
    {
        var user = await _manager.FindByIdAsync(id);

        if (user == null)
            return Result.Failure(UserErrors.UserNotFound);

        // Update basic info
        if (!string.IsNullOrWhiteSpace(request.UserFullName))
            user.FullName = request.UserFullName;

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            user.PhoneNumber = request.PhoneNumber;

        if (!string.IsNullOrWhiteSpace(request.UserAddress))
            user.Address = request.UserAddress;

        // Update new fields
        if (!string.IsNullOrWhiteSpace(request.Bio))
            user.Bio = request.Bio;

        if (request.DateOfBirth.HasValue)
            user.DateOfBirth = request.DateOfBirth.Value;

        if (!string.IsNullOrWhiteSpace(request.Nationality))
            user.Nationality = request.Nationality;

        var result = await _manager.UpdateAsync(user);

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();
        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> ChangePassword(string id, ChangePasswordRequest request)
    {
        var user = await _manager.FindByIdAsync(id);

        if (user == null)
            return Result.Failure(UserErrors.UserNotFound);
            
        var result = await _manager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();
        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> ChangeRoleForUser(string email, string newRole)
    {
        var user = await _manager.FindByEmailAsync(email);

        if (user == null)
            return Result.Failure(UserErrors.UserNotFound);

        var roles = await _manager.GetRolesAsync(user);

        if (roles == null || roles.Count == 0)
            return Result.Failure(RolesErrors.somethingwrong);

        if (roles.Contains(newRole))
            return Result.Failure(RolesErrors.haveit);

        if (!await _roleManager.RoleExistsAsync(newRole))
            return Result.Failure(RolesErrors.notFound);

        var removeRoleResult = await _manager.RemoveFromRolesAsync(user, roles);

        if (!removeRoleResult.Succeeded)
            return Result.Failure(RolesErrors.somethingwrong);

        var addRoleResult = await _manager.AddToRoleAsync(user, newRole);

        if (!addRoleResult.Succeeded)
            return Result.Failure(RolesErrors.somethingwrong);

        return Result.Success();
    }

    #endregion

    #region Avatar Management

    public async Task<Result<string>> UploadUserAvatarAsync(string userId, IFormFile image)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            // Verify user exists
            var user = await _manager.FindByIdAsync(userId);
            if (user == null)
                return Result.Failure<string>(UserErrors.UserNotFound);

            // Validate image
            var validationResult = ValidateImage(image);
            if (!validationResult.IsSuccess)
                return Result.Failure<string>(validationResult.Error);

            // Delete old avatar if exists
            if (!string.IsNullOrEmpty(user.AvatarUrl))
            {
                var oldS3Key = ExtractS3KeyFromUrl(user.AvatarUrl);
                if (!string.IsNullOrEmpty(oldS3Key))
                {
                    _ = DeleteImageFromS3Async(oldS3Key); // Fire and forget
                }
            }

            // Upload new avatar to S3
            var fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();
            var s3Key = $"users/{userId}/avatar/{Guid.NewGuid()}.wepb";

            var transferUtility = new TransferUtility(_s3Client);
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
                    ["user-id"] = userId
                }
            };

            await transferUtility.UploadAsync(uploadRequest);

            // Generate CloudFront URL
            var avatarUrl = GetCloudFrontImageUrl(s3Key);

            // Update user with new avatar URL
            user.AvatarUrl = avatarUrl;
            user.AvatarUploadedAt = DateTime.UtcNow.AddHours(3);
            await _manager.UpdateAsync(user);

            await transaction.CommitAsync();

            // Generate thumbnails asynchronously for bigger projects
            //_ = GenerateThumbnailAsync(image, s3Key);

            //_logger.LogInformation(
            //    "Avatar uploaded for user {UserId}. S3 Key: {S3Key}",
            //    userId, s3Key);

            return Result.Success(avatarUrl);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error uploading avatar for user {UserId}", userId);
            return Result.Failure<string>(
                new Error("UploadFailed", $"Failed to upload avatar: {ex.Message}", 500));
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
    public async Task<Result> DeleteUserAvatarAsync(string userId)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var user = await _manager.FindByIdAsync(userId);

            if (user == null)
                return Result.Failure(UserErrors.UserNotFound);

            if (string.IsNullOrEmpty(user.AvatarUrl))
                return Result.Failure(
                    new Error("NoAvatar", "User has no avatar", 400));

            // Extract S3 key from URL
            var s3Key = ExtractS3KeyFromUrl(user.AvatarUrl);
            if (string.IsNullOrEmpty(s3Key))
                return Result.Failure(
                    new Error("InvalidAvatarUrl", "Could not extract S3 key from avatar URL", 400));

            // Delete from S3
            var deleteResult = await DeleteImageFromS3Async(s3Key);
            if (!deleteResult.IsSuccess)
            {
                _logger.LogWarning("Failed to delete avatar from S3: {Error}", deleteResult.Error.Description);
            }

            // Update user
            user.AvatarUrl = null;
            user.AvatarUploadedAt = null;
            await _manager.UpdateAsync(user);

            await transaction.CommitAsync();

            _logger.LogInformation("Avatar deleted for user {UserId}", userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting avatar for user {UserId}", userId);
            return Result.Failure(
                new Error("DeleteFailed", $"Failed to delete avatar: {ex.Message}", 500));
        }
    }

    public async Task<Result<string>> GetPresignedImageUrlAsync(string s3Key, int expirationMinutes = 60)
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
            _logger.LogError(ex, "Error generating presigned URL for S3 key: {S3Key}", s3Key);
            return Result.Failure<string>(
                new Error("UrlFailed", $"Failed to generate presigned URL: {ex.Message}", 500));
        }
    }

    public string GetCloudFrontImageUrl(string s3Key)
    {
        if (string.IsNullOrEmpty(s3Key))
            return string.Empty;

        if (string.IsNullOrEmpty(_cloudFrontDomain))
            return $"https://{_bucketName}.s3.amazonaws.com/{s3Key}";

        return $"https://{_cloudFrontDomain}/{s3Key}";
    }

    #endregion

    #region Private Helper Methods

    private Result ValidateImage(IFormFile image)
    {
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var maxFileSize = 5 * 1024 * 1024; // 5MB

        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            return Result.Failure(
                new Error("InvalidFormat", $"Invalid image format: {extension}. Allowed: jpg, jpeg, png, webp", 400));

        if (image.Length > maxFileSize)
            return Result.Failure(
                new Error("FileTooLarge", "Avatar size must be less than 5MB", 400));

        if (image.Length == 0)
            return Result.Failure(
                new Error("EmptyFile", "Empty image file detected", 400));

        return Result.Success();
    }

    private async Task GenerateThumbnailAsync(IFormFile originalImage, string originalS3Key)
    {
        try
        {
            using var image = await Image.LoadAsync(originalImage.OpenReadStream());

            // Generate multiple sizes for avatar
            await GenerateAvatarSizeAsync(image, originalS3Key, 150, "thumbnail"); // Small
            await GenerateAvatarSizeAsync(image, originalS3Key, 300, "medium"); // Medium
            await GenerateAvatarSizeAsync(image, originalS3Key, 500, "large"); // Large

            _logger.LogInformation("Thumbnails generated for {S3Key}", originalS3Key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate thumbnails for {S3Key}", originalS3Key);
        }
    }

    private async Task GenerateAvatarSizeAsync(
        Image image,
        string originalS3Key,
        int size,
        string suffix)
    {
        var resized = image.Clone(x => x.Resize(new ResizeOptions
        {
            Size = new Size(size, size),
            Mode = ResizeMode.Crop // Crop to ensure square avatars
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

    private async Task<Result> DeleteImageFromS3Async(string s3Key)
    {
        try
        {
            // Delete main image
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = s3Key
            };

            await _s3Client.DeleteObjectAsync(deleteRequest);

            // Delete thumbnails
            var thumbnailSuffixes = new[] { "thumbnail", "medium", "large" };
            foreach (var suffix in thumbnailSuffixes)
            {
                var thumbnailKey = GetThumbnailKey(s3Key, suffix);
                var thumbDeleteRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = thumbnailKey
                };

                try
                {
                    await _s3Client.DeleteObjectAsync(thumbDeleteRequest);
                }
                catch { /* Ignore thumbnail delete errors */ }
            }

            _logger.LogInformation("Avatar deleted from S3: {S3Key}", s3Key);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting avatar from S3: {S3Key}", s3Key);
            return Result.Failure(
                new Error("S3DeleteFailed", $"Failed to delete avatar from S3: {ex.Message}", 500));
        }
    }

    private string GetThumbnailKey(string originalKey, string suffix)
    {
        var directory = Path.GetDirectoryName(originalKey)?.Replace("\\", "/");
        var filename = Path.GetFileNameWithoutExtension(originalKey);
        var extension = Path.GetExtension(originalKey);

        return $"{directory}/{filename}_{suffix}{extension}";
    }

    private string ExtractS3KeyFromUrl(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
            return string.Empty;

        try
        {
            // Handle CloudFront URLs
            if (!string.IsNullOrEmpty(_cloudFrontDomain) && imageUrl.Contains(_cloudFrontDomain))
            {
                var uri = new Uri(imageUrl);
                return uri.AbsolutePath.TrimStart('/');
            }

            // Handle S3 direct URLs
            if (imageUrl.Contains(".s3."))
            {
                var uri = new Uri(imageUrl);
                return uri.AbsolutePath.TrimStart('/');
            }

            // If it's already just the key
            if (imageUrl.StartsWith("users/"))
            {
                return imageUrl;
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract S3 key from URL: {ImageUrl}", imageUrl);
            return string.Empty;
        }
    }

    #endregion
}