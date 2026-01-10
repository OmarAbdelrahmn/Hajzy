using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class updatereview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CheckInDate",
                table: "Reviews",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckOutDate",
                table: "Reviews",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 10, 10, 26, 39, 515, DateTimeKind.Utc).AddTicks(5367), "AQAAAAIAAYagAAAAEBK7H/61eLyDom9oyJDNvdRjA5YvrNKNmbByGrCupoVCJD8w+KYrXHAoSt578aoJwA==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 10, 10, 26, 39, 605, DateTimeKind.Utc).AddTicks(2890), "AQAAAAIAAYagAAAAEMbKEZU7S6QSoHqu8izQD2FrnPIW19BbMCKs1etSbSr9FUd6vlHeQ+H/CR5SqF+jBg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 10, 10, 26, 39, 681, DateTimeKind.Utc).AddTicks(4507), "AQAAAAIAAYagAAAAEPfcE1USl7ZTXPu1sUxPA67lPZhKefhTDqbN80Zt8aYebxoH4qM/s/EMRj2+7ICUKQ==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckInDate",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "CheckOutDate",
                table: "Reviews");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 9, 14, 21, 38, 799, DateTimeKind.Utc).AddTicks(5012), "AQAAAAIAAYagAAAAEJXFbsHNrWxmEfTXXzc+97oZYNIoF7h5vEkPCUq5R5KwvmCkpUZzU1fGupIrnpVu4A==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 9, 14, 21, 38, 858, DateTimeKind.Utc).AddTicks(8273), "AQAAAAIAAYagAAAAEDRjA1XrY8Zkxv5VU7AVuL22nltFx32q3H5QXeNhkWvnP8169REWhqoRAMDlvybatg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 9, 14, 21, 38, 913, DateTimeKind.Utc).AddTicks(9442), "AQAAAAIAAYagAAAAECX5lALQCuUDEQl7Fa6Kh3zKSoFEnIqpEUa/rzJXgR/3ySsBWt3HyqeUcEbfCrjE+w==" });
        }
    }
}
