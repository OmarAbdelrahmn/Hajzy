using Application.Abstraction;
using Application.Abstraction.Consts;
using Application.Abstraction.Errors;
using Application.Authentication;
using Application.Contracts.Auth;
using Application.Helpers;
using Domain;
using Domain.Consts;
using Domain.Entities;
using Hangfire;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;
using System.Text;


namespace Application.Service.Auth;

public class AuthService(
    UserManager<ApplicationUser> manager,
    SignInManager<ApplicationUser> signInManager
    , IJwtProvider jwtProvider,
    ILogger<AuthService> logger,
    IEmailSender emailSender,
    IHttpContextAccessor httpContextAccessor,
    ApplicationDbcontext dbcontext) : IAuthService
{
    private readonly UserManager<ApplicationUser> manager = manager;
    private readonly SignInManager<ApplicationUser> signInManager = signInManager;
    private readonly IJwtProvider jwtProvider = jwtProvider;
    private readonly ILogger<AuthService> logger = logger;
    private readonly IEmailSender emailSender = emailSender;
    private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;
    private readonly ApplicationDbcontext dbcontext = dbcontext;
    private readonly int RefreshTokenExpiryDays = 60;

    public async Task<Result<AuthResponse>> SingInAsync(AuthRequest request)
    {

        if (await manager.FindByEmailAsync(request.Email) is not { } user)
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

        if (user.IsDisable)
            return Result.Failure<AuthResponse>(UserErrors.Disableuser);

        var userRoles = await manager.GetRolesAsync(user);

        if (!userRoles.Contains(DefaultRoles.User))
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

        var result = await signInManager.PasswordSignInAsync(user, request.Password, false, true);

        if (result.Succeeded)
        {
            var (Token, ExpiresIn) = jwtProvider.GenerateToken(user,userRoles.FirstOrDefault()!);

            var RefreshToken = GenerateRefreshToken();

            var RefreshExpiresIn = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays);


            user.RefreshTokens.Add(new RefreshToken
            {
                Token = RefreshToken,
                ExpiresOn = RefreshExpiresIn,

            });

            user.LastLoginAt = DateTime.UtcNow.AddHours(3);


            await manager.UpdateAsync(user);

            var response = new AuthResponse(
                user.Id,
                user.Email!,
                user.FullName ??" ",
                user.Address ?? " ",
                Token,
                ExpiresIn * 60,
                RefreshToken,
                RefreshExpiresIn
            );

            return Result.Success(response);
        }

        var error = result.IsNotAllowed ?
             UserErrors.EmailNotConfirmed :
             result.IsLockedOut ?
             UserErrors.userLockedout :
             UserErrors.InvalidCredentials;


        return Result.Failure<AuthResponse>(error);

    }

    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(220));
    }

    public async Task<Result<AuthResponse>> GetRefreshTokenAsync(string Token, string RefreshToken)
    {
        var UserId = jwtProvider.ValidateToken(Token);

        if (UserId is null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

        var user = await manager.FindByIdAsync(UserId);

        if (user is null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

        if (user.IsDisable)
            return Result.Failure<AuthResponse>(UserErrors.Disableuser);

        if (user.LockoutEnd > DateTime.UtcNow)
            return Result.Failure<AuthResponse>(UserErrors.userLockedout);


        var UserRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == RefreshToken && x.IsActive);

        if (UserRefreshToken is null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

        UserRefreshToken.RevokedOn = DateTime.UtcNow;

        var userRoles = await manager.GetRolesAsync(user);
        
        var (newToken, ExpiresIn) = jwtProvider.GenerateToken(user , userRoles.FirstOrDefault()!);

        var newRefreshToken = GenerateRefreshToken();

        var RefreshExpiresIn = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays);


        user.RefreshTokens.Add(new RefreshToken
        {
            Token = newRefreshToken,
            ExpiresOn = RefreshExpiresIn,

        });

        await manager.UpdateAsync(user);

        var response = new AuthResponse(
            user.Id,
            user.Email!,
            user.FullName??" ",
            user.Address??" ",
            newToken,
            ExpiresIn * 60,
            newRefreshToken,
            RefreshExpiresIn
        );

        return Result.Success(response);
    }

    public async Task<Result> RevokeRefreshTokenAsync(string Token, string RefreshToken)
    {
        var UserId = jwtProvider.ValidateToken(Token);

        if (UserId is null)
            return Result.Failure(UserErrors.InvalidCredentials);

        var user = await manager.FindByIdAsync(UserId);

        if (user is null)
            return Result.Failure(UserErrors.UserNotFound);

        var UserRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == RefreshToken && x.IsActive);

        if (UserRefreshToken is null)
            return Result.Failure(UserErrors.InvalidCredentials);

        UserRefreshToken.RevokedOn = DateTime.UtcNow;

        await manager.UpdateAsync(user);
        return Result.Success();
    }

    public async Task<Result> UserRegisterAsync(Registerrequest request)
    {
        var emailisex = await manager.Users.AnyAsync(i => i.Email == request.Email);

        if (emailisex)
            return Result.Failure(UserErrors.EmailAlreadyExist);

        var user = request.Adapt<ApplicationUser>();
        user.UserName = request.Email;
        user.FullName = request.FullName;
        user.Address = " no data";
        user.PhoneNumber = " no data";
        var result = await manager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            await manager.AddToRoleAsync(user, DefaultRoles.User);

            var code = await manager.GenerateEmailConfirmationTokenAsync(user);

            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            await sendemail(user, code);

            return Result.Success();
        }
        var errors = result.Errors.First();
        return Result.Failure(new Error(errors.Code, errors.Description, StatusCodes.Status400BadRequest));

    }

    public async Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request)
    {

        if (await manager.FindByIdAsync(request.UserId) is not { } user)
            return Result.Failure(UserErrors.UserNotFound);


        if (user.EmailConfirmed)
            return Result.Failure(UserErrors.DuplicatedConfermation);

        var code = request.Code;
        try
        {
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        }
        catch (FormatException)
        {

            return Result.Failure(UserErrors.InvalidCredentials);
        }


        var result = await manager.ConfirmEmailAsync(user, code);

        if (result.Succeeded)
        {



            return Result.Success();
        }
        var errors = result.Errors.First();
        return Result.Failure(new Error(errors.Code, errors.Description, StatusCodes.Status400BadRequest));

    }

    public async Task<Result> ResendEmailAsync(ResendEmailRequest request)
    {
        if (await manager.FindByEmailAsync(request.Email) is not { } user)
            return Result.Success();


        if (user.EmailConfirmed)
            return Result.Failure(UserErrors.DuplicatedConfermation);

        var code = await manager.GenerateEmailConfirmationTokenAsync(user);

        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        logger.LogInformation("Configration code : {code}", code);

        await sendemail(user, code);

        return Result.Success();
    }

    private async Task sendemail(ApplicationUser user, string code)
    {
        var origin = httpContextAccessor.HttpContext?.Request.Headers.Origin;

        var emailbody = EmailBodyBuilder.GenerateEmailBody("EmailConfirmation",
            new Dictionary<string, string> {
                    { "{{name}}", user.FullName ?? "hello" } ,
                    { "{{action_url}}", $"{origin}/auth/emailconfigration?userid={user.Id}&code={code}" }

            });

        BackgroundJob.Enqueue(() => emailSender.SendEmailAsync(user.Email!, "Hujjzy : Email configration", emailbody));
        await Task.CompletedTask;
    }

    private async Task sendchangepasswordemail(ApplicationUser user, string code)
    {
        var origin = httpContextAccessor.HttpContext?.Request.Headers.Origin;

        var emailbody = EmailBodyBuilder.GenerateEmailBody("ForgetPassword",
            new Dictionary<string, string> {
                    { "{{name}}", user.FullName ?? "hello" } ,
                    { "{{action_url}}", $"{origin}/auth/forgetpassword?email={user.Email}&code={code}" }

            });

        BackgroundJob.Enqueue(() => emailSender.SendEmailAsync(user.Email!, "Hujjzy : change password", emailbody));
        await Task.CompletedTask;
    }

    public async Task<Result> ForgetPassordAsync(ForgetPasswordRequest request)
    {
        if (await manager.FindByEmailAsync(request.Email) is not { } user)
            return Result.Success();

        if (!user.EmailConfirmed)
            return Result.Failure(UserErrors.EmailNotConfirmed);

        var code = await manager.GeneratePasswordResetTokenAsync(user);

        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        logger.LogInformation("Reset code : {code}", code);

        //send email
        await sendchangepasswordemail(user, code);

        return Result.Success();

    }

    public async Task<Result> ResetPasswordAsync(Contracts.Auth.ResetPasswordRequest request)
    {
        var user = await manager.FindByEmailAsync(request.Email);

        if (user == null || !user.EmailConfirmed)
            return Result.Failure(UserErrors.InvalidCredentials);

        IdentityResult identityResult;

        try
        {
            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));

            identityResult = await manager.ResetPasswordAsync(user, code, request.Password);

        }
        catch (FormatException)
        {

            identityResult = IdentityResult.Failed(manager.ErrorDescriber.InvalidToken());
        }

        if (identityResult.Succeeded)
            return Result.Success();

        var error = identityResult.Errors.First();

        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status401Unauthorized));
    }

    public async Task<Result<AuthResponse>> HAdminSingInAsync(AuthRequest request)
    {
        if (await manager.FindByEmailAsync(request.Email) is not { } user)
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

        if (user.IsDisable)
            return Result.Failure<AuthResponse>(UserErrors.Disableuser);

        var userRoles = await manager.GetRolesAsync(user);

        if (!userRoles.Contains(DefaultRoles.HotelAdmin))
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

        var result = await signInManager.PasswordSignInAsync(user, request.Password, false, true);

        if (result.Succeeded)
        {

            var (Token, ExpiresIn) = jwtProvider.GenerateToken(user, userRoles.FirstOrDefault()!);

            var RefreshToken = GenerateRefreshToken();

            var RefreshExpiresIn = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays);


            user.RefreshTokens.Add(new RefreshToken
            {
                Token = RefreshToken,
                ExpiresOn = RefreshExpiresIn,

            });
            user.LastLoginAt = DateTime.UtcNow.AddHours(3);

            await manager.UpdateAsync(user);

            var response = new AuthResponse(
                user.Id,
                user.Email!,
                user.FullName ?? " ",
                user.Address ?? " ",
                Token,
                ExpiresIn * 60,
                RefreshToken,
                RefreshExpiresIn
            );

            return Result.Success(response);
        }

        var error = result.IsNotAllowed ?
             UserErrors.EmailNotConfirmed :
             result.IsLockedOut ?
             UserErrors.userLockedout :
             UserErrors.InvalidCredentials;


        return Result.Failure<AuthResponse>(error);
    }

    public async Task<Result<AuthResponse>> CAdminSingInAsync(AuthRequest request)
    {
        if (await manager.FindByEmailAsync(request.Email) is not { } user)
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

        if (user.IsDisable)
            return Result.Failure<AuthResponse>(UserErrors.Disableuser);

        var result = await signInManager.PasswordSignInAsync(user, request.Password, false, true);


        var userRoles = await manager.GetRolesAsync(user);

        if (!userRoles.Contains(DefaultRoles.CityAdmin))
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

        if (result.Succeeded)
        {

            var (Token, ExpiresIn) = jwtProvider.GenerateToken(user, userRoles.FirstOrDefault()!);

            var RefreshToken = GenerateRefreshToken();

            var RefreshExpiresIn = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays);


            user.RefreshTokens.Add(new RefreshToken
            {
                Token = RefreshToken,
                ExpiresOn = RefreshExpiresIn,

            });
            user.LastLoginAt = DateTime.UtcNow.AddHours(3);

            await manager.UpdateAsync(user);

            var response = new AuthResponse(
                user.Id,
                user.Email!,
                user.FullName ?? " ",
                user.Address ?? " ",
                Token,
                ExpiresIn * 60,
                RefreshToken,
                RefreshExpiresIn
            );

            return Result.Success(response);
        }

        var error = result.IsNotAllowed ?
             UserErrors.EmailNotConfirmed :
             result.IsLockedOut ?
             UserErrors.userLockedout :
             UserErrors.InvalidCredentials;


        return Result.Failure<AuthResponse>(error);
    }

    public async Task<Result<AuthResponse>> SAdminSingInAsync(AuthRequest request)
    {
        if (await manager.FindByEmailAsync(request.Email) is not { } user)
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

        if (user.IsDisable)
            return Result.Failure<AuthResponse>(UserErrors.Disableuser);


        var userRoles = await manager.GetRolesAsync(user);

        if (!userRoles.Contains(DefaultRoles.SuperAdmin))
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);


        var result = await signInManager.PasswordSignInAsync(user, request.Password, false, true);

        if (result.Succeeded)
        {
            var (Token, ExpiresIn) = jwtProvider.GenerateToken(user, userRoles.FirstOrDefault()!);

            var RefreshToken = GenerateRefreshToken();

            var RefreshExpiresIn = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays);


            user.RefreshTokens.Add(new RefreshToken
            {
                Token = RefreshToken,
                ExpiresOn = RefreshExpiresIn,

            });

            user.LastLoginAt = DateTime.UtcNow.AddHours(3);

            await manager.UpdateAsync(user);

            var response = new AuthResponse(
                user.Id,
                user.Email!,
                user.FullName ?? " ",
                user.Address ?? " ",
                Token,
                ExpiresIn * 60,
                RefreshToken,
                RefreshExpiresIn
            );

            return Result.Success(response);
        }

        var error = result.IsNotAllowed ?
             UserErrors.EmailNotConfirmed :
             result.IsLockedOut ?
             UserErrors.userLockedout :
             UserErrors.InvalidCredentials;


        return Result.Failure<AuthResponse>(error);
    }
}
