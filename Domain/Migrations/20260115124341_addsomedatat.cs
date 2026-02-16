using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class addsomedatat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageProcessingError",
                table: "UnitRegistrationRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ImageProcessingStatus",
                table: "UnitRegistrationRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ImagesProcessedAt",
                table: "UnitRegistrationRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 15, 12, 43, 39, 941, DateTimeKind.Utc).AddTicks(6604), "AQAAAAIAAYagAAAAEITRXFMxvzDL9Uy4smR5QOoFa+dM6FSyR1Vg+Ca4jQrvXpJBzrITLARuMjTf8Jdgmw==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 15, 12, 43, 40, 44, DateTimeKind.Utc).AddTicks(9734), "AQAAAAIAAYagAAAAELp2s7/jBYTBGWcmXnF4FcLB8vjk8madI6nmjB/3Dg5l1bLw8+FSNgDnQUpExm1wxA==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 15, 12, 43, 40, 124, DateTimeKind.Utc).AddTicks(5277), "AQAAAAIAAYagAAAAENDJ7/K6m9RyD5VE0pKc1b5uRuhiPHijcYkR5jh+BY9xFlxdZIzKyylevlJvRiKP1g==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageProcessingError",
                table: "UnitRegistrationRequests");

            migrationBuilder.DropColumn(
                name: "ImageProcessingStatus",
                table: "UnitRegistrationRequests");

            migrationBuilder.DropColumn(
                name: "ImagesProcessedAt",
                table: "UnitRegistrationRequests");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 10, 10, 26, 39, 515, DateTimeKind.Utc).AddTicks(5367), "AQAAAAIAAYagAAAAEBK7H/61eLyDom9oyJDNvdRjA5YvrNKNmbByGrCupoVCJD8w+KYrXHAoSt578aoJwA==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 10, 10, 26, 39, 605, DateTimeKind.Utc).AddTicks(2890), "AQAAAAIAAYagAAAAEMbKEZU7S6QSoHqu8izQD2FrnPIW19BbMCKs1etSbSr9FUd6vlHeQ+H/CR5SqF+jBg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 10, 10, 26, 39, 681, DateTimeKind.Utc).AddTicks(4507), "AQAAAAIAAYagAAAAEPfcE1USl7ZTXPu1sUxPA67lPZhKefhTDqbN80Zt8aYebxoH4qM/s/EMRj2+7ICUKQ==" });
        }
    }
}
