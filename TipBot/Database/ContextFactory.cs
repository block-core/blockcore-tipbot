using Microsoft.EntityFrameworkCore;

namespace TipBot.Database
{
    public class ContextFactory : IContextFactory
    {
        private readonly Settings settings;

        public ContextFactory(Settings settings)
        {
            this.settings = settings;
        }

        public BotDbContext CreateContext()
        {
            string connectionString = this.settings.ConnectionString;

            DbContextOptions<BotDbContext> options;

            if (connectionString == null)
                options = new DbContextOptionsBuilder<BotDbContext>().UseSqlite("Data Source=testOnlyDb.db").Options;
            else
                options = new DbContextOptionsBuilder<BotDbContext>().UseSqlServer(connectionString).Options;

            return new BotDbContext(options);
        }
    }

    public interface IContextFactory
    {
        BotDbContext CreateContext();
    }
}
