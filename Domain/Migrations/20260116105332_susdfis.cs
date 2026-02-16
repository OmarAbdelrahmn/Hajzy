using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class susdfis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Offers_AspNetUsers_UploadedById",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "UploadedByUserId",
                table: "Offers");

            migrationBuilder.RenameColumn(
                name: "UploadedById",
                table: "Offers",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Offers_UploadedById",
                table: "Offers",
                newName: "IX_Offers_UserId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Offers_AspNetUsers_UserId",
                table: "Offers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Offers_AspNetUsers_UserId",
                table: "Offers");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Offers",
                newName: "UploadedById");

            migrationBuilder.RenameIndex(
                name: "IX_Offers_UserId",
                table: "Offers",
                newName: "IX_Offers_UploadedById");

            migrationBuilder.AddColumn<string>(
                name: "UploadedByUserId",
                table: "Offers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 15, 20, 24, 19, 690, DateTimeKind.Utc).AddTicks(2069), "AQAAAAIAAYagAAAAELxZGabbm9IV87PzMSpqGx8TSNdS4SLebfhbIAHCIIJa3JOItfJHgqWgW5p1wngTfg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 15, 20, 24, 19, 772, DateTimeKind.Utc).AddTicks(4093), "AQAAAAIAAYagAAAAEHLIsnbVWmeOH/kf0fD+58SbphXm0L4BABzb12hIQAsLU6W0hLEeumDys4T7RLeykg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 15, 20, 24, 19, 920, DateTimeKind.Utc).AddTicks(7153), "AQAAAAIAAYagAAAAECUku4qRp721WEKM6BXtibgprsQKDPAuhhV/6PM7w0M6poFhk+k9Gz9amttpQ9s4BQ==" });

            migrationBuilder.AddForeignKey(
                name: "FK_Offers_AspNetUsers_UploadedById",
                table: "Offers",
                column: "UploadedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
