using Microsoft.EntityFrameworkCore;
using TipBot.Database.Models;

namespace TipBot.Database
{
    public class BotDbContext : DbContext
    {
        public BotDbContext()
        {
        }

        public BotDbContext(DbContextOptions<BotDbContext> options)
            : base(options)
        {
        }

        public DbSet<DiscordUser> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Local DB is for testing only. TODO remove when bot is written.
            optionsBuilder.UseSqlite("Data Source=testOnlyDb.db");
            /*
             * In order to update DB do `Add-Migration [MigrationName]` in Package Manager Console
             */
        }
    }
}
