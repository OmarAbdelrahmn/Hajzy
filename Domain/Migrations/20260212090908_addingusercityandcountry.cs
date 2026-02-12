using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class addingusercityandcountry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "City", "Country", "CreatedAt", "PasswordHash" },
                values: new object[] { null, null, new DateTime(2026, 2, 12, 9, 9, 6, 404, DateTimeKind.Utc).AddTicks(9449), "AQAAAAIAAYagAAAAEBrZYTAR9YeV4zP9kc+dvTe0dpMscCi8AOHM+VmLZh9oBfUFO/IEEKHcpMSdmT+/Cw==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "City", "Country", "CreatedAt", "PasswordHash" },
                values: new object[] { null, null, new DateTime(2026, 2, 12, 9, 9, 6, 479, DateTimeKind.Utc).AddTicks(4500), "AQAAAAIAAYagAAAAEH9/XbqfSebntlqUmdISM4RV2RH6UwblMs5laA9L0O/ZaXXBJXAwWs/bXBSOHaKNXg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "City", "Country", "CreatedAt", "PasswordHash" },
                values: new object[] { null, null, new DateTime(2026, 2, 12, 9, 9, 6, 553, DateTimeKind.Utc).AddTicks(9836), "AQAAAAIAAYagAAAAEJlk1sye6X6wz9LRPrFth1ODeKtp/YwN9zapWqboc0qtJMTsGx5slmI//pk/8nMF9w==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 12, 8, 43, 52, 782, DateTimeKind.Utc).AddTicks(7842), "AQAAAAIAAYagAAAAEG7UuxzfzhvP16EwD9vhGK1xe61AyTbcrft5diiff0zz5fTIVkt+m6rUStPlGpmAHw==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 12, 8, 43, 52, 868, DateTimeKind.Utc).AddTicks(8047), "AQAAAAIAAYagAAAAECrI0QBIV4G7XHMiQYQJf6NPya7igCSKOsbiWU9+sm41LrOWWcLzfJMTxHxVGCniDg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 12, 8, 43, 52, 937, DateTimeKind.Utc).AddTicks(1236), "AQAAAAIAAYagAAAAEI8w7J0KAZ6yq/5JmxwHMCilGQVy1E8tZ8GMRQ9ft2HWoam0fhj9C2VuarE+FoJTkg==" });
        }
    }
}
