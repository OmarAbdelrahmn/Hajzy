using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class UnitAmenityConfiguration : IEntityTypeConfiguration<UnitAmenity>
{
    public void Configure(EntityTypeBuilder<UnitAmenity> builder)
    {
        builder.HasKey(ua => new { ua.UnitId, ua.AmenityId });

        builder.HasOne(ua => ua.Unit)
            .WithMany(u => u.UnitAmenities)
            .HasForeignKey(ua => ua.UnitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ua => ua.Amenity)
            .WithMany(a => a.UnitAmenities)
            .HasForeignKey(ua => ua.AmenityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}