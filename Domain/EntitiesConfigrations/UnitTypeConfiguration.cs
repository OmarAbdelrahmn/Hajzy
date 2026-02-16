using Domain.Consts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;


public class UnitTypeConfiguration : IEntityTypeConfiguration<UnitType>
{
    public void Configure(EntityTypeBuilder<UnitType> builder)
    {
        builder.HasKey(ut => ut.Id);

        builder.Property(ut => ut.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ut => ut.Description)
            .HasMaxLength(200);

        builder.HasIndex(ut => ut.Name);
        builder.HasIndex(ut => ut.IsActive);

        // Seed data
        builder.HasData(
            [
                new UnitType
                {
                    Id = DefaultUnitTypes.HotelId,
                    Name = "Hotel",
                    Description = "Traditional hotel accommodation with various amenities",
                    IsActive = true
                },
                new UnitType
                {
                    Id = DefaultUnitTypes.ApartmentId,
                    Name = "Apartment",
                    Description = "Self-contained apartment for longer stays",
                    IsActive = true
                },
                new UnitType
                {
                    Id = DefaultUnitTypes.VillaId,
                    Name = "Villa",
                    Description = "Luxury villa with private amenities",
                    IsActive = true
                },
                new UnitType
                {
                    Id = DefaultUnitTypes.ResortId,
                    Name = "Resort",
                    Description = "Full-service resort with comprehensive facilities",
                    IsActive = true
                },
                new UnitType
                {
                    Id = DefaultUnitTypes.HostelId,
                    Name = "Hostel",
                    Description = "Budget-friendly hostel accommodation",
                    IsActive = true
                },
                new UnitType
                {
                    Id = DefaultUnitTypes.GuestHouseId,
                    Name = "Guest House",
                    Description = "Cozy guest house with homely atmosphere",
                    IsActive = true
                },
                new UnitType
                {
                    Id = DefaultUnitTypes.hall,
                    Name = "Hall",
                    Description = "A wedding Hall",
                    IsActive = true
                },
                new UnitType
                {
                    Id = DefaultUnitTypes.shalet,
                    Name = "Chalet",
                    Description = "A Chalet",
                    IsActive = true
                },
                new UnitType
                {
                    Id = DefaultUnitTypes.Camp,
                    Name = "A Camp",
                    Description = "A Camp",
                    IsActive = true
                }
            ]
        );
    }
}