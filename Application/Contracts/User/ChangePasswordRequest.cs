namespace Application.Contracts.User;


public record ChangeUserRoleRequest
(string Email, string NewRole
    );
