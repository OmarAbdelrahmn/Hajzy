namespace Application.Contracts.Admin;

public record CreateUserRequest
(
    string Email,
    string Password,
    string UserFullName,
    string UserAddress,
    string Role
    );
