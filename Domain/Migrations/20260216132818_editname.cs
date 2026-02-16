using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class editname : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UnitCustomPolicies_AspNetUsers_CreatedById",
                table: "UnitCustomPolicies");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "UnitCustomPolicies");

            migrationBuilder.RenameColumn(
                name: "CreatedById",
                table: "UnitCustomPolicies",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UnitCustomPolicies_CreatedById",
                table: "UnitCustomPolicies",
                newName: "IX_UnitCustomPolicies_UserId");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 16, 13, 28, 16, 357, DateTimeKind.Utc).AddTicks(5334), "AQAAAAIAAYagAAAAEAmQIdadMKzAu+TY1JwemCXFy8dkzSde1DutwCRzo8YKG0bB5DU/FYHSksBoPKZDCA==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 16, 13, 28, 16, 451, DateTimeKind.Utc).AddTicks(6567), "AQAAAAIAAYagAAAAEDIl2pzIm6qm2sd8G4R2TZZjG8AIyeHXsrNtyMKc5aLm7bjHfu+8XUD7pggjnw+yPA==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 16, 13, 28, 16, 545, DateTimeKind.Utc).AddTicks(3453), "AQAAAAIAAYagAAAAEIJkGdtKIv2Lt5DvRRJIo1oP4nWs5d9xKDUgBz3ykHSv/tqa239hg8tReIXftBN4kg==" });

            migrationBuilder.AddForeignKey(
                name: "FK_UnitCustomPolicies_AspNetUsers_UserId",
                table: "UnitCustomPolicies",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UnitCustomPolicies_AspNetUsers_UserId",
                table: "UnitCustomPolicies");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UnitCustomPolicies",
                newName: "CreatedById");

            migrationBuilder.RenameIndex(
                name: "IX_UnitCustomPolicies_UserId",
                table: "UnitCustomPolicies",
                newName: "IX_UnitCustomPolicies_CreatedById");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "UnitCustomPolicies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.AddForeignKey(
                name: "FK_UnitCustomPolicies_AspNetUsers_CreatedById",
                table: "UnitCustomPolicies",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
