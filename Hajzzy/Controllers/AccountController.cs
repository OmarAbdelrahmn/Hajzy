using Application.Contracts.User;
using Application.Extensions;
using Application.User;
using Hajzzy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;



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

}
