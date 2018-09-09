using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using TipBot.Database;
using TipBot.Database.Models;
using TipBot.Helpers;
using TipBot.Logic.NodeIntegrations;
using TipBot.Services;

namespace TipBot.Logic
{
    public class TipBot : IDisposable
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected IServiceProvider services;

        public async Task StartAsync(string[] args)
        {
            this.logger.Trace("({0}.{1}:{2})", nameof(args), nameof(args.Length), args.Length);
            this.logger.Info("Starting the bot.");

            try
            {
                this.services = this.GetServicesCollection().BuildServiceProvider();

                var settings = this.services.GetRequiredService<Settings>();
                settings.Initialize(new TextFileConfiguration(args));

                if (settings.EnableMigrations)
                {
                    // Migrate DB in case there are updates in the db layout.
                    using (BotDbContext db = this.services.GetRequiredService<IContextFactory>().CreateContext())
                    {
                        db.Database.Migrate();
                    }
                }

                this.services.GetRequiredService<INodeIntegration>().Initialize();
                this.services.GetRequiredService<QuizExpiryChecker>().Initialize();

                // Initialize discord API wrapper.
                var client = this.services.GetRequiredService<DiscordSocketClient>();

                client.Log += this.LogAsync;
                this.services.GetRequiredService<CommandService>().Log += this.LogAsync;

                await client.LoginAsync(TokenType.Bot, settings.BotToken).ConfigureAwait(false);
                await client.StartAsync().ConfigureAwait(false);

                this.services.GetRequiredService<DiscordConnectionKeepAlive>().Initialize();

                await this.services.GetRequiredService<CommandHandlingService>().InitializeAsync().ConfigureAwait(false);
                await this.services.GetRequiredService<FatalErrorNotifier>().InitializeAsync(client, settings).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                this.logger.Fatal(exception.ToString());
                throw;
            }

            this.logger.Trace("(-)");
        }

        private Task LogAsync(LogMessage log)
        {
            this.logger.Info("DiscordLog: " + log.ToString());

            return Task.CompletedTask;
        }

        protected virtual IServiceCollection GetServicesCollection()
        {
            this.logger.Trace("()");

            IServiceCollection collection = new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<Settings>()
                .AddSingleton<CommandsManager>()
                .AddSingleton<QuizExpiryChecker>()
                .AddSingleton<FatalErrorNotifier>()
                .AddSingleton<IContextFactory, ContextFactory>()
                .AddSingleton<DiscordConnectionKeepAlive>()
                .AddSingleton<MessagesHelper>()
                // Replace implementation to use API instead of RPC.
                .AddSingleton<INodeIntegration, RPCNodeIntegration>();

            this.logger.Trace("(-)");
            return collection;
        }

        public void Dispose()
        {
            this.logger.Trace("()");
            this.logger.Info("Application is shutting down...");

            this.services.GetRequiredService<DiscordSocketClient>()?.Dispose();
            this.services.GetRequiredService<INodeIntegration>()?.Dispose();
            this.services.GetRequiredService<QuizExpiryChecker>()?.Dispose();
            this.services.GetRequiredService<DiscordConnectionKeepAlive>()?.Dispose();

            this.logger.Info("Shutdown completed.");
            this.logger.Trace("(-)");
        }
    }
}
