using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly Settings settings;

        private List<string> prefixes;

        public CommandHandlingService(IServiceProvider services, Settings settings)
        {
            this.commands = services.GetRequiredService<CommandService>();
            this.discord = services.GetRequiredService<DiscordSocketClient>();
            this.services = services;
            this.settings = settings;

            this.discord.MessageReceived += this.MessageReceivedAsync;
        }

        private List<string> GetPrefixes()
        {
            if (this.prefixes != null)
                return this.prefixes;

            var prefixesRaw = new List<string>();

            SocketSelfUser bot = this.discord.CurrentUser;

            prefixesRaw.Add($"<@{bot.Id}> ");
            prefixesRaw.Add($"<@!{bot.Id}> ");
            prefixesRaw.Add($"@{this.discord.CurrentUser.Username}#{this.discord.CurrentUser.Discriminator} ");

            // By optional alias.
            if (this.settings.BotOptionalPrefix != null)
                prefixesRaw.Add(this.settings.BotOptionalPrefix + " ");

            // Allow double spaces
            foreach (string prefix in prefixesRaw.ToList())
                prefixesRaw.Add(prefix + " ");

            prefixesRaw.Reverse();
            this.prefixes = prefixesRaw;
            return this.prefixes;
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
            if (!this.BotPrefixMentioned(message, ref argPos))
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

        private bool BotPrefixMentioned(SocketUserMessage message, ref int argPos)
        {
            foreach (string prefix in this.GetPrefixes())
            {
                if (message.HasStringPrefix(prefix, ref argPos))
                    return true;
            }

            return false;
        }
    }
}
