using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NBitcoin;
using TipBot.Database;
using TipBot.Database.Models;
using TipBot.Logic.NodeIntegrations.Models;

namespace TipBot.Logic.NodeIntegrations
{
    public class BlockCoreNodeIntegration : INodeIntegration
    {
        private const string AccountName = "account 0";

        private readonly IContextFactory contextFactory;
        private readonly Settings settings;
        private readonly Logger logger;

        private Task depositsCheckingTask;
        private readonly CancellationTokenSource cancellation;

        private readonly FatalErrorNotifier fatalNotifier;

        private readonly BlockCoreNodeAPI blockCoreNodeAPI;

        public BlockCoreNodeIntegration(Settings settings, IContextFactory contextFactory, FatalErrorNotifier fatalNotifier)
        {
            var apiUrl = settings.ConfigReader.GetOrDefault<string>("apiUrl", "http://127.0.0.1:48334/");
            var walletName = settings.ConfigReader.GetOrDefault<string>("walletName", "walletName");
            var walletPassword = settings.ConfigReader.GetOrDefault<string>("walletPassword", "walletPassword");
            var useSegwit = settings.ConfigReader.GetOrDefault<bool>("useSegwit", true);

            this.contextFactory = contextFactory;
            this.settings = settings;
            this.cancellation = new CancellationTokenSource();
            this.logger = LogManager.GetCurrentClassLogger();
            this.fatalNotifier = fatalNotifier;
            this.blockCoreNodeAPI = new BlockCoreNodeAPI(apiUrl, walletName, walletPassword, AccountName, this.settings.NetworkFee, useSegwit);
        }

        /// <inheritdoc />
        public void Initialize()
        {
            this.logger.Trace("()");

            using (BotDbContext context = this.contextFactory.CreateContext())
            {
                int addressesCount = context.UnusedAddresses.Count();

                // Generate addresses when running for the first time.
                if (addressesCount == 0)
                    this.PregenerateAddresses(context);
            }

            this.StartCheckingDepositsContinously();

            this.logger.Trace("(-)");
        }

        /// <inheritdoc />
        /// <exception cref="InvalidAddressException">Thrown if <paramref name="address"/> is invalid.</exception>
        public void Withdraw(decimal amount, string address)
        {
            this.logger.Trace("({0}:{1},{2}:'{3}')", nameof(amount), amount, nameof(address), address);

            ValidateAddressResult validationResult = blockCoreNodeAPI.ValidateAddress(address).Result;

            if (!validationResult.isvalid)
            {
                this.logger.Trace("(-)[INVALID_ADDRESS]");
                throw new InvalidAddressException();
            }

            try
            {
                blockCoreNodeAPI.SendTo(address, amount).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ex.Message == "No spendable transactions found")
                {
                    // This should never happen.
                    this.logger.Fatal(ex.Message);
                    this.fatalNotifier.NotifySupport(ex.Message);
                }

                this.logger.Error(ex.ToString);
                throw;
            }

            this.logger.Trace("(-)");
        }

        /// <summary>Populates database with unused addresses.</summary>
        private void PregenerateAddresses(BotDbContext context)
        {
            this.logger.Trace("()");
            this.logger.Info("Database was not prefilled with unused addresses. Starting.");

            this.logger.Info("Generating {0} addresses.", this.settings.PregeneratedAddressesCount);

            try
            {
                var unusedAddressResult = blockCoreNodeAPI.GetUnusedAddresses(settings.PregeneratedAddressesCount).Result;

                foreach (string address in unusedAddressResult)
                    context.UnusedAddresses.Add(new AddressModel() { Address = address });

                context.SaveChanges();
                this.logger.Info("Addresses generated.");
            }
            catch (Exception ex)
            {
                this.logger.Error(ex.ToString);
            }
            this.logger.Trace("(-)");
        }

