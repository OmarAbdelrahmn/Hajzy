using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class hungfire : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAENrvU5zX448CSlTe9YOQygtdS7fnp2nUzi8bbad+QWNXRYEOY1i0M2Pg+82rjDezxQ==");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAED2i7SFahYiDm5ok8UJgLO7PTDA6JEtkECvXaEfofoEl8dLUkNzRH9HNTI4TSzB7Ww==");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEIENitpeJg021gN1s6nu9sWnVD9X42S4hYNnJ6IB+mWqPkq9BrMtlfRj9Gg61aPDQg==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478147B03",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEHqG3RB134vLF6Gv92j93QLNGXLZTGM4ohEqPr2ich8yCtohs2H7V6aAgHGX6kOosg==");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93878317B03",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEH56QpgRjTfgVToPGoi0XxkzpvJW63kf5Yvmz3O3HSjhCa4vXCRbRfisbHDD7tF0Jw==");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D99478317B03",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEPUwT+5CjkJBMS80se4TCKHdRLrbFfbCUssNpHYMGqo/6sYdek2rWN4ObkzBHxKHYg==");
        }
    }
}
