using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceTracking.API.Migrations
{
    /// <inheritdoc />
    public partial class NullableSellerName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_receipts_sellers_seller_id_group_id",
                table: "receipts");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "sellers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "seller_id",
                table: "receipts",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_receipts_sellers_seller_id_group_id",
                table: "receipts",
                columns: new[] { "seller_id", "group_id" },
                principalTable: "sellers",
                principalColumns: new[] { "id", "group_id" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_receipts_sellers_seller_id_group_id",
                table: "receipts");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "sellers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "seller_id",
                table: "receipts",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "fk_receipts_sellers_seller_id_group_id",
                table: "receipts",
                columns: new[] { "seller_id", "group_id" },
                principalTable: "sellers",
                principalColumns: new[] { "id", "group_id" },
                onDelete: ReferentialAction.SetNull);
        }
    }
}
