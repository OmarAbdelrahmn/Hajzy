using System.ComponentModel.DataAnnotations;

namespace Application.Contracts.Auth;

public record ForgetPasswordRequest
([EmailAddress]
    [Required]
    string Email);
