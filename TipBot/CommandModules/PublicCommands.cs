using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Blockcore.Settings;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using NLog;
using TipBot.Database.Models;
using TipBot.Helpers;
using TipBot.Logic;

namespace TipBot.CommandModules
{
    public class PublicCommands : ModuleBase<SocketCommandContext>
    {
        public PublicCommands(CommandsManager commandsManager,
            IOptionsMonitor<TipBotSettings> options,
            IOptionsMonitor<ChainSettings> chainOptions,
            MessagesHelper messagesHelper)
        {
            this.CommandsManager = commandsManager;
            this.Settings = options.CurrentValue;
            this.ChainSettings = chainOptions.CurrentValue;
            this.MessagesHelper = messagesHelper;
        }

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
        public CommandsManager CommandsManager { get; private set; }

        /// <inheritdoc cref="Settings"/>
        public TipBotSettings Settings { get; private set; }

        public ChainSettings ChainSettings { get; private set; }

        /// <inheritdoc cref="MessagesHelper"/>
        public MessagesHelper MessagesHelper { get; private set; }

        /// <summary>Protects access to <see cref="CommandsManager"/>.</summary>
        private readonly object lockObject = new object();

        private readonly string tadaEmoji = ":tada:";

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        byte[] logo = null;

        [CommandWithHelp("tip", "Transfers specified amount of money to mentioned user.", "{tipbot} tip <user> <amount> <message>*")]
        public Task TipAsync(IUser userBeingTipped, decimal amount, [Remainder] string message = null)
        {
            this.logger.Trace("({0}:{1},{2}:{3},{4}:'{5}')", nameof(userBeingTipped), userBeingTipped.Id, nameof(amount), amount, nameof(message), message);
            
            IUser sender = this.Context.User;

            string response;

            if (Settings.TipsEnabled)
            {
                lock (this.lockObject)
                {
                    try
                    {
                        this.CommandsManager.TipUser(sender, userBeingTipped, amount);

                        response = $"{sender.Mention} tipped {userBeingTipped.Mention} {amount} {this.Settings.Ticker}";

                        if (message != null)
                            response += $" with message `{message.Replace("`", "")}`";
                    }
                    catch (CommandExecutionException exception)
                    {
                        response = "Error: " + exception.Message;
                    }
                }

                response = this.TrimMessage(response);
            }
            else
            {
                string tipsDisabledMessage = "Sorry, tips are currently disabled.";
                response = this.TrimMessage(tipsDisabledMessage);
            }
            this.logger.Trace("(-)");
            return this.ReplyAsync(response);
        }

        [CommandWithHelp("deposit", "Displays your unique deposit address or assigns you one if it wasn't assigned before.", "{tipbot} deposit")]
        public Task DepositAsync()
        {
            this.logger.Trace("()");

            SocketUser user = this.Context.User;

            string response;

            lock (this.lockObject)
            {
                try
                {
                    string depositAddress = this.CommandsManager.GetDepositAddress(user);

                    response = $"{user.Mention}, your unique deposit address is `{depositAddress}`";
                    response += Environment.NewLine + $"Money are deposited after {this.Settings.MinConfirmationsForDeposit} confirmations.";
                }
                catch (OutOfDepositAddressesException)
                {
                    response = $"Bot ran out of deposit addresses. Tell bot admin ({this.Settings.Discord.SupportUsername}:{this.Settings.Discord.SupportDiscriminator}) about it.";
                }
            }

            response = this.TrimMessage(response);

            this.logger.Trace("(-)");
            return this.ReplyAsync(response);
        }

        [CommandWithHelp("withdraw", "Withdraws given amount to specified address. Fee will be subtracted from given amount." + "\n" +
                                     "Keep in mind that withdrawal address will be publicly visible to all users in this channel. " +
                                     "To avoid exposing your address use withdraw command in private messages with the bot.", "{tipbot} withdraw <amount> <address>")]
        public Task WithdrawAsync(decimal amount, string address)
        {
            this.logger.Trace("({0}:{1},{2}:{3})", nameof(amount), amount, nameof(address), address);

            IUser sender = this.Context.User;

            string response;

            lock (this.lockObject)
            {
                try
                {
                    this.CommandsManager.Withdraw(sender, amount, address);

                    response = $"{sender.Mention}, withdrawal of {amount} {this.Settings.Ticker} completed.";
                }
                catch (CommandExecutionException exception)
                {
                    response = "Error: " + exception.Message;
                }
            }

            response = this.TrimMessage(response);

            this.logger.Trace("(-)");
            return this.ReplyAsync(response);
        }

