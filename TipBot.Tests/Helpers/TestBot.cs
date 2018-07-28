using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TipBot.Database;

namespace TipBot.Tests.Helpers
{
    public class TestBot : Logic.TipBot
    {
        public async Task StartAsync()
        {
            await base.StartAsync(new string[0]);
        }

        protected override IServiceCollection GetServicesCollection()
        {
            // Replace real context factory with the one that serves contexts that are using in-memory database.
            IServiceCollection servicesCollection = base.GetServicesCollection();

            ServiceDescriptor factoryToReplace = servicesCollection.First(x => x.ServiceType == typeof(IContextFactory));
            servicesCollection.Remove(factoryToReplace);

            var descriptor = new ServiceDescriptor(typeof(IContextFactory), typeof(TestContextFactory), ServiceLifetime.Singleton);
            servicesCollection.Add(descriptor);

            return servicesCollection;
        }

        public T GetService<T>()
        {
            var service = this.services.GetRequiredService<T>();
            return service;
        }
    }

    public class TestContextFactory : IContextFactory
    {
        private readonly string uniqueDbName;

        public TestContextFactory()
        {
            // Unique in-memory DB is generated for every instance of TestContextFactory so the tests can ran in parallel.
            this.uniqueDbName = RandomString(30);
        }

        public BotDbContext CreateContext()
        {
            DbContextOptions<BotDbContext> options = new DbContextOptionsBuilder<BotDbContext>()
                .UseInMemoryDatabase(databaseName: this.uniqueDbName).Options;

            return new BotDbContext(options);
        }

        private static Random random = new Random();

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
