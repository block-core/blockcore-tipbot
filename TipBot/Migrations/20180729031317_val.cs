using Microsoft.EntityFrameworkCore.Migrations;

namespace TipBot.Migrations
{
    public partial class val : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LastCheckedReceivedAmountByAddress",
                table: "Users",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastCheckedReceivedAmountByAddress",
                table: "Users");
        }
    }
}
