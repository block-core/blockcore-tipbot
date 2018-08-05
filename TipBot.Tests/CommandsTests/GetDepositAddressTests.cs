using System.Linq;
using Discord;
using TipBot.Database;
using TipBot.Database.Models;
using TipBot.Logic;
using TipBot.Tests.Helpers;
using Xunit;

namespace TipBot.Tests.CommandsTests
{
    public class GetDepositAddressTests
    {
        private readonly TestContext testContext;

        private readonly IUser caller;

        public GetDepositAddressTests()
        {
            this.testContext = new TestContext();

            this.caller = this.testContext.SetupUser(1, "caller");
        }

        [Fact]
        public void ReceivesFirstAddress()
        {
            // Make sure new user was created.
            using (BotDbContext dbContext = this.testContext.CreateDbContext())
            {
                dbContext.UnusedAddresses.AddRange(new AddressModel[]
                {
                    new AddressModel() {Address = "1"},
                    new AddressModel() {Address = "2"},
                    new AddressModel() {Address = "3"}
                });

                dbContext.SaveChanges();
            }

            string address = this.testContext.CommandsManager.GetDepositAddress(this.caller);

            Assert.Equal("1", address);

            // Calling it 2nd time to make sure same address is given.
            string address2 = this.testContext.CommandsManager.GetDepositAddress(this.caller);

            Assert.Equal("1", address2);

            using (BotDbContext dbContext = this.testContext.CreateDbContext())
            {
                Assert.Equal(2, dbContext.UnusedAddresses.Count());
            }
        }

        [Fact]
        public void ThrowsWhenThereAreNoAddresses()
        {
            Assert.Throws<OutOfDepositAddressesException>(() => this.testContext.CommandsManager.GetDepositAddress(this.caller));
        }
    }
}
