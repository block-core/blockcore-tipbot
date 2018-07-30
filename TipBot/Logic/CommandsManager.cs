using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using NLog;
using TipBot.Database;
using TipBot.Database.Models;
using TipBot.Logic.NodeIntegrations;

namespace TipBot.Logic
{
    /// <summary>Implements logic behind all commands that can be invoked by bot's users.</summary>
    /// <remarks>This class is not thread safe.</remarks>
    public class CommandsManager
    {
        private readonly IContextFactory contextFactory;

        private readonly INodeIntegration nodeIntegration;

        private readonly Settings settings;

        private readonly Logger logger;

        public CommandsManager(IContextFactory contextFactory, INodeIntegration nodeIntegration, Settings settings)
        {
            this.contextFactory = contextFactory;
            this.nodeIntegration = nodeIntegration;
            this.settings = settings;

            this.logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>Transfers <paramref name="amount"/> of money from <paramref name="sender"/> to <paramref name="userBeingTipped"/>.</summary>
        /// <exception cref="CommandExecutionException">Thrown when user supplied invalid input data.</exception>
        public void TipUser(IUser sender, IUser userBeingTipped, decimal amount)
        {
            this.logger.Trace("({0}:{1},{2}:'{3}',{4}:{5})", nameof(sender), sender.Id, nameof(userBeingTipped), userBeingTipped.Id, nameof(amount), amount);

            this.AssertAmountPositive(amount);
            this.AssertUsersNotEqual(sender, userBeingTipped);

            using (BotDbContext context = this.contextFactory.CreateContext())
            {
                DiscordUser discordUserSender = this.GetOrCreateUser(context, sender);

                this.AssertBalanceIsSufficient(discordUserSender, amount);

                DiscordUser discordUserReceiver = this.GetOrCreateUser(context, userBeingTipped);

                discordUserSender.Balance -= amount;
                discordUserReceiver.Balance += amount;

                context.Update(discordUserReceiver);
                context.SaveChanges();
            }

            this.logger.Trace("(-)");
        }

        /// <summary>Gets deposit address for a user.</summary>
        /// <exception cref="OutOfDepositAddressesException">Thrown when bot ran out of unused deposit addresses.</exception>
        public string GetDepositAddress(IUser user)
        {
            this.logger.Trace("({0}:{1})", nameof(user), user.Id);

            using (BotDbContext context = this.contextFactory.CreateContext())
            {
                DiscordUser discordUser = this.GetOrCreateUser(context, user);

                string depositAddress = discordUser.DepositAddress;

                // Assign deposit address if it wasn't assigned it.
                if (depositAddress == null)
                {
                    this.logger.Trace("Assigning deposit address for '{0}'.", discordUser);

                    AddressModel unusedAddress = context.UnusedAddresses.FirstOrDefault();

                    if (unusedAddress == null)
                    {
                        this.logger.Fatal("Bot ran out of deposit addresses!");
                        this.logger.Trace("(-)[NO_ADDRESSES]");
                        throw new OutOfDepositAddressesException();
                    }

                    context.UnusedAddresses.Remove(unusedAddress);

                    depositAddress = unusedAddress.Address;
                    discordUser.DepositAddress = depositAddress;
                    context.Update(discordUser);
                    context.SaveChanges();
                }

                this.logger.Trace("(-):'{0}'", depositAddress);
                return depositAddress;
            }
        }

        /// <summary>Withdraws given amount of money to specified address.</summary>
        /// <exception cref="CommandExecutionException">Thrown when user supplied invalid input data.</exception>
        public void Withdraw(IUser user, decimal amount, string address)
        {
            this.logger.Trace("({0}:{1},{2}:{3},{4}:'{5}')", nameof(user), user.Id, nameof(amount), amount, nameof(address), address);

            this.AssertAmountPositive(amount);

            using (BotDbContext context = this.contextFactory.CreateContext())
            {
                DiscordUser discordUser = this.GetOrCreateUser(context, user);

                this.AssertBalanceIsSufficient(discordUser, amount);

                if (amount < this.settings.MinWithdrawAmount)
                {
                    this.logger.Trace("(-)[MIN_WITHDRAW_AMOUNT]");
                    throw new CommandExecutionException($"Minimal withdraw amount is {this.settings.MinWithdrawAmount} {this.settings.Ticker}.");
                }

                try
                {
                    this.nodeIntegration.Withdraw(amount, address);
                }
                catch (InvalidAddressException)
                {
                    this.logger.Trace("(-)[INVALID_ADDRESS]");
                    throw new CommandExecutionException("Address specified is invalid.");
                }

                discordUser.Balance -= amount;

                context.Update(discordUser);
                context.SaveChanges();
            }

            this.logger.Trace("(-)");
        }

        public decimal GetUserBalance(IUser user)
        {
            this.logger.Trace("({0}:{1})", nameof(user), user.Id);

            using (BotDbContext context = this.contextFactory.CreateContext())
            {
                DiscordUser discordUser = this.GetOrCreateUser(context, user);

                decimal balance = discordUser.Balance;

                this.logger.Trace("(-):{0}", balance);
                return balance;
            }
        }

        /// <exception cref="CommandExecutionException">Thrown when user supplied invalid input data.</exception>
        public void StartQuiz(IUser user, decimal amount, string answerSHA256, int durationMinutes, string question)
        {
            this.logger.Trace("({0}:{1},{2}:{3},{4}:'{5}',{6}:{7},{8}:'{9}')", nameof(user), user.Id, nameof(amount), amount, nameof(answerSHA256), answerSHA256,
                nameof(durationMinutes), durationMinutes, nameof(question), question);

            this.AssertAmountPositive(amount);

            if (answerSHA256.Length != 64)
            {
                this.logger.Trace("(-)[INCORRECT_HASH]'");
                throw new CommandExecutionException("SHA256 hash should contain 64 characters!");
            }

            if (durationMinutes < 1)
            {
                this.logger.Trace("(-)[INCORRECT_DURATION]'");
                throw new CommandExecutionException("Duration in minutes can't be less than 1!");
            }

            var maxQuestionLenght = 1024;
            if (question.Length > maxQuestionLenght)
            {
                this.logger.Trace("(-)[QUESTION_TOO_LONG]'");
                throw new CommandExecutionException($"Questions longer than {maxQuestionLenght} characters are not allowed!");
            }

            using (BotDbContext context = this.contextFactory.CreateContext())
            {
                if (context.ActiveQuizes.Any(x => x.AnswerHash == answerSHA256))
                {
                    this.logger.Trace("(-)[HASH_ALREADY_EXISTS]");
                    throw new CommandExecutionException("There is already a quiz with that answer hash!");
                }

                DiscordUser discordUser = this.GetOrCreateUser(context, user);

                this.AssertBalanceIsSufficient(discordUser, amount);

                if (amount < this.settings.MinQuizAmount)
                {
                    this.logger.Trace("(-)[AMOUNT_TOO_LOW]");
                    throw new CommandExecutionException($"Minimal quiz reward is {this.settings.MinQuizAmount} {this.settings.Ticker}!");
                }

                var quiz = new QuizModel()
                {
                    AnswerHash = answerSHA256,
                    CreationTime = DateTime.Now,
                    CreatorDiscordUserId = discordUser.DiscordUserId,
                    DurationMinutes = durationMinutes,
                    Question = question,
                    Reward = amount
                };

                discordUser.Balance -= amount;
                context.Update(discordUser);

                context.ActiveQuizes.Add(quiz);

                context.SaveChanges();

                this.logger.Info("Quiz with reward {0} and answer hash '{1}' was created!", amount, answerSHA256);
            }

            this.logger.Trace("(-)");
        }

        public List<QuizModel> GetActiveQuizes()
        {
            this.logger.Trace("()");

            using (BotDbContext context = this.contextFactory.CreateContext())
            {
                List<QuizModel> quizes = context.ActiveQuizes.ToList();

                this.logger.Trace("(-):{0}", quizes.Count);
                return quizes;
            }
        }

        private DiscordUser GetOrCreateUser(BotDbContext context, IUser user)
        {
            this.logger.Trace("({0}:{1})", nameof(user), user.Id);

            DiscordUser discordUser = context.Users.SingleOrDefault(x => x.DiscordUserId == user.Id);

            if (discordUser == null)
                discordUser = this.CreateUser(context, user);

            this.logger.Trace("(-):'{0}'", discordUser);
            return discordUser;
        }

        private DiscordUser CreateUser(BotDbContext context, IUser user)
        {
            this.logger.Trace("({0}:{1})", nameof(user), user.Id);
            this.logger.Debug("Creating a new user with id {0} and username '{1}'.", user.Id, user.Username);

            var discordUser = new DiscordUser()
            {
                Balance = 0,
                DiscordUserId = user.Id,
                Username = user.Username
            };

            context.Users.Add(discordUser);
            context.SaveChanges();

            this.logger.Trace("(-):'{0}'", discordUser);
            return discordUser;
        }

        private bool UserExists(BotDbContext context, ulong discordUserId)
        {
            this.logger.Trace("({0}:{1})", nameof(discordUserId), discordUserId);

            bool userExists = context.Users.Any(x => x.DiscordUserId == discordUserId);

            this.logger.Trace("(-):{0}", userExists);
            return userExists;
        }

        private void AssertBalanceIsSufficient(DiscordUser user, decimal balanceRequired)
        {
            this.logger.Trace("({0}:'{1}',{2}:{3})", nameof(user), user, nameof(balanceRequired), balanceRequired);

            if (user.Balance < balanceRequired)
            {
                this.logger.Trace("(-)[INVALID_AMOUNT_VALUE]");
                throw new CommandExecutionException("Insufficient funds.");
            }

            this.logger.Trace("(-)");
        }

        private void AssertUsersNotEqual(IUser user1, IUser user2)
        {
            this.logger.Trace("({0}:{1},{2}:{3})", nameof(user1), user1.Id, nameof(user2), user2.Id);

            if (user1.Id == user2.Id)
            {
                this.logger.Trace("(-)[SAME_USERS]'");
                throw new CommandExecutionException("You can't tip yourself!");
            }

            this.logger.Trace("(-)");
        }

        private void AssertAmountPositive(decimal amount)
        {
            this.logger.Trace("({0}:{1})", nameof(amount), amount);

            if (amount <= 0)
            {
                this.logger.Trace("(-)[AMOUNT_NOT_POSITIVE]'");
                throw new CommandExecutionException("Amount can't be less than or equal to zero.");
            }

            this.logger.Trace("(-)");
        }
    }

    public class CommandExecutionException : Exception
    {
        public CommandExecutionException(string message) : base(message) { }
    }

    public class OutOfDepositAddressesException : Exception
    {
        public OutOfDepositAddressesException() : base("Bot ran out of deposit addresses.") { }
    }
}
