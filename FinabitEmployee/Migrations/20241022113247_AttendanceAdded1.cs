using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinabitEmployee.Migrations
{
    /// <inheritdoc />
    public partial class AttendanceAdded1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConsecutiveMismatches",
                table: "AttendanceRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsecutiveMismatches",
                table: "AttendanceRecords");
        }
    }
}
