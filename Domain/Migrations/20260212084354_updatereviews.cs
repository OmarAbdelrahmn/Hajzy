using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class updatereviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVisible",
                table: "Reviews",
                type: "bit",
                nullable: false,
                defaultValue: false);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVisible",
                table: "Reviews");

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
        }
    }
}
