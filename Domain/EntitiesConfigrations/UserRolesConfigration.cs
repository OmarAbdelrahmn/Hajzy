using Application.Abstraction.Consts;
using Domain.Consts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class UserRolesConfigration : IEntityTypeConfiguration<IdentityUserRole<string>>
{
    public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
    {

        builder.HasData(
            new IdentityUserRole<string>
            {
                UserId = DefaultUsers.HotelAdminId,
                RoleId = DefaultRoles.HotelAdminRoleId
            },
            new IdentityUserRole<string>
            {
                UserId = DefaultUsers.CityAdminId,
                RoleId = DefaultRoles.CityAdminRoleId
            },
            new IdentityUserRole<string>
            {
                UserId = DefaultUsers.SuperAdminId,
                RoleId = DefaultRoles.SuperAdminRoleId
            }
        );

    }
}

