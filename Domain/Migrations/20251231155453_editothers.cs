using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class editothers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "PrivacyPolicies");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Contracts");

            migrationBuilder.RenameColumn(
                name: "Version",
                table: "TermsAndConditions",
                newName: "TitleE");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "TermsAndConditions",
                newName: "TitleA");

            migrationBuilder.RenameColumn(
                name: "ContentEnglish",
                table: "TermsAndConditions",
                newName: "DescriptionE");

            migrationBuilder.RenameColumn(
                name: "ContentArabic",
                table: "TermsAndConditions",
                newName: "DescriptionA");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "PublicCancelPolicies",
                newName: "TitleE");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "PublicCancelPolicies",
                newName: "TitleA");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "PrivacyPolicies",
                newName: "TitleA");

            migrationBuilder.RenameColumn(
                name: "ContentEnglish",
                table: "PrivacyPolicies",
                newName: "TitleE");

            migrationBuilder.RenameColumn(
                name: "ContentArabic",
                table: "PrivacyPolicies",
                newName: "DescreptionE");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionA",
                table: "PublicCancelPolicies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionE",
                table: "PublicCancelPolicies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescreptionA",
                table: "PrivacyPolicies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ContentEnglish",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ContentArabic",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 31, 15, 54, 51, 660, DateTimeKind.Utc).AddTicks(3608), "AQAAAAIAAYagAAAAEFkKcKZMjpWk+K/eaGxkGKglnVixCS6kFL5cujuOEcPbsqfSf6aNZ0xERNfsf+42Vw==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 31, 15, 54, 51, 752, DateTimeKind.Utc).AddTicks(4061), "AQAAAAIAAYagAAAAEF2B65P8wWaI7A00DzR/rFWjOZC4fkavQCXnKSdyhg4W363RUSiAJTPZzd4l4HSMdg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 31, 15, 54, 51, 834, DateTimeKind.Utc).AddTicks(626), "AQAAAAIAAYagAAAAEKERGRSTZwWnt6Yj8oQvR53LxPmzC4oBzl1LCzV9qmO7ucAe6TX42Wh2vtdFR7XpsA==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescriptionA",
                table: "PublicCancelPolicies");

            migrationBuilder.DropColumn(
                name: "DescriptionE",
                table: "PublicCancelPolicies");

            migrationBuilder.DropColumn(
                name: "DescreptionA",
                table: "PrivacyPolicies");

            migrationBuilder.RenameColumn(
                name: "TitleE",
                table: "TermsAndConditions",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "TitleA",
                table: "TermsAndConditions",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "DescriptionE",
                table: "TermsAndConditions",
                newName: "ContentEnglish");

            migrationBuilder.RenameColumn(
                name: "DescriptionA",
                table: "TermsAndConditions",
                newName: "ContentArabic");

            migrationBuilder.RenameColumn(
                name: "TitleE",
                table: "PublicCancelPolicies",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "TitleA",
                table: "PublicCancelPolicies",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "TitleE",
                table: "PrivacyPolicies",
                newName: "ContentEnglish");

            migrationBuilder.RenameColumn(
                name: "TitleA",
                table: "PrivacyPolicies",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "DescreptionE",
                table: "PrivacyPolicies",
                newName: "ContentArabic");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "PrivacyPolicies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "ContentEnglish",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContentArabic",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

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
        }
    }
}
