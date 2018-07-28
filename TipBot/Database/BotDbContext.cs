using Microsoft.EntityFrameworkCore;
using TipBot.Database.Models;

namespace TipBot.Database
{
    public class BotDbContext : DbContext
    {
        /*
         * In order to update DB do `Add-Migration [MigrationName]` in Package Manager Console
         */

        public BotDbContext(DbContextOptions<BotDbContext> options)
            : base(options)
        {
        }

        public DbSet<DiscordUser> Users { get; set; }
    }
}
