using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TipBot.Database.Models;
using TipBot.Helpers;

namespace TipBot.Database
{
    public class BotDbContext : DbContext
    {
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
            var settings = new Settings();
            settings.Initialize(new TextFileConfiguration(args));

            return new ContextFactory(settings).CreateContext();
        }
    }
}
