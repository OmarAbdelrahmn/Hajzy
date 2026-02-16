using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class addingcreateunitrequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Contracts");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Contracts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HowToUses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HowToUses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PublicCancelPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicCancelPolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnitRegistrationRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OwnerFullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OwnerEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    OwnerPhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OwnerPassword = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UnitName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    UnitTypeId = table.Column<int>(type: "int", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(10,8)", precision: 10, scale: 8, nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(11,8)", precision: 11, scale: 8, nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxGuests = table.Column<int>(type: "int", nullable: true),
                    Bedrooms = table.Column<int>(type: "int", nullable: true),
                    Bathrooms = table.Column<int>(type: "int", nullable: true),
                    ImageS3Keys = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageCount = table.Column<int>(type: "int", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedByAdminId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedUnitId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitRegistrationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitRegistrationRequests_AspNetUsers_CreatedUserId",
                        column: x => x.CreatedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UnitRegistrationRequests_AspNetUsers_ReviewedByAdminId",
                        column: x => x.ReviewedByAdminId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UnitRegistrationRequests_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UnitRegistrationRequests_UnitTypes_UnitTypeId",
                        column: x => x.UnitTypeId,
                        principalTable: "UnitTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UnitRegistrationRequests_Units_CreatedUnitId",
                        column: x => x.CreatedUnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 30, 20, 59, 5, 180, DateTimeKind.Utc).AddTicks(2369), "AQAAAAIAAYagAAAAEDUVJchQDeYv5cUF28FKQMMxN+D8EEQjbaFmqLYtlvlZMWoJcXGE8mV49bXlBGO0zA==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 30, 20, 59, 5, 242, DateTimeKind.Utc).AddTicks(495), "AQAAAAIAAYagAAAAEDmASl53TbhQPf8m/sqjwyJ5sTmlo4ofiw+wIuaCJZrpdQhijduiCjyLjuoJwzV4nQ==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 30, 20, 59, 5, 299, DateTimeKind.Utc).AddTicks(6211), "AQAAAAIAAYagAAAAEKdYYM8hRT43zvTavh/m9AFXoj8LxCOq5GtYMc6vTx9HIjvzGjtXsTka4Qj9SC3vEg==" });

            migrationBuilder.CreateIndex(
                name: "IX_UnitRegistrationRequest_Dept_Status",
                table: "UnitRegistrationRequests",
                columns: new[] { "DepartmentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_UnitRegistrationRequest_OwnerEmail",
                table: "UnitRegistrationRequests",
                column: "OwnerEmail");

            migrationBuilder.CreateIndex(
                name: "IX_UnitRegistrationRequest_Status",
                table: "UnitRegistrationRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_UnitRegistrationRequest_Status_SubmittedAt",
                table: "UnitRegistrationRequests",
                columns: new[] { "Status", "SubmittedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UnitRegistrationRequest_SubmittedAt",
                table: "UnitRegistrationRequests",
                column: "SubmittedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UnitRegistrationRequests_CreatedUnitId",
                table: "UnitRegistrationRequests",
                column: "CreatedUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitRegistrationRequests_CreatedUserId",
                table: "UnitRegistrationRequests",
                column: "CreatedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitRegistrationRequests_ReviewedByAdminId",
                table: "UnitRegistrationRequests",
                column: "ReviewedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitRegistrationRequests_UnitTypeId",
                table: "UnitRegistrationRequests",
                column: "UnitTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HowToUses");

            migrationBuilder.DropTable(
                name: "PublicCancelPolicies");

            migrationBuilder.DropTable(
                name: "UnitRegistrationRequests");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "Contracts");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Contracts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 30, 16, 40, 49, 681, DateTimeKind.Utc).AddTicks(6624), "AQAAAAIAAYagAAAAEHU42Op6+6sGD620SszlP3QM0DJsEOS7z+AIhRJZEOg8ZK8aTQHSAYCHvup34mKdoQ==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 30, 16, 40, 49, 765, DateTimeKind.Utc).AddTicks(7313), "AQAAAAIAAYagAAAAEEqt4gNaiEylra48NE6rJgIEYQai2LtEhR/+2uj03xydUlsyb+ODH3aqvOveFtJ9UQ==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 30, 16, 40, 49, 842, DateTimeKind.Utc).AddTicks(7686), "AQAAAAIAAYagAAAAEA9kFYt2FoBoRA0nYyB/7mpUip/7p06nb1OqxDNT2s7ccFE4dTuRv87NPCsVdVIDjA==" });
        }
    }
}
