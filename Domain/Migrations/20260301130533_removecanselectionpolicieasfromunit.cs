using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class removecanselectionpolicieasfromunit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 3, 1, 13, 5, 32, 324, DateTimeKind.Utc).AddTicks(7901), "AQAAAAIAAYagAAAAENin4HpSiQn3cOpAcdOjmo+DkPkhDez95ugeBLnjXuqmo/71zJ1bo+w1hAxmOMzlAg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 3, 1, 13, 5, 32, 383, DateTimeKind.Utc).AddTicks(6005), "AQAAAAIAAYagAAAAEIg6zgqM3EBbUajaWjSY+0VqJ9Itee5WIP2j73cG4PeDT5ZmAJFejOdnsRdj2qIE6Q==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 3, 1, 13, 5, 32, 439, DateTimeKind.Utc).AddTicks(3255), "AQAAAAIAAYagAAAAEGjfxr4/TDRMQleXMn6mkIGCiOasb990sK1awoMDnbd7oNA7WiMciyT//lmeiN5QQQ==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 28, 14, 18, 13, 48, DateTimeKind.Utc).AddTicks(6608), "AQAAAAIAAYagAAAAEHhhCPW3z69Ulv3JkP7gB1M/dOCSziK5AirESMKEPjv6gs1+HgxAie1+q1fx+GdByg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 28, 14, 18, 13, 133, DateTimeKind.Utc).AddTicks(597), "AQAAAAIAAYagAAAAEM/5l0WuY7GQWgf9f2BDCuYdSOM6GxhWMlGknjJF7nJ1hjM/M9kMenBLrGNiPukXpw==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 28, 14, 18, 13, 220, DateTimeKind.Utc).AddTicks(3867), "AQAAAAIAAYagAAAAEAZcW1CSUrjAo4Vt063WfIb1heMD8MmN4VT0NeUrxS3CH8867eA3AEW0c5Qw8U88jw==" });
        }
    }
}
