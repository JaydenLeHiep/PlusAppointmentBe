using Microsoft.EntityFrameworkCore.Migrations;

namespace PlusAppointment.Migrations
{
    public partial class RenameColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename columns in 'users' table
            migrationBuilder.RenameColumn(name: "UserId", table: "users", newName: "user_id");
            migrationBuilder.RenameColumn(name: "Username", table: "users", newName: "username");
            migrationBuilder.RenameColumn(name: "Password", table: "users", newName: "password");
            migrationBuilder.RenameColumn(name: "Email", table: "users", newName: "email");
            migrationBuilder.RenameColumn(name: "CreatedAt", table: "users", newName: "created_at");
            migrationBuilder.RenameColumn(name: "UpdatedAt", table: "users", newName: "updated_at");
            migrationBuilder.RenameColumn(name: "Role", table: "users", newName: "role");
            migrationBuilder.RenameColumn(name: "Phone", table: "users", newName: "phone");
            migrationBuilder.RenameColumn(name: "RefreshToken", table: "users", newName: "refresh_token");
            migrationBuilder.RenameColumn(name: "RefreshTokenExpiryTime", table: "users", newName: "refresh_token_expiry_time");

            // Rename columns in 'businesses' table
            migrationBuilder.RenameColumn(name: "BusinessId", table: "businesses", newName: "business_id");
            migrationBuilder.RenameColumn(name: "Name", table: "businesses", newName: "name");
            migrationBuilder.RenameColumn(name: "Address", table: "businesses", newName: "address");
            migrationBuilder.RenameColumn(name: "Phone", table: "businesses", newName: "phone");
            migrationBuilder.RenameColumn(name: "Email", table: "businesses", newName: "email");
            migrationBuilder.RenameColumn(name: "UserID", table: "businesses", newName: "user_id");

            // Rename columns in 'appointments' table
            migrationBuilder.RenameColumn(name: "AppointmentId", table: "appointments", newName: "appointment_id");
            migrationBuilder.RenameColumn(name: "CustomerId", table: "appointments", newName: "customer_id");
            migrationBuilder.RenameColumn(name: "BusinessId", table: "appointments", newName: "business_id");
            migrationBuilder.RenameColumn(name: "StaffId", table: "appointments", newName: "staff_id");
            migrationBuilder.RenameColumn(name: "AppointmentTime", table: "appointments", newName: "appointment_time");
            migrationBuilder.RenameColumn(name: "Duration", table: "appointments", newName: "duration");
            migrationBuilder.RenameColumn(name: "Status", table: "appointments", newName: "status");
            migrationBuilder.RenameColumn(name: "CreatedAt", table: "appointments", newName: "created_at");
            migrationBuilder.RenameColumn(name: "UpdatedAt", table: "appointments", newName: "updated_at");
            migrationBuilder.RenameColumn(name: "Comment", table: "appointments", newName: "comment");

            // Rename columns in 'services' table
            migrationBuilder.RenameColumn(name: "ServiceId", table: "services", newName: "service_id");
            migrationBuilder.RenameColumn(name: "Name", table: "services", newName: "name");
            migrationBuilder.RenameColumn(name: "Description", table: "services", newName: "description");
            migrationBuilder.RenameColumn(name: "Duration", table: "services", newName: "duration");
            migrationBuilder.RenameColumn(name: "Price", table: "services", newName: "price");
            migrationBuilder.RenameColumn(name: "BusinessId", table: "services", newName: "business_id");

            // Rename columns in 'staffs' table
            migrationBuilder.RenameColumn(name: "StaffId", table: "staffs", newName: "staff_id");
            migrationBuilder.RenameColumn(name: "BusinessId", table: "staffs", newName: "business_id");
            migrationBuilder.RenameColumn(name: "Name", table: "staffs", newName: "name");
            migrationBuilder.RenameColumn(name: "Email", table: "staffs", newName: "email");
            migrationBuilder.RenameColumn(name: "Phone", table: "staffs", newName: "phone");
            migrationBuilder.RenameColumn(name: "Password", table: "staffs", newName: "password");

            // Rename columns in 'customers' table
            migrationBuilder.RenameColumn(name: "CustomerId", table: "customers", newName: "customer_id");
            migrationBuilder.RenameColumn(name: "Name", table: "customers", newName: "name");
            migrationBuilder.RenameColumn(name: "Email", table: "customers", newName: "email");
            migrationBuilder.RenameColumn(name: "Phone", table: "customers", newName: "phone");

            // Rename columns in 'appointment_services' table
            migrationBuilder.RenameColumn(name: "AppointmentId", table: "appointment_services", newName: "appointment_id");
            migrationBuilder.RenameColumn(name: "ServiceId", table: "appointment_services", newName: "service_id");

            // Rename indexes if necessary (only if their names have changed)
            // Uncomment and adjust the following lines if your index names have changed
            /*
            migrationBuilder.RenameIndex(
                name: "IX_appointment_services_ServiceId",
                table: "appointment_services",
                newName: "IX_appointment_services_service_id");
            */
        }
    }
}
