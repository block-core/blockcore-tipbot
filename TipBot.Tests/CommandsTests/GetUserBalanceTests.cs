using System.Linq;
using Discord;
using TipBot.Database;
using TipBot.Database.Models;
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

            decimal balance = this.testContext.CommandsManager.GetUserBalance(user);
            Assert.Equal(0, balance);

            // Make sure new user was created.
            using (BotDbContext dbContext = this.testContext.CreateContext())
            {
                Assert.Equal(1, dbContext.Users.Count());

                DiscordUserModel discordUser = dbContext.Users.First();

                Assert.Equal(user.Id, discordUser.DiscordUserId);
                Assert.Equal(user.Username, discordUser.Username);
            }
        }

        [Fact]
        public void ReturnsBalance()
        {
            IUser user = this.testContext.SetupUser(1, "user");

            this.testContext.CreateDiscordUser(user, 100);

            decimal balance = this.testContext.CommandsManager.GetUserBalance(user);

            Assert.Equal(100, balance);
        }
    }
}