        /// <summary>Starts process of continuously checking if a new deposit happened.</summary>
        private void StartCheckingDepositsContinously()
        {
            this.logger.Trace("()");

            uint lastCheckedBlock = 0;

            this.depositsCheckingTask = Task.Run(async () =>
            {
                try
                {
                    while (!this.cancellation.IsCancellationRequested)
                    {
                        try
                        {
                            var nodeStatus = blockCoreNodeAPI.GetNodeStatus().Result;
                            uint currentBlock = Convert.ToUInt32(nodeStatus.blockStoreHeight);

                            this.logger.Trace("Current block is {0}, last checked block is {1}", currentBlock, lastCheckedBlock);

                            if (currentBlock > lastCheckedBlock)
                            {
                                lastCheckedBlock = currentBlock;

                                using (BotDbContext context = this.contextFactory.CreateContext())
                                {
                                    this.CheckDeposits(context);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            this.logger.Error(ex.ToString);
                        }

                        await Task.Delay(30 * 1000, this.cancellation.Token).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                    this.logger.Trace("Operation canceled exception!");
                }
                catch (Exception exception)
                {
                    this.logger.Fatal(exception.ToString);
                    this.fatalNotifier.NotifySupport(exception.ToString());
                    throw;
                }
            });

            this.logger.Trace("(-)");
        }

        /// <summary>
        /// Checks if money were deposited to an address associated with any user who has a deposit address.
        /// When money are deposited user's balance is updated.
        /// </summary>
        private void CheckDeposits(BotDbContext context)
        {
            this.logger.Trace("()");

            List<DiscordUserModel> usersToTrack = context.Users.AsQueryable().Where(x => x.DepositAddress != null).ToList();
            this.logger.Trace("Tracking {0} users.", usersToTrack.Count);
            try
            {
                foreach (DiscordUserModel user in usersToTrack)
                {
                    var addressHistory = blockCoreNodeAPI.GetAddressesHistory(user.DepositAddress).Result;

                    var transactionHistory = addressHistory.history.Where(x => x.accountName == AccountName).SelectMany(x => x.transactionsHistory);

                    var receivedByAddress = transactionHistory.Where(x => x.type == "received");

                    if (receivedByAddress.Count() > 0)
                    {
                        decimal totalRecivedByAddress = receivedByAddress.Sum(x => x.amount);

                        decimal balance = Money.FromUnit(totalRecivedByAddress, MoneyUnit.Satoshi).ToUnit(MoneyUnit.BTC);
                        
                        if (balance > user.LastCheckedReceivedAmountByAddress)
                        {
                            decimal recentlyReceived = balance - user.LastCheckedReceivedAmountByAddress;

                            // Prevent users from spamming small amounts of coins.
                            // Also keep in mind if you'd like to change that- EF needs to be configured to track such changes.
                            // https://stackoverflow.com/questions/25891795/entityframework-not-detecting-changes-to-decimals-after-a-certain-precision
                            if (recentlyReceived < 0.01m)
                            {
                                this.logger.Trace("Skipping dust {0} for user {1}.", recentlyReceived, user);
                                continue;
                            }

                            this.logger.Debug("New value for received by address is {0}. Old was {1}. Address is {2}.", receivedByAddress, user.LastCheckedReceivedAmountByAddress, user.DepositAddress);

                            user.LastCheckedReceivedAmountByAddress = balance;
                            user.Balance += recentlyReceived;

                            context.Attach(user);
                            context.Entry(user).Property(x => x.Balance).IsModified = true;
                            context.Entry(user).Property(x => x.LastCheckedReceivedAmountByAddress).IsModified = true;
                            context.SaveChanges();

                            this.logger.Info("User '{0}' deposited {1}!. New balance is {2}.", user, recentlyReceived, user.Balance);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex.ToString);
            }

            this.logger.Trace("(-)");
        }

        public void Dispose()
        {
            this.logger.Trace("()");

            this.cancellation.Cancel();
            this.depositsCheckingTask?.GetAwaiter().GetResult();

            this.logger.Trace("(-)");
        }
    }
}
