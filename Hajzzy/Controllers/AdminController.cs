using Application.Admin;
using Application.Contracts.Admin;
using Application.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.ConstrainedExecution;

namespace Hajzzy.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AdminController(IAdminService service,IUserService service1) : ControllerBase
{
    private readonly IAdminService service = service;
    private readonly IUserService service1 = service1;

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await service.GetAllUsers();

        return users.IsSuccess ? Ok(users.Value) : users.ToProblem();
    }

    [HttpPost("users/role")]
    public async Task<IActionResult> ChangeRoles([FromBody] Rer request)
    {
        var result = await service1.ChangeRoleForUser(request.Email, request.NewRole);

        return result.IsSuccess ? Ok(new Re("Role updated successfully")) : result.ToProblem();
    }

    [HttpGet("users/id/{Id}")]
    public async Task<IActionResult> GetUser(string Id)
    {
        var user = await service.GetUserAsync(Id);

        return user.IsSuccess ?
            Ok(user.Value) :
            user.ToProblem();
    }

    [HttpGet("users/name/{UserName}")]
    public async Task<IActionResult> GetUser2(string UserName)
    {
        var user = await service.GetUser2Async(UserName);

        return user.IsSuccess ?
            Ok(user.Value) :
            user.ToProblem();
    }


    [HttpPut("users/{UserId}")]
    public async Task<IActionResult> Update(string UserId , UpdateUserRequest request)
    {
        var user = await service.UpdateUserAsync(UserId,request);

        return user.IsSuccess ?
            Ok(new Re("done")) :
            user.ToProblem();
    }

    [HttpDelete("users/{UserId}")]
    public async Task<IActionResult> DeleteAsync(string UserId)
    {
        var user = await service.DeletaUserAsync(UserId);

        return user.IsSuccess ?
            Ok(new Re("done")) :
            user.ToProblem();
    }

    [HttpPost("users")]
    public async Task<IActionResult> AddingUser(CreateUserRequest request)
    {
        var user = await service.AddUserAsync(request);

        return user.IsSuccess ?
            Ok(new Re("done")) :
            user.ToProblem();
    }

    [HttpPut("users/{UserId}/toggle-status")]
    public async Task<IActionResult> ToggleStatusAsync(string UserId)
    {
        var user = await service.ToggleStatusAsync(UserId);
        return user.IsSuccess ?
            Ok(new Re("done")) :
            user.ToProblem();
    }

    [HttpPut("users/{UserId}/end-lockout")]
    public async Task<IActionResult> EndLockoutAsync(string UserId)
    {
        var user = await service.EndLockOutAsync(UserId);
        return user.IsSuccess ?
            Ok(new Re("done")) :
            user.ToProblem();
    }



}
public record Rer(string Email , string NewRole);

public record Re(string Massage);