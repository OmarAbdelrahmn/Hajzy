using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class upddatetheoffers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "Offers",
                type: "bit",
                nullable: false,
                defaultValue: false);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "Offers");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 16, 12, 19, 28, 755, DateTimeKind.Utc).AddTicks(9327), "AQAAAAIAAYagAAAAEFsEqVq4QCwtNim2qOxNTEqjLamWYEwBsCKTjSj2G9fjyGNDKyoFoPGDJgceQCBv2w==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 16, 12, 19, 28, 876, DateTimeKind.Utc).AddTicks(1940), "AQAAAAIAAYagAAAAEL6nC6nGOnriGbVZXXeQEyoOBuXyWGiIkeNPvtrCtcKzpvOkQSrxpmktwS8NCmivpQ==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 16, 12, 19, 28, 999, DateTimeKind.Utc).AddTicks(2080), "AQAAAAIAAYagAAAAEP15VNWisHhyOLm/e8jFBFM2eM3FWfH5aHUIXMMxB+R0Bp+bzrUtfnqKOPaY9UeGGw==" });
        }
    }
}
