using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlusAppointment.Migrations
{
    /// <inheritdoc />
    public partial class RenameDiscountTierTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiscountTiers_businesses_business_id",
                table: "DiscountTiers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DiscountTiers",
                table: "DiscountTiers");

            migrationBuilder.RenameTable(
                name: "DiscountTiers",
                newName: "discount_tiers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_discount_tiers",
                table: "discount_tiers",
                column: "discount_tier_id");

            migrationBuilder.AddForeignKey(
                name: "FK_discount_tiers_businesses_business_id",
                table: "discount_tiers",
                column: "business_id",
                principalTable: "businesses",
                principalColumn: "business_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_discount_tiers_businesses_business_id",
                table: "discount_tiers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_discount_tiers",
                table: "discount_tiers");

            migrationBuilder.RenameTable(
                name: "discount_tiers",
                newName: "DiscountTiers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DiscountTiers",
                table: "DiscountTiers",
                column: "discount_tier_id");

            migrationBuilder.AddForeignKey(
                name: "FK_DiscountTiers_businesses_business_id",
                table: "DiscountTiers",
                column: "business_id",
                principalTable: "businesses",
                principalColumn: "business_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
