using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BitcoinLib.ExceptionHandling.Rpc;
using BitcoinLib.Services.Coins.Base;
using BitcoinLib.Services.Coins.Bitcoin;
using NLog;
using TipBot.Database;
using TipBot.Database.Models;

namespace TipBot.Logic
{
    public class RPCIntegration
    {
        private readonly ICoinService coinService;

        private readonly IContextFactory contextFactory;

        private readonly Settings settings;

        private const string AccountName = "account 0";

        private readonly Logger logger;

        public RPCIntegration(Settings settings, IContextFactory contextFactory)
        {
            this.coinService = new BitcoinService(settings.DaemonUrl, settings.RpcUsername, settings.RpcPassword, settings.WalletPassword, settings.RpcRequestTimeoutInSeconds);
            this.contextFactory = contextFactory;
            this.settings = settings;
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

            this.logger.Trace("(-)");
        }

        /// <summary>Populates database with unused addresses.</summary>
        private void PregenerateAddresses(BotDbContext context)
        {
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
        }

        //TODO monitor if some address received a payment
        //private void QWE()
        //{
        //    this.coinService.GetReceivedByAddress()
        //}

        public uint GetBlocksCount()
        {
            return this.coinService.GetBlockCount();
        }
    }
}
