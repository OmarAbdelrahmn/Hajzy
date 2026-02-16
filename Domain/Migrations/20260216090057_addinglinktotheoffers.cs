using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class addinglinktotheoffers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Link",
                table: "Offers",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 16, 9, 0, 55, 374, DateTimeKind.Utc).AddTicks(5174), "AQAAAAIAAYagAAAAENBcPyopBr2O0tfbvMyxKjEOq1aBcnYPz3MBip8OykNX8r3ULED1E5yvItbfaRBhcQ==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 16, 9, 0, 55, 491, DateTimeKind.Utc).AddTicks(5585), "AQAAAAIAAYagAAAAENQxfhFk8GlTJsQe5mAvUQmLyv2iQZ5hO9ZQJpd2SnTTPAPBuYv2dMa4IG/NQNQkrg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 16, 9, 0, 55, 568, DateTimeKind.Utc).AddTicks(4703), "AQAAAAIAAYagAAAAEOkKh5ZJ9uMrbWlDG2btNbGdlYyxD/5o7+vCFfXBUuxSUXN9QCoFMn/qo507jEiJMw==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Link",
                table: "Offers");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 16, 8, 21, 3, 379, DateTimeKind.Utc).AddTicks(8341), "AQAAAAIAAYagAAAAEPYoD6Kmdz9CfiQWYtqhpZZes4yrHGj/VvFcYxzZd2rVyoM1RRetpKgZtwaA0kbTrQ==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 16, 8, 21, 3, 436, DateTimeKind.Utc).AddTicks(4015), "AQAAAAIAAYagAAAAENQnVSSpA78c/36WM4iIGGnH5hG1dtKUkvoL/VPW6IcE77JJYP8u3WdoPEPHdGas4g==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 16, 8, 21, 3, 489, DateTimeKind.Utc).AddTicks(6713), "AQAAAAIAAYagAAAAEJzDNUIvwuyK/KwkRLIwNlxO5A+KYDsBDyTwBE3ZsPCLfR9BYI8GDS7v12cR3KL4fg==" });
        }
    }
}
