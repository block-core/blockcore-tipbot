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

            var descriptor = new ServiceDescriptor(typeof(IContextFactory), typeof(TestContextFactory), ServiceLifetime.Transient);
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
        public BotDbContext CreateContext()
        {
            DbContextOptions<BotDbContext> options = new DbContextOptionsBuilder<BotDbContext>()
                .UseInMemoryDatabase(databaseName: "testDb").Options;

            return new BotDbContext(options);
        }
    }
}
