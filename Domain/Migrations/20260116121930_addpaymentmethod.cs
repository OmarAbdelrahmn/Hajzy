using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class addpaymentmethod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentMethods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TitleA = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TitleE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionA = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionE = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethods", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 16, 12, 19, 28, 755, DateTimeKind.Utc).AddTicks(9327), "AQAAAAIAAYagAAAAEFsEqVq4QCwtNim2qOxNTEqjLamWYEwBsCKTjSj2G9fjyGNDKyoFoPGDJgceQCBv2w==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 16, 12, 19, 28, 876, DateTimeKind.Utc).AddTicks(1940), "AQAAAAIAAYagAAAAEL6nC6nGOnriGbVZXXeQEyoOBuXyWGiIkeNPvtrCtcKzpvOkQSrxpmktwS8NCmivpQ==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 16, 12, 19, 28, 999, DateTimeKind.Utc).AddTicks(2080), "AQAAAAIAAYagAAAAEP15VNWisHhyOLm/e8jFBFM2eM3FWfH5aHUIXMMxB+R0Bp+bzrUtfnqKOPaY9UeGGw==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentMethods");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 16, 10, 53, 31, 86, DateTimeKind.Utc).AddTicks(7158), "AQAAAAIAAYagAAAAEPWH4zbihTmDtgLyfIzv721oggNKBUat7nvagfuJ5kgeWa0IZ5QVdZ+dEn5fagTtMg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 16, 10, 53, 31, 170, DateTimeKind.Utc).AddTicks(9080), "AQAAAAIAAYagAAAAEN9/AWINXzLtseeM1Ho0YfzrTiYmWB5INXcxym+0Ma4iUmGhdt96FtYOHokWf6G6gA==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 16, 10, 53, 31, 250, DateTimeKind.Utc).AddTicks(7803), "AQAAAAIAAYagAAAAEJ7HAy7yrghvvK4r7IJzc4THhQBtfAma8PQToavfzmnBBKZl8gAOKeM1Dgxia/3qpg==" });
        }
    }
}
