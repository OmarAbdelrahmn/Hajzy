namespace Application.Contracts.User;

public record ChangePasswordRequest
(
    string CurrentPassword,
    string NewPassord
    );
public record ChangeUserRoleRequest
(string Email, string NewRole
    );
