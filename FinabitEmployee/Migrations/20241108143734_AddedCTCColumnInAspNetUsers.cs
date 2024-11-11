using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinabitEmployee.Migrations
{
    /// <inheritdoc />
    public partial class AddedCTCColumnInAspNetUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CTC",
                table: "AspNetUsers",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CTC",
                table: "AspNetUsers");
        }
    }
}
