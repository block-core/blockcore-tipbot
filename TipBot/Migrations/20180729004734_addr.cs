using Microsoft.EntityFrameworkCore.Migrations;

namespace TipBot.Migrations
{
    public partial class addr : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DepositAddress",
                table: "Users",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepositAddress",
                table: "Users");
        }
    }
}
