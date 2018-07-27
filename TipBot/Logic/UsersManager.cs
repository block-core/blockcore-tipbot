using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore.Internal;
using NLog;
using TipBot.Database;
using TipBot.Database.Models;

namespace TipBot.Logic
{
    /// <summary>
    /// TODO
    /// </summary>
    /// <remarks>This class is not thread safe.</remarks>
    public class UsersManager : IDisposable
    {
        private BotDbContext context;

        private readonly Logger logger;

        public UsersManager()
        {
            this.context = new BotDbContext();
            this.logger = LogManager.GetCurrentClassLogger();
        }

        public void TipUser(IUser sender, IUser userBeingTipped, double amount, string message = null)
        {
            this.logger.Trace("({0}:'{1}',{2}:'{3}',{4}:{5},{6}:'{7}')", nameof(sender), sender.Id,
                nameof(userBeingTipped), userBeingTipped.Id, nameof(amount), amount, nameof(message), message);

            this.AssertAmountPositive(amount);
            this.AssertUsersNotEqual(sender, userBeingTipped);

            DiscordUser discordUserSender = this.GetOrCreateUser(sender);

            this.AssertBalanceIsSufficient(discordUserSender, amount);

            // TODO
            throw new NotImplementedException();
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
            this.context.Dispose();
        }
    }

    public class CommandExecutionException : Exception
    {
        public CommandExecutionException(string message) : base(message)
        {
        }
    }
}
