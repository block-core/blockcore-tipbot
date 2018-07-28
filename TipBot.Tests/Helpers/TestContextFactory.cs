using Microsoft.EntityFrameworkCore;
using TipBot.Database;

namespace TipBot.Tests.Helpers
{
    public class TestContextFactory : IContextFactory
    {
        private readonly string uniqueDbName;

        public TestContextFactory()
        {
            // Unique in-memory DB is generated for every instance of TestContextFactory so the tests can ran in parallel.
            this.uniqueDbName = RandomStringGenerator.RandomString(30);
        }

        public BotDbContext CreateContext()
        {
            DbContextOptions<BotDbContext> options = new DbContextOptionsBuilder<BotDbContext>()
                .UseInMemoryDatabase(databaseName: this.uniqueDbName).Options;

            return new BotDbContext(options);
        }
    }
}
