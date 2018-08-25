using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using NLog;

namespace TipBot.Helpers
{
    public class MessagesHelper
    {
        private readonly Settings settings;

        private readonly Logger logger;

        public MessagesHelper(Settings settings)
        {
            this.settings = settings;

            this.logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>Sends a message that will be removed after <see cref="Settings.SelfDestructedMessagesDelaySeconds"/> seconds.</summary>
        public async Task SendSelfDesctructedMessage(SocketCommandContext context, string text, bool addPostfix = true)
        {
            this.logger.Trace("({0}.{1}:{2})", nameof(text), nameof(text.Length), text.Length);

            int delaySeconds = this.settings.SelfDestructedMessagesDelaySeconds;
            string messageToSend = text + (addPostfix ? this.GetSelfDesctructionPostfix(delaySeconds) : string.Empty);

            RestUserMessage message = await context.Channel.SendMessageAsync(messageToSend).ConfigureAwait(false);

            this.SelfDestruct(message, delaySeconds);

            this.logger.Trace("(-)");
        }

        /// <summary>Sends a message that will be removed after <paramref name="delaySeconds"/> seconds.</summary>
        public async Task SendSelfDesctructedMessage(SocketCommandContext context, string text, int delaySeconds, bool addPostfix = true)
        {
            this.logger.Trace("({0}.{1}:{2},{3}:{4})", nameof(text), nameof(text.Length), text.Length, nameof(delaySeconds), delaySeconds);

            string messageToSend = text + (addPostfix ? this.GetSelfDesctructionPostfix(delaySeconds) : string.Empty);

            RestUserMessage message = await context.Channel.SendMessageAsync(messageToSend).ConfigureAwait(false);

            this.SelfDestruct(message, delaySeconds);

            this.logger.Trace("(-)");
        }

        /// <summary>Deletes target message after specified amount of time.</summary>
        public void SelfDestruct(IUserMessage message, int delaySeconds)
        {
            this.logger.Trace("({0}:{1})", nameof(delaySeconds), delaySeconds);

            Task.Run(async () =>
            {
                if (delaySeconds != 0)
                    await Task.Delay(delaySeconds * 1000).ConfigureAwait(false);
                await message.DeleteAsync().ConfigureAwait(false);
            });

            this.logger.Trace("(-)");
        }

        private string GetSelfDesctructionPostfix(int delaySeconds)
        {
            return Environment.NewLine + Environment.NewLine +
                   $"_`This message will be automatically removed in {delaySeconds} seconds.`_";
        }
    }
}
