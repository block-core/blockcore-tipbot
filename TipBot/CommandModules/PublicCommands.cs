using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TipBot.Logic;
using TipBot.Services;

namespace TipBot.CommandModules
{
    public class PublicCommands : ModuleBase<SocketCommandContext>
    {
        /// <inheritdoc cref="PictureService"/>
        /// <remarks>Set by DI.</remarks>
        public PictureService PictureService { get; set; }

        /// <inheritdoc cref="CommandsManager"/>
        /// <remarks>
        /// Set by DI.
        /// <para>
        /// All access to it should be protected by <see cref="lockObject"/>.
        /// This basically reduces the amount of simultaneous calls the bot can handle and this is intentional
        /// because at the same time such approach guarantees that there will be no race conditions which might
        /// lead to funds being lost.
        /// </para>
        /// </remarks>
        public CommandsManager CommandsManager { get; set; }

        /// <inheritdoc cref="Settings"/>
        /// <remarks>Set by DI.</remarks>
        public Settings Settings { get; set; }

        /// <summary>Protects access to <see cref="CommandsManager"/>.</summary>
        private object lockObject = new object();

        [Command("tip")]
        public Task TipAsync(IUser userBeingTipped, double amount, [Remainder]string message = null)
        {
            IUser sender = this.Context.User;

            string response;

            lock (this.lockObject)
            {
                try
                {
                    this.CommandsManager.TipUser(sender, userBeingTipped, amount);

                    response = $"{sender.Mention} tipped {userBeingTipped.Mention} {amount} {this.Settings.Ticker}";

                    if (message != null)
                        response += $"with message '{message}'";
                }
                catch (CommandExecutionException exception)
                {
                    response = "Error: " + exception.Message;
                }
            }

            return this.ReplyAsync(response);
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
            // TODO
            throw new NotImplementedException();
        }

        [Command("balance")]
        public Task BalanceAsync()
        {
            IUser sender = this.Context.User;

            string response;

            lock (this.lockObject)
            {
                double balance = this.CommandsManager.GetBalance(sender);

                response = $"{sender.Mention}, you have {balance} {this.Settings.Ticker}!";
            }

            return this.ReplyAsync(response);
        }

        [Command("createQuiz")]
        public Task CreateQuizAsync(double amount, string answerSHA256, int durationMinutes, [Remainder]string question)
        {
            // TODO user will be able to start a quiz. First to answer will get a reward.
            // Quiz creator specifies SHA256 of an answer.

            // TODO
            throw new NotImplementedException();
        }

        [Command("answerQuiz")]
        public Task AnswerQuizAsync([Remainder]string answer)
        {
            // TODO
            throw new NotImplementedException();
        }

        [Command("help")]
        public Task HelpAsync()
        {
            // TODO
            throw new NotImplementedException();
        }

        [Command("about")]
        public async Task AboutAsync()
        {
            Stream stream = await this.PictureService.GetLogoAsync();

            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            string text = $"Version: {version}" + Environment.NewLine + "github: https://github.com/noescape00/DiscordTipBot";
            await this.Context.Channel.SendFileAsync(stream, "logo.png", text);
        }
    }
}
