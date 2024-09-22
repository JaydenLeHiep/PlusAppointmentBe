using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PlusAppointment.Migrations
{
    public partial class AddOpeningHoursTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "opening_hours",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    business_id = table.Column<int>(type: "integer", nullable: false),

                    // Columns for each day's opening and closing times
                    monday_opening_time = table.Column<TimeSpan>(type: "interval", nullable: true),
                    monday_closing_time = table.Column<TimeSpan>(type: "interval", nullable: true),

                    tuesday_opening_time = table.Column<TimeSpan>(type: "interval", nullable: true),
                    tuesday_closing_time = table.Column<TimeSpan>(type: "interval", nullable: true),

                    wednesday_opening_time = table.Column<TimeSpan>(type: "interval", nullable: true),
                    wednesday_closing_time = table.Column<TimeSpan>(type: "interval", nullable: true),

                    thursday_opening_time = table.Column<TimeSpan>(type: "interval", nullable: true),
                    thursday_closing_time = table.Column<TimeSpan>(type: "interval", nullable: true),

                    friday_opening_time = table.Column<TimeSpan>(type: "interval", nullable: true),
                    friday_closing_time = table.Column<TimeSpan>(type: "interval", nullable: true),

                    saturday_opening_time = table.Column<TimeSpan>(type: "interval", nullable: true),
                    saturday_closing_time = table.Column<TimeSpan>(type: "interval", nullable: true),

                    sunday_opening_time = table.Column<TimeSpan>(type: "interval", nullable: true),
                    sunday_closing_time = table.Column<TimeSpan>(type: "interval", nullable: true),

                    minimum_advance_booking_hours = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_opening_hours", x => x.id);
                    table.ForeignKey(
                        name: "FK_opening_hours_businesses_business_id",
                        column: x => x.business_id,
                        principalTable: "businesses",
                        principalColumn: "business_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OpeningHours_BusinessId",
                table: "opening_hours",
                column: "business_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "opening_hours");
        }
    }
}