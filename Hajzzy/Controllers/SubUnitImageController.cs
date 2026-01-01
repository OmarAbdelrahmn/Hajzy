using Application.Service.SubUnitImage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hajzzy.Controllers;

[Route("api/subunits/{subUnitId}/images")]
[ApiController]
public class SubUnitImageController(ISubUnitImageService service) : ControllerBase
{
    private readonly ISubUnitImageService _service = service;

    /// <summary>
    /// Upload new images for a subunit
    /// </summary>
    /// <param name="subUnitId">SubUnit ID</param>
    /// <param name="images">Image files (max 15)</param>
    [HttpPost("")]
    [RequestSizeLimit(100_000_000)] // 100MB
    public async Task<IActionResult> UploadImages(
        int subUnitId,
        [FromForm] List<IFormFile> images)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException();

        var result = await _service.UploadSubUnitImagesAsync(images, subUnitId, userId);

        return result.IsSuccess
            ? Ok(new
            {
                Message = $"{images.Count} images uploaded successfully",
                S3Keys = result.Value
            })
            : result.ToProblem();
    }

    /// <summary>
    /// Move images from temporary location to subunit's permanent location
    /// </summary>
    [HttpPost("move")]
    public async Task<IActionResult> MoveImages(
        int subUnitId,
        [FromBody] MoveImagesRequest request)
    {
        var result = await _service.MoveImagesToSubUnitAsync(request.TempS3Keys, subUnitId);

        return result.IsSuccess
            ? Ok(new { Message = "Images moved successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Delete specific images by S3 keys
    /// </summary>
    [HttpDelete("")]
 
    public async Task<IActionResult> DeleteImages(
        int subUnitId,
        [FromBody] DeleteImagesRequest request)
    {
        var result = await _service.DeleteImagesAsync(request.S3Keys);

        return result.IsSuccess
            ? Ok(new { Message = $"{request.S3Keys.Count} images deleted successfully" })
            : result.ToProblem();
    }

    /// <summary>
    /// Get presigned URL for temporary access to an image
    /// </summary>
    [HttpGet("presigned-url")]
 
    public async Task<IActionResult> GetPresignedUrl(
        int subUnitId,
        [FromQuery] string s3Key,
        [FromQuery] int expirationMinutes = 60)
    {
        var result = await _service.GetPresignedUrlAsync(s3Key, expirationMinutes);

        return result.IsSuccess
            ? Ok(new { Url = result.Value, ExpiresIn = expirationMinutes })
            : result.ToProblem();
    }

    /// <summary>
    /// Get CloudFront URL for an image
    /// </summary>
    [HttpGet("cloudfront-url")]
    public IActionResult GetCloudFrontUrl(
        int subUnitId,
        [FromQuery] string s3Key)
    {
        var url = _service.GetCloudFrontUrl(s3Key);

        return Ok(new { Url = url });
    }

    /// <summary>
    /// Reorder images
    /// </summary>
    [HttpPut("reorder")]
 
    public async Task<IActionResult> ReorderImages(
        int subUnitId,
        [FromBody] ReorderImagesRequest request)
    {
        var result = await _service.ReorderImagesAsync(subUnitId, request.S3KeysInOrder);

        return result.IsSuccess
            ? Ok(new { Message = "Images reordered successfully" })
            : result.ToProblem();
    }
}

// ============= REQUEST MODELS =============
