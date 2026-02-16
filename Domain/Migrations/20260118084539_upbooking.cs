using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class upbooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubUnits_SubUnitTypee_SubUnitTypeId",
                table: "SubUnits");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SubUnitTypee",
                table: "SubUnitTypee");

            migrationBuilder.RenameTable(
                name: "SubUnitTypee",
                newName: "SubUnitTypees");

            migrationBuilder.AddColumn<int>(
                name: "BookingType",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubUnitTypees",
                table: "SubUnitTypees",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 18, 8, 45, 38, 65, DateTimeKind.Utc).AddTicks(6378), "AQAAAAIAAYagAAAAEO8cDJcnHFlZbPKveDK7C571szANzdd7d+pco+iCQT83fnlIm9w9hOIJPDl787n39w==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 18, 8, 45, 38, 164, DateTimeKind.Utc).AddTicks(1970), "AQAAAAIAAYagAAAAELGeK4cB238oY0k881yLjeKL16QjWPKdA+NpqKQz6o36tJj+7TEVNuOObEoypcAI2Q==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 18, 8, 45, 38, 234, DateTimeKind.Utc).AddTicks(1586), "AQAAAAIAAYagAAAAECaCdfC5YZRqecn/6vaMzuSggOTatyupOO/0WtzIhqcPii5OSqgeiWiUIvQgG9l1vw==" });

            migrationBuilder.AddForeignKey(
                name: "FK_SubUnits_SubUnitTypees_SubUnitTypeId",
                table: "SubUnits",
                column: "SubUnitTypeId",
                principalTable: "SubUnitTypees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubUnits_SubUnitTypees_SubUnitTypeId",
                table: "SubUnits");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SubUnitTypees",
                table: "SubUnitTypees");

            migrationBuilder.DropColumn(
                name: "BookingType",
                table: "Bookings");

            migrationBuilder.RenameTable(
                name: "SubUnitTypees",
                newName: "SubUnitTypee");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubUnitTypee",
                table: "SubUnitTypee",
                column: "Id");

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

            migrationBuilder.AddForeignKey(
                name: "FK_SubUnits_SubUnitTypee_SubUnitTypeId",
                table: "SubUnits",
                column: "SubUnitTypeId",
                principalTable: "SubUnitTypee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
