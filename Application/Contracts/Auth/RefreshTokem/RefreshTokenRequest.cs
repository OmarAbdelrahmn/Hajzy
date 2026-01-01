namespace Application.Contracts.Auth.RefreshTokem;

public record RefreshTokenRequest
(
    string Token,
    string RefreshToken
    );
