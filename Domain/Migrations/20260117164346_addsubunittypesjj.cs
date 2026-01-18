using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class addsubunittypesjj : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "SubUnits");

            migrationBuilder.AddColumn<int>(
                name: "SubUnitTypeId",
                table: "SubUnits",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "SubUnitTypee",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubUnitTypee", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 17, 16, 43, 44, 581, DateTimeKind.Utc).AddTicks(9161), "AQAAAAIAAYagAAAAEABKh+NUxpQuE+dmx7zWSrZLn6Sn7BNWcKxB4BFenm43PbjKld7J7vRiZbwscJDngQ==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 17, 16, 43, 44, 656, DateTimeKind.Utc).AddTicks(6011), "AQAAAAIAAYagAAAAEG7OD69ceKuv6+WvGr5Uq5JoyPMZ137uXVtDIhVYsPKIJmp42svq73hULV4InK9cQA==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 17, 16, 43, 44, 727, DateTimeKind.Utc).AddTicks(6629), "AQAAAAIAAYagAAAAELBYpRUyqwXzm/EHPFqu7LtSLZP8efPytz6WQN1K2vwhLWVyzDnb2SddoBUEYxthWg==" });

            migrationBuilder.InsertData(
                table: "SubUnitTypee",
                columns: new[] { "Id", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "Standard hotel room", true, "StandardRoom" },
                    { 2, "Deluxe hotel room with extra amenities", true, "DeluxeRoom" },
                    { 3, "Spacious hotel suite", true, "Suite" },
                    { 4, "Executive-level suite", true, "ExecutiveSuite" },
                    { 5, "Luxury penthouse suite", true, "PenthouseSuite" },
                    { 6, "Private standalone villa", true, "Villa" },
                    { 7, "Small vacation cottage", true, "Cottage" },
                    { 8, "Single-story bungalow", true, "Bungalow" },
                    { 9, "Wooden or nature cabin", true, "Cabin" },
                    { 10, "Fully furnished apartment", true, "Apartment" },
                    { 11, "Studio-style unit", true, "Studio" },
                    { 12, "Outdoor tent site", true, "TentSite" },
                    { 13, "Luxury glamping tent", true, "GlamingTent" },
                    { 14, "Space for recreational vehicles", true, "RVSpace" },
                    { 15, "Event or banquet hall", true, "Hall" },
                    { 16, "Mountain or resort chalet", true, "Chalet" },
                    { 17, "Bed in shared dormitory", true, "DormBed" },
                    { 18, "Private room in shared property", true, "PrivateRoom" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubUnits_SubUnitTypeId",
                table: "SubUnits",
                column: "SubUnitTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubUnits_SubUnitTypee_SubUnitTypeId",
                table: "SubUnits",
                column: "SubUnitTypeId",
                principalTable: "SubUnitTypee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubUnits_SubUnitTypee_SubUnitTypeId",
                table: "SubUnits");

            migrationBuilder.DropTable(
                name: "SubUnitTypee");

            migrationBuilder.DropIndex(
                name: "IX_SubUnits_SubUnitTypeId",
                table: "SubUnits");

            migrationBuilder.DropColumn(
                name: "SubUnitTypeId",
                table: "SubUnits");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "SubUnits",
                type: "int",
                maxLength: 100,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 16, 13, 40, 21, 564, DateTimeKind.Utc).AddTicks(4708), "AQAAAAIAAYagAAAAEFRnUNgyDFbKziq76C/6XoAb6nIt3+fShe0Rlh0l0vfxzmXsPQopoYsppNRhCeEuqg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 16, 13, 40, 21, 681, DateTimeKind.Utc).AddTicks(9633), "AQAAAAIAAYagAAAAEKflcaNxjqJ7qR3IH7OpX4ptTT/wzfkq+H5lsfFfc854BtAapKaC3CllTE4G9i/eeA==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 16, 13, 40, 21, 751, DateTimeKind.Utc).AddTicks(8935), "AQAAAAIAAYagAAAAEHVRXa6jGq5gR0cmcyk6hTFGtDTZXpiYqtNzC9Q/77Jc8wfJ96H3blp0srBeKGtDVA==" });
        }
    }
}
