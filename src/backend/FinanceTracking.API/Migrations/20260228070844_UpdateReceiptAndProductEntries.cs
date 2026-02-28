using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceTracking.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReceiptAndProductEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "original_file_name",
                table: "receipts");

            migrationBuilder.DropColumn(
                name: "source_type",
                table: "receipts");

            migrationBuilder.AlterColumn<decimal>(
                name: "total_amount",
                table: "receipts",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "payment_date",
                table: "receipts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "quantity",
                table: "product_entries",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "price",
                table: "product_entries",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "total_amount",
                table: "receipts",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<DateTime>(
                name: "payment_date",
                table: "receipts",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "original_file_name",
                table: "receipts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "source_type",
                table: "receipts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<decimal>(
                name: "quantity",
                table: "product_entries",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "price",
                table: "product_entries",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");
        }
    }
}
