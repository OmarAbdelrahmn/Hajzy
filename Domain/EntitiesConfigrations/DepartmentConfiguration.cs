using Domain.Consts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;


public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Country)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.HasIndex(c => c.Name);
        builder.HasIndex(c => c.IsActive);

        // Seed data
        builder.HasData(
[
    new Department
    {
        Id = DefaultYemenDepartments.AbyanId,
        Name = DefaultYemenDepartments.AbyanName,
        Country = "Yemen",
        Description = "Governorate in southern Yemen",
        IsActive = true,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    },
    new Department
    {
        Id = DefaultYemenDepartments.AdenId,
        Name = DefaultYemenDepartments.AdenName,
        Country = "Yemen",
        Description = "Port city and temporary capital of Yemen",
        IsActive = true,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    },
    new Department
    {
        Id = DefaultYemenDepartments.AlBaydaId,
        Name = DefaultYemenDepartments.AlBaydaName,
        Country = "Yemen",
        Description = "Central Yemeni governorate",
        IsActive = true,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    },
    new Department
    {
        Id = DefaultYemenDepartments.AlDhaleId,
        Name = DefaultYemenDepartments.AlDhaleName,
        Country = "Yemen",
        Description = "Southern highlands governorate",
        IsActive = true,
        CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    },
    new Department
    {
        Id = DefaultYemenDepartments.AlHudaydahId,
        Name = DefaultYemenDepartments.AlHudaydahName,
        Country = "Yemen",
        Description = "Red Sea coastal governorate",
        IsActive = true,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    },
    new Department
    {
        Id = DefaultYemenDepartments.AlJawfId,
        Name = DefaultYemenDepartments.AlJawfName,
        Country = "Yemen",
        Description = "Northeastern desert governorate",
        IsActive = true,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    },
    new Department
    {
        Id = DefaultYemenDepartments.AlMahrahId,
        Name = DefaultYemenDepartments.AlMahrahName,
        Country = "Yemen",
        Description = "Easternmost governorate of Yemen",
        IsActive = true,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    },
    new Department
    {
        Id = DefaultYemenDepartments.AlMahwitId,
        Name = DefaultYemenDepartments.AlMahwitName,
        Country = "Yemen",
        Description = "Mountainous northwestern governorate",
        IsActive = true,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    },
    new Department
    {
        Id = DefaultYemenDepartments.AmanatAlAsimahId,
        Name = DefaultYemenDepartments.AmanatAlAsimahName,
        Country = "Yemen",
        Description = "Capital city area (Sana'a City)",
        IsActive = true,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    },
    new Department
    {
        Id = DefaultYemenDepartments.AmranId,
        Name = DefaultYemenDepartments.AmranName,
        Country = "Yemen",
        Description = "Northern governorate near Sana'a",
        IsActive = true,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    },
    new Department
    {
        Id = DefaultYemenDepartments.DhamarId,
        Name = DefaultYemenDepartments.DhamarName,
        Country = "Yemen",
        Description = "Highland agricultural governorate",
        IsActive = true,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    },
    new Department
    {
        Id = DefaultYemenDepartments.HadhramautId,
        Name = DefaultYemenDepartments.HadhramautName,
        Country = "Yemen",
        Description = "Largest governorate by area",
        IsActive = true,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    },
    new Department
    {
        Id = DefaultYemenDepartments.HajjahId,
        Name = DefaultYemenDepartments.HajjahName,
        Country = "Yemen",
        Description = "Northwestern mountainous governorate",
        IsActive = true,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    },
    new Department
    {
        Id = DefaultYemenDepartments.IbbId,
        Name = DefaultYemenDepartments.IbbName,
        Country = "Yemen",
        Description = "One of the greenest regions in Yemen",
        IsActive = true,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    },
    new Department
    {
        Id = DefaultYemenDepartments.LahijId,
        Name = DefaultYemenDepartments.LahijName,
        Country = "Yemen",
        Description = "Agricultural governorate near Aden",
        IsActive = true,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    },
    new Department
    {
        Id = DefaultYemenDepartments.MaribId,
        Name = DefaultYemenDepartments.MaribName,
        Country = "Yemen",
        Description = "Historic governorate with ancient ruins",
        IsActive = true,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    },
    new Department
    {
        Id = DefaultYemenDepartments.RaymahId,
        Name = DefaultYemenDepartments.RaymahName,
        Country = "Yemen",
        Description = "Mountainous agricultural governorate",
        IsActive = true,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    },
    new Department
    {
        Id = DefaultYemenDepartments.SaadaId,
        Name = DefaultYemenDepartments.SaadaName,
        Country = "Yemen",
        Description = "Northernmost governorate",
        IsActive = true,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    },
    new Department
    {
        Id = DefaultYemenDepartments.ShabwahId,
        Name = DefaultYemenDepartments.ShabwahName,
        Country = "Yemen",
        Description = "Oil-rich governorate in southeastern Yemen",
        IsActive = true,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    },
    new Department
    {
        Id = DefaultYemenDepartments.SocotraId,
        Name = DefaultYemenDepartments.SocotraName,
        Country = "Yemen",
        Description = "UNESCO World Heritage archipelago",
        IsActive = true,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    },
    new Department
    {
        Id = DefaultYemenDepartments.TaizId,
        Name = DefaultYemenDepartments.TaizName,
        Country = "Yemen",
        Description = "Cultural and industrial center of Yemen",
        IsActive = true,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    }
]);

    }
}
