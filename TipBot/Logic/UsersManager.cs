using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
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

        public TipCommandResponse TipUser(IUser sender, IUser userBeingTipped, double amount, string message = null)
        {
            this.logger.Trace("({0}:'{1}',{2}:'{3}',{4}:{5},{6}:'{7}')", nameof(sender), sender.Id,
                nameof(userBeingTipped), userBeingTipped.Id, nameof(amount), amount, nameof(message), message);

            if (amount <= 0)
            {
                var errorResponse = new TipCommandResponse() { Success = false, ErrorMessage = "Amount can't be less than or equal to zero." };

                this.logger.Trace("(-)[INVALID_AMOUNT_VALUE]:'{0}'", errorResponse);
                return errorResponse;
            }

            if (sender.Id == userBeingTipped.Id)
            {
                var errorResponse = new TipCommandResponse() { Success = false, ErrorMessage = "You can't tip yourself!" };

                this.logger.Trace("(-)[SELFTIPPING]:'{0}'", errorResponse);
                return errorResponse;
            }

            DiscordUser discordUserSender = this.GetOrCreateUser(sender);

            if (discordUserSender.Balance < amount)
            {
                var errorResponse = new TipCommandResponse() { Success = false, ErrorMessage = "Insufficient funds." };

                this.logger.Trace("(-)[INVALID_AMOUNT_VALUE]:'{0}'", errorResponse);
                return errorResponse;
            }

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

        public void Dispose()
        {
            this.context.Dispose();
        }
    }

    public class TipCommandResponse
    {
        public bool Success;

        public string ErrorMessage;

        public override string ToString()
        {
            if (!this.Success)
                return "Error: " + this.ErrorMessage;

            // TODO
            throw new NotImplementedException();
        }
    }
}
