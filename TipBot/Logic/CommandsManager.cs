using System;
using System.Linq;
using Discord;
using NLog;
using TipBot.Database;
using TipBot.Database.Models;

namespace TipBot.Logic
{
    /// <summary>Implements logic behind all commands that can be invoked by bot's users.</summary>
    /// <remarks>This class is not thread safe.</remarks>
    public class CommandsManager : IDisposable
    {
        private readonly BotDbContext context;

        private readonly Logger logger;

        public CommandsManager(IContextFactory contextFactory)
        {
            this.context = contextFactory.CreateContext();
            this.logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>Transfers <paramref name="amount"/> of money from <paramref name="sender"/> to <paramref name="userBeingTipped"/>.</summary>
        /// <exception cref="CommandExecutionException">Thrown when user supplied invalid input data.</exception>
        public void TipUser(IUser sender, IUser userBeingTipped, double amount)
        {
            this.logger.Trace("({0}:'{1}',{2}:'{3}',{4}:{5})", nameof(sender), sender.Id, nameof(userBeingTipped), userBeingTipped.Id, nameof(amount), amount);

            this.AssertAmountPositive(amount);
            this.AssertUsersNotEqual(sender, userBeingTipped);

            DiscordUser discordUserSender = this.GetOrCreateUser(sender);

            this.AssertBalanceIsSufficient(discordUserSender, amount);

            DiscordUser discordUserReceiver = this.GetOrCreateUser(userBeingTipped);

            discordUserSender.Balance -= amount;
            discordUserReceiver.Balance += amount;

            this.context.SaveChanges();

            this.logger.Trace("(-)");
        }

        public double GetUserBalance(IUser user)
        {
            this.logger.Trace("({0}:{1})", nameof(user), user.Id);

            DiscordUser discordUser = this.GetOrCreateUser(user);

            double balance = discordUser.Balance;

            this.logger.Trace("(-):{0}", balance);
            return balance;
        }

        private DiscordUser GetOrCreateUser(IUser user)
        {
            this.logger.Trace("({0}:{1})", nameof(user), user.Id);

            DiscordUser discordUser = this.context.Users.SingleOrDefault(x => x.DiscordUserId == user.Id);

            if (discordUser == null)
                discordUser = this.CreateUser(user);

            this.logger.Trace("(-):'{0}'", discordUser);
            return discordUser;
        }

        private DiscordUser CreateUser(IUser user)
        {
            this.logger.Trace("({0}:{1})", nameof(user), user.Id);
            this.logger.Debug("Creating a new user with id {0} and username '{1}'.", user.Id, user.Username);

            var discordUser = new DiscordUser()
            {
                Balance = 0,
                DiscordUserId = user.Id,
                Username = user.Username
            };

            this.context.Users.Add(discordUser);
            this.context.SaveChanges();

            this.logger.Trace("(-):'{0}'", discordUser);
            return discordUser;
        }

        private bool UserExists(ulong discordUserId)
        {
            this.logger.Trace("({0}:{1})", nameof(discordUserId), discordUserId);

            bool userExists = this.context.Users.Any(x => x.DiscordUserId == discordUserId);

            this.logger.Trace("(-):{0}", userExists);
            return userExists;
        }

        private void AssertBalanceIsSufficient(DiscordUser user, double balanceRequired)
        {
            this.logger.Trace("({0}:'{1}',{2}:{3})", nameof(user), user, nameof(balanceRequired), balanceRequired);

            if (user.Balance < balanceRequired)
            {
                this.logger.Trace("(-)[INVALID_AMOUNT_VALUE]");
                throw new CommandExecutionException("Insufficient funds.");
            }
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

        private void AssertAmountPositive(double amount)
        {
            this.logger.Trace("({0}:{1})", nameof(amount), amount);

            if (amount <= 0)
            {
                this.logger.Trace("(-)[AMOUNT_NOT_POSITIVE]'");
                throw new CommandExecutionException("Amount can't be less than or equal to zero.");
            }

            this.logger.Trace("(-)");
        }

        public void Dispose()
        {
            this.logger.Trace("()");

            this.context.Dispose();

            this.logger.Trace("(-)");
        }
    }

    public class CommandExecutionException : Exception
    {
        public CommandExecutionException(string message) : base(message)
        {
        }
    }
}
