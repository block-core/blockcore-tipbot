using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BitcoinLib.ExceptionHandling.Rpc;
using BitcoinLib.Responses;
using BitcoinLib.Services.Coins.Base;
using BitcoinLib.Services.Coins.Bitcoin;
using NLog;
using TipBot.Database;
using TipBot.Database.Models;

namespace TipBot.Logic.NodeIntegrations
{
    public class RPCNodeIntegration : INodeIntegration
    {
        private const string AccountName = "account 0";

        private readonly ICoinService coinService;
        private readonly IContextFactory contextFactory;
        private readonly Settings settings;
        private readonly Logger logger;

        private Task depositsCheckingTask;
        private readonly CancellationTokenSource cancellation;

        private readonly string walletPassword;

        public RPCNodeIntegration(Settings settings, IContextFactory contextFactory)
        {
            // To run stratis daemon that supports RPC use "dotnet exec ...\netcoreapp2.1\Stratis.StratisD.dll -rpcuser=user -rpcpassword=4815162342 -rpcport=23521 -server=1"
            var daemonUrl = settings.ConfigReader.GetOrDefault<string>("daemonUrl", "http://127.0.0.1:23521/");
            var rpcUsername = settings.ConfigReader.GetOrDefault<string>("rpcUsername", "user");
            var rpcPassword = settings.ConfigReader.GetOrDefault<string>("rpcPassword", "4815162342");
            var rpcRequestTimeoutInSeconds = settings.ConfigReader.GetOrDefault<short>("rpcTimeout", 20);
            this.walletPassword = settings.ConfigReader.GetOrDefault<string>("walletPassword", "4815162342");

            this.coinService = new BitcoinService(daemonUrl, rpcUsername, rpcPassword, this.walletPassword, rpcRequestTimeoutInSeconds);
            this.contextFactory = contextFactory;
            this.settings = settings;
            this.cancellation = new CancellationTokenSource();
            this.logger = LogManager.GetCurrentClassLogger();
        }

        /// <inheritdoc />
        public void Initialize()
        {
            this.logger.Trace("()");

            // Unlock wallet.
            if (this.coinService.IsWalletEncrypted())
                this.coinService.WalletPassphrase(this.walletPassword, int.MaxValue);

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

            ValidateAddressResponse validationResult = this.coinService.ValidateAddress(address);

            if (!validationResult.IsValid)
            {
                this.logger.Trace("(-)[INVALID_ADDRESS]");
                throw new InvalidAddressException();
            }

            try
            {
                this.coinService.SendFrom(AccountName, address, amount);
            }
            catch (RpcInternalServerErrorException e)
            {
                if (e.Message == "Insufficient funds")
                {
                    // This should never happen.
                    this.logger.Fatal("Insufficient funds!");
                }

                this.logger.Error(e.ToString);
                throw;
            }

            this.logger.Trace("(-)");
        }

        /// <summary>Populates database with unused addresses.</summary>
        private void PregenerateAddresses(BotDbContext context)
        {
            this.logger.Trace("()");
            this.logger.Info("Database was not prefilled with addresses. Starting.");

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

            List<DiscordUserModel> usersToTrack = context.Users.Where(x => x.DepositAddress != null).ToList();
            this.logger.Trace("Tracking {0} users.", usersToTrack.Count);

            foreach (DiscordUserModel user in usersToTrack)
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
