using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlusAppointment.Migrations
{
    /// <inheritdoc />
    public partial class RenameMinimumAdvanceBookingColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "minimum_advance_booking_hours",
                table: "opening_hours",
                newName: "minimum_advance_booking_minutes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "minimum_advance_booking_minutes",
                table: "opening_hours",
                newName: "minimum_advance_booking_hours");
        }
    }
}
