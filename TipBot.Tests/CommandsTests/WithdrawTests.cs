using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using TipBot.Database;
using TipBot.Database.Models;
using TipBot.Logic;
using TipBot.Tests.Helpers;
using Xunit;

namespace TipBot.Tests.CommandsTests
{
    public class WithdrawTests
    {
        private readonly TestContext testContext;

        private readonly IUser caller;

        public WithdrawTests()
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
        }

        [Fact]
        public void AssertsAmountIsPosistive()
        {
            Assert.Throws<CommandExecutionException>(() => this.testContext.CommandsManager.Withdraw(this.caller, -1, "Addr"));
        }

        [Fact]
        public void AssertsAmountIsMoreThanMinimum()
        {
            Assert.Throws<CommandExecutionException>(() => this.testContext.CommandsManager.Withdraw(this.caller, this.testContext.Settings.MinWithdrawAmount/2, "Addr"));
        }

        [Fact]
        public void AssertsBalanceIsSufficient()
        {
            Assert.Throws<CommandExecutionException>(() => this.testContext.CommandsManager.Withdraw(this.caller, 15, "Addr"));
        }

        [Fact]
        public void WithdrawsSuccessfully()
        {
            this.testContext.CommandsManager.Withdraw(this.caller, 0.5m, "Addr");

            using (BotDbContext dbContext = this.testContext.CreateDbContext())
            {
                Assert.True(dbContext.Users.First().Balance == 9.5m);
            }
        }
    }
}
