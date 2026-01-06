namespace Application.Contracts.Admin;

public record UserResponse
(
    string Id,
    string FullName,
    string Address,
    string Email,
    bool IsDisable,
    string Role,
    bool IsEmailConfirmed,
    string PhoneNumber,
    string? AvatarUrl,
    string? Bio,
    DateTime? DateOfBirth,
    string? Nationality,
    DateTime CreatedAt,
    DateTime? LastLoginAt
    );
