using Application.Abstraction;
using Application.Contracts.Auth;
using Microsoft.AspNetCore.Identity.Data;
using ResetPasswordRequest = Application.Contracts.Auth.ResetPasswordRequest;


namespace Application.Auth;

public interface IAuthService
{
    Task<Result<AuthResponse>> SingInAsync(AuthRequest request);
    Task<Result<AuthResponse>> HAdminSingInAsync(AuthRequest request);
    Task<Result<AuthResponse>> CAdminSingInAsync(AuthRequest request);
    Task<Result<AuthResponse>> SAdminSingInAsync(AuthRequest request);
    Task<Result> UserRegisterAsync(Registerrequest request);
    Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request);
    Task<Result> ResendEmailAsync(ResendEmailRequest request);
    Task<Result<AuthResponse>> GetRefreshTokenAsync(string Token, string RefreshToken);
    Task<Result> RevokeRefreshTokenAsync(string Token, string RefreshToken);
    Task<Result> ForgetPassordAsync(ForgetPasswordRequest request);
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request);
}