        [CommandWithHelp("fee", "The network fee that is subtracted from each withdraw", "{tipbot} fee")]
        public Task Fee()
        {
            string response = $"The network fee is: {this.Settings.NetworkFee}";
            response = this.TrimMessage(response);

            this.logger.Trace("(-)");
            return this.ReplyAsync(response);
        }

        [CommandWithHelp("balance", "Displays your current balance.", "{tipbot} balance")]
        public Task BalanceAsync()
        {
            this.logger.Trace("()");

            IUser sender = this.Context.User;

            string response;

            lock (this.lockObject)
            {
                decimal balance = this.CommandsManager.GetUserBalance(sender);

                response = $"{sender.Mention}, you have {balance} {this.Settings.Ticker}!";
            }

            response = this.TrimMessage(response);

            this.logger.Trace("(-)");
            return this.ReplyAsync(response);
        }

        [CommandWithHelp("makeItRain", "Randomly selects online users from the current server and tips them 1 coin (or another value if specified by caller)." +
                                       " Amount of users that will be tipped is equal to totalAmount / tipAmount.", "{tipbot} makeItRain <totalAmount> <tipAmount=1>*")]
        public async Task MakeItRainAsync(decimal amount, decimal tipAmount = 1)
        {
            this.logger.Trace("({0}:{1},{2}:{3})", nameof(amount), amount, nameof(tipAmount), tipAmount);

            IUser caller = this.Context.User;

            var onlineUsers = new List<IUser>();
            IAsyncEnumerable<IReadOnlyCollection<IUser>> usersCollection = this.Context.Channel.GetUsersAsync();

            await usersCollection.ForEachAsync(delegate (IReadOnlyCollection<IUser> users)
            {
                onlineUsers.AddRange(users.Where(x => x.Status != UserStatus.Offline && !x.IsBot));
            }).ConfigureAwait(false);

            onlineUsers.Remove(caller);

            string response;

            lock (this.lockObject)
            {
                try
                {
                    List<DiscordUserModel> usersBeingTipped = this.CommandsManager.RandomlyTipUsers(caller, onlineUsers, amount, tipAmount);

                    var builder = new StringBuilder();

                    builder.AppendLine($"{this.tadaEmoji}{caller.Mention} just tipped {usersBeingTipped.Count} users {tipAmount} {this.Settings.Ticker} each!{this.tadaEmoji}");
                    builder.AppendLine();

                    foreach (DiscordUserModel tippedUser in usersBeingTipped)
                    {
                        builder.Append($"<@!{tippedUser.DiscordUserId}>");

                        if (tippedUser != usersBeingTipped.Last())
                            builder.Append(", ");
                    }

                    builder.Append($" - you all received {tipAmount} {this.Settings.Ticker}!");

                    response = builder.ToString();
                }
                catch (CommandExecutionException exception)
                {
                    response = "Error: " + exception.Message;
                }
            }

            response = this.TrimMessage(response);

            this.logger.Trace("(-)");
            await this.ReplyAsync(response).ConfigureAwait(false);
        }

        [CommandWithHelp("chart", "Displays top 3 tippers and users being tipped over the last 7 days.", "{tipbot} chart <days=7>*")]
        public async Task ChartAsync(int days = 7)
        {
            this.logger.Trace("({0}:{1})", nameof(days), days);

            string response;

            lock (this.lockObject)
            {
                try
                {
                    TippingChartsModel chart = this.CommandsManager.GetTopTippers(days, this.Settings.MaxChartUsersCount);

                    var builder = new StringBuilder();

                    // Best tippers.
                    if (chart.BestTippers.Count != 0)
                    {
                        builder.AppendLine($"Top {chart.BestTippers.Count} users who tipped the most in the last {days} days:");

                        foreach (UserViewModel tipper in chart.BestTippers)
                            builder.AppendLine($"**{tipper.UserName}** tipped {tipper.Amount} {this.Settings.Ticker}");
                    }
                    else
                        builder.AppendLine($"No one tipped anyone in the last {days} days!");

                    builder.AppendLine();

                    // Best being tipped.
                    if (chart.BestBeingTipped.Count != 0)
                    {
                        builder.AppendLine($"Top {chart.BestBeingTipped.Count} users who were tipped the most in the last {days} days:");

                        foreach (UserViewModel beingTipped in chart.BestBeingTipped)
                            builder.AppendLine($"**{beingTipped.UserName}** received {beingTipped.Amount} {this.Settings.Ticker}");
                    }
                    else
                        builder.AppendLine($"No one was tipped in the last {days} days!");

                    response = builder.ToString();
                }
                catch (CommandExecutionException exception)
                {
                    response = "Error: " + exception.Message;
                }
            }

            response = this.TrimMessage(response);

            this.logger.Trace("(-)");
            await this.ReplyAsync(response).ConfigureAwait(false);
        }

