using Microsoft.EntityFrameworkCore.Migrations;

namespace TipBot.Migrations
{
    public partial class tipModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReceiverDiscordUserName",
                table: "TipsHistory",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SenderDiscordUserName",
                table: "TipsHistory",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceiverDiscordUserName",
                table: "TipsHistory");

            migrationBuilder.DropColumn(
                name: "SenderDiscordUserName",
                table: "TipsHistory");
        }
    }
}
