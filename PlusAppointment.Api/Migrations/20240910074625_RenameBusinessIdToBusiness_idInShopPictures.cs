using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlusAppointment.Migrations
{
    /// <inheritdoc />
    public partial class RenameBusinessIdToBusiness_idInShopPictures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_shop_pictures_businesses_BusinessId",
                table: "shop_pictures");

            migrationBuilder.RenameColumn(
                name: "BusinessId",
                table: "shop_pictures",
                newName: "business_id");

            migrationBuilder.AddForeignKey(
                name: "FK_shop_pictures_businesses_business_id",
                table: "shop_pictures",
                column: "business_id",
                principalTable: "businesses",
                principalColumn: "business_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_shop_pictures_businesses_business_id",
                table: "shop_pictures");

            migrationBuilder.RenameColumn(
                name: "business_id",
                table: "shop_pictures",
                newName: "BusinessId");

            migrationBuilder.AddForeignKey(
                name: "FK_shop_pictures_businesses_BusinessId",
                table: "shop_pictures",
                column: "BusinessId",
                principalTable: "businesses",
                principalColumn: "business_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
