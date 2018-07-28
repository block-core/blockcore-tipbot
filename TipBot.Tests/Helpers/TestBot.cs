using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TipBot.Database;

namespace TipBot.Tests.Helpers
{
    /// <summary>
    /// Bot for testing that is exactly the same as <see cref="TipBot.Logic.TipBot"/>
    /// except for it uses temporary in-memory database.
    /// </summary>
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
}
