using System.Linq;
using Discord;
using TipBot.Database;
using TipBot.Logic;
using TipBot.Tests.Helpers;
using Xunit;

namespace TipBot.Tests.CommandsTests
{
    public class TipUserTests
    {
        private readonly TestContext testContext;

        private readonly IUser sender, receiver;

        public TipUserTests()
        {
            this.testContext = new TestContext();

            this.sender = this.testContext.SetupUser(1, "sender");
            this.receiver = this.testContext.SetupUser(2, "receiver");
        }

        [Fact]
        public void ThrowsIfAmountNegativeOrZero()
        {
            this.testContext.CreateDiscordUser(this.sender, 100);

            Assert.Throws<CommandExecutionException>(() => this.testContext.CommandsManager.TipUser(this.sender, this.receiver, -1));
            Assert.Throws<CommandExecutionException>(() => this.testContext.CommandsManager.TipUser(this.sender, this.receiver, 0));
        }

        [Fact]
        public void ThrowsIfUsersAreEqual()
        {
            this.testContext.CreateDiscordUser(this.sender, 100);

            Assert.Throws<CommandExecutionException>(() => this.testContext.CommandsManager.TipUser(this.sender, this.sender, 1));
        }

        [Fact]
        public void ThrowsIfBalanceIsInsufficient()
        {
            this.testContext.CreateDiscordUser(this.sender, 5);

            Assert.Throws<CommandExecutionException>(() => this.testContext.CommandsManager.TipUser(this.sender, this.receiver, 10));
        }

        [Fact]
        public void TipsCorrectly()
        {
            this.testContext.CreateDiscordUser(this.sender, 50);

            this.testContext.CommandsManager.TipUser(this.sender, this.receiver, 10);

            using (BotDbContext dbContext = this.testContext.CreateDbContext())
            {
                Assert.Equal(2, dbContext.Users.Count());

                Assert.Equal(40, dbContext.Users.Single(x => x.DiscordUserId == this.sender.Id).Balance);
                Assert.Equal(10, dbContext.Users.Single(x => x.DiscordUserId == this.receiver.Id).Balance);
            }
        }
    }
}
