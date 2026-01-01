using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.EntitiesConfigrations;


public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(n => n.CreatedByRole)
            .HasMaxLength(50);

        builder.HasIndex(n => n.Type);
        builder.HasIndex(n => n.Target);
        builder.HasIndex(n => n.Priority);
        builder.HasIndex(n => n.IsSent);
        builder.HasIndex(n => n.IsScheduled);
        builder.HasIndex(n => n.ScheduledFor);
        builder.HasIndex(n => n.CreatedByUserId);
        builder.HasIndex(n => n.CreatedAt);

        builder.HasIndex(n => new { n.IsScheduled, n.ScheduledFor, n.IsSent })
           .HasFilter("[IsDeleted] = 0");

        // ADD: Index for retry processing
        builder.HasIndex(n => new { n.IsSent, n.RetryCount, n.LastRetryAt });
        // ADD: Index for expiration cleanup
        builder.HasIndex(n => n.ExpiresAt)
            .HasFilter("[IsDeleted] = 0");


        builder.HasOne(n => n.CreatedBy)
            .WithMany()
            .HasForeignKey(n => n.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(n => n.TargetDepartment)
            .WithMany()
            .HasForeignKey(n => n.TargetDepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(n => n.TargetUnit)
            .WithMany()
            .HasForeignKey(n => n.TargetUnitId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
