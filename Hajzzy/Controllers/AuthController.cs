using Application.Contracts.Auth;
using Application.Contracts.Auth.RefreshTokem;
using Application.Service.Auth;
using Hajzzy;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;


namespace Hajzzy.Controllers;


[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService service) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthRequest request)
    {
        var response = await service.SingInAsync(request);

        return response.IsSuccess ?
            Ok(response.Value) :
            response.ToProblem();
    }
    [HttpPost("hotel-admin-login")]
    public async Task<IActionResult> Login2([FromBody] AuthRequest request)
    {
        var response = await service.HAdminSingInAsync(request);

        return response.IsSuccess ?
            Ok(response.Value) :
            response.ToProblem();
    }
    [HttpPost("city-admin-login")]
    public async Task<IActionResult> Login3([FromBody] AuthRequest request)
    {
        var response = await service.CAdminSingInAsync(request);

        return response.IsSuccess ?
            Ok(response.Value) :
            response.ToProblem();
    }
    [HttpPost("super-admin-login")]
    public async Task<IActionResult> Login4([FromBody] AuthRequest request)
    {
        var response = await service.SAdminSingInAsync(request);

        return response.IsSuccess ?
            Ok(response.Value) :
            response.ToProblem();
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] Registerrequest request)
    {
        var response = await service.UserRegisterAsync(request);

        return response.IsSuccess ?
            Ok(new Re("done")) :
            response.ToProblem();
    }

    [HttpPost("email-confirmation")]
    public async Task<IActionResult> ConfirmEmailAsync([FromBody] ConfirmEmailRequest request )
    {
        var response = await service.ConfirmEmailAsync(request);

        return response.IsSuccess ?
            Ok(new Re("done")) :
            response.ToProblem();
    }


    [HttpPost("resend-configration-email")]
    public async Task<IActionResult> ResendConfirmEmailAsync([FromBody] ResendEmailRequest request)
    {
        var response = await service.ResendEmailAsync(request);

        return response.IsSuccess ?
            Ok(new Re("done")) :
            response.ToProblem();
    }



    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var response = await service.GetRefreshTokenAsync(request.Token, request.RefreshToken);

        return response.IsSuccess ?
            Ok(response.Value) :
            response.ToProblem();
    }



    [HttpPost("revoke-refresh-token")]
    public async Task<IActionResult> RevokeRefreshToken([FromBody] RefreshTokenRequest request)
    {
        var response = await service.RevokeRefreshTokenAsync(request.Token, request.RefreshToken);

        return response.IsSuccess ?
            Ok(new Re("done")) :
            response.ToProblem();
    }

    [HttpPost("forget-password")]
    public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequest request)
    {
        var response = await service.ForgetPassordAsync(request);

        return response.IsSuccess ?
                Ok(new Re("done")) :
                response.ToProblem();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] Application.Contracts.Auth.ResetPasswordRequest request)
    {
        var response = await service.ResetPasswordAsync(request);

        return response.IsSuccess ?
                Ok(new Re("done")) :
                response.ToProblem();
    }
}