        [CommandWithHelp("startQuiz", "You ask a question, supply hash of an answer and for how long the quiz will be running." +
                                       " First user to provide correct answer gets the prize! In case no one answers money will return back to you after quiz expiry." +
                                       " For hash generation use <https://passwordsgenerator.net/sha256-hash-generator/>",
                                        "{tipbot} startQuiz <amount> <SHA256 of an answer> <duration in minutes> <question>")]
        public async Task StartQuizAsync(decimal amount, string answerSHA256, int durationMinutes, [Remainder] string question)
        {
            this.logger.Trace("({0}:{1},{2}:'{3}',{4}:{5},{6}:'{7}')", nameof(amount), amount, nameof(answerSHA256), answerSHA256, nameof(durationMinutes), durationMinutes, nameof(question), question);

            IUser user = this.Context.User;

            string response;

            lock (this.lockObject)
            {
                try
                {
                    this.CommandsManager.StartQuiz(user, amount, answerSHA256, durationMinutes, question);

                    response = $"{user.Mention} started a quiz!" + Environment.NewLine +
                               $"Question is: `{question}`" + Environment.NewLine +
                               $"You have {durationMinutes} minutes to answer correctly and claim {amount} {this.Settings.Ticker}!" + Environment.NewLine +
                               $"If no one answers before time runs out {amount} {this.Settings.Ticker} will be returned to {user.Mention}.";

                    // Remove original message to hide hash.
                    this.MessagesHelper.SelfDestruct(this.Context.Message, 0);
                }
                catch (CommandExecutionException exception)
                {
                    response = "Error: " + exception.Message;
                }
            }

            response = this.TrimMessage(response);

            this.logger.Trace("(-)");
            await this.ReplyAsync(response).ConfigureAwait(false);
        }

        [CommandWithHelp("answerQuiz", "Answer to any active quiz. Answer will be checked against all of them. In case your answer will be correct you'll receive a reward.",
            "{tipbot} answerQuiz <answer>")]
        public async Task AnswerQuizAsync([Remainder] string answer)
        {
            this.logger.Trace("({0}:'{1}')", nameof(answer), answer);

            IUser user = this.Context.User;

            string response;
            bool deleteMessage = false;

            lock (this.lockObject)
            {
                try
                {
                    AnswerToQuizResponseModel result = this.CommandsManager.AnswerToQuiz(user, answer);

                    if (!result.Success)
                    {
                        response = "Unfortunately you are not correct, that's not the answer to any of the active quizes." +
                                    Environment.NewLine + Environment.NewLine +
                                   $"_`This and your message will be removed in {this.Settings.SelfDestructedMessagesDelaySeconds} seconds to avoid bloating the channel.`_";

                        this.MessagesHelper.SelfDestruct(this.Context.Message, this.Settings.SelfDestructedMessagesDelaySeconds);

                        deleteMessage = true;
                    }
                    else
                    {
                        response = $"{user.Mention} bingo!" + Environment.NewLine +
                                   $"Question was `{result.QuizQuestion}`" + Environment.NewLine +
                                   $"And the answer is `{answer}`" + Environment.NewLine +
                                   $"Your reward of {result.Reward} {this.Settings.Ticker} was deposited to your account!" + Environment.NewLine +
                                   $"<@!{result.QuizCreatorDiscordUserId}>, your quiz was solved!";
                    }
                }
                catch (CommandExecutionException exception)
                {
                    response = "Error: " + exception.Message;
                }
            }

            response = this.TrimMessage(response);

            if (deleteMessage)
            {
                await this.MessagesHelper.SendSelfDesctructedMessage(this.Context, response, false).ConfigureAwait(false);
            }
            else
            {
                await this.ReplyAsync(response).ConfigureAwait(false);
            }

            this.logger.Trace("(-)");
        }

