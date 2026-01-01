using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class inti2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DepartmentAdmins_CityId",
                table: "DepartmentAdmins");

       }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_DepartmentAdmins_CityId",
                table: "DepartmentAdmins",
                column: "CityId",
                unique: true,
                filter: "[IsActive] = 1");
        }
    }
}
