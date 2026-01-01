using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.User;
using Azure;
using Domain.Entities;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.User;

public class UserServices(UserManager<ApplicationUser> manager , RoleManager<ApplicationRole> roleManager) : IUserService
{
    private readonly UserManager<ApplicationUser> manager = manager;
    private readonly RoleManager<ApplicationRole> roleManager = roleManager;

    public async Task<Result> ChangePassword(string id, ChangePasswordRequest request)
    {
        var user = await manager.FindByIdAsync(id);

        var result = await manager.ChangePasswordAsync(user!, request.CurrentPassword, request.NewPassord);

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();

        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> ChangeRoleForUser(string Email, string NewRole)
    {
        var User = await manager.FindByEmailAsync(Email);

        if (User == null)
            return Result.Failure(UserErrors.UserNotFound);

        var Roles = await manager.GetRolesAsync(User);

        if(Roles == null || Roles.Count ==0)
            return Result.Failure(RolesErrors.somethingwrong);
        
        if (Roles.Contains(NewRole))
            return Result.Failure(RolesErrors.haveit);

        if (!await roleManager.RoleExistsAsync(NewRole))
            return Result.Failure(RolesErrors.notFound);

        var RemoveRoleResult = await manager.RemoveFromRolesAsync(User, Roles);
        
        if (!RemoveRoleResult.Succeeded)
            return Result.Failure(RolesErrors.somethingwrong);

        var AddRoleResult = await manager.AddToRoleAsync(User, NewRole);

        if (!AddRoleResult.Succeeded)
            return Result.Failure(RolesErrors.somethingwrong);

        return Result.Success();
    }

    public async Task<Result<UserProfileResponse>> GetUserProfile(string id)
    {
        var user = await manager.Users
            .Where(i => i.Id == id)
            .SingleAsync();

        if(user == null)
            return Result.Failure<UserProfileResponse>(UserErrors.UserNotFound);

        var response = new UserProfileResponse(user.Email!, user.FullName ?? " ", user.Address ?? " ", user.PhoneNumber ?? " ");
        return Result.Success(response);
    }

    public async Task<Result> UpdateUserProfile(string id, UpdateUserProfileRequest request)
    {
        var user = await manager.FindByIdAsync(id);

        if(user == null)
            return Result.Failure(UserErrors.UserNotFound);

        user.FullName = request.UserFullName;

        user.PhoneNumber = request.PhoneNumber;

        user.Address = request.UserAddress;

        var result = await manager.UpdateAsync(user);

        if(result.Succeeded)
        return Result.Success();

        var error = result.Errors.First();

        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }
}
