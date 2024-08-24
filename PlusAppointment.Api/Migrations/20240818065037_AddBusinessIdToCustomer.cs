using Microsoft.EntityFrameworkCore.Migrations;

namespace PlusAppointment.Migrations
{
    public partial class AddBusinessIdToCustomer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add the business_id column to the customers table
            migrationBuilder.AddColumn<int>(
                name: "business_id",
                table: "customers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Add a foreign key constraint linking business_id in customers to business_id in businesses
            migrationBuilder.AddForeignKey(
                name: "FK_customers_businesses_business_id",
                table: "customers",
                column: "business_id",
                principalTable: "businesses",
                principalColumn: "business_id",
                onDelete: ReferentialAction.Cascade);

            // Create an index on business_id in the customers table for better query performance
            migrationBuilder.CreateIndex(
                name: "IX_customers_business_id",
                table: "customers",
                column: "business_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the index on business_id in the customers table
            migrationBuilder.DropIndex(
                name: "IX_customers_business_id",
                table: "customers");

            // Remove the foreign key constraint linking business_id in customers to business_id in businesses
            migrationBuilder.DropForeignKey(
                name: "FK_customers_businesses_business_id",
                table: "customers");

            // Remove the business_id column from the customers table
            migrationBuilder.DropColumn(
                name: "business_id",
                table: "customers");
        }
    }
}