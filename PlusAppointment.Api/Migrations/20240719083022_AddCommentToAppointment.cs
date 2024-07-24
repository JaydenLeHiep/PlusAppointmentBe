using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlusAppointment.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentToAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "Appointments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comment",
                table: "Appointments");
        }
    }
}
