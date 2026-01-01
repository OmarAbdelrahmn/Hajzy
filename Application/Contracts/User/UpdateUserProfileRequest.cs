namespace Application.Contracts.User;

public record UpdateUserProfileRequest
(
    string UserFullName,
    string UserAddress,
    string PhoneNumber
    );
