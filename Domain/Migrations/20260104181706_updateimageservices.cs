using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class updateimageservices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SmallUrl",
                table: "UnitImages",
                newName: "ThumbnailS3Key");

            migrationBuilder.RenameColumn(
                name: "LargeUrl",
                table: "UnitImages",
                newName: "MediumS3Key");

            migrationBuilder.RenameColumn(
                name: "SmallUrl",
                table: "SubUnitImages",
                newName: "ThumbnailS3Key");

            migrationBuilder.RenameColumn(
                name: "LargeUrl",
                table: "SubUnitImages",
                newName: "MediumS3Key");

            migrationBuilder.RenameColumn(
                name: "SmallUrl",
                table: "ReviewImages",
                newName: "ThumbnailS3Key");

            migrationBuilder.RenameColumn(
                name: "LargeUrl",
                table: "ReviewImages",
                newName: "MediumS3Key");

            migrationBuilder.RenameColumn(
                name: "SmallUrl",
                table: "DepartmentImages",
                newName: "ThumbnailS3Key");

            migrationBuilder.RenameColumn(
                name: "LargeUrl",
                table: "DepartmentImages",
                newName: "MediumS3Key");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ThumbnailS3Key",
                table: "UnitImages",
                newName: "SmallUrl");

            migrationBuilder.RenameColumn(
                name: "MediumS3Key",
                table: "UnitImages",
                newName: "LargeUrl");

            migrationBuilder.RenameColumn(
                name: "ThumbnailS3Key",
                table: "SubUnitImages",
                newName: "SmallUrl");

            migrationBuilder.RenameColumn(
                name: "MediumS3Key",
                table: "SubUnitImages",
                newName: "LargeUrl");

            migrationBuilder.RenameColumn(
                name: "ThumbnailS3Key",
                table: "ReviewImages",
                newName: "SmallUrl");

            migrationBuilder.RenameColumn(
                name: "MediumS3Key",
                table: "ReviewImages",
                newName: "LargeUrl");

            migrationBuilder.RenameColumn(
                name: "ThumbnailS3Key",
                table: "DepartmentImages",
                newName: "SmallUrl");

            migrationBuilder.RenameColumn(
                name: "MediumS3Key",
                table: "DepartmentImages",
                newName: "LargeUrl");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 1, 17, 14, 9, 42, DateTimeKind.Utc).AddTicks(4697), "AQAAAAIAAYagAAAAEBLarBvufDRD5NAm9AFguHw3Zj0ErnpP15OFqdl9VSr8E28s5+X9bHCozUkBjF8aug==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 1, 17, 14, 9, 124, DateTimeKind.Utc).AddTicks(39), "AQAAAAIAAYagAAAAEPfSCzqzy/AaDfMoAbpkKvscL4a0VL8F5AC0twUJ80cc0w3kJqOKwsQ3cCUqu6jUww==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 1, 17, 14, 9, 206, DateTimeKind.Utc).AddTicks(3190), "AQAAAAIAAYagAAAAEEBcVB6Xgb4aMgzu71zJcDY/oeqIVhOLT3/k86cIl6uNgMgwBxxwrobwVyF+/qI+fQ==" });
        }
    }
}
