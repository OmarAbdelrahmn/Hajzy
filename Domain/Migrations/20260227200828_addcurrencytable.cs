using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class addcurrencytable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriceCurrency",
                table: "Units");

            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                table: "Units",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MaxOccupancy",
                table: "SubUnits",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NameEnglish = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameArabic = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 27, 20, 8, 26, 781, DateTimeKind.Utc).AddTicks(635), "AQAAAAIAAYagAAAAEPKaOAWdxWfhP+BnU2j5BTQdmPLoVPcBGhsV83ANvc8r9qtbgj3dVAgNlW2KDi38iQ==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 27, 20, 8, 26, 838, DateTimeKind.Utc).AddTicks(5724), "AQAAAAIAAYagAAAAEDrwBJEghnk1i0ExcyV4AJVCo3YJBVJbLdlMFep1ENYya3m3hEdEt11ZHUmAYkLAJw==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 27, 20, 8, 26, 892, DateTimeKind.Utc).AddTicks(5570), "AQAAAAIAAYagAAAAEIL3rEiMtuYhiFL1PgF1524YFt2rZYSDW3jQcllt7kYE3i6Zn3xIiXTR5YrtfLPcPw==" });

            migrationBuilder.CreateIndex(
                name: "IX_Units_CurrencyId",
                table: "Units",
                column: "CurrencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Units_Currencies_CurrencyId",
                table: "Units",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Units_Currencies_CurrencyId",
                table: "Units");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropIndex(
                name: "IX_Units_CurrencyId",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "Units");

            migrationBuilder.AddColumn<int>(
                name: "PriceCurrency",
                table: "Units",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "MaxOccupancy",
                table: "SubUnits",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 25, 19, 56, 55, 32, DateTimeKind.Utc).AddTicks(2600), "AQAAAAIAAYagAAAAEN4G3bPGl5xx0GygxIo3xyTQN/cOIcZWAzj2q1kAtatEALSNBEiKRkt+ql8/AUNX8g==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 25, 19, 56, 55, 96, DateTimeKind.Utc).AddTicks(8519), "AQAAAAIAAYagAAAAEGKcpV9Z7U64CkS04JD5r9wB/F2g8ejcE2nNYOEkNzYSPW+/k3kombdt0jR6m/XPhg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 25, 19, 56, 55, 152, DateTimeKind.Utc).AddTicks(7365), "AQAAAAIAAYagAAAAEEOZB+wMqADbGZ6DihyZfbWuk92uSm/TyheYegPr9RAIzwhX4Lm5DbP+LHJJq/XJjg==" });
        }
    }
}
