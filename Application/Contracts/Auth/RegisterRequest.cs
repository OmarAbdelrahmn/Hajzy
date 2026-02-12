namespace Application.Contracts.Auth;

public record Registerrequest
(
    string Email,
    string Password,
    string FullName,
    string PhoneNumber,
    string? City,
    string Country
    );
