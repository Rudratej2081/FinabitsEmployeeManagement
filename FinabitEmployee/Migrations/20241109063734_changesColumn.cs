using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinabitEmployee.Migrations
{
    /// <inheritdoc />
    public partial class changesColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "salarySlips",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "salarySlips",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "salarySlips",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "salarySlips",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "salarySlips");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "salarySlips");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "salarySlips");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "salarySlips");
        }
    }
}
