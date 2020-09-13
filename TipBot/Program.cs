using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using TipBot.Database;
using TipBot.Helpers;
using TipBot.Logic;
using TipBot.Logic.NodeIntegrations;
using TipBot.Services;

namespace TipBot
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();

            // new Program().MainAsync(args).GetAwaiter().GetResult();
        }

        internal static IConfiguration Configuration2 { get; set; }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                //.ConfigureHostConfiguration(config =>
                //{
                //    config.AddCommandLine(args);
                //    config.AddEnvironmentVariables();
                //})
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    //if (hostContext.HostingEnvironment.IsDevelopment())
                    //{

                    // User Secrets is not added by default to the generic host builder.
                    config.AddUserSecrets<Program>();

                    //}
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<Logic.TipBot>();
                    services.AddSingleton<DiscordSocketClient>();
                    services.AddSingleton<CommandService>();
                    services.AddSingleton<CommandHandlingService>();
                    services.AddSingleton<CommandsManager>();
                    services.AddSingleton<QuizExpiryChecker>();
                    services.AddSingleton<FatalErrorNotifier>();
                    services.AddSingleton<IContextFactory, ContextFactory>();
                    services.AddSingleton<DiscordConnectionKeepAlive>();
                    services.AddSingleton<MessagesHelper>();
                    services.AddSingleton<INodeIntegration, BlockCoreNodeIntegration>();

                    services.AddHostedService<Worker>();
                    services.Configure<Settings>(hostContext.Configuration.GetSection("Blockcore:TipBot"));
                });

        //private async Task MainAsync(string[] args)
        //{
        //    Console.CancelKeyPress += this.ShutdownHandler;

        //    this.bot = new Logic.TipBot();
        //    await this.bot.StartAsync(args);

        //    await Task.Delay(-1);
        //}

        ///// <summary>Shutdown the handler. Executed when user presses CTRL+C on console.</summary>
        //private void ShutdownHandler(object sender, ConsoleCancelEventArgs args)
        //{
        //    this.bot.Dispose();

        //    args.Cancel = true;
        //    Environment.Exit(0);
        //}
    }
}
