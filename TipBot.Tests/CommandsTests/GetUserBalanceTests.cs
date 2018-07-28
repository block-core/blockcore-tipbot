using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Moq;
using TipBot.CommandModules;
using TipBot.Logic;
using TipBot.Tests.Helpers;
using Xunit;

namespace TipBot.Tests.CommandsTests
{
    public class GetUserBalanceTests
    {
        [Fact]
        public async Task ReturnsZeroIfUserNotFoundAsync()
        {
            // TODO move most of this test to a setup class
            var testBot = new TestBot();
            await testBot.StartAsync();

            var commandsManager = testBot.GetService<CommandsManager>();

            var userMock = new Mock<IUser>();
            userMock.Setup(x => x.Id).Returns(1);

            double balance = commandsManager.GetUserBalance(userMock.Object);

            Assert.Equal(0, balance);
        }
    }
}
