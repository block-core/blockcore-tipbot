using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using NLog;

namespace TipBot.Logic
{
    public class DiscordConnectionKeepAlive : IDisposable
    {
        // How long should we wait on the client to reconnect before resetting?
        private readonly TimeSpan timeout = TimeSpan.FromSeconds(80);

        private readonly DiscordSocketClient discord;
        private readonly Settings settings;

        private readonly Logger logger;

        private readonly CancellationTokenSource cancellation;

        private Task keepAliveTask;

        public DiscordConnectionKeepAlive(DiscordSocketClient discord, Settings settings)
        {
            this.cancellation = new CancellationTokenSource();

            this.discord = discord;
            this.settings = settings;

            this.logger = LogManager.GetCurrentClassLogger();
        }

        public void Initialize()
        {
            this.logger.Trace("()");

            this.keepAliveTask = this.KeepAliveContinouslyAsync();

            this.logger.Trace("(-)");
        }

        private async Task KeepAliveContinouslyAsync()
        {
            this.logger.Trace("()");

            try
            {
                while (!this.cancellation.IsCancellationRequested)
                {
                    await Task.Delay(10000, this.cancellation.Token).ConfigureAwait(false);

                    // Client reconnected, no need to reset
                    if (this.discord.ConnectionState == ConnectionState.Connected)
                        continue;

                    Task timeoutTask = Task.Delay(this.timeout, this.cancellation.Token);

                    Task connect = Task.Run(async () =>
                    {
                        await this.discord.LoginAsync(TokenType.Bot, this.settings.BotToken).ConfigureAwait(false);
                        await this.discord.StartAsync().ConfigureAwait(false);
                    });

                    Task task = await Task.WhenAny(timeoutTask, connect).ConfigureAwait(false);

                    if (task != timeoutTask || connect.IsFaulted)
                    {
                        if (connect.IsFaulted)
                            this.logger.Fatal("Client reset faulted. Exception: '{0}'", connect.Exception);
                        else
                            this.logger.Fatal("Client reset timed out.");
                    }
                    else if (connect.IsCompletedSuccessfully)
                        this.logger.Info("Client reset was successful!");
                }
            }
            catch (OperationCanceledException)
            {
            }

            this.logger.Trace("(-)");
        }

        public void Dispose()
        {
            this.logger.Trace("()");

            this.cancellation.Cancel();
            this.keepAliveTask?.GetAwaiter().GetResult();

            this.logger.Trace("(-)");
        }
    }
}