        [CommandWithHelp("quizzes", "Displays all quizzes that are active.", "{tipbot} quizzes")]
        public Task ListActiveQuizes()
        {
            this.logger.Trace("()");

            lock (this.lockObject)
            {
                var builder = new StringBuilder();
                List<QuizViewModel> quizes = this.CommandsManager.GetActiveQuizes();

                if (quizes.Count != 0)
                {
                    builder.AppendLine("__List of all active quizes:__");
                    builder.AppendLine();

                    foreach (QuizViewModel quiz in quizes)
                    {
                        builder.AppendLine($"Question: `{quiz.Question}`");
                        builder.AppendLine($"Reward: **{quiz.Reward}** {this.Settings.Ticker}");
                        builder.AppendLine($"Created by: **{quiz.DiscordUserName}**");

                        var minutesLeft = (int)((quiz.CreationTime + TimeSpan.FromMinutes(quiz.DurationMinutes)) - DateTime.Now).TotalMinutes;
                        if (minutesLeft < 0)
                            minutesLeft = 0;

                        builder.AppendLine($"Expires in {minutesLeft} minutes.");
                        builder.AppendLine();
                    }
                }
                else
                {
                    builder.AppendLine("There are no active quizes.");
                    builder.AppendLine("Start a new one yourself using `startQuiz` command!");
                }

                string response = this.TrimMessage(builder.ToString());

                this.logger.Trace("(-)");
                return this.ReplyAsync(response);
            }
        }

        [CommandWithHelp("help", "Show the help instructions with command examples.", "{tipbot} help")]
        public Task HelpAsync()
        {
            this.logger.Trace("()");

            var helpAttributes = new List<CommandWithHelpAttribute>();

            foreach (MemberInfo memberInfo in this.GetType().GetMembers())
            {
                foreach (CommandWithHelpAttribute attribute in memberInfo.GetCustomAttributes(typeof(CommandWithHelpAttribute), true).ToList())
                    helpAttributes.Add(attribute);
            }

            var builder = new StringBuilder();

            builder.AppendLine("__List of bot commands:__");
            builder.AppendLine("parameters marked with * are optional");
            builder.AppendLine("");

            foreach (CommandWithHelpAttribute helpAttr in helpAttributes)
            {
                string helpStr = $"**{helpAttr.Text}**- {helpAttr.HelpInfo}";

                if (helpAttr.UsageExample != null)
                {
                    helpStr += Environment.NewLine + "`" + helpAttr.UsageExample.Replace("{tipbot}", Settings.BotOptionalPrefix) + "`";
                }

                builder.AppendLine(helpStr);
                builder.AppendLine();
            }

            string response = this.TrimMessage(builder.ToString());

            this.logger.Trace("(-)");
            return this.ReplyAsync(response);
        }

        [CommandWithHelp("about", "Displays information about the bot.", "{tipbot} about")]
        public Task AboutAsync()
        {
            this.logger.Trace("()");

            Stream stream = this.GetLogo();

            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            StringBuilder str = new StringBuilder();

            str.AppendLine("`TipBot`");
            str.AppendLine($"Chain: {ChainSettings.Name}");
            str.AppendLine($"Symbol: {ChainSettings.Symbol}");
            str.AppendLine($"About: {ChainSettings.Description}");
            str.AppendLine($"Web: <{ChainSettings.Url}>");
            str.AppendLine($"Version: {version}");
            str.AppendLine("GitHub: <https://github.com/block-core/blockcore-tipbot>");

            string text = str.ToString();

            this.logger.Trace("(-)");

            return this.Context.Channel.SendFileAsync(stream, "logo.png", text);
        }

        /// <summary>Trims the message to be shorter than 2000 characters.</summary>
        private string TrimMessage(string message)
        {
            this.logger.Trace("({0}:{1})", nameof(message), message.Length);

            var limit = 2000;

            if (message.Length < limit)
                return message;

            string trimmed = message.Substring(0, limit - 3) + "...";

            this.logger.Trace("()");
            return trimmed;
        }

        private Stream GetLogo()
        {
            this.logger.Trace("()");

            if (logo == null)
            {
                var client = new WebClient();
                logo = client.DownloadData(ChainSettings.Icon);
            }

            var stream = new MemoryStream(logo);

            this.logger.Trace("(-)");
            return stream;
        }
    }
}
