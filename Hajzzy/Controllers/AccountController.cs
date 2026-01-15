using Application.Contracts.User;
using Application.Extensions;
using Application.User;
using Hajzzy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;



namespace Hajzzy.Controllers;

[Route("api/me")]
[ApiController]
[Authorize]
public class AccountController(IUserService service) : ControllerBase
{
    private readonly IUserService service = service;

    [HttpGet("")]
    public async Task<IActionResult> ShowUserProfile()
    {
        var result = await service.GetUserProfile(User.GetUserId()!);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("info")]
    public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserProfileRequest request)
    {
        var result = await service.UpdateUserProfile(User.GetUserId()!, request);
        
        return Ok(new Re("done"));
    }

    [HttpPut("change-passord")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var result = await service.ChangePassword(User.GetUserId()!, request);

        return result.IsSuccess ? Ok(new Re("done")) : result.ToProblem();
    }
    
    [HttpPut("change-user-role")]
    public async Task<IActionResult> Changerole([FromBody] ChangeUserRoleRequest request)
    {
        var result = await service.ChangeRoleForUser(request.Email,request.NewRole);

        return result.IsSuccess ? Ok(new Re("done")) : result.ToProblem();
    }

    [HttpPost("avatar")]
    public async Task<IActionResult> UploadAvatar([FromForm] Im Im)
    {
        if (Im.Image == null)
            return BadRequest(new { error = "No image file provided" });

        var userId = User.GetUserId();
        var result = await service.UploadUserAvatarAsync(userId, Im.Image);

        return result.IsSuccess
            ? Ok(new { message = "Avatar uploaded successfully", avatarUrl = result.Value })
            : result.ToProblem();
    }

    [HttpDelete("avatar")]
    public async Task<IActionResult> DeleteAvatar()
    {
        var userId = User.GetUserId();
        var result = await service.DeleteUserAvatarAsync(userId);

        return result.IsSuccess
            ? Ok(new { message = "Avatar deleted successfully" })
            : result.ToProblem();
    }

    [HttpGet("avatar/presigned-url")]
    public async Task<IActionResult> GetPresignedAvatarUrl([FromQuery] string s3Key, [FromQuery] int expirationMinutes = 60)
    {
        if (string.IsNullOrWhiteSpace(s3Key))
            return BadRequest(new { error = "S3 key is required" });

        var result = await service.GetPresignedImageUrlAsync(s3Key, expirationMinutes);

        return result.IsSuccess
            ? Ok(new { url = result.Value })
            : result.ToProblem();
    }
}

public record Im(IFormFile Image);