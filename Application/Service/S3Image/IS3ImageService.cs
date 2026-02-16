using Application.Abstraction;
using Microsoft.AspNetCore.Http;

namespace Application.Service.S3Image;

public interface IS3ImageService
{
    Task<Result<List<string>>> UploadRegistrationImagesAsync(
        List<IFormFile> images,
        int requestId);

    Task<Result<List<string>>> UploadRegistrationImagesQuickAsync(
        List<IFormFile> images,
        int requestId);

    Task<Result<List<string>>> MoveImagesToUnitAsync(
        List<string> tempS3Keys,
        int unitId);

    Task<Result> DeleteImagesAsync(List<string> s3Keys);

    Task<Result<string>> GetPresignedUrlAsync(string s3Key, int expirationMinutes = 60);

    string GetCloudFrontUrl(string s3Key);

    /// <summary>
    /// Upload department image to S3
    /// </summary>
    /// <summary>
    /// Upload department image - converted to WebP, single size only
    /// </summary>
    Task<Result<S3UploadResult>> UploadDepartmentImageAsync(
        IFormFile image,
        int departmentId,
        string userId);

    /// <summary>
    /// Upload unit image - converted to WebP, single size only
    /// </summary>
    Task<Result<S3UploadResult>> UploadUnitImageAsync(
        IFormFile image,
        int unitId,
        string userId);

    public record S3UploadResult(
        string ImageUrl,
        string S3Key);

}
