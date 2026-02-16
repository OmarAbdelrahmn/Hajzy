using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class addingupdatestothesystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OptionsJson",
                table: "Units",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PriceCurrency",
                table: "Units",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SelectedOptionsJson",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UnitCustomPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitCustomPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitCustomPolicies_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UnitCustomPolicies_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_UnitCustomPolicies_CreatedById",
                table: "UnitCustomPolicies",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_UnitCustomPolicies_UnitId",
                table: "UnitCustomPolicies",
                column: "UnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UnitCustomPolicies");

            migrationBuilder.DropColumn(
                name: "OptionsJson",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "PriceCurrency",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "SelectedOptionsJson",
                table: "Bookings");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 12, 9, 9, 6, 404, DateTimeKind.Utc).AddTicks(9449), "AQAAAAIAAYagAAAAEBrZYTAR9YeV4zP9kc+dvTe0dpMscCi8AOHM+VmLZh9oBfUFO/IEEKHcpMSdmT+/Cw==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 12, 9, 9, 6, 479, DateTimeKind.Utc).AddTicks(4500), "AQAAAAIAAYagAAAAEH9/XbqfSebntlqUmdISM4RV2RH6UwblMs5laA9L0O/ZaXXBJXAwWs/bXBSOHaKNXg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 12, 9, 9, 6, 553, DateTimeKind.Utc).AddTicks(9836), "AQAAAAIAAYagAAAAEJlk1sye6X6wz9LRPrFth1ODeKtp/YwN9zapWqboc0qtJMTsGx5slmI//pk/8nMF9w==" });
        }
    }
}
