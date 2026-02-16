using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;


public class ReviewImageConfiguration : IEntityTypeConfiguration<ReviewImage>
{
    public void Configure(EntityTypeBuilder<ReviewImage> builder)
    {
        builder.HasKey(ri => ri.Id);

        builder.HasIndex(ri => new { ri.ReviewId, ri.DisplayOrder, ri.IsDeleted })
            .HasDatabaseName("IX_ReviewImage_Review_DisplayOrder");

        builder.HasIndex(ri => ri.S3Key)
            .IsUnique()
            .HasDatabaseName("IX_ReviewImage_S3Key");

        builder.Property(ri => ri.ImageUrl)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(ri => ri.S3Key)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasOne(ri => ri.Review)
            .WithMany(r => r.Images)
            .HasForeignKey(ri => ri.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ri => ri.UploadedBy)
            .WithMany()
            .HasForeignKey(ri => ri.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
