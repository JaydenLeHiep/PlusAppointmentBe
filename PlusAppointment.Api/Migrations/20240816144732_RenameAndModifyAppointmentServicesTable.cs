using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlusAppointment.Migrations
{
    /// <inheritdoc />
    public partial class RenameAndModifyAppointmentServicesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_appointments_staffs_staff_id",
                table: "appointments");

            migrationBuilder.DropTable(
                name: "appointment_services");

            migrationBuilder.DropIndex(
                name: "IX_appointments_staff_id",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "staff_id",
                table: "appointments");

            migrationBuilder.CreateTable(
                name: "appointment_services_staffs",
                columns: table => new
                {
                    appointment_id = table.Column<int>(type: "integer", nullable: false),
                    service_id = table.Column<int>(type: "integer", nullable: false),
                    staff_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appointment_services_staffs", x => new { x.appointment_id, x.service_id, x.staff_id });
                    table.ForeignKey(
                        name: "FK_appointment_services_staffs_appointments_appointment_id",
                        column: x => x.appointment_id,
                        principalTable: "appointments",
                        principalColumn: "appointment_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_appointment_services_staffs_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "service_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_appointment_services_staffs_staffs_staff_id",
                        column: x => x.staff_id,
                        principalTable: "staffs",
                        principalColumn: "staff_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_appointment_services_staffs_service_id",
                table: "appointment_services_staffs",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "IX_appointment_services_staffs_staff_id",
                table: "appointment_services_staffs",
                column: "staff_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "appointment_services_staffs");

            migrationBuilder.AddColumn<int>(
                name: "staff_id",
                table: "appointments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "appointment_services",
                columns: table => new
                {
                    appointment_id = table.Column<int>(type: "integer", nullable: false),
                    service_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appointment_services", x => new { x.appointment_id, x.service_id });
                    table.ForeignKey(
                        name: "FK_appointment_services_appointments_appointment_id",
                        column: x => x.appointment_id,
                        principalTable: "appointments",
                        principalColumn: "appointment_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_appointment_services_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "service_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_appointments_staff_id",
                table: "appointments",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_appointment_services_service_id",
                table: "appointment_services",
                column: "service_id");

            migrationBuilder.AddForeignKey(
                name: "FK_appointments_staffs_staff_id",
                table: "appointments",
                column: "staff_id",
                principalTable: "staffs",
                principalColumn: "staff_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
