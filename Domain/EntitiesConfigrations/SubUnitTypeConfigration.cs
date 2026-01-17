using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.EntitiesConfigrations;

public class SubUnitTypeConfigration : IEntityTypeConfiguration<SubUnitTypee>
{
    public void Configure(EntityTypeBuilder<SubUnitTypee> builder)
    {
        builder.HasData(
    // ===== HOTELS =====
        new SubUnitTypee { Id = 1, Name = SubUnitType.StandardRoom.ToString(), Description = "Standard hotel room" },
        new SubUnitTypee { Id = 2, Name = SubUnitType.DeluxeRoom.ToString(), Description = "Deluxe hotel room with extra amenities" },
        new SubUnitTypee { Id = 3, Name = SubUnitType.Suite.ToString(), Description = "Spacious hotel suite" },
        new SubUnitTypee { Id = 4, Name = SubUnitType.ExecutiveSuite.ToString(), Description = "Executive-level suite" },
        new SubUnitTypee { Id = 5, Name = SubUnitType.PenthouseSuite.ToString(), Description = "Luxury penthouse suite" },

        // ===== VILLAS & RESORTS =====
        new SubUnitTypee { Id = 6, Name = SubUnitType.Villa.ToString(), Description = "Private standalone villa" },
        new SubUnitTypee { Id = 7, Name = SubUnitType.Cottage.ToString(), Description = "Small vacation cottage" },
        new SubUnitTypee { Id = 8, Name = SubUnitType.Bungalow.ToString(), Description = "Single-story bungalow" },
        new SubUnitTypee { Id = 9, Name = SubUnitType.Cabin.ToString(), Description = "Wooden or nature cabin" },

        // ===== ALTERNATIVE =====
        new SubUnitTypee { Id = 10, Name = SubUnitType.Apartment.ToString(), Description = "Fully furnished apartment" },
        new SubUnitTypee { Id = 11, Name = SubUnitType.Studio.ToString(), Description = "Studio-style unit" },

        // ===== CAMPING / OUTDOOR =====
        new SubUnitTypee { Id = 12, Name = SubUnitType.TentSite.ToString(), Description = "Outdoor tent site" },
        new SubUnitTypee { Id = 13, Name = SubUnitType.GlamingTent.ToString(), Description = "Luxury glamping tent" },
        new SubUnitTypee { Id = 14, Name = SubUnitType.RVSpace.ToString(), Description = "Space for recreational vehicles" },

        // ===== EVENT SPACES =====
        new SubUnitTypee { Id = 15, Name = SubUnitType.Hall.ToString(), Description = "Event or banquet hall" },
        new SubUnitTypee { Id = 16, Name = SubUnitType.Chalet.ToString(), Description = "Mountain or resort chalet" },

        // ===== HOSTELS =====
        new SubUnitTypee { Id = 17, Name = SubUnitType.DormBed.ToString(), Description = "Bed in shared dormitory" },
        new SubUnitTypee { Id = 18, Name = SubUnitType.PrivateRoom.ToString(), Description = "Private room in shared property" }
);

    }

}
