

using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; } = string.Empty;
    public string? Address { get; set; } = string.Empty;
    public bool IsDisable { get; set; }


    public string? AvatarUrl { get; set; }
    public DateTime? AvatarUploadedAt { get; set; }

    public string? Bio { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Nationality { get; set; }

    public string? City { get; set; }   
    public string? Country { get; set; }    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    public List<RefreshToken> RefreshTokens { get; set; } = [];

}
