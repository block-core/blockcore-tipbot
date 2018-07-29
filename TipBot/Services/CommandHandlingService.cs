using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace TipBot.Services
{
    /// <summary>Processes raw message and calls appropriate command.</summary>
    public class CommandHandlingService
    {
        private readonly CommandService commands;
        private readonly DiscordSocketClient discord;
        private readonly IServiceProvider services;

        public CommandHandlingService(IServiceProvider services)
        {
            this.commands = services.GetRequiredService<CommandService>();
            this.discord = services.GetRequiredService<DiscordSocketClient>();
            this.services = services;

            this.discord.MessageReceived += this.MessageReceivedAsync;
        }

        public async Task InitializeAsync()
        {
            await this.commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            // Ignore system messages, or messages from other bots.
            if (!(rawMessage is SocketUserMessage message))
                return;

            if (message.Source != MessageSource.User)
                return;

            // This value holds the offset where the prefix ends.
            var argPos = 0;
            if (!message.HasMentionPrefix(this.discord.CurrentUser, ref argPos))
                return;

            var context = new SocketCommandContext(this.discord, message);
            IResult result = await this.commands.ExecuteAsync(context, argPos, this.services);

            if (result.Error.HasValue)
            {
                if (result.Error.Value == CommandError.UnknownCommand)
                {
                    await context.Channel.SendMessageAsync(":no_entry: Unknown command. Use `help` command to display all supported commands.");
                }
                else
                {
                    await context.Channel.SendMessageAsync($":no_entry: Error: {result.Error}. {result.ErrorReason}");
                }
            }
        }
    }
}
