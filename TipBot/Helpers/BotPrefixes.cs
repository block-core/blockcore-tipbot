using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord.Commands;
using Discord.WebSocket;

namespace TipBot.Helpers
{
    public class BotPrefixes
    {
        private List<string> prefixes;

        private readonly Settings settings;

        /// <summary>Allow 2 spaces after bot name before the commands starts.</summary>
        private const bool AllowDoubleSpaces = true;

        public BotPrefixes(Settings settings)
        {
            this.settings = settings;
        }

        private List<string> GetPrefixes(SocketSelfUser botUser)
        {
            if (this.prefixes != null)
                return this.prefixes;

            var prefixesRaw = new List<string>();

            prefixesRaw.Add($"<@{botUser.Id}> ");
            prefixesRaw.Add($"<@!{botUser.Id}> ");
            prefixesRaw.Add($"@{botUser.Username}#{botUser.Discriminator} ");

            // By optional alias.
            if (this.settings.BotOptionalPrefix != null)
                prefixesRaw.Add(this.settings.BotOptionalPrefix + " ");

            // Allow double spaces
            if (AllowDoubleSpaces)
            {
                foreach (string prefix in prefixesRaw.ToList())
                    prefixesRaw.Add(prefix + " ");
            }

            prefixesRaw = prefixesRaw.OrderByDescending(s => s.Length).ToList();
            this.prefixes = prefixesRaw;

            return this.prefixes;
        }

        public bool BotPrefixMentioned(SocketUserMessage message, SocketSelfUser botUser, ref int argPos)
        {
            foreach (string prefix in this.GetPrefixes(botUser))
            {
                if (message.HasStringPrefix(prefix, ref argPos))
                    return true;
            }

            return false;
        }
    }
}
