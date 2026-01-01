using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class UnitRegistrationRequestConfiguration : IEntityTypeConfiguration<UnitRegistrationRequest>
{
    public void Configure(EntityTypeBuilder<UnitRegistrationRequest> builder)
    {
        builder.HasKey(r => r.Id);

        // Owner Information
        builder.Property(r => r.OwnerFullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.OwnerEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(r => r.OwnerPhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(r => r.OwnerPassword)
            .IsRequired()
            .HasMaxLength(100);

        // Unit Information
        builder.Property(r => r.UnitName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Description)
            .IsRequired();

        builder.Property(r => r.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(r => r.Latitude)
            .HasPrecision(10, 8);

        builder.Property(r => r.Longitude)
            .HasPrecision(11, 8);

        builder.Property(r => r.BasePrice)
            .HasPrecision(18, 2);

        // Images stored as JSON
        builder.Property(r => r.ImageS3Keys)
            .IsRequired()
            .HasColumnType("nvarchar(max)"); // Allows large JSON arrays

        builder.Property(r => r.RejectionReason)
            .HasMaxLength(1000);

        // Indexes for performance
        builder.HasIndex(r => r.Status)
            .HasDatabaseName("IX_UnitRegistrationRequest_Status");

        builder.HasIndex(r => r.OwnerEmail)
            .HasDatabaseName("IX_UnitRegistrationRequest_OwnerEmail");

        builder.HasIndex(r => new { r.Status, r.SubmittedAt })
            .HasDatabaseName("IX_UnitRegistrationRequest_Status_SubmittedAt");

        builder.HasIndex(r => new { r.DepartmentId, r.Status })
            .HasDatabaseName("IX_UnitRegistrationRequest_Dept_Status");

        builder.HasIndex(r => r.SubmittedAt)
            .HasDatabaseName("IX_UnitRegistrationRequest_SubmittedAt");

        // Relationships
        builder.HasOne(r => r.Department)
           .WithMany()
           .HasForeignKey(r => r.DepartmentId)
           .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.UnitType)
            .WithMany()
            .HasForeignKey(r => r.UnitTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // 🔒 ADMIN → NO ACTION (audit safety)
        builder.HasOne(r => r.ReviewedByAdmin)
            .WithMany()
            .HasForeignKey(r => r.ReviewedByAdminId)
            .OnDelete(DeleteBehavior.NoAction);

        // 🔒 CREATOR → NO ACTION (history)
        builder.HasOne(r => r.CreatedUser)
            .WithMany()
            .HasForeignKey(r => r.CreatedUserId)
            .OnDelete(DeleteBehavior.NoAction);

        // 🟢 Unit can be deleted safely
        builder.HasOne(r => r.CreatedUnit)
            .WithMany()
            .HasForeignKey(r => r.CreatedUnitId)
            .OnDelete(DeleteBehavior.SetNull);

    }
}