using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlusAppointment.Migrations
{
    public partial class RenameTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename tables
            migrationBuilder.RenameTable(name: "Users", newName: "users");
            migrationBuilder.RenameTable(name: "Businesses", newName: "businesses");
            migrationBuilder.RenameTable(name: "Appointments", newName: "appointments");
            migrationBuilder.RenameTable(name: "Services", newName: "services");
            migrationBuilder.RenameTable(name: "Staffs", newName: "staffs");
            migrationBuilder.RenameTable(name: "Customers", newName: "customers");
            migrationBuilder.RenameTable(name: "AppointmentServices", newName: "appointment_services");

            // Rename indexes
            migrationBuilder.RenameIndex(
                name: "IX_Business_UserID",
                table: "businesses",
                newName: "IX_businesses_UserID");

            migrationBuilder.RenameIndex(
                name: "IX_Appointment_CustomerId",
                table: "appointments",
                newName: "IX_appointments_CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_Appointment_BusinessId",
                table: "appointments",
                newName: "IX_appointments_BusinessId");

            migrationBuilder.RenameIndex(
                name: "IX_Appointment_StaffId",
                table: "appointments",
                newName: "IX_appointments_StaffId");

            migrationBuilder.RenameIndex(
                name: "IX_Service_BusinessId",
                table: "services",
                newName: "IX_services_BusinessId");

            migrationBuilder.RenameIndex(
                name: "IX_Staff_BusinessId",
                table: "staffs",
                newName: "IX_staffs_BusinessId");

            migrationBuilder.RenameIndex(
                name: "IX_AppointmentServiceMapping_ServiceId",
                table: "appointment_services",
                newName: "IX_appointment_services_ServiceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert table names
            migrationBuilder.RenameTable(name: "users", newName: "Users");
            migrationBuilder.RenameTable(name: "businesses", newName: "Businesses");
            migrationBuilder.RenameTable(name: "appointments", newName: "Appointments");
            migrationBuilder.RenameTable(name: "services", newName: "Services");
            migrationBuilder.RenameTable(name: "staffs", newName: "Staffs");
            migrationBuilder.RenameTable(name: "customers", newName: "Customers");
            migrationBuilder.RenameTable(name: "appointment_services", newName: "AppointmentServices");

            // Revert indexes
            migrationBuilder.RenameIndex(
                name: "IX_businesses_UserID",
                table: "Businesses",
                newName: "IX_Business_UserID");

            migrationBuilder.RenameIndex(
                name: "IX_appointments_CustomerId",
                table: "Appointments",
                newName: "IX_Appointment_CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_appointments_BusinessId",
                table: "Appointments",
                newName: "IX_Appointment_BusinessId");

            migrationBuilder.RenameIndex(
                name: "IX_appointments_StaffId",
                table: "Appointments",
                newName: "IX_Appointment_StaffId");

            migrationBuilder.RenameIndex(
                name: "IX_services_BusinessId",
                table: "Services",
                newName: "IX_Service_BusinessId");

            migrationBuilder.RenameIndex(
                name: "IX_staffs_BusinessId",
                table: "Staffs",
                newName: "IX_Staff_BusinessId");

            migrationBuilder.RenameIndex(
                name: "IX_appointment_services_ServiceId",
                table: "AppointmentServices",
                newName: "IX_AppointmentServiceMapping_ServiceId");
        }
    }
}
