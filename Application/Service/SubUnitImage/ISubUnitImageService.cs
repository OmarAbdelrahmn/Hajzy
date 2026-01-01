using Application.Abstraction;
using Microsoft.AspNetCore.Http;

namespace Application.Service.SubUnitImage;

public interface ISubUnitImageService
{
    /// <summary>
    /// Upload images directly to subunit's permanent location
    /// </summary>
    Task<Result<List<string>>> UploadSubUnitImagesAsync(
        List<IFormFile> images,
        int subUnitId,
        string userId);

    /// <summary>
    /// Move images from temp location to subunit's permanent location
    /// </summary>
    Task<Result> MoveImagesToSubUnitAsync(
        List<string> tempS3Keys,
        int subUnitId);

    /// <summary>
    /// Delete subunit images from S3
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
    Task<Result> ReorderImagesAsync(int subUnitId, List<string> s3KeysInOrder);
}