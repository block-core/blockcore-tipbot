using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace TipBot.Database
{
    public class ContextFactory : IContextFactory
    {
        private readonly Settings settings;

        public ContextFactory(IOptionsMonitor<Settings> options)
        {
            this.settings = options.CurrentValue;
        }

        public BotDbContext CreateContext()
        {
            string connectionString = this.settings.ConnectionString;

            DbContextOptions<BotDbContext> options = new DbContextOptionsBuilder<BotDbContext>().UseSqlServer(connectionString, builder =>
            {
                builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            }).Options;

            return new BotDbContext(options);
        }
    }

    public interface IContextFactory
    {
        BotDbContext CreateContext();
    }
}
