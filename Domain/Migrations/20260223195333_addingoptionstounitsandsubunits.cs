using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class addingoptionstounitsandsubunits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubUnitOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubUnitId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    InputType = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubUnitOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubUnitOptions_SubUnits_SubUnitId",
                        column: x => x.SubUnitId,
                        principalTable: "SubUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnitOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    InputType = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitOptions_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubUnitOptionSelections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubUnitOptionId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubUnitOptionSelections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubUnitOptionSelections_SubUnitOptions_SubUnitOptionId",
                        column: x => x.SubUnitOptionId,
                        principalTable: "SubUnitOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnitOptionSelections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitOptionId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitOptionSelections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitOptionSelections_UnitOptions_UnitOptionId",
                        column: x => x.UnitOptionId,
                        principalTable: "UnitOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 23, 19, 53, 31, 298, DateTimeKind.Utc).AddTicks(7530), "AQAAAAIAAYagAAAAEC05xDNuAwrmsftVjHvEhwShJklVmjdQPIt0j5IyOswmnbvFg/W/KuL1M0xoeeXBhg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 23, 19, 53, 31, 391, DateTimeKind.Utc).AddTicks(7883), "AQAAAAIAAYagAAAAEF+j3BB08QIAzLVviyo/9g48jHte+yLvAtwReJMLjyuMqksTdw5yLeFvBWzGyccR0g==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 23, 19, 53, 31, 497, DateTimeKind.Utc).AddTicks(8238), "AQAAAAIAAYagAAAAEC9WI0PFeG0vjis7I5ylc13Lg4BRFCCUB/JhJpk3k231Onje48l4UY3MWRnb/g8ygw==" });

            migrationBuilder.CreateIndex(
                name: "IX_SubUnitOptions_SubUnitId",
                table: "SubUnitOptions",
                column: "SubUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_SubUnitOptionSelections_SubUnitOptionId",
                table: "SubUnitOptionSelections",
                column: "SubUnitOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitOptions_UnitId",
                table: "UnitOptions",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitOptionSelections_UnitOptionId",
                table: "UnitOptionSelections",
                column: "UnitOptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubUnitOptionSelections");

            migrationBuilder.DropTable(
                name: "UnitOptionSelections");

            migrationBuilder.DropTable(
                name: "SubUnitOptions");

            migrationBuilder.DropTable(
                name: "UnitOptions");

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
        }
    }
}
