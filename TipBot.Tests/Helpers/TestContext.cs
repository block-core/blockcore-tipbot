using Discord;
using Moq;
using TipBot.Database;
using TipBot.Database.Models;
using TipBot.Logic;

namespace TipBot.Tests.Helpers
{
    public class TestContext
    {
        private readonly TestBot testBot;

        public readonly CommandsManager CommandsManager;

        public readonly Settings Settings;

        public TestContext()
        {
            this.testBot = new TestBot();
            this.testBot.StartAsync(new string[]{  }).GetAwaiter().GetResult();

            this.CommandsManager = this.testBot.GetService<CommandsManager>();
            this.Settings = this.testBot.GetService<Settings>();
        }

        public IUser SetupUser(ulong id, string username)
        {
            var userMock = new Mock<IUser>();
            userMock.Setup(x => x.Id).Returns(id);
            userMock.Setup(x => x.Username).Returns(username);

            return userMock.Object;
        }

        public void CreateDiscordUser(IUser user, decimal amount)
        {
            using (BotDbContext dbContext = this.CreateDbContext())
            {
                dbContext.Users.Add(new DiscordUserModel()
                {
                    DiscordUserId = user.Id,
                    Username = user.Username,
                    Balance = amount
                });

                dbContext.SaveChanges();
            }
        }

        public BotDbContext CreateDbContext()
        {
            var factory = this.testBot.GetService<IContextFactory>();

            return factory.CreateContext();
        }
    }
}
