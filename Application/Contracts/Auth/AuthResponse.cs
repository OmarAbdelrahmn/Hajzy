namespace Application.Contracts.Auth;

public record AuthResponse
(
    string Id,
    string Email,
    string UserFullName,
    string UserAddress,
    string Token,
    int ExpiresIn,
    string RefreshToken,
    DateTime RefreshExpiresIn
    );
