using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;


public class SubUnitImageConfiguration : IEntityTypeConfiguration<SubUnitImage>
{
    public void Configure(EntityTypeBuilder<SubUnitImage> builder)
    {
        builder.HasKey(si => si.Id);

        builder.HasIndex(si => new { si.SubUnitId, si.IsPrimary, si.IsDeleted })
            .HasDatabaseName("IX_SubUnitImage_SubUnit_Primary");

        builder.HasIndex(si => new { si.SubUnitId, si.DisplayOrder, si.IsDeleted })
            .HasDatabaseName("IX_SubUnitImage_SubUnit_DisplayOrder");

        builder.HasIndex(si => new { si.SubUnitId, si.ImageType, si.IsDeleted })
            .HasDatabaseName("IX_SubUnitImage_SubUnit_Type");

        builder.HasIndex(si => si.S3Key)
            .IsUnique()
            .HasDatabaseName("IX_SubUnitImage_S3Key");

        builder.Property(si => si.ImageUrl)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(si => si.S3Key)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasOne(si => si.SubUnit)
            .WithMany(s => s.SubUnitImages)
            .HasForeignKey(si => si.SubUnitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(si => si.UploadedBy)
            .WithMany()
            .HasForeignKey(si => si.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
