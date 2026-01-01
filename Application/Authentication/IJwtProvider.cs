using Domain.Entities;

namespace Application.Authentication;

public interface IJwtProvider
{
    (string Token, int Expiry) GenerateToken(ApplicationUser user, string role);

    string? ValidateToken(string token);
}
