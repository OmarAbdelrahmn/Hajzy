using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class addingsom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFavorites_Units_UnitId",
                table: "UserFavorites");

            migrationBuilder.AlterColumn<int>(
                name: "UnitId",
                table: "UserFavorites",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "FavId",
                table: "UserFavorites",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SubUnitId",
                table: "UserFavorites",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "UserFavorites",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "Units",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 5, 17, 22, 24, 402, DateTimeKind.Utc).AddTicks(9582), "AQAAAAIAAYagAAAAEIfW7U6lNtRC6Yp5m6fqg/n9WFFAGNFO2r6DCujaxhQlngm39lS9W8frvUVV+JXwGQ==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 5, 17, 22, 24, 464, DateTimeKind.Utc).AddTicks(57), "AQAAAAIAAYagAAAAEDbrmiBc2VeKySwDMH8ULREb14V/yoBZyTpFIRHtjx9YiI6idG3uHwdEZbDk9UENNA==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 5, 17, 22, 24, 518, DateTimeKind.Utc).AddTicks(6468), "AQAAAAIAAYagAAAAEIV8E6CiX8SjHqqWapdPlmzG7Crt4AsUCRhOg186ncm2h4XbXSEXeRW1S1YiFF5JNQ==" });

            migrationBuilder.CreateIndex(
                name: "IX_UserFavorites_SubUnitId",
                table: "UserFavorites",
                column: "SubUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavorites_SubUnits_SubUnitId",
                table: "UserFavorites",
                column: "SubUnitId",
                principalTable: "SubUnits",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavorites_Units_UnitId",
                table: "UserFavorites",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFavorites_SubUnits_SubUnitId",
                table: "UserFavorites");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFavorites_Units_UnitId",
                table: "UserFavorites");

            migrationBuilder.DropIndex(
                name: "IX_UserFavorites_SubUnitId",
                table: "UserFavorites");

            migrationBuilder.DropColumn(
                name: "FavId",
                table: "UserFavorites");

            migrationBuilder.DropColumn(
                name: "SubUnitId",
                table: "UserFavorites");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "UserFavorites");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "Units");

            migrationBuilder.AlterColumn<int>(
                name: "UnitId",
                table: "UserFavorites",
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
                values: new object[] { new DateTime(2026, 1, 4, 18, 17, 5, 591, DateTimeKind.Utc).AddTicks(3086), "AQAAAAIAAYagAAAAENi/lvUf+lR1WqqaT/kIDGOIY4K2/s7aTp1h8YohM7Avtu549lARpn7Q4g4RZBFrqA==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 4, 18, 17, 5, 645, DateTimeKind.Utc).AddTicks(5894), "AQAAAAIAAYagAAAAEMSExSXQ4SV2DEE8Y1rGLsU3bdBlTBtPAuPYO9h2WJOKOQ3A3OH/JnGf75B9b6HEPw==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 4, 18, 17, 5, 697, DateTimeKind.Utc).AddTicks(122), "AQAAAAIAAYagAAAAEBe4ihZzZbGkb+se0aSAR82GLjL/ksEJExGPgQKIYYqz0uwjofBWRXVhW4fxs7Os1g==" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavorites_Units_UnitId",
                table: "UserFavorites",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
