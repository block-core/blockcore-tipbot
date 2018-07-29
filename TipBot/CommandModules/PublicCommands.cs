using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
        private readonly object lockObject = new object();

        [CommandWithHelp("tip", "Transfers specified amount of money to mentioned user.", "tip <user> <amount> <message>*")]
        public Task TipAsync(IUser userBeingTipped, decimal amount, [Remainder]string message = null)
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

        [CommandWithHelp("deposit", "Displays your unique deposit address or assigns you one if it wasn't assigned before.")]
        public Task DepositAsync()
        {
            SocketUser user = this.Context.User;

            string depositAddress = null;

            lock (this.lockObject)
            {
                try
                {
                    depositAddress = this.CommandsManager.GetDepositAddress(user);
                }
                catch (OutOfDepositAddresses)
                {
                    return this.ReplyAsync("Bot ran out of deposit addresses. Tell bot admin about it.");
                }
            }

            string response = $"Your unique deposit address is `{depositAddress}`";
            response += Environment.NewLine + $"Money are deposited after {this.Settings.MinConfirmationsForDeposit} confirmations.";

            return this.ReplyAsync(response);
        }

        [CommandWithHelp("withdraw", "Withdraws given amount to specified address.", "withdraw <address> <amount>")]
        public Task WithdrawAsync(string address, decimal amount)
        {
            // TODO
            throw new NotImplementedException();
        }

        [CommandWithHelp("balance", "Displays your current balance.")]
        public Task BalanceAsync()
        {
            IUser sender = this.Context.User;

            string response;

            lock (this.lockObject)
            {
                decimal balance = this.CommandsManager.GetUserBalance(sender);

                response = $"{sender.Mention}, you have {balance} {this.Settings.Ticker}!";
            }

            return this.ReplyAsync(response);
        }

        [CommandWithHelp("createQuiz", "TODO")]
        public Task CreateQuizAsync(decimal amount, string answerSHA256, int durationMinutes, [Remainder]string question)
        {
            // TODO user will be able to start a quiz. First to answer will get a reward.
            // Quiz creator specifies SHA256 of an answer.

            // TODO
            throw new NotImplementedException();
        }

        [CommandWithHelp("answerQuiz", "TODO")]
        public Task AnswerQuizAsync([Remainder]string answer)
        {
            // TODO
            throw new NotImplementedException();
        }

        [Command("help")]
        public Task HelpAsync()
        {
            var helpAttributes = new List<CommandWithHelpAttribute>();

            foreach (MemberInfo memberInfo in this.GetType().GetMembers())
            {
                foreach (CommandWithHelpAttribute attribute in memberInfo.GetCustomAttributes(typeof(CommandWithHelpAttribute), true).ToList())
                    helpAttributes.Add(attribute);
            }

            var builder = new StringBuilder();

            builder.AppendLine("__List of bot commands:__");
            builder.AppendLine("");

            foreach (CommandWithHelpAttribute helpAttr in helpAttributes)
            {
                string helpStr = $"`{helpAttr.Text}`- " + helpAttr.HelpInfo;

                if (helpAttr.UsageExample != null)
                    helpStr += Environment.NewLine + "      " + helpAttr.UsageExample;

                builder.AppendLine(helpStr);
            }

            builder.AppendLine("");
            builder.AppendLine("parameters marked with * are optional");

            return this.ReplyAsync(builder.ToString());
        }

        [CommandWithHelp("about", "Displays information about the bot.")]
        public async Task AboutAsync()
        {
            Stream stream = await this.PictureService.GetLogoAsync();

            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            string text = "`TipBot`" + Environment.NewLine +
                $"Version: {version}" + Environment.NewLine + "Github: https://github.com/noescape00/DiscordTipBot";
            await this.Context.Channel.SendFileAsync(stream, "logo.png", text);
        }
    }
}
