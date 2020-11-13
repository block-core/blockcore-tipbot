using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using NLog;

namespace TipBot.Logic
{
    /// <summary>Notifies a specific user about an error.</summary>
    public class FatalErrorNotifier
    {
        private SocketUser SupportUser;

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public async Task InitializeAsync(DiscordSocketClient client, TipBotSettings settings)
        {
            this.logger.Trace("()");

            while (this.SupportUser == null)
            {
                await Task.Delay(20000).ConfigureAwait(false);

                if (settings.Discord.SupportUserId > 0)
                {
                    this.SupportUser = client.GetUser(settings.Discord.SupportUserId);
                }
                else
                {
                    this.SupportUser = client.GetUser(settings.Discord.SupportUsername, settings.Discord.SupportDiscriminator);
                }

                if (this.SupportUser == null)
                {
                    this.logger.Warn("Support user is null!");
                }
            }
            this.logger.Trace("(-)");
        }

        public void NotifySupport(string message)
        {
            this.logger.Trace("()");

            var maxLenght = 2000;
            if (message.Length > maxLenght)
                message = message.Substring(0, maxLenght);

            this.SupportUser?.SendMessageAsync(message).GetAwaiter().GetResult();

            this.logger.Trace("(-)");
        }
    }
}
