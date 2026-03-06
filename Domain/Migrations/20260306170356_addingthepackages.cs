using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class addingthepackages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PackagePrice",
                table: "Bookings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "SelectedPackageId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Package",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FeaturesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Package", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Package_Units_UnitId",
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
                values: new object[] { new DateTime(2026, 3, 6, 17, 3, 55, 49, DateTimeKind.Utc).AddTicks(9335), "AQAAAAIAAYagAAAAEE2Qtjk/iQ1Y1zVSYg4JcSNfD9jMJYhu5SNHsp/Q5ox06oM4iPomlWohTA9gyFKWgA==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 3, 6, 17, 3, 55, 112, DateTimeKind.Utc).AddTicks(6244), "AQAAAAIAAYagAAAAEKdWQN1Fds0Fdsk9dzxPKij/0TzOTXXs8b8FCbf1n4yX4zgJdqJrAv0j8Ogchd3N3A==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 3, 6, 17, 3, 55, 170, DateTimeKind.Utc).AddTicks(1576), "AQAAAAIAAYagAAAAEBOHJEia8Cw5LfB0QFx9EStG5iVfJnyIzFKq8PYSuN5w1ztXnFtWd/xA9/8nYZBv2g==" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_SelectedPackageId",
                table: "Bookings",
                column: "SelectedPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_Package_UnitId",
                table: "Package",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Package_SelectedPackageId",
                table: "Bookings",
                column: "SelectedPackageId",
                principalTable: "Package",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Package_SelectedPackageId",
                table: "Bookings");

            migrationBuilder.DropTable(
                name: "Package");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_SelectedPackageId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PackagePrice",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "SelectedPackageId",
                table: "Bookings");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 3, 4, 21, 57, 58, 758, DateTimeKind.Utc).AddTicks(397), "AQAAAAIAAYagAAAAEFsQQt7QjwHEhUO7LCy7rVslxE0hmUfc5/Qgb1ThXulzmhvYiJ3YMckiNjugP0nZPw==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 3, 4, 21, 57, 58, 856, DateTimeKind.Utc).AddTicks(5335), "AQAAAAIAAYagAAAAELVonqaX7dBmSZ9K0Df7E8azKXNNlULTCvqe9lIk18U/CnTlXpjmQkKtlO/IfHWOlg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 3, 4, 21, 57, 58, 966, DateTimeKind.Utc).AddTicks(5168), "AQAAAAIAAYagAAAAECo0cP1ZhHH9r8T3/BQIrRiaQpG+TrytoZaE0Hnn4vbIHS935YLq7RmCy1GD0Udv7w==" });
        }
    }
}
