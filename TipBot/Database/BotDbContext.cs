using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TipBot.Database.Models;

namespace TipBot.Database
{
    public class BotDbContext : DbContext
    {
        /*
         * In order to update DB do `Add-Migration [MigrationName]` in Package Manager Console
         */

        public BotDbContext(DbContextOptions options) : base (options)
        {
        }

        public BotDbContext(DbContextOptions<BotDbContext> options) : base(options)
        {
        }

        public DbSet<DiscordUserModel> Users { get; set; }

        public DbSet<QuizModel> ActiveQuizes { get; set; }

        public DbSet<TipModel> TipsHistory { get; set; }

        /// <summary>Pregenerated receiving addresses that will be assigned to users who wish to deposit.</summary>
        public DbSet<AddressModel> UnusedAddresses { get; set; }
    }

    public class DesignTimeBotDbContextContextFactory : IDesignTimeDbContextFactory<BotDbContext>
    {
        public BotDbContext CreateDbContext(string[] args)
        {
            return new ContextFactory().CreateContext();
        }
    }
}
