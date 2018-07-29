using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BitcoinLib.ExceptionHandling.Rpc;
using BitcoinLib.Services.Coins.Base;
using BitcoinLib.Services.Coins.Bitcoin;
using NLog;
using TipBot.Database;
using TipBot.Database.Models;

namespace TipBot.Logic
{
    public class RPCIntegration : IDisposable
    {
        private readonly ICoinService coinService;

        private readonly IContextFactory contextFactory;

        private readonly Settings settings;

        private const string AccountName = "account 0";

        private readonly Logger logger;

        private Task depositsCheckingTask;

        private readonly CancellationTokenSource cancellation;

        public RPCIntegration(Settings settings, IContextFactory contextFactory)
        {
            this.coinService = new BitcoinService(settings.DaemonUrl, settings.RpcUsername, settings.RpcPassword, settings.WalletPassword, settings.RpcRequestTimeoutInSeconds);
            this.contextFactory = contextFactory;
            this.settings = settings;
            this.cancellation = new CancellationTokenSource();
            this.logger = LogManager.GetCurrentClassLogger();
        }

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

        /// <summary>Populates database with unused addresses.</summary>
        private void PregenerateAddresses(BotDbContext context)
        {
            this.logger.Trace("()");
            this.logger.Info("Database was not prefilled with addresses. Starting.");

            this.coinService.WalletPassphrase(this.settings.WalletPassword, int.MaxValue);

            int alreadyGenerated = this.coinService.GetAddressesByAccount(AccountName).Count;

            this.logger.Info("{0} addresses are already in the wallet.", alreadyGenerated);

            long toGenerateCount = this.settings.PregeneratedAddressesCount - alreadyGenerated;

            this.logger.Info("Generating {0} addresses.", toGenerateCount);

            for (long i = 0; i < toGenerateCount; ++i)
            {
                try
                {
                    this.coinService.GetNewAddress(AccountName);
                }
                catch (RpcException e)
                {
                    this.logger.Info("Too many attempts. Waiting 10 sec.", toGenerateCount);
                    Thread.Sleep(10000);
                    i--;
                }

                if (i % 1000 == 0)
                    this.logger.Info("Generated {0} addresses.", i);
            }

            List<string> allAddresses = this.coinService.GetAddressesByAccount(AccountName);

            foreach (string address in allAddresses)
                context.UnusedAddresses.Add(new AddressModel() { Address = address });

            context.SaveChanges();
            this.logger.Info("Addresses generated.");
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
                        uint currentBlock = this.coinService.GetBlockCount();

                        this.logger.Trace("Current block is {0}, last checked block is {1}", currentBlock, lastCheckedBlock);

                        if (currentBlock > lastCheckedBlock)
                        {
                            lastCheckedBlock = currentBlock;

                            using (BotDbContext context = this.contextFactory.CreateContext())
                            {
                                this.CheckDeposits(context);
                            }
                        }

                        await Task.Delay(30 * 1000, this.cancellation.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception exception)
                {
                    this.logger.Fatal(exception.ToString);
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

            List<DiscordUser> usersToTrack = context.Users.Where(x => x.DepositAddress != null).ToList();
            this.logger.Trace("Tracking {0} users.", usersToTrack.Count);

            foreach (DiscordUser user in usersToTrack)
            {
                decimal receivedByAddress = this.coinService.GetReceivedByAddress(user.DepositAddress, this.settings.MinConfirmationsForDeposit);

                if (receivedByAddress > user.LastCheckedReceivedAmountByAddress)
                {
                    this.logger.Debug("New value for received by address is {0}. Old was {1}. Address is {2}.", receivedByAddress, user.LastCheckedReceivedAmountByAddress, user.DepositAddress);

                    decimal recentlyReceived = receivedByAddress - user.LastCheckedReceivedAmountByAddress;
                    user.LastCheckedReceivedAmountByAddress = receivedByAddress;

                    user.Balance += recentlyReceived;

                    this.logger.Info("User '{0}' deposited {1}!", user, recentlyReceived);

                    context.Update(user);
                    context.SaveChanges();
                }
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
