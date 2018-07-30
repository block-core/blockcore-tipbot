using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TipBot.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActiveQuizes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatorDiscordUserId = table.Column<ulong>(nullable: false),
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
                name: "UnusedAddresses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
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
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(nullable: true),
                    DiscordUserId = table.Column<ulong>(nullable: false),
                    Balance = table.Column<decimal>(nullable: false),
                    DepositAddress = table.Column<string>(nullable: true),
                    LastCheckedReceivedAmountByAddress = table.Column<decimal>(nullable: false)
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
                name: "UnusedAddresses");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
