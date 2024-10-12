using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlusAppointment.Migrations
{
    /// <inheritdoc />
    public partial class AddColorToServiceCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "color",
                table: "service_categories",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCategory_Color",
                table: "service_categories",
                column: "color");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ServiceCategory_Color",
                table: "service_categories");

            migrationBuilder.DropColumn(
                name: "color",
                table: "service_categories");
        }
    }
}
