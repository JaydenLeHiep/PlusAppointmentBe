using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PlusAppointment.Migrations
{
    public partial class AddNotAvailableTimeTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "not_available_times",
                columns: table => new
                {
                    not_available_time_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    staff_id = table.Column<int>(type: "integer", nullable: false),
                    business_id = table.Column<int>(type: "integer", nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    
                    // Change 'from' and 'to' to DateTime
                    from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    to = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),

                    reason = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_not_available_times", x => x.not_available_time_id);
                    table.ForeignKey(
                        name: "FK_not_available_times_businesses_business_id",
                        column: x => x.business_id,
                        principalTable: "businesses",
                        principalColumn: "business_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_not_available_times_staffs_staff_id",
                        column: x => x.staff_id,
                        principalTable: "staffs",
                        principalColumn: "staff_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotAvailableTime_BusinessId",
                table: "not_available_times",
                column: "business_id");

            migrationBuilder.CreateIndex(
                name: "IX_NotAvailableTime_DateRange",
                table: "not_available_times",
                columns: new[] { "date", "from", "to" });

            migrationBuilder.CreateIndex(
                name: "IX_NotAvailableTime_StaffId",
                table: "not_available_times",
                column: "staff_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "not_available_times");
        }
    }
}