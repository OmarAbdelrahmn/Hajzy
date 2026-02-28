using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class addlinktonewsletter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Link",
                table: "NewsletterSubscribers",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Link",
                table: "NewsletterCampaigns",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Link",
                table: "NewsletterSubscribers");

            migrationBuilder.DropColumn(
                name: "Link",
                table: "NewsletterCampaigns");

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
        }
    }
}
