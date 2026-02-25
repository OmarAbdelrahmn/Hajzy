using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class updatehtenewsletters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NewsletterCampaigns_AspNetUsers_CreatedById",
                table: "NewsletterCampaigns");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "NewsletterCampaigns");

            migrationBuilder.RenameColumn(
                name: "CreatedById",
                table: "NewsletterCampaigns",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_NewsletterCampaigns_CreatedById",
                table: "NewsletterCampaigns",
                newName: "IX_NewsletterCampaigns_UserId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_NewsletterCampaigns_AspNetUsers_UserId",
                table: "NewsletterCampaigns",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NewsletterCampaigns_AspNetUsers_UserId",
                table: "NewsletterCampaigns");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "NewsletterCampaigns",
                newName: "CreatedById");

            migrationBuilder.RenameIndex(
                name: "IX_NewsletterCampaigns_UserId",
                table: "NewsletterCampaigns",
                newName: "IX_NewsletterCampaigns_CreatedById");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "NewsletterCampaigns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 24, 20, 43, 39, 546, DateTimeKind.Utc).AddTicks(7110), "AQAAAAIAAYagAAAAEOe5TGTiksETHDdosa9RRoVxHnRhrc5gzwLpKgNK1jtXMOvNTcFnlsPwkB4btNqIdQ==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 24, 20, 43, 39, 692, DateTimeKind.Utc).AddTicks(114), "AQAAAAIAAYagAAAAEJs46DXmmADdydsevrK/L0dv34069N4dGHJpM2p5awCGYuC/6wUT8TYjwiA+znYaJA==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 24, 20, 43, 39, 768, DateTimeKind.Utc).AddTicks(4308), "AQAAAAIAAYagAAAAECBho6f/tGdJhYsNZGr2PKVnOJnUXU9IHfXQeMo0R6QSlzmJpP6cnxnXJxyjby2POg==" });

            migrationBuilder.AddForeignKey(
                name: "FK_NewsletterCampaigns_AspNetUsers_CreatedById",
                table: "NewsletterCampaigns",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
