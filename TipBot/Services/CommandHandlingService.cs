using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TipBot.Helpers;
using Microsoft.Extensions.Options;

namespace TipBot.Services
{
    /// <summary>Processes raw message and calls appropriate command.</summary>
    public class CommandHandlingService
    {
        private readonly CommandService commands;
        private readonly DiscordSocketClient discord;
        private readonly IServiceProvider services;
        private readonly BotPrefixes prefixes;
        private readonly ErrorMessageCreator errorMessageCreator;
        private readonly MessagesHelper messagesHelper;

        public CommandHandlingService(IServiceProvider services, IOptionsMonitor<TipBotSettings> options, MessagesHelper messagesHelper)
        {
            this.commands = services.GetRequiredService<CommandService>();
            this.discord = services.GetRequiredService<DiscordSocketClient>();
            this.services = services;
            this.messagesHelper = messagesHelper;

            this.prefixes = new BotPrefixes(options);
            this.errorMessageCreator = new ErrorMessageCreator();

            this.discord.MessageReceived += this.MessageReceivedAsync;
        }

        public async Task InitializeAsync(IServiceProvider services)
        {
            await this.commands.AddModulesAsync(Assembly.GetEntryAssembly(), services).ConfigureAwait(false);
        }

        private async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            // Ignore system messages, or messages from other bots.
            if (!(rawMessage is SocketUserMessage message))
                return;

            // Ignore messages from bots.
            if (message.Source != MessageSource.User)
                return;

            // This value holds the offset where the prefix ends.
            var argPos = 0;
            if (!this.prefixes.BotPrefixMentioned(message, this.discord.CurrentUser, ref argPos))
                return;

            var context = new SocketCommandContext(this.discord, message);
            IResult result = await this.commands.ExecuteAsync(context, argPos, this.services).ConfigureAwait(false);

            // Handle errors.
            if (result.Error.HasValue)
            {
                string errorMessage = this.errorMessageCreator.CreateErrorMessage(result);

                await this.messagesHelper.SendSelfDesctructedMessage(context, errorMessage).ConfigureAwait(false);
            }
        }
    }
}
