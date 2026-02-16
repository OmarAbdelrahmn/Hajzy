using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class addunitrank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Rank",
                table: "Units",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 16, 9, 42, 27, 324, DateTimeKind.Utc).AddTicks(8732), "AQAAAAIAAYagAAAAEBypztK8DJXrJzwV0Kd5+igUEEeLQ++7dfQF9Mwc5ZLh4I5eDAF7lMa4tZ8qyonuIg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 16, 9, 42, 27, 420, DateTimeKind.Utc).AddTicks(2595), "AQAAAAIAAYagAAAAECDH6dt23KTI0t5Kj+mYa+N2SqlhrIcCw53W1dC7deTScRxzqwNv2srkjzGWoMKhfg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 16, 9, 42, 27, 498, DateTimeKind.Utc).AddTicks(9662), "AQAAAAIAAYagAAAAECxQVhyEM30iHOFrbFlOvQwSPhL5bjl7Y/vuEJ6crTkQm9t/WyMRG3C7dk73RElo0w==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rank",
                table: "Units");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 16, 9, 0, 55, 374, DateTimeKind.Utc).AddTicks(5174), "AQAAAAIAAYagAAAAENBcPyopBr2O0tfbvMyxKjEOq1aBcnYPz3MBip8OykNX8r3ULED1E5yvItbfaRBhcQ==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 16, 9, 0, 55, 491, DateTimeKind.Utc).AddTicks(5585), "AQAAAAIAAYagAAAAENQxfhFk8GlTJsQe5mAvUQmLyv2iQZ5hO9ZQJpd2SnTTPAPBuYv2dMa4IG/NQNQkrg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 16, 9, 0, 55, 568, DateTimeKind.Utc).AddTicks(4703), "AQAAAAIAAYagAAAAEOkKh5ZJ9uMrbWlDG2btNbGdlYyxD/5o7+vCFfXBUuxSUXN9QCoFMn/qo507jEiJMw==" });
        }
    }
}
