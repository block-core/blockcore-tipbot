using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TipBot.Services;

namespace TipBot.CommandModules
{
    public class PublicCommands : ModuleBase<SocketCommandContext>
    {
        /// <remarks>Set by DI.</remarks>
        public PictureService PictureService { get; set; }

        [Command("tip")]
        public Task TipAsync(IUser user, double amount, [Remainder]string message = null)
        {
            string answer = $"User: {user.Mention}, Amount: {amount}";
            if (message != null)
            {
                answer += $" Reason: {message}";
            }

            return this.ReplyAsync(answer);
        }

        [Command("deposit")]
        public Task DepositAsync()
        {
            SocketUser user = this.Context.User;

            // TODO generate an address for a user and display it to him
            throw new NotImplementedException();
        }

        [Command("withdraw")]
        public Task WithdrawAsync(string address, double amount)
        {
            throw new NotImplementedException();
        }

        [Command("createQuiz")]
        public Task CreateQuizAsync(double amount, string answerSHA256, int durationMinutes, [Remainder]string question)
        {
            // TODO user will be able to start a quiz. First to answer will get a reward.
            // Quiz creator specifies SHA256 of an answer.

            throw new NotImplementedException();
        }

        [Command("about")]
        public async Task AboutAsync()
        {
            Stream stream = await this.PictureService.GetStratisLogoAsync();

            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            string text = $"Version: {version}";
            await this.Context.Channel.SendFileAsync(stream, "logo.png", text);
        }
    }
}
