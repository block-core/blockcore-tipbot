using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TipBot.Database.Models;
using TipBot.Logic;
using TipBot.Services;

namespace TipBot.CommandModules
{
    public class PublicCommands : ModuleBase<SocketCommandContext>
    {
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
                catch (OutOfDepositAddressesException)
                {
                    return this.ReplyAsync("Bot ran out of deposit addresses. Tell bot admin about it.");
                }
            }

            string response = $"Your unique deposit address is `{depositAddress}`";
            response += Environment.NewLine + $"Money are deposited after {this.Settings.MinConfirmationsForDeposit} confirmations.";

            return this.ReplyAsync(response);
        }

        [CommandWithHelp("withdraw", "Withdraws given amount to specified address. Fee will be subtracted from given amount.", "withdraw <address> <amount>")]
        public Task WithdrawAsync(decimal amount, string address)
        {
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

            return this.ReplyAsync(response);
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

        [CommandWithHelp("startQuiz", "You ask a question, supply hash of an answer and for how long the quiz will be running." +
                                       " First user to provide correct answer gets the prize! In case no one answers money will return back to you after quiz expiry." +
                                       " For hash generation use <https://passwordsgenerator.net/sha256-hash-generator/>",
                                        "createQuiz <amount> <SHA256 of an answer> <duration in minures> <question>")]
        public Task StartQuizAsync(decimal amount, string answerSHA256, int durationMinutes, [Remainder]string question)
        {
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
                }
                catch (CommandExecutionException exception)
                {
                    response = "Error: " + exception.Message;
                }
            }

            return this.ReplyAsync(response);
        }

        [CommandWithHelp("answerQuiz", "Answer to any active quiz. Answer will be checked against all of them. In case your answer will be correct you'll receive a reward.",
            "answerQuiz <answer>")]
        public Task AnswerQuizAsync([Remainder]string answer)
        {
            IUser user = this.Context.User;

            string response;

            lock (this.lockObject)
            {
                try
                {
                    AnswerToQuizResponseModel result = this.CommandsManager.AnswerToQuiz(user, answer);

                    if (!result.Success)
                    {
                        response = "Unfortunately you are not correct, that's not the answer to any of the active quizes.";
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

            return this.ReplyAsync(response);
        }

        [CommandWithHelp("listActiveQuizes", "Displays all quizes that are active.")]
        public Task ListActiveQuizes()
        {
            lock (this.lockObject)
            {
                var response = new StringBuilder();
                List<QuizModel> quizes = this.CommandsManager.GetActiveQuizes();

                if (quizes.Count != 0)
                {
                    response.AppendLine("__List of all active quizes:__");
                    response.AppendLine();

                    foreach (QuizModel quiz in quizes)
                    {
                        response.AppendLine($"Question: `{quiz.Question}`");
                        response.AppendLine($"Reward: {quiz.Reward} {this.Settings.Ticker}");

                        var minutesLeft = (int) ((quiz.CreationTime + TimeSpan.FromMinutes(quiz.DurationMinutes)) - DateTime.Now).TotalMinutes;
                        if (minutesLeft < 0)
                            minutesLeft = 0;

                        response.AppendLine($"Expires in {minutesLeft} minutes.");
                        response.AppendLine();
                    }
                }
                else
                {
                    response.AppendLine("There are no active quizes.");
                    response.AppendLine("Start a new one yourself using `startQuiz` command!");
                }

                return this.ReplyAsync(response.ToString());
            }
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
                builder.AppendLine();
            }

            builder.AppendLine("");
            builder.AppendLine("parameters marked with * are optional");

            return this.ReplyAsync(builder.ToString());
        }

        [CommandWithHelp("makeItRain", "Randomly selects online users from the current server and tips them 1 coin.", "makeItRain <amount>")]
        public async Task MakeItRainAsync(decimal amount)
        {
            IUser caller = this.Context.User;

            var onlineUsers = new List<IUser>();
            IAsyncEnumerable<IReadOnlyCollection<IUser>> usersCollection = this.Context.Channel.GetUsersAsync();

            await usersCollection.ForEachAsync(delegate(IReadOnlyCollection<IUser> users)
            {
                onlineUsers.AddRange(users.Where(x => x.Status != UserStatus.Offline));
            });

            string response = null;

            //TODO maybe weight by activity?

            lock (this.lockObject)
            {
                try
                {
                    List<DiscordUserModel> usersBeingTipped = this.CommandsManager.RandomlyTipUsers(caller, onlineUsers, amount);

                    var builder = new StringBuilder();

                    var tadaEmoji = ":tada:";

                    builder.AppendLine($"{tadaEmoji}{caller.Mention} just tipped {usersBeingTipped.Count} users 1 {this.Settings.Ticker} each!{tadaEmoji}");
                    builder.AppendLine();

                    foreach (DiscordUserModel tippedUser in usersBeingTipped)
                    {
                        builder.Append($"<@!{tippedUser.DiscordUserId}>");

                        if (tippedUser != usersBeingTipped.Last())
                            builder.Append(", ");
                    }

                    builder.Append($" - you all received 1 {this.Settings.Ticker}!");

                    response = builder.ToString();

                    // TODO add hoorray smiles and maybe image
                }
                catch (CommandExecutionException exception)
                {
                    response = "Error: " + exception.Message;
                }
            }

            await this.ReplyAsync(response);
        }

        [CommandWithHelp("about", "Displays information about the bot.")]
        public async Task AboutAsync()
        {
            Stream stream = this.GetLogo();

            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            string text = "`TipBot`" + Environment.NewLine +
                $"Version: {version}" + Environment.NewLine + "Github: <https://github.com/noescape00/DiscordTipBot>";
            await this.Context.Channel.SendFileAsync(stream, "logo.png", text);
        }

        private Stream GetLogo()
        {
            Assembly assembly = this.GetType().GetTypeInfo().Assembly;
            Stream stream = assembly.GetManifestResourceStream("TipBot.Content.Logo.png");

            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }
    }
}
