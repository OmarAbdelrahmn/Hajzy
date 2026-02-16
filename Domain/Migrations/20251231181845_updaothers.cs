using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class updaothers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingRoom_Bookings_BookingId",
                table: "BookingRoom");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingRoom_SubUnits_RoomId",
                table: "BookingRoom");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookingRoom",
                table: "BookingRoom");

            migrationBuilder.RenameTable(
                name: "BookingRoom",
                newName: "BookingRooms");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "FAQs",
                newName: "CategoryA");

            migrationBuilder.RenameIndex(
                name: "IX_BookingRoom_RoomId_BookingId",
                table: "BookingRooms",
                newName: "IX_BookingRooms_RoomId_BookingId");

            migrationBuilder.AddColumn<string>(
                name: "CategoryE",
                table: "FAQs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookingRooms",
                table: "BookingRooms",
                columns: new[] { "BookingId", "RoomId" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 31, 18, 18, 43, 928, DateTimeKind.Utc).AddTicks(9927), "AQAAAAIAAYagAAAAEKG8edG/eWBPNSyqHbH+6aJ8Zz5aUJWhTp+QOLtc6/spdvproVRvezl+j7LnPbbtkQ==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 31, 18, 18, 43, 997, DateTimeKind.Utc).AddTicks(7669), "AQAAAAIAAYagAAAAEKTlknBSlozKFUOQTQGhbz2wxokjQHC/EbeVNRoQKa7KXlYdRpX1afnirQ6jyeqLEg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 31, 18, 18, 44, 76, DateTimeKind.Utc).AddTicks(5482), "AQAAAAIAAYagAAAAEGue0DqWMjSQBzWj9upxeDlqoYWEc7WMLnhY0Z+gXDidiqsFbquVfDSVcJjaZ+lV6Q==" });

            migrationBuilder.AddForeignKey(
                name: "FK_BookingRooms_Bookings_BookingId",
                table: "BookingRooms",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingRooms_SubUnits_RoomId",
                table: "BookingRooms",
                column: "RoomId",
                principalTable: "SubUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingRooms_Bookings_BookingId",
                table: "BookingRooms");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingRooms_SubUnits_RoomId",
                table: "BookingRooms");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookingRooms",
                table: "BookingRooms");

            migrationBuilder.DropColumn(
                name: "CategoryE",
                table: "FAQs");

            migrationBuilder.RenameTable(
                name: "BookingRooms",
                newName: "BookingRoom");

            migrationBuilder.RenameColumn(
                name: "CategoryA",
                table: "FAQs",
                newName: "Category");

            migrationBuilder.RenameIndex(
                name: "IX_BookingRooms_RoomId_BookingId",
                table: "BookingRoom",
                newName: "IX_BookingRoom_RoomId_BookingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookingRoom",
                table: "BookingRoom",
                columns: new[] { "BookingId", "RoomId" });

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

            migrationBuilder.AddForeignKey(
                name: "FK_BookingRoom_Bookings_BookingId",
                table: "BookingRoom",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingRoom_SubUnits_RoomId",
                table: "BookingRoom",
                column: "RoomId",
                principalTable: "SubUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
