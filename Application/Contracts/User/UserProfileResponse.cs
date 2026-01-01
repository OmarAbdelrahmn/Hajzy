namespace Application.Contracts.User;

public record UserProfileResponse
(
    string Email,
    string FullName,
    string UserAddress,
    string PhoneNumber
    );
