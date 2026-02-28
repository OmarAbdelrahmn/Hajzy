using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class addingunitandsubunitrelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UnitTypeSubUnitTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitTypeId = table.Column<int>(type: "int", nullable: false),
                    SubUnitTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitTypeSubUnitTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitTypeSubUnitTypes_SubUnitTypees_SubUnitTypeId",
                        column: x => x.SubUnitTypeId,
                        principalTable: "SubUnitTypees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UnitTypeSubUnitTypes_UnitTypes_UnitTypeId",
                        column: x => x.UnitTypeId,
                        principalTable: "UnitTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 28, 13, 56, 54, 668, DateTimeKind.Utc).AddTicks(8587), "AQAAAAIAAYagAAAAELm/zyc4W+CaVA0LEzzXjM/mq9eHawJ3Edyn1bnXJPWGhOcINQeDepGcMI3cBFJYEQ==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 28, 13, 56, 54, 760, DateTimeKind.Utc).AddTicks(5486), "AQAAAAIAAYagAAAAENylIuzkxvWHL1VR5Gg9+2xX2zjUG/t+atuFs8MRGDedIwDgdWMjWx4llTBSUqYlSQ==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 28, 13, 56, 54, 842, DateTimeKind.Utc).AddTicks(3797), "AQAAAAIAAYagAAAAEKlae+jvuy69fkvp3rvV7rR/9ddh9nDBPXSjdBXaWXVotrofq1iGJScpOR+/YeWG3w==" });

            migrationBuilder.CreateIndex(
                name: "IX_UnitTypeSubUnitTypes_SubUnitTypeId",
                table: "UnitTypeSubUnitTypes",
                column: "SubUnitTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitTypeSubUnitTypes_UnitTypeId",
                table: "UnitTypeSubUnitTypes",
                column: "UnitTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UnitTypeSubUnitTypes");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 28, 13, 37, 1, 87, DateTimeKind.Utc).AddTicks(4818), "AQAAAAIAAYagAAAAECPjKcrHh66Jrkmm42epmcvQbBi3NCDzF1ZTnSMAX3nFNktb9RPlyk8BmIzwdYM3nA==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 28, 13, 37, 1, 229, DateTimeKind.Utc).AddTicks(6881), "AQAAAAIAAYagAAAAEJ2MesUPIRmMvJcrUOZoFh03Z6YqfnIOaSFNREyA7FRkTP7NWAsGs5gck8NwRExe+w==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 28, 13, 37, 1, 308, DateTimeKind.Utc).AddTicks(9591), "AQAAAAIAAYagAAAAEAF+KqQQ77Wz4OOQC5r1+WvNAYKTRxpjG+O9Ucz6+vJ095RBm81utuOIfsi35v7w9Q==" });
        }
    }
}
