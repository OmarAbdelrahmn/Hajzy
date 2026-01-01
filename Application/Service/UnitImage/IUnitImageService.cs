using Application.Abstraction;
using Microsoft.AspNetCore.Http;

namespace Application.Service.UnitImage;

public interface IUnitImageService
{
    /// <summary>
    /// Upload images directly to unit's permanent location
    /// </summary>
    Task<Result<List<string>>> UploadUnitImagesAsync(
        List<IFormFile> images,
        int unitId,
        string userId);

    /// <summary>
    /// Move images from temp location to unit's permanent location
    /// </summary>
    Task<Result> MoveImagesToUnitAsync(
        List<string> tempS3Keys,
        int unitId);

    /// <summary>
    /// Delete unit images from S3
    /// </summary>
    Task<Result> DeleteImagesAsync(List<string> s3Keys);

    /// <summary>
    /// Get presigned URL for temporary access
    /// </summary>
    Task<Result<string>> GetPresignedUrlAsync(string s3Key, int expirationMinutes = 60);

    /// <summary>
    /// Get CloudFront URL for public access
    /// </summary>
    string GetCloudFrontUrl(string s3Key);

    /// <summary>
    /// Reorder images by updating display order in S3 metadata
    /// </summary>
    Task<Result> ReorderImagesAsync(int unitId, List<string> s3KeysInOrder);
}