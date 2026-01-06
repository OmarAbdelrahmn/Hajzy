using Application.Abstraction;
using Application.Contracts.User;
using Microsoft.AspNetCore.Http;

namespace Application.User;

public interface IUserService
{
    Task<Result<UserProfileResponse>> GetUserProfile(string id);
    Task<Result> UpdateUserProfile(string id, UpdateUserProfileRequest request);
    Task<Result> ChangePassword(string id, ChangePasswordRequest request);
    Task<Result> ChangeRoleForUser(string email, string newRole);

    // Avatar Management
    Task<Result<string>> UploadUserAvatarAsync(string userId, IFormFile image);
    Task<Result> DeleteUserAvatarAsync(string userId);
    Task<Result<string>> GetPresignedImageUrlAsync(string s3Key, int expirationMinutes = 60);
    string GetCloudFrontImageUrl(string s3Key);
}
