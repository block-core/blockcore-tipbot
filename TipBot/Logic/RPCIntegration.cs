using BitcoinLib.Services.Coins.Base;
using BitcoinLib.Services.Coins.Bitcoin;

namespace TipBot.Logic
{
    public class RPCIntegration
    {
        private ICoinService coinService;

        public RPCIntegration(Settings settings)
        {
            this.coinService = new BitcoinService(settings.DaemonUrl, settings.RpcUsername, settings.RpcPassword, settings.WalletPassword, settings.RpcRequestTimeoutInSeconds);
        }

        public uint GetBlocksCount()
        {
            return this.coinService.GetBlockCount();
        }
    }
}
