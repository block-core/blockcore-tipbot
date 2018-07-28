using Microsoft.EntityFrameworkCore;

namespace TipBot.Database
{
    public class ContextFactory : IContextFactory
    {
        public BotDbContext CreateContext()
        {
            // Local DB is for development only. TODO remove when bot is written.
            DbContextOptions<BotDbContext> options = new DbContextOptionsBuilder<BotDbContext>()
                .UseSqlite("Data Source=testOnlyDb.db").Options;

            return new BotDbContext(options);
        }
    }

    public interface IContextFactory
    {
        BotDbContext CreateContext();
    }
}
