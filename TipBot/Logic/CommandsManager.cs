using System;
using System.Linq;
using Discord;
using Discord.Commands;
using NLog;
using TipBot.Database;
using TipBot.Database.Models;

namespace TipBot.Logic
{
    /// <summary>Implements logic behind all commands that can be invoked by bot's users.</summary>
    /// <remarks>This class is not thread safe.</remarks>
    public class CommandsManager
    {
        private readonly IContextFactory contextFactory;

        private readonly RPCIntegration rpc;

        private readonly Settings settings;

        private readonly Logger logger;

        public CommandsManager(IContextFactory contextFactory, RPCIntegration rpc, Settings settings)
        {
            this.contextFactory = contextFactory;
            this.rpc = rpc;
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
                    throw new CommandExecutionException($"Minimal withdraw amount is {this.settings.MinWithdrawAmount}.");
                }

                try
                {
                    this.rpc.Withdraw(amount, address);
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
