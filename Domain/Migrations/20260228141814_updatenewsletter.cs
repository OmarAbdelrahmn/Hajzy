using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class updatenewsletter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FilterCityId",
                table: "NewsletterCampaigns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FilterFromDate",
                table: "NewsletterCampaigns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FilterRegisteredUsersOnly",
                table: "NewsletterCampaigns",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FilterToDate",
                table: "NewsletterCampaigns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FilterUnitId",
                table: "NewsletterCampaigns",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 28, 14, 18, 13, 48, DateTimeKind.Utc).AddTicks(6608), "AQAAAAIAAYagAAAAEHhhCPW3z69Ulv3JkP7gB1M/dOCSziK5AirESMKEPjv6gs1+HgxAie1+q1fx+GdByg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 28, 14, 18, 13, 133, DateTimeKind.Utc).AddTicks(597), "AQAAAAIAAYagAAAAEM/5l0WuY7GQWgf9f2BDCuYdSOM6GxhWMlGknjJF7nJ1hjM/M9kMenBLrGNiPukXpw==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 28, 14, 18, 13, 220, DateTimeKind.Utc).AddTicks(3867), "AQAAAAIAAYagAAAAEAZcW1CSUrjAo4Vt063WfIb1heMD8MmN4VT0NeUrxS3CH8867eA3AEW0c5Qw8U88jw==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilterCityId",
                table: "NewsletterCampaigns");

            migrationBuilder.DropColumn(
                name: "FilterFromDate",
                table: "NewsletterCampaigns");

            migrationBuilder.DropColumn(
                name: "FilterRegisteredUsersOnly",
                table: "NewsletterCampaigns");

            migrationBuilder.DropColumn(
                name: "FilterToDate",
                table: "NewsletterCampaigns");

            migrationBuilder.DropColumn(
                name: "FilterUnitId",
                table: "NewsletterCampaigns");

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
        }
    }
}
