using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;


public class UnitImageConfiguration : IEntityTypeConfiguration<UnitImage>
{
    public void Configure(EntityTypeBuilder<UnitImage> builder)
    {
        builder.HasKey(ui => ui.Id);

        // PERFORMANCE: Composite indexes
        builder.HasIndex(ui => new { ui.UnitId, ui.IsPrimary, ui.IsDeleted })
            .HasDatabaseName("IX_UnitImage_Unit_Primary");

        builder.HasIndex(ui => new { ui.UnitId, ui.DisplayOrder, ui.IsDeleted })
            .HasDatabaseName("IX_UnitImage_Unit_DisplayOrder");

        builder.HasIndex(ui => new { ui.UnitId, ui.ImageType, ui.IsDeleted })
            .HasDatabaseName("IX_UnitImage_Unit_Type");

        builder.HasIndex(ui => ui.S3Key)
            .IsUnique()
            .HasDatabaseName("IX_UnitImage_S3Key");

        builder.HasIndex(ui => ui.ProcessingStatus)
            .HasFilter("[ProcessingStatus] != 2") // Not Completed
            .HasDatabaseName("IX_UnitImage_ProcessingStatus");

        builder.Property(ui => ui.ImageUrl)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(ui => ui.S3Key)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ui => ui.S3Bucket)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(ui => ui.Unit)
            .WithMany(u => u.Images)
            .HasForeignKey(ui => ui.UnitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ui => ui.UploadedBy)
            .WithMany()
            .HasForeignKey(ui => ui.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ui => ui.DeletedBy)
            .WithMany()
            .HasForeignKey(ui => ui.DeletedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
