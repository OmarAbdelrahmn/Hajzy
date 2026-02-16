using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;


public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {

        builder.HasIndex(b => new { b.UserId, b.Status, b.CheckInDate })
            .IncludeProperties(b => new { b.TotalPrice, b.PaymentStatus })
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(b => new { b.UnitId, b.CheckInDate, b.CheckOutDate })
            .HasFilter("[IsDeleted] = 0");


        builder.HasKey(b => b.Id);

        builder.Property(b => b.BookingNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(b => b.UserId)
            .IsRequired();

        builder.Property(b => b.TotalPrice)
            .HasPrecision(18, 2);

        builder.Property(b => b.PaidAmount)
            .HasPrecision(18, 2);

        builder.Property(b => b.SpecialRequests)
            .HasMaxLength(1000);

        builder.Property(b => b.CancellationReason)
            .HasMaxLength(1000);

        builder.HasIndex(b => b.BookingNumber)
            .IsUnique();
        builder.HasIndex(b => b.UserId);
        builder.HasIndex(b => b.UnitId);
        builder.HasIndex(b => b.Status);
        builder.HasIndex(b => b.PaymentStatus);
        builder.HasIndex(b => new { b.CheckInDate, b.CheckOutDate });

        builder.HasOne(b => b.Unit)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.UnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => b.CheckInDate);
        builder.HasIndex(b => b.CheckOutDate);
        builder.HasIndex(b => b.CreatedAt);

    }
}
