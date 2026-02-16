using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;


public class DepartmentImageConfiguration : IEntityTypeConfiguration<DepartmentImage>
{
    public void Configure(EntityTypeBuilder<DepartmentImage> builder)
    {
        builder.HasKey(di => di.Id);

        builder.HasIndex(di => new { di.DepartmentId, di.IsPrimary, di.IsDeleted })
            .HasDatabaseName("IX_DepartmentImage_Dept_Primary");

        builder.HasIndex(di => new { di.DepartmentId, di.DisplayOrder, di.IsDeleted })
            .HasDatabaseName("IX_DepartmentImage_Dept_DisplayOrder");

        builder.HasIndex(di => di.S3Key)
            .IsUnique()
            .HasDatabaseName("IX_DepartmentImage_S3Key");

        builder.Property(di => di.ImageUrl)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(di => di.S3Key)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasOne(di => di.Department)
            .WithMany(d => d.DepartmentImages)
            .HasForeignKey(di => di.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(di => di.UploadedBy)
            .WithMany()
            .HasForeignKey(di => di.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}