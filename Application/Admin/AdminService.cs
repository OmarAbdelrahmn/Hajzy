using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.Admin;
using Application.Roles;
using Domain;
using Domain.Entities;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Admin;

public class AdminService(UserManager<ApplicationUser> manager, ApplicationDbcontext dbcontext, IRoleService roleService) : IAdminService
{
    private readonly IRoleService roleService = roleService;

    public async Task<Result<UserResponse>> AddUserAsync(CreateUserRequest request)
    {
        var EmailIsexist = await manager.Users.AnyAsync(c => c.Email == request.Email);

        if (EmailIsexist)
            return Result.Failure<UserResponse>(UserErrors.EmailAlreadyExist);

        var allowedroles = await roleService.GetRolesAsync();

        if (!allowedroles.Value.Any(r => r.Name == request.Role))
            return Result.Failure<UserResponse>(RolesErrors.InvalidRoles);

        var user = request.Adapt<ApplicationUser>();
        user.UserName = request.Email;
        user.EmailConfirmed = true;

        user.Address = request.UserAddress;
        user.FullName = request.UserFullName;   

        var result = await manager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            await manager.AddToRoleAsync(user, request.Role);

            var response = (user, request.Role).Adapt<UserResponse>();

            return Result.Success(response);
        }

        var error = result.Errors.First();
        return Result.Failure<UserResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> DeletaUserAsync(string UserId)
    {
        if (await manager.FindByIdAsync(UserId) is not { } user)
            return Result.Failure(UserErrors.UserNotFound);

        var result = await manager.DeleteAsync(user);

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();

        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));

    }

    public async Task<Result> EndLockOutAsync(string UserId)
    {
        if (await manager.FindByIdAsync(UserId) is not { } user)
            return Result.Failure(UserErrors.UserNotFound);

        var result = await manager.SetLockoutEndDateAsync(user, null);

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();
        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result<IEnumerable<UserResponse>>> GetAllUsers()
    {
        var users = await (from u in dbcontext.Users
               join ur in dbcontext.UserRoles
               on u.Id equals ur.UserId
               join r in dbcontext.Roles
               on ur.RoleId equals r.Id into roles
               select new
               {
                   u.Id,
                   u.FullName,
                   u.Address,
                   u.Email,
                   u.IsDisable,
                   u.EmailConfirmed,
                   u.PhoneNumber,
                   role = roles.Select(r => r.Name!).FirstOrDefault(),
                   u.AvatarUrl,
                   u.Bio,
                   u.DateOfBirth,
                   u.Nationality,
                   u.CreatedAt,
                   u.LastLoginAt
               })
                  .GroupBy(x => new { x.Id, x.FullName, x.Address, x.Email, x.IsDisable , x.EmailConfirmed , x.PhoneNumber , x.AvatarUrl , x.Bio , x.DateOfBirth , x.Nationality , x.CreatedAt , x.LastLoginAt})
                  .Select(c => new UserResponse(
                      c.Key.Id,
                      c.Key.FullName,
                      c.Key.Address,
                      c.Key.Email,
                      c.Key.IsDisable,
                      c.Select(x => x.role).FirstOrDefault()!,
                      c.Key.EmailConfirmed,
                      c.Key.PhoneNumber,
                      c.Key.AvatarUrl,
                      c.Key.Bio,
                      c.Key.DateOfBirth,
                      c.Key.Nationality,
                      c.Key.CreatedAt,
                      c.Key.LastLoginAt
                      ))
                  .ToListAsync();

        if(users.Count() == 0)
            return Result.Failure<IEnumerable<UserResponse>>(UserErrors.UserNotFound);

            return Result.Success<IEnumerable<UserResponse>>(users);
        
    }

    public async Task<Result<UserResponse>> GetUser2Async(string UserName)
    {
        if (await manager.FindByNameAsync(UserName) is not { } user)
            return Result.Failure<UserResponse>(UserErrors.UserNotFound);

        var userroles = await manager.GetRolesAsync(user);

        var response = (user, userroles).Adapt<UserResponse>();

        return Result.Success(response);
    }

    public async Task<Result<UserResponse>> GetUserAsync(string Id)
    {
        if (await manager.FindByIdAsync(Id) is not { } user)
            return Result.Failure<UserResponse>(UserErrors.UserNotFound);

        var userroles = await manager.GetRolesAsync(user);

        var response = new UserResponse
        (
            Id: user.Id,
            FullName: user.FullName!,
            Address: user.Address!,
            Email: user.Email!,
            IsDisable: user.IsDisable,
            Role: userroles.FirstOrDefault()!,
            user.EmailConfirmed,
            PhoneNumber: user.PhoneNumber!,
            AvatarUrl: user.AvatarUrl,
            Bio: user.Bio,
            DateOfBirth: user.DateOfBirth,
            user.Nationality,
            user.CreatedAt,
            user.LastLoginAt);

        return Result.Success(response);
    }

    public async Task<Result> ToggleStatusAsync(string UserId)
    {
        if (await manager.FindByIdAsync(UserId) is not { } user)
            return Result.Failure(UserErrors.UserNotFound);

        user.IsDisable = !user.IsDisable;

        var result = await manager.UpdateAsync(user);
        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();
        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> UpdateUserAsync(string userId, UpdateUserRequest request)
    {
        using var transaction = await dbcontext.Database.BeginTransactionAsync();
        try
        {
            // Find user
            if (await manager.FindByIdAsync(userId) is not { } user)
                return Result.Failure(UserErrors.UserNotFound);

            // Check duplicate email only if email changed
            if (user.Email != request.Email)
            {
                var duplicatedEmail = await manager.Users
                    .AnyAsync(c => c.Email == request.Email && c.Id != userId);
                if (duplicatedEmail)
                    return Result.Failure(UserErrors.EmailAlreadyExist);
            }

            // Validate role
            var allowedRoles = await roleService.GetRolesAsync();
            if (!allowedRoles.Value.Any(r => r.Name == request.Role))
                return Result.Failure(RolesErrors.InvalidRoles);

            // Update user properties explicitly
            user.Email = request.Email;
            user.NormalizedEmail = request.Email.ToUpperInvariant();
            user.UserName = request.Email;
            user.NormalizedUserName = request.Email.ToUpperInvariant();
            user.FullName = request.UserFullName;
            user.Address = request.UserAddress;

            // Update user first
            var updateResult = await manager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var error = updateResult.Errors.First();
                return Result.Failure(new Error(error.Code, error.Description,
                    StatusCodes.Status400BadRequest));
            }

            // Update password if provided
            if (!string.IsNullOrEmpty(request.Password))
            {
                await manager.RemovePasswordAsync(user);
                var passwordResult = await manager.AddPasswordAsync(user, request.Password);
                if (!passwordResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return Result.Failure(new Error("PasswordUpdateFailed",
                        passwordResult.Errors.First().Description,
                        StatusCodes.Status400BadRequest));
                }
            }

            // Update roles
            var currentRoles = await manager.GetRolesAsync(user);
            await manager.RemoveFromRolesAsync(user, currentRoles);
            var roleResult = await manager.AddToRoleAsync(user, request.Role);
            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return Result.Failure(new Error("RoleUpdateFailed",
                    roleResult.Errors.First().Description,
                    StatusCodes.Status400BadRequest));
            }

            await transaction.CommitAsync();
            return Result.Success();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

}
