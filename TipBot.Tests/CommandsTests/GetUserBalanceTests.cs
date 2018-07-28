using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Moq;
using TipBot.CommandModules;
using TipBot.Database;
using TipBot.Database.Models;
using TipBot.Logic;
using TipBot.Tests.Helpers;
using Xunit;

namespace TipBot.Tests.CommandsTests
{
    public class GetUserBalanceTests
    {
        private readonly TestContext testContext;

        public GetUserBalanceTests()
        {
            this.testContext = new TestContext();
        }

        [Fact]
        public void ReturnsZeroIfUserNotFound()
        {
            IUser user = this.testContext.SetupUser(1, "user");

            double balance = this.testContext.CommandsManager.GetUserBalance(user);
            Assert.Equal(0, balance);

            // Make sure new user was created.
            using (BotDbContext dbContext = this.testContext.CreateContext())
            {
                Assert.Equal(1, dbContext.Users.Count());

                DiscordUser discordUser = dbContext.Users.First();

                Assert.Equal(user.Id, discordUser.DiscordUserId);
                Assert.Equal(user.Username, discordUser.Username);
            }
        }
    }
}
