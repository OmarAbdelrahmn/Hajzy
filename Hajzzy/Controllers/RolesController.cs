using Application.Contracts.Roles;
using Application.Roles;
using Medical_E_Commerce.Service.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hajzzy.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RolesController(IRoleService roleService) : ControllerBase
{
    private readonly IRoleService roleService = roleService;

    [HttpGet("")]
    public async Task<IActionResult> GetAllRoles()
    {
        var response = await roleService.GetRolesAsync();

        return response.IsSuccess ?
            Ok(response.Value) :
            response.ToProblem();

    }

    [HttpPut("toggle-status")]
    public async Task<IActionResult> ToggleStatus(string RoleName)
    {
        var response = await roleService.ToggleStatusAsync(RoleName);

        return response.IsSuccess ?
            Ok(new Re(new("done"))) :
            response.ToProblem();

    }
}
