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
    public class TopTippersChartTestscs
    {
        private readonly TestContext testContext;

        private readonly IUser caller;

        private readonly List<IUser> onlineUsers;

        public TopTippersChartTestscs()
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
        public void ThrowsIfPeriodTooLong()
        {
            Assert.Throws<CommandExecutionException>(() => this.testContext.CommandsManager.GetTopTippers(this.testContext.Settings.MaxDaysChartCount * 2, 3));
        }

        [Fact]
        public void ThrowsIfPeriodLassThanOne()
        {
            Assert.Throws<CommandExecutionException>(() => this.testContext.CommandsManager.GetTopTippers(0, 3));
        }

        [Fact]
        public void ReturnsChart()
        {
            this.testContext.CommandsManager.TipUser(this.caller, this.onlineUsers[0], 1);
            this.testContext.CommandsManager.TipUser(this.caller, this.onlineUsers[1], 1);
            this.testContext.CommandsManager.TipUser(this.caller, this.onlineUsers[2], 1);

            TippingChartsModel chart = this.testContext.CommandsManager.GetTopTippers(1, 3);

            Assert.Single(chart.BestTippers);
            Assert.Equal(3, chart.BestTippers.First().Amount);

            Assert.Equal(3, chart.BestBeingTipped.Count);

            foreach (var tipped in chart.BestBeingTipped)
                Assert.Equal(1, tipped.Amount);
        }
    }
}
