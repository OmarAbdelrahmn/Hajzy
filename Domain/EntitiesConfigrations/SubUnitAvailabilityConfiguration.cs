using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.EntitiesConfigrations;

public class SubUnitAvailabilityConfiguration : IEntityTypeConfiguration<SubUnitAvailability>
{
    public void Configure(EntityTypeBuilder<SubUnitAvailability> builder)
    {
        builder.HasKey(sa => sa.Id);

        // CRITICAL: Unique constraint on SubUnit + Date
        builder.HasIndex(sa => new { sa.SubUnitId, sa.StartDate,sa.EndDate })
            .IsUnique()
            .HasDatabaseName("IX_SubUnitAvailability_SubUnitId_Date");

        // PERFORMANCE: Composite index for availability queries
        builder.HasIndex(sa => new { sa.SubUnitId, sa.StartDate, sa.EndDate, sa.IsAvailable })
            .HasDatabaseName("IX_SubUnitAvailability_Search");

        // PERFORMANCE: Index for date range queries
        builder.HasIndex(sa => new { sa.StartDate, sa.EndDate, sa.IsAvailable })
            .IncludeProperties(sa => new { sa.SubUnitId, sa.SpecialPrice })
            .HasDatabaseName("IX_SubUnitAvailability_DateRange");

        builder.Property(sa => sa.SpecialPrice)
            .HasPrecision(18, 2);

        builder.Property(sa => sa.WeekendPrice)
            .HasPrecision(18, 2);


        builder.HasOne(sa => sa.UpdatedBy)
            .WithMany()
            .HasForeignKey(sa => sa.UpdatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
