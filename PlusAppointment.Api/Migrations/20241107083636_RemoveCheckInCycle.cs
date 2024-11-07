using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlusAppointment.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCheckInCycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "check_in_cycle",
                table: "check_ins");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "check_in_cycle",
                table: "check_ins",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }
    }
}
