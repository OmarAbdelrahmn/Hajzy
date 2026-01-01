//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Domain.EntitiesConfigrations;


//public class UnitAdminConfiguration : IEntityTypeConfiguration<UnitAdmin>
//{
//    public void Configure(EntityTypeBuilder<UnitAdmin> builder)
//    {
//        builder.HasKey(ua => ua.Id);

//        builder.Property(ua => ua.UserId)
//            .IsRequired();

//        builder.HasIndex(ua => new { ua.UserId, ua.UnitId })
//            .IsUnique();

//        builder.HasIndex(ua => ua.UnitId);

//        builder.HasOne(ua => ua.User)
//            .WithMany()
//            .HasForeignKey(ua => ua.UserId)
//            .OnDelete(DeleteBehavior.Restrict);

//        builder.HasOne(ua => ua.Unit)
//            .WithMany(u => u.UnitAdmins)
//            .HasForeignKey(ua => ua.UnitId)
//            .OnDelete(DeleteBehavior.Cascade);
//    }
//}