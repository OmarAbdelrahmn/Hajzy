using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class addingisstandalonetotheunittypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsStandalone",
                table: "UnitTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 24, 16, 30, 43, 819, DateTimeKind.Utc).AddTicks(5521), "AQAAAAIAAYagAAAAEAE9udLdXqhTEMBOm2AcfFQj+D/2B0RM2X8nuunMEnct3hjvb0/DGOiCY4TMXm+k/A==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 24, 16, 30, 43, 970, DateTimeKind.Utc).AddTicks(3430), "AQAAAAIAAYagAAAAEHh5Z1vmOFie9FR5U9qPzzk2JWiLpirtrJ5Rr2mwwFHaggx3FI55LE9+yHa85sALcw==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 24, 16, 30, 44, 53, DateTimeKind.Utc).AddTicks(8180), "AQAAAAIAAYagAAAAEFNap+VHIFg5Io//X1K2G3TN1+IDczfQNsXJTWBC02NnKhzHujmNhg2UbBkZl9OhIg==" });

            migrationBuilder.UpdateData(
                table: "UnitTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsStandalone",
                value: false);

            migrationBuilder.UpdateData(
                table: "UnitTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "IsStandalone",
                value: false);

            migrationBuilder.UpdateData(
                table: "UnitTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "IsStandalone",
                value: false);

            migrationBuilder.UpdateData(
                table: "UnitTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "IsStandalone",
                value: false);

            migrationBuilder.UpdateData(
                table: "UnitTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "IsStandalone",
                value: false);

            migrationBuilder.UpdateData(
                table: "UnitTypes",
                keyColumn: "Id",
                keyValue: 6,
                column: "IsStandalone",
                value: false);

            migrationBuilder.UpdateData(
                table: "UnitTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "IsStandalone",
                value: false);

            migrationBuilder.UpdateData(
                table: "UnitTypes",
                keyColumn: "Id",
                keyValue: 8,
                column: "IsStandalone",
                value: false);

            migrationBuilder.UpdateData(
                table: "UnitTypes",
                keyColumn: "Id",
                keyValue: 9,
                column: "IsStandalone",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsStandalone",
                table: "UnitTypes");

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
        }
    }
}
