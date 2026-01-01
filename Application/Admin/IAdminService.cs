using Application.Abstraction;
using Application.Contracts.Admin;


namespace Application.Admin;

public interface IAdminService
{
    Task<Result<IEnumerable<UserResponse>>> GetAllUsers();
    Task<Result<UserResponse>> GetUserAsync(string Id);
    Task<Result<UserResponse>> GetUser2Async(string UserName);
    Task<Result<UserResponse>> AddUserAsync(CreateUserRequest request);
    Task<Result> UpdateUserAsync(string UserId, UpdateUserRequest request);
    Task<Result> ToggleStatusAsync(string UserId);
    Task<Result> EndLockOutAsync(string UserId);
    Task<Result> DeletaUserAsync(string UserId);
}
