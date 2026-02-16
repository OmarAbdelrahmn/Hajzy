using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;


public class BookingRoomConfiguration : IEntityTypeConfiguration<BookingRoom>
{
    public void Configure(EntityTypeBuilder<BookingRoom> builder)
    {
        builder.HasKey(br => new { br.BookingId, br.RoomId });

        builder.Property(br => br.PricePerNight)
            .HasPrecision(18, 2);

        builder.HasOne(br => br.Booking)
            .WithMany(b => b.BookingRooms)
            .HasForeignKey(br => br.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(br => br.Room)
            .WithMany(r => r.BookingRooms)
            .HasForeignKey(br => br.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(br => new { br.RoomId, br.BookingId })
            .IsUnique();
    }
}

