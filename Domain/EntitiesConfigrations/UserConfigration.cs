using Domain.Consts;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class UserConfigration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasData(new ApplicationUser
        {
            Id = DefaultUsers.HotelAdminId,
            UserName = DefaultUsers.HotelAdminName,
            Email = DefaultUsers.HotelAdminEmail,
            NormalizedEmail = DefaultUsers.HotelAdminEmail.ToUpper(),
            NormalizedUserName = DefaultUsers.HotelAdminName.ToUpper(),
            EmailConfirmed = true,
            PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(null!, "P@ssword1234"),
            SecurityStamp = DefaultUsers.HotelAdminSecurityStamp,
            ConcurrencyStamp = DefaultUsers.HotelAdminConcurrencyStamp,
            FullName = "hotel-Admin",
            Address = "lives in hotel details"
        });
        builder.HasData(new ApplicationUser
        {
            Id = DefaultUsers.CityAdminId,
            UserName = DefaultUsers.CityAdminName,
            Email = DefaultUsers.CityAdminEmail,
            NormalizedEmail = DefaultUsers.CityAdminEmail.ToUpper(),
            NormalizedUserName = DefaultUsers.CityAdminName.ToUpper(),
            EmailConfirmed = true,
            PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(null!, "P@ssword1234"),
            SecurityStamp = DefaultUsers.CityAdminSecurityStamp,
            ConcurrencyStamp = DefaultUsers.CityAdminConcurrencyStamp,
            FullName = "city-Admin",
            Address = "lives in city details"
        });

        builder.HasData(new ApplicationUser
        {
            Id = DefaultUsers.SuperAdminId,
            UserName = DefaultUsers.SuperAdminName,
            Email = DefaultUsers.SuperAdminEmail,
            NormalizedEmail = DefaultUsers.SuperAdminEmail.ToUpper(),
            NormalizedUserName = DefaultUsers.SuperAdminName.ToUpper(),
            EmailConfirmed = true,
            PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(null!, "P@ssword1234"),
            SecurityStamp = DefaultUsers.SuperAdminSecurityStamp,
            ConcurrencyStamp = DefaultUsers.SuperAdminConcurrencyStamp,
            FullName = "super-Admin",
            Address = "lives in every details"
        });

    }
}

