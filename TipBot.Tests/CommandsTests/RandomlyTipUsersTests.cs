using System.Collections.Generic;
using System.Linq;
using Discord;
using TipBot.Database;
using TipBot.Database.Models;
using TipBot.Logic;
using TipBot.Tests.Helpers;
using Xunit;

namespace TipBot.Tests.CommandsTests
{
    public class RandomlyTipUsersTests
    {
        private readonly TestContext testContext;

        private readonly IUser caller;

        private readonly List<IUser> onlineUsers;

        public RandomlyTipUsersTests()
        {
            this.testContext = new TestContext();

            this.caller = this.testContext.SetupUser(1, "caller");

            // That will create a user in db.
            this.testContext.CommandsManager.GetUserBalance(this.caller);

            using (BotDbContext dbContext = this.testContext.CreateDbContext())
            {
                DiscordUserModel user = dbContext.Users.First();
                user.Balance = 10;
                dbContext.Update(user);
                dbContext.SaveChanges();
            }

            this.onlineUsers = new List<IUser>();
            for (var i = 0; i < 10; i++)
            {
                IUser user = this.testContext.SetupUser((ulong)(i + 2), i.ToString());
                this.onlineUsers.Add(user);
            }
        }

        [Fact]
        public void AssertsAmountPositive()
        {
            Assert.Throws<CommandExecutionException>(() => this.testContext.CommandsManager.RandomlyTipUsers(this.caller, this.onlineUsers, -1, 1));
        }

        [Fact]
        public void ThrowsIfAmountIsLessThanMinimum()
        {
            Assert.Throws<CommandExecutionException>(() =>
                this.testContext.CommandsManager.RandomlyTipUsers(this.caller, this.onlineUsers, this.testContext.Settings.MinMakeItRainTipAmount/2, 1));
        }

        [Fact]
        public void ThrowsIfTotalAmountIsLessThanTipAmount()
        {
            Assert.Throws<CommandExecutionException>(() => this.testContext.CommandsManager.RandomlyTipUsers(this.caller, this.onlineUsers, 2, 3));
        }

        [Fact]
        public void ThrowsIfThereAreNoUsers()
        {
            Assert.Throws<CommandExecutionException>(() => this.testContext.CommandsManager.RandomlyTipUsers(this.caller, new List<IUser>(), 2, 1));
        }

        [Fact]
        public void TipsSuccessfully()
        {
            this.testContext.CommandsManager.RandomlyTipUsers(this.caller, this.onlineUsers, 10, 1);

            Assert.Equal(0, this.testContext.CommandsManager.GetUserBalance(this.caller));

            foreach (IUser user in this.onlineUsers)
            {
                Assert.Equal(1, this.testContext.CommandsManager.GetUserBalance(user));
            }
        }

        [Fact]
        public void TipsSuccessfully2()
        {
            this.testContext.CommandsManager.RandomlyTipUsers(this.caller, this.onlineUsers, 20, 0.5m);

            Assert.Equal(5, this.testContext.CommandsManager.GetUserBalance(this.caller));

            foreach (IUser user in this.onlineUsers)
            {
                Assert.Equal(0.5m, this.testContext.CommandsManager.GetUserBalance(user));
            }
        }
    }
}
