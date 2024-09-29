using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PlusAppointment.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckInEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "check_ins",
                columns: table => new
                {
                    check_in_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    business_id = table.Column<int>(type: "integer", nullable: false),
                    check_in_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    check_in_type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_check_ins", x => x.check_in_id);
                    table.ForeignKey(
                        name: "FK_check_ins_businesses_business_id",
                        column: x => x.business_id,
                        principalTable: "businesses",
                        principalColumn: "business_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_check_ins_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CheckIn_BusinessId",
                table: "check_ins",
                column: "business_id");

            migrationBuilder.CreateIndex(
                name: "IX_CheckIn_CheckInTime",
                table: "check_ins",
                column: "check_in_time");

            migrationBuilder.CreateIndex(
                name: "IX_CheckIn_CustomerId",
                table: "check_ins",
                column: "customer_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "check_ins");
        }
    }
}
