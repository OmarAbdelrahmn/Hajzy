//using Application.Service.UnitImage;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using System.Security.Claims;

//namespace Hajzzy.Controllers;

//[Route("api/units/{unitId}/images")]
//[ApiController]
//[Authorize]
//public class UnitImageController(IUnitImageService service) : ControllerBase
//{
//    private readonly IUnitImageService _service = service;

//    /// <summary>
//    /// Upload new images for a unit
//    /// </summary>
//    /// <param name="unitId">Unit ID</param>
//    /// <param name="images">Image files (max 20)</param>
//    [HttpPost("")]
 
//    [RequestSizeLimit(100_000_000)] // 100MB
//    public async Task<IActionResult> UploadImages(
//        int unitId,
//        [FromForm] List<IFormFile> images)
//    {
//        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
//            ?? throw new UnauthorizedAccessException();

//        var result = await _service.UploadUnitImagesAsync(images, unitId, userId);

//        return result.IsSuccess
//            ? Ok(new
//            {
//                Message = $"{images.Count} images uploaded successfully",
//                S3Keys = result.Value
//            })
//            : result.ToProblem();
//    }

//    /// <summary>
//    /// Move images from temporary location to unit's permanent location
//    /// </summary>
//    [HttpPost("move")]
 
//    public async Task<IActionResult> MoveImages(
//        int unitId,
//        [FromBody] MoveImagesRequest request)
//    {
//        var result = await _service.MoveImagesToUnitAsync(request.TempS3Keys, unitId);

//        return result.IsSuccess
//            ? Ok(new { Message = "Images moved successfully" })
//            : result.ToProblem();
//    }

//    /// <summary>
//    /// Delete specific images by S3 keys
//    /// </summary>
//    [HttpDelete("")]
 
//    public async Task<IActionResult> DeleteImages(
//        int unitId,
//        [FromBody] DeleteImagesRequest request)
//    {
//        var result = await _service.DeleteImagesAsync(request.S3Keys);

//        return result.IsSuccess
//            ? Ok(new { Message = $"{request.S3Keys.Count} images deleted successfully" })
//            : result.ToProblem();
//    }

//    /// <summary>
//    /// Get presigned URL for temporary access to an image
//    /// </summary>
//    [HttpGet("presigned-url")]
 
//    public async Task<IActionResult> GetPresignedUrl(
//        int unitId,
//        [FromQuery] string s3Key,
//        [FromQuery] int expirationMinutes = 60)
//    {
//        var result = await _service.GetPresignedUrlAsync(s3Key, expirationMinutes);

//        return result.IsSuccess
//            ? Ok(new { Url = result.Value, ExpiresIn = expirationMinutes })
//            : result.ToProblem();
//    }

//    /// <summary>
//    /// Get CloudFront URL for an image
//    /// </summary>
//    [HttpGet("cloudfront-url")]
//    public IActionResult GetCloudFrontUrl(
//        int unitId,
//        [FromQuery] string s3Key)
//    {
//        var url = _service.GetCloudFrontUrl(s3Key);

//        return Ok(new { Url = url });
//    }

//    /// <summary>
//    /// Reorder images
//    /// </summary>
//    [HttpPut("reorder")]
 
//    public async Task<IActionResult> ReorderImages(
//        int unitId,
//        [FromBody] ReorderImagesRequest request)
//    {
//        var result = await _service.ReorderImagesAsync(unitId, request.S3KeysInOrder);

//        return result.IsSuccess
//            ? Ok(new { Message = "Images reordered successfully" })
//            : result.ToProblem();
//    }
//}

//// ============= REQUEST MODELS =============

//public record MoveImagesRequest(List<string> TempS3Keys);
//public record DeleteImagesRequest(List<string> S3Keys);
//public record ReorderImagesRequest(List<string> S3KeysInOrder);