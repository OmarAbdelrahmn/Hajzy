using Application.Abstraction;
using Application.Contracts.Roles;

namespace Application.Roles;

public interface IRoleService
{
    Task<Result<IEnumerable<RolesResponse>>> GetRolesAsync(bool? IncludeDisable = true);
    Task<Result> ToggleStatusAsync(string RoleName);
    Task<Result> UpdateRoleAsync(string Id, RoleRequest request);
}
