using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class addingtheprimaryadminfordepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                table: "DepartmentAdmins",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentAdmin_CityId_IsActive",
                table: "DepartmentAdmins",
                columns: new[] { "CityId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentAdmin_CityId_IsPrimary_Unique",
                table: "DepartmentAdmins",
                columns: new[] { "CityId", "IsPrimary" },
                unique: true,
                filter: "[IsActive] = 1 AND [IsPrimary] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentAdmin_UserId_IsActive",
                table: "DepartmentAdmins",
                columns: new[] { "UserId", "IsActive" },
                filter: "[IsActive] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DepartmentAdmin_CityId_IsActive",
                table: "DepartmentAdmins");

            migrationBuilder.DropIndex(
                name: "IX_DepartmentAdmin_CityId_IsPrimary_Unique",
                table: "DepartmentAdmins");

            migrationBuilder.DropIndex(
                name: "IX_DepartmentAdmin_UserId_IsActive",
                table: "DepartmentAdmins");

            migrationBuilder.DropColumn(
                name: "IsPrimary",
                table: "DepartmentAdmins");

        }
    }
}
