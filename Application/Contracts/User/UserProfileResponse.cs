using System.ComponentModel.DataAnnotations;

namespace Application.Contracts.User;


// ============= RESPONSE MODELS =============

public record UserProfileResponse(
    string Email,
    string FullName,
    string Address,
    string PhoneNumber,
    string? AvatarUrl,
    string? Bio,
    DateTime? DateOfBirth,
    string? Nationality,
    DateTime CreatedAt,
    DateTime? LastLoginAt
);

// ============= REQUEST MODELS =============

public record UpdateUserProfileRequest
{
    [MaxLength(200)]
    public string? UserFullName { get; init; }

    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; init; }

    [MaxLength(500)]
    public string? UserAddress { get; init; }

    [MaxLength(1000)]
    public string? Bio { get; init; }

    public DateTime? DateOfBirth { get; init; }

    [MaxLength(100)]
    public string? Nationality { get; init; }
}

public record ChangePasswordRequest
{
    [Required]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; init; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [MinLength(6)]
    public string NewPassword { get; init; } = string.Empty;

    [Required]
    [Compare(nameof(NewPassword))]
    [DataType(DataType.Password)]
    public string ConfirmNewPassword { get; init; } = string.Empty;
}

public record ChangeRoleRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string NewRole { get; init; } = string.Empty;
}

// ============= AVATAR RESPONSE =============

public record AvatarResponse(
    string AvatarUrl,
    string? ThumbnailUrl,
    string? MediumUrl,
    string? LargeUrl,
    DateTime UploadedAt
);