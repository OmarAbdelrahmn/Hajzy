using Domain.Consts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.EntitiesConfigrations;

public class DepartmentAdminConfiguration : IEntityTypeConfiguration<DepartmentAdmin>
{
    public void Configure(EntityTypeBuilder<DepartmentAdmin> builder)
    {
        builder.HasKey(ca => ca.Id);

        builder.Property(ca => ca.UserId)
            .IsRequired();

        builder.HasIndex(ca => new { ca.UserId, ca.CityId })
            .IsUnique();

        builder.HasOne(ca => ca.User)
            .WithMany()
            .HasForeignKey(ca => ca.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ca => ca.City)
            .WithMany(c => c.DepartmentAdmins)
            .HasForeignKey(ca => ca.CityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ca => new { ca.UserId, ca.CityId });

        builder.HasIndex(ca => new { ca.CityId, ca.IsPrimary })
           .IsUnique()
           .HasFilter("[IsActive] = 1 AND [IsPrimary] = 1")
           .HasDatabaseName("IX_DepartmentAdmin_CityId_IsPrimary_Unique");

        // Index: Find active admins for a department
        builder.HasIndex(ca => new { ca.CityId, ca.IsActive })
            .HasDatabaseName("IX_DepartmentAdmin_CityId_IsActive");

        // Index: Check if user is already assigned as active admin
        builder.HasIndex(ca => new { ca.UserId, ca.IsActive })
            .HasFilter("[IsActive] = 1")
            .HasDatabaseName("IX_DepartmentAdmin_UserId_IsActive");


    }
}