using Application.Abstraction.Consts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class RolesConfigration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {

        builder.HasData(
            [
                new ApplicationRole
                {
                    Id = DefaultRoles.UserRoleId,
                    Name = DefaultRoles.User,
                    ConcurrencyStamp = DefaultRoles.UserRoleConcurrencyStamp,
                    NormalizedName = DefaultRoles.User.ToUpper(),
                    IsDefault = true,
                    IsDeleted = false
                },
                new ApplicationRole
                {
                    Id = DefaultRoles.HotelAdminRoleId,
                    Name = DefaultRoles.HotelAdmin,
                    ConcurrencyStamp = DefaultRoles.HotelAdminRoleConcurrencyStamp,
                    NormalizedName = DefaultRoles.HotelAdmin.ToUpper(),
                    IsDefault = false,
                    IsDeleted = false
                },
                 new ApplicationRole
                {
                    Id = DefaultRoles.CityAdminRoleId,
                    Name = DefaultRoles.CityAdmin,
                    ConcurrencyStamp = DefaultRoles.CityAdminRoleConcurrencyStamp,
                    NormalizedName = DefaultRoles.CityAdmin.ToUpper(),
                    IsDefault = false,
                    IsDeleted = false
                },
                 new ApplicationRole
                {
                    Id = DefaultRoles.SuperAdminRoleId,
                    Name = DefaultRoles.SuperAdmin,
                    ConcurrencyStamp = DefaultRoles.SuperAdminRoleConcurrencyStamp,
                    NormalizedName = DefaultRoles.SuperAdmin.ToUpper(),
                    IsDefault = false,
                    IsDeleted = false
                }
            ]
        );

    }
}

