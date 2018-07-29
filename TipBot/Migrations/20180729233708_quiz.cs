using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TipBot.Migrations
{
    public partial class quiz : Migration
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActiveQuizes");
        }
    }
}
