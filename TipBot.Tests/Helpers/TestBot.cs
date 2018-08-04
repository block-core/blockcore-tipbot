using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TipBot.Database;
using TipBot.Logic.NodeIntegrations;

namespace TipBot.Tests.Helpers
{
    /// <summary>
    /// Bot for testing that is exactly the same as <see cref="TipBot.Logic.TipBot"/>
    /// except for it uses temporary in-memory database.
    /// </summary>
    public class TestBot : Logic.TipBot
    {
        public new async Task StartAsync(string[] args)
        {
            await base.StartAsync(args);
        }

        protected override IServiceCollection GetServicesCollection()
        {
            // Replace real context factory with the one that serves contexts that are using in-memory database.
            IServiceCollection servicesCollection = base.GetServicesCollection();

            servicesCollection.Remove(servicesCollection.First(x => x.ServiceType == typeof(IContextFactory)));
            var contextFactoryDescriptor = new ServiceDescriptor(typeof(IContextFactory), typeof(TestContextFactory), ServiceLifetime.Singleton);
            servicesCollection.Add(contextFactoryDescriptor);

            servicesCollection.Remove(servicesCollection.First(x => x.ServiceType == typeof(INodeIntegration)));
            var nodeIntegrationDescriptor = new ServiceDescriptor(typeof(INodeIntegration), typeof(TestNodeIntegration), ServiceLifetime.Singleton);
            servicesCollection.Add(nodeIntegrationDescriptor);

            return servicesCollection;
        }

        public T GetService<T>()
        {
            var service = this.services.GetRequiredService<T>();
            return service;
        }
    }
}
