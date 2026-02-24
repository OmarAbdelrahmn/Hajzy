using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class updatetheoptionstotypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubUnitOptionSelections");

            migrationBuilder.DropTable(
                name: "UnitOptionSelections");

            migrationBuilder.DropTable(
                name: "SubUnitOptions");

            migrationBuilder.DropTable(
                name: "UnitOptions");

            migrationBuilder.CreateTable(
                name: "SubUnitTypeOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubUnitTypeId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_SubUnitTypeOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubUnitTypeOptions_SubUnitTypees_SubUnitTypeId",
                        column: x => x.SubUnitTypeId,
                        principalTable: "SubUnitTypees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnitTypeOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitTypeId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_UnitTypeOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitTypeOptions_UnitTypes_UnitTypeId",
                        column: x => x.UnitTypeId,
                        principalTable: "UnitTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubUnitOptionValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubUnitId = table.Column<int>(type: "int", nullable: false),
                    SubUnitTypeOptionId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubUnitOptionValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubUnitOptionValues_SubUnitTypeOptions_SubUnitTypeOptionId",
                        column: x => x.SubUnitTypeOptionId,
                        principalTable: "SubUnitTypeOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubUnitOptionValues_SubUnits_SubUnitId",
                        column: x => x.SubUnitId,
                        principalTable: "SubUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubUnitTypeOptionSelections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubUnitTypeOptionId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubUnitTypeOptionSelections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubUnitTypeOptionSelections_SubUnitTypeOptions_SubUnitTypeOptionId",
                        column: x => x.SubUnitTypeOptionId,
                        principalTable: "SubUnitTypeOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnitOptionValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    UnitTypeOptionId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitOptionValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitOptionValues_UnitTypeOptions_UnitTypeOptionId",
                        column: x => x.UnitTypeOptionId,
                        principalTable: "UnitTypeOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UnitOptionValues_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnitTypeOptionSelections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitTypeOptionId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitTypeOptionSelections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitTypeOptionSelections_UnitTypeOptions_UnitTypeOptionId",
                        column: x => x.UnitTypeOptionId,
                        principalTable: "UnitTypeOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 24, 11, 6, 32, 198, DateTimeKind.Utc).AddTicks(4088), "AQAAAAIAAYagAAAAEP30152GcLEst8kyi7C6sfKdMLoNmoba8lARs8ErC6Qme+btK+2ntCwB/RYjrG1ljQ==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 24, 11, 6, 32, 263, DateTimeKind.Utc).AddTicks(1956), "AQAAAAIAAYagAAAAELuqvXtUuFvhFWDaL4nqSbRw+q/a2uQ7k9emmSbhPnw9GNUn+GKlmXMRpypF0HJk/A==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 24, 11, 6, 32, 322, DateTimeKind.Utc).AddTicks(6116), "AQAAAAIAAYagAAAAEMJ1CiYu8tnU66igo2bpmD56l0pVYQRug98VkA4ak0nb2YfNKns/EwZ/mnQBgx5Nug==" });

            migrationBuilder.CreateIndex(
                name: "IX_SubUnitOptionValues_SubUnitId",
                table: "SubUnitOptionValues",
                column: "SubUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_SubUnitOptionValues_SubUnitTypeOptionId",
                table: "SubUnitOptionValues",
                column: "SubUnitTypeOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubUnitTypeOptions_SubUnitTypeId",
                table: "SubUnitTypeOptions",
                column: "SubUnitTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SubUnitTypeOptionSelections_SubUnitTypeOptionId",
                table: "SubUnitTypeOptionSelections",
                column: "SubUnitTypeOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitOptionValues_UnitId",
                table: "UnitOptionValues",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitOptionValues_UnitTypeOptionId",
                table: "UnitOptionValues",
                column: "UnitTypeOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitTypeOptions_UnitTypeId",
                table: "UnitTypeOptions",
                column: "UnitTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitTypeOptionSelections_UnitTypeOptionId",
                table: "UnitTypeOptionSelections",
                column: "UnitTypeOptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubUnitOptionValues");

            migrationBuilder.DropTable(
                name: "SubUnitTypeOptionSelections");

            migrationBuilder.DropTable(
                name: "UnitOptionValues");

            migrationBuilder.DropTable(
                name: "UnitTypeOptionSelections");

            migrationBuilder.DropTable(
                name: "SubUnitTypeOptions");

            migrationBuilder.DropTable(
                name: "UnitTypeOptions");

            migrationBuilder.CreateTable(
                name: "SubUnitOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubUnitId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    InputType = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
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
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    InputType = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
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
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
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
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
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
    }
}
