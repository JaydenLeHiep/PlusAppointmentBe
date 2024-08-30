using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PlusAppointment.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "category_id",
                table: "services",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "service_categories",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_categories", x => x.category_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_services_category_id",
                table: "services",
                column: "category_id");

            migrationBuilder.AddForeignKey(
                name: "FK_services_service_categories_category_id",
                table: "services",
                column: "category_id",
                principalTable: "service_categories",
                principalColumn: "category_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_services_service_categories_category_id",
                table: "services");

            migrationBuilder.DropTable(
                name: "service_categories");

            migrationBuilder.DropIndex(
                name: "IX_services_category_id",
                table: "services");

            migrationBuilder.DropColumn(
                name: "category_id",
                table: "services");
        }
    }
}
