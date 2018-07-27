using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using NLog;
using TipBot.Services;

namespace TipBot.CommandModules
{
    // TODO that came from example. Remove it later.
    // Modules must be public and inherit from an IModuleBase
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        /// <remarks>Set by DI.</remarks>
        public PictureService PictureService { get; set; }

        private Logger logger = LogManager.GetCurrentClassLogger();

        [Command("ping")]
        [Alias("pong", "hello")]
        public Task PingAsync()
        {
            this.logger.Trace("()");

            Task<IUserMessage> task = this.ReplyAsync("pong!");

            this.logger.Trace("(-)");
            return task;
        }

        // Get info on a user, or the user who invoked the command if one is not specified
        [Command("userinfo")]
        public async Task UserInfoAsync(IUser user = null)
        {
            user = user ?? this.Context.User;

            await this.ReplyAsync(user.ToString());
        }

        // Ban a user
        [Command("ban")]
        [RequireContext(ContextType.Guild)]
        // make sure the user invoking the command can ban
        [RequireUserPermission(GuildPermission.BanMembers)]
        // make sure the bot itself can ban
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanUserAsync(IGuildUser user, [Remainder] string reason = null)
        {
            await user.Guild.AddBanAsync(user, reason: reason);
            await this.ReplyAsync("ok!");
        }

        // [Remainder] takes the rest of the command's arguments as one argument, rather than splitting every space
        [Command("echo")]
        public Task EchoAsync([Remainder] string text)
        {
            // Insert a ZWSP before the text to prevent triggering other bots!
            return this.ReplyAsync('\u200B' + text);
        }
    }
}
