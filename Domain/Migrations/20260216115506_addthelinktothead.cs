using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class addthelinktothead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Link",
                table: "Ads",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 16, 11, 55, 4, 123, DateTimeKind.Utc).AddTicks(1249), "AQAAAAIAAYagAAAAENhbY+r8StZn0Npaw6UqWPbS0M/fCDHnls0EzxWJeMwrIfibuxQ0DKgs6+4Hyb7oww==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 16, 11, 55, 4, 231, DateTimeKind.Utc).AddTicks(5883), "AQAAAAIAAYagAAAAEOJmZIuOkFQfjm8GcM1TzuU0NEqx/Bi+0Ukjby2orJnAaahbEWiTwMuqLWxDRP7G5Q==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 16, 11, 55, 4, 320, DateTimeKind.Utc).AddTicks(3074), "AQAAAAIAAYagAAAAEEK9kMlN7v+ye82XBN4y5tBPpgxGxbtOcMxQyD4eCJz5e4KcuYaL1DVPVOtplPXKcg==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Link",
                table: "Ads");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 16, 9, 42, 27, 324, DateTimeKind.Utc).AddTicks(8732), "AQAAAAIAAYagAAAAEBypztK8DJXrJzwV0Kd5+igUEEeLQ++7dfQF9Mwc5ZLh4I5eDAF7lMa4tZ8qyonuIg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 16, 9, 42, 27, 420, DateTimeKind.Utc).AddTicks(2595), "AQAAAAIAAYagAAAAECDH6dt23KTI0t5Kj+mYa+N2SqlhrIcCw53W1dC7deTScRxzqwNv2srkjzGWoMKhfg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 16, 9, 42, 27, 498, DateTimeKind.Utc).AddTicks(9662), "AQAAAAIAAYagAAAAECxQVhyEM30iHOFrbFlOvQwSPhL5bjl7Y/vuEJ6crTkQm9t/WyMRG3C7dk73RElo0w==" });
        }
    }
}
