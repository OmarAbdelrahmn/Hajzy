using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.EntitiesConfigrations;

// Configuration
public class GeneralPolicyConfiguration : IEntityTypeConfiguration<GeneralPolicy>
{
    public void Configure(EntityTypeBuilder<GeneralPolicy> builder)
    {
        builder.HasKey(gp => gp.Id);

        builder.Property(gp => gp.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(gp => gp.Description)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(gp => gp.CustomPolicyName)
            .HasMaxLength(100);

        //builder.Property(gp => gp.TitleAr)
        //    .HasMaxLength(200);

        //builder.Property(gp => gp.DescriptionAr)
        //    .HasMaxLength(5000);

        //// JSON storage for flexible policy data
        //builder.Property(gp => gp.PolicyData)
        //    .HasColumnType("nvarchar(max)");

        //// PERFORMANCE: Index for queries
        //builder.HasIndex(gp => new { gp.UnitId, gp.Category, gp.IsActive })
        //    .HasDatabaseName("IX_GeneralPolicy_Unit_Category");

        //builder.HasIndex(gp => new { gp.SubUnitId, gp.Category, gp.IsActive })
        //    .HasDatabaseName("IX_GeneralPolicy_SubUnit_Category");

        //builder.HasIndex(gp => new { gp.Category, gp.IsActive })
        //    .HasFilter("[UnitId] IS NULL AND [SubUnitId] IS NULL") // Global policies
        //    .HasDatabaseName("IX_GeneralPolicy_Global");

        //builder.HasIndex(gp => gp.DisplayOrder)
        //    .HasDatabaseName("IX_GeneralPolicy_DisplayOrder");

        // Relationships
        builder.HasOne(gp => gp.Unit)
            .WithMany()
            .HasForeignKey(gp => gp.UnitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(gp => gp.SubUnit)
            .WithMany()
            .HasForeignKey(gp => gp.SubUnitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(gp => gp.CancellationPolicy)
            .WithMany()
            .HasForeignKey(gp => gp.CancellationPolicyId)
            .OnDelete(DeleteBehavior.SetNull);

        //builder.HasOne(gp => gp.CreatedBy)
        //    .WithMany()
        //    .HasForeignKey(gp => gp.CreatedByUserId)
        //    .OnDelete(DeleteBehavior.SetNull);

        //builder.HasOne(gp => gp.UpdatedBy)
        //    .WithMany()
        //    .HasForeignKey(gp => gp.UpdatedByUserId)
        //    .OnDelete(DeleteBehavior.SetNull);
    }
}