using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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

        private TipBotSettings settings;

        protected readonly IServiceProvider services;

        private readonly DiscordSocketClient client;

        private readonly INodeIntegration nodeIntegration;

        private readonly QuizExpiryChecker quizExpiryChecker;

        private readonly CommandService commandService;

        private readonly DiscordConnectionKeepAlive discordConnectionKeepAlive;

        private readonly CommandHandlingService commandHandlingService;

        private readonly FatalErrorNotifier fatalErrorNotifier;

        public TipBot(
            IOptionsMonitor<TipBotSettings> options, 
            IServiceProvider services,
            DiscordSocketClient client,
            INodeIntegration nodeIntegration,
            QuizExpiryChecker quizExpiryChecker,
            CommandService commandService,
            DiscordConnectionKeepAlive discordConnectionKeepAlive,
            CommandHandlingService commandHandlingService,
            FatalErrorNotifier fatalErrorNotifier
            )
        {
            this.settings = options.CurrentValue;
            this.services = services;
            this.client = client;
            this.nodeIntegration = nodeIntegration;
            this.quizExpiryChecker = quizExpiryChecker;
            this.commandService = commandService;
            this.discordConnectionKeepAlive = discordConnectionKeepAlive;
            this.commandHandlingService = commandHandlingService;
            this.fatalErrorNotifier = fatalErrorNotifier;

            options.OnChange(config =>
            {
                this.settings = config;
            });
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // this.logger.Trace("({0}.{1}:{2})", nameof(args), nameof(args.Length), args.Length);
            this.logger.Info("Starting the bot.");

            try
            {
                //IConfiguration Configuration = new ConfigurationBuilder()
                //    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                //    .AddEnvironmentVariables()
                //    .AddCommandLine(args)
                //    .Build();

                // this.services = this.GetServicesCollection().BuildServiceProvider();

                //settings.Initialize(new TextFileConfiguration(args));

                if (settings.EnableMigrations)
                {
                    // Migrate DB in case there are updates in the db layout.
                    using (BotDbContext db = this.services.GetRequiredService<IContextFactory>().CreateContext())
                    {
                        db.Database.Migrate();
                    }
                }

                this.nodeIntegration.Initialize();
                this.quizExpiryChecker.Initialize();

                client.Log += this.LogAsync;
                this.commandService.Log += this.LogAsync;

                await client.LoginAsync(TokenType.Bot, settings.BotToken).ConfigureAwait(false);
                await client.StartAsync().ConfigureAwait(false);

                this.discordConnectionKeepAlive.Initialize();

                await this.commandHandlingService.InitializeAsync(this.services).ConfigureAwait(false);
                await this.fatalErrorNotifier.InitializeAsync(client, settings).ConfigureAwait(false);
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

        //protected virtual IServiceCollection GetServicesCollection()
        //{
        //    this.logger.Trace("()");

        //    IServiceCollection collection = new ServiceCollection()
        //        .AddSingleton<DiscordSocketClient>()
        //        .AddSingleton<CommandService>()
        //        .AddSingleton<CommandHandlingService>()
        //        .AddSingleton<Settings>()
        //        .AddSingleton<CommandsManager>()
        //        .AddSingleton<QuizExpiryChecker>()
        //        .AddSingleton<FatalErrorNotifier>()
        //        .AddSingleton<IContextFactory, ContextFactory>()
        //        .AddSingleton<DiscordConnectionKeepAlive>()
        //        .AddSingleton<MessagesHelper>()
        //        .AddSingleton<INodeIntegration, BlockCoreNodeIntegration>();

        //    this.logger.Trace("(-)");
        //    return collection;
        //}

        public void Dispose()
        {
            this.logger.Trace("()");
            this.logger.Info("Application is shutting down...");

            this.client?.Dispose();
            this.nodeIntegration?.Dispose();
            this.quizExpiryChecker?.Dispose();
            this.discordConnectionKeepAlive?.Dispose();

            this.logger.Info("Shutdown completed.");
            this.logger.Trace("(-)");
        }
    }
}
