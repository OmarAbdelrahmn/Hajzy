using System.ComponentModel.DataAnnotations;

namespace Application.Contracts.Roles;

public record RoleRequest
(
    [Length(3,20)]
    [Required]
    string Name
    );
