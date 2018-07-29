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
using TipBot.Services;

namespace TipBot.Logic
{
    public class TipBot : IDisposable
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected IServiceProvider services;

        public async Task StartAsync(string[] args)
        {
            this.logger.Info("Starting the bot.");

            try
            {
                this.services = this.GetServicesCollection().BuildServiceProvider();

                var settings = this.services.GetRequiredService<Settings>();
                settings.Initialize(new TextFileConfiguration(args));

                // Migrate DB in case there are updates in the db layout.
                using (BotDbContext db = this.services.GetRequiredService<IContextFactory>().CreateContext())
                {
                    db.Database.Migrate();
                }

                this.services.GetRequiredService<RPCIntegration>().Initialize();

                var client = this.services.GetRequiredService<DiscordSocketClient>();

                client.Log += this.LogAsync;
                this.services.GetRequiredService<CommandService>().Log += this.LogAsync;

                await client.LoginAsync(TokenType.Bot, settings.BotToken);
                await client.StartAsync();

                await this.services.GetRequiredService<CommandHandlingService>().InitializeAsync();
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

        protected virtual IServiceCollection GetServicesCollection()
        {
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<HttpClient>()
                .AddSingleton<PictureService>()
                .AddSingleton<Settings>()
                .AddSingleton<CommandsManager>()
                .AddSingleton<IContextFactory, ContextFactory>()
                .AddSingleton<RPCIntegration>();
        }

        public void Dispose()
        {
            this.logger.Info("Application is shutting down...");

            this.services.GetRequiredService<DiscordSocketClient>()?.Dispose();
            this.services.GetRequiredService<HttpClient>()?.Dispose();
            this.services.GetRequiredService<RPCIntegration>()?.Dispose();

            this.logger.Info("Shutdown completed.");
        }
    }
}
