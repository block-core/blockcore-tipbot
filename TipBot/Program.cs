using System;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using TipBot.Database;
using TipBot.Helpers;
using TipBot.Logic;
using TipBot.Services;

namespace TipBot
{
    internal class Program
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private IServiceProvider services;

        private static void Main(string[] args)
        {
            new Program().MainAsync(args).GetAwaiter().GetResult();
        }

        private async Task MainAsync(string[] args)
        {
            this.logger.Info("Starting application.");

            Console.CancelKeyPress += this.ShutdownHandler;

            try
            {
                // Migrate DB in case there are updates in the db layout.
                using (var db = new BotDbContext())
                {
                    db.Database.Migrate();
                }

                this.services = this.ConfigureServices();

                var settings = this.services.GetRequiredService<Settings>();
                settings.Initialize(new TextFileConfiguration(args));

                var client = this.services.GetRequiredService<DiscordSocketClient>();

                client.Log += this.LogAsync;
                this.services.GetRequiredService<CommandService>().Log += this.LogAsync;

                await client.LoginAsync(TokenType.Bot, settings.BotToken);
                await client.StartAsync();

                await this.services.GetRequiredService<CommandHandlingService>().InitializeAsync();

                await Task.Delay(-1);
            }
            catch (Exception exception)
            {
                this.logger.Fatal(exception.ToString());
            }
        }

        private Task LogAsync(LogMessage log)
        {
            this.logger.Info("DiscordLog: " + log.ToString());

            return Task.CompletedTask;
        }

        private IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<HttpClient>()
                .AddSingleton<PictureService>()
                .AddSingleton<Settings>()
                .AddSingleton<CommandsManager>()
                .BuildServiceProvider();
        }

        /// <summary>Shutdown the handler. Executed when user presses CTRL+C on console.</summary>
        private void ShutdownHandler(object sender, ConsoleCancelEventArgs args)
        {
            this.logger.Info("Application is shutting down...");

            this.services.GetRequiredService<DiscordSocketClient>()?.Dispose();
            this.services.GetRequiredService<HttpClient>()?.Dispose();
            this.services.GetRequiredService<CommandsManager>()?.Dispose();

            args.Cancel = true;
            this.logger.Info("Shutdown completed.");

            Environment.Exit(0);
        }
    }
}
