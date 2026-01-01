using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.EntitiesConfigrations;


public class SubUnitConfiguration : IEntityTypeConfiguration<SubUnit>
{
    public void Configure(EntityTypeBuilder<SubUnit> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.RoomNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Type)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.Property(r => r.PricePerNight)
            .HasPrecision(18, 2);

        builder.HasIndex(r => new { r.UnitId, r.RoomNumber })
            .IsUnique();

        builder.HasOne(r => r.Unit)
            .WithMany(u => u.Rooms)
            .HasForeignKey(r => r.UnitId)
            .OnDelete(DeleteBehavior.Cascade);

        //// BookingConfiguration.cs
        //builder.HasIndex(b => new { b.CheckInDate, b.CheckOutDate, b.Status })
        //    .HasFilter("[IsDeleted] = 0");

        // SubUnitConfiguration.cs  
        builder.HasIndex(s => new { s.UnitId, s.IsAvailable })
            .HasFilter("[IsDeleted] = 0");
    }
}
