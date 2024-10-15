using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PlusAppointment.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesToEmailContents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "email_contents",
                columns: table => new
                {
                    email_content_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    subject = table.Column<string>(type: "text", nullable: false),
                    body = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_contents", x => x.email_content_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_email_content_id",
                table: "email_contents",
                column: "email_content_id");

            migrationBuilder.CreateIndex(
                name: "IX_email_subject",
                table: "email_contents",
                column: "subject");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "email_contents");
        }
    }
}
