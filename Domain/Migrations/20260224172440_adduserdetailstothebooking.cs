using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class adduserdetailstothebooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GuestEmail",
                table: "Bookings",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestFirstName",
                table: "Bookings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestLastName",
                table: "Bookings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestPhone",
                table: "Bookings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 24, 17, 24, 32, 779, DateTimeKind.Utc).AddTicks(1997), "AQAAAAIAAYagAAAAEAQMFDKK1lyOPh+WQOF4hF3UQt2fNksyMaZ6MtPs4d1rxZzlgkKoG4R8MAkKHrTF4Q==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 24, 17, 24, 32, 956, DateTimeKind.Utc).AddTicks(400), "AQAAAAIAAYagAAAAEFQi4dRGC2zPNb5lS9WnASG23gtoQacPUMAOatiwIfhCp3iBEc5bDqCvgHHo+NrsSg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 24, 17, 24, 33, 129, DateTimeKind.Utc).AddTicks(9712), "AQAAAAIAAYagAAAAEAiz5tFoLCXfshDwy0AEJAa0YnLpJhauG55htMdDfMr6Rh9kEkKnTGURPkhPBLVaCg==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuestEmail",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "GuestFirstName",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "GuestLastName",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "GuestPhone",
                table: "Bookings");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 24, 16, 30, 43, 819, DateTimeKind.Utc).AddTicks(5521), "AQAAAAIAAYagAAAAEAE9udLdXqhTEMBOm2AcfFQj+D/2B0RM2X8nuunMEnct3hjvb0/DGOiCY4TMXm+k/A==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 24, 16, 30, 43, 970, DateTimeKind.Utc).AddTicks(3430), "AQAAAAIAAYagAAAAEHh5Z1vmOFie9FR5U9qPzzk2JWiLpirtrJ5Rr2mwwFHaggx3FI55LE9+yHa85sALcw==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 24, 16, 30, 44, 53, DateTimeKind.Utc).AddTicks(8180), "AQAAAAIAAYagAAAAEFNap+VHIFg5Io//X1K2G3TN1+IDczfQNsXJTWBC02NnKhzHujmNhg2UbBkZl9OhIg==" });
        }
    }
}
