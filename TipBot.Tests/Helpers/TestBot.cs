//using System;
//using System.Linq;
//using System.Threading.Tasks;
//using Discord;
//using Discord.Commands;
//using Discord.WebSocket;
//using Microsoft.Extensions.DependencyInjection;
//using TipBot.Database;
//using TipBot.Helpers;
//using TipBot.Logic;
//using TipBot.Logic.NodeIntegrations;
//using TipBot.Services;

//namespace TipBot.Tests.Helpers
//{
//    /// <summary>
//    /// Bot for testing that is exactly the same as <see cref="TipBot"/>
//    /// except for it uses temporary in-memory database.
//    /// </summary>
//    public class TestBot : Logic.TipBot
//    {
//        public new Task StartAsync(string[] args)
//        {
//            //this.services = this.GetServicesCollection().BuildServiceProvider();

//            var settings = this.services.GetRequiredService<Settings>();
//            //settings.Initialize(new TextFileConfiguration(args));

//            return Task.CompletedTask;
//        }

//        //protected override IServiceCollection GetServicesCollection()
//        //{
//        //    // Replace real context factory with the one that serves contexts that are using in-memory database.
//        //    IServiceCollection servicesCollection = base.GetServicesCollection();

//        //    servicesCollection.Remove(servicesCollection.First(x => x.ServiceType == typeof(IContextFactory)));
//        //    var contextFactoryDescriptor = new ServiceDescriptor(typeof(IContextFactory), typeof(TestContextFactory), ServiceLifetime.Singleton);
//        //    servicesCollection.Add(contextFactoryDescriptor);

//        //    servicesCollection.Remove(servicesCollection.First(x => x.ServiceType == typeof(INodeIntegration)));
//        //    var nodeIntegrationDescriptor = new ServiceDescriptor(typeof(INodeIntegration), typeof(TestNodeIntegration), ServiceLifetime.Singleton);
//        //    servicesCollection.Add(nodeIntegrationDescriptor);

//        //    return servicesCollection;
//        //}

//        public T GetService<T>()
//        {
//            var service = this.services.GetRequiredService<T>();
//            return service;
//        }
//    }
//}
