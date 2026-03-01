using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceTracking.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedGroupHistoryDto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "name_after",
                table: "group_member_histories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "name_before",
                table: "group_member_histories",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "name_after",
                table: "group_member_histories");

            migrationBuilder.DropColumn(
                name: "name_before",
                table: "group_member_histories");
        }
    }
}
