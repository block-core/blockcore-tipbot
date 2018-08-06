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

        public async Task InitializeAsync(DiscordSocketClient client, Settings settings)
        {
            this.logger.Trace("()");

            await Task.Delay(2000);

            this.SupportUser = client.GetUser(settings.SupportUsername, settings.SupportDiscriminator);

            if (this.SupportUser == null)
                throw new Exception("Support user is null!");

            this.logger.Trace("(-)");
        }

        public void NotifySupport(string message)
        {
            this.logger.Trace("()");

            var maxLenght = 2000;
            if (message.Length > maxLenght)
                message = message.Substring(0, maxLenght);

            this.SupportUser.SendMessageAsync(message).GetAwaiter().GetResult();

            this.logger.Trace("(-)");
        }
    }
}
