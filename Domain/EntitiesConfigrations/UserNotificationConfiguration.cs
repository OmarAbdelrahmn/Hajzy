using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.EntitiesConfigrations;


public class UserNotificationConfiguration : IEntityTypeConfiguration<UserNotification>
{
    public void Configure(EntityTypeBuilder<UserNotification> builder)
    {
        builder.HasKey(un => un.Id);

        builder.Property(un => un.UserId)
            .IsRequired();

        builder.HasIndex(un => new { un.UserId, un.IsRead });
        builder.HasIndex(un => un.NotificationId);
        builder.HasIndex(un => un.ReceivedAt);

        builder.HasOne(un => un.Notification)
            .WithMany(n => n.UserNotifications)
            .HasForeignKey(un => un.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(un => un.User)
            .WithMany()
            .HasForeignKey(un => un.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}