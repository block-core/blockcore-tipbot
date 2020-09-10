using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TipBot.Migrations
{
    public partial class IntDataStruct : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActiveQuizes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatorDiscordUserId = table.Column<decimal>(nullable: false),
                    AnswerHash = table.Column<string>(nullable: true),
                    Question = table.Column<string>(nullable: true),
                    Reward = table.Column<decimal>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    DurationMinutes = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveQuizes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TipsHistory",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    SenderDiscordUserId = table.Column<decimal>(nullable: false),
                    SenderDiscordUserName = table.Column<string>(nullable: true),
                    ReceiverDiscordUserId = table.Column<decimal>(nullable: false),
                    ReceiverDiscordUserName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipsHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnusedAddresses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnusedAddresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(nullable: true),
                    DiscordUserId = table.Column<decimal>(nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    DepositAddress = table.Column<string>(nullable: true),
                    LastCheckedReceivedAmountByAddress = table.Column<decimal>(type: "decimal(18,8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActiveQuizes");

            migrationBuilder.DropTable(
                name: "TipsHistory");

            migrationBuilder.DropTable(
                name: "UnusedAddresses");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
