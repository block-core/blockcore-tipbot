using Discord;
using Moq;
using TipBot.Database;
using TipBot.Logic;

namespace TipBot.Tests.Helpers
{
    public class TestContext
    {
        public readonly TestBot TestBot;

        public readonly CommandsManager CommandsManager;

        public TestContext()
        {
            this.TestBot = new TestBot();
            this.TestBot.StartAsync().GetAwaiter().GetResult();

            this.CommandsManager = this.TestBot.GetService<CommandsManager>();
        }

        public IUser SetupUser(ulong id, string username)
        {
            var userMock = new Mock<IUser>();
            userMock.Setup(x => x.Id).Returns(id);
            userMock.Setup(x => x.Username).Returns(username);

            return userMock.Object;
        }

        public BotDbContext CreateContext()
        {
            var factory = this.TestBot.GetService<IContextFactory>();

            return factory.CreateContext();
        }
    }
}
