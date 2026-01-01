using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.EntitiesConfigrations;


public class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.HasKey(u => u.Id);

        builder.HasIndex(u => new { u.CityId, u.IsActive, u.IsVerified })
            .HasFilter("[IsDeleted] = 0");

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.Description)
            .IsRequired();

        builder.Property(u => u.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.BasePrice)
            .HasPrecision(18, 2);

        builder.Property(u => u.Latitude)
            .HasPrecision(10, 8);

        builder.Property(u => u.Longitude)
            .HasPrecision(11, 8);

        builder.Property(u => u.AverageRating)
            .HasPrecision(3, 2);

        builder.HasIndex(u => u.Name);
        builder.HasIndex(u => u.CityId);
        builder.HasIndex(u => u.UnitTypeId);
        builder.HasIndex(u => u.IsActive);
        builder.HasIndex(u => u.IsVerified);
        builder.HasIndex(u => u.AverageRating);

        builder.HasIndex(u => new { u.Latitude, u.Longitude });



    }
}
