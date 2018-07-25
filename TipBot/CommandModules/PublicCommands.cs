using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TipBot.Services;

namespace TipBot.CommandModules
{
    public class PublicCommands : ModuleBase<SocketCommandContext>
    {
        /// <remarks>Set by DI.</remarks>
        public PictureService PictureService { get; set; }

        [Command("tip")]
        public Task TipTestAsync(IUser user, double amount, [Remainder]string reason = null)
        {
            string answer = $"User: {user.Mention}, Amount: {amount}";
            if (reason != null)
            {
                answer += $" Reason: {reason}";
            }

            return this.ReplyAsync(answer);
        }

        [Command("about")]
        public async Task AboutAsync()
        {
            Stream stream = await this.PictureService.GetStratisLogoAsync();

            await this.Context.Channel.SendFileAsync(stream, "logo.png", "qwe");
        }
    }
}
