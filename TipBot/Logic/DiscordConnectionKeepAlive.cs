using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using NLog;

namespace TipBot.Logic
{
    public class DiscordConnectionKeepAlive
    {
        // How long should we wait on the client to reconnect before resetting?
        private readonly TimeSpan timeout = TimeSpan.FromSeconds(30);

        private readonly DiscordSocketClient discord;
        private CancellationTokenSource cancellation;

        private readonly Logger logger;

        public DiscordConnectionKeepAlive(DiscordSocketClient discord)
        {
            this.discord = discord;
            this.logger = LogManager.GetCurrentClassLogger();
        }

        public void Initialize()
        {
            this.logger.Trace("()");

            this.cancellation = new CancellationTokenSource();

            this.discord.Connected += this.ConnectedAsync;
            this.discord.Disconnected += this.DisconnectedAsync;

            this.logger.Trace("(-)");
        }

        private Task ConnectedAsync()
        {
            this.logger.Trace("()");

            //Client reconnected, reset cancellation tokens.
            this.cancellation.Cancel();
            this.cancellation = new CancellationTokenSource();

            this.logger.Trace("(-)");
            return Task.CompletedTask;
        }

        private Task DisconnectedAsync(Exception exception)
        {
            this.logger.Trace("()");

            this.logger.Info("Client disconnected, starting timeout task...");

            CancellationToken token = this.cancellation.Token;

            Task.Run(async () =>
            {
                await Task.Delay(this.timeout, token).ConfigureAwait(false);

                this.logger.Debug("Timeout expired, continuing to check client state...");

                await this.CheckStateAsync().ConfigureAwait(false);

                this.logger.Debug("State came back okay.");
            });

            this.logger.Trace("(-)");
            return Task.CompletedTask;
        }

        private async Task CheckStateAsync()
        {
            this.logger.Trace("()");

            // Client reconnected, no need to reset
            if (this.discord.ConnectionState == ConnectionState.Connected)
            {
                this.logger.Trace("(-)[CONNECTED]");
                return;
            }

            Task timeoutTask = Task.Delay(this.timeout);
            Task connect = this.discord.StartAsync();

            Task task = await Task.WhenAny(timeoutTask, connect).ConfigureAwait(false);

            if (task != timeoutTask || connect.IsFaulted)
            {
                if (connect.IsFaulted)
                    this.logger.Fatal("Client reset faulted, killing process. Exception: '{0}'", connect.Exception);
                else
                    this.logger.Fatal("Client reset timed out (task deadlocked?), killing process.");

                this.CheckStateAsync();
            }
            else if (connect.IsCompletedSuccessfully)
                this.logger.Info("Client reset succesfully!");

            this.logger.Trace("(-)");
        }
    }
}
