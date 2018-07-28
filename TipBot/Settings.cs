using TipBot.Helpers;

namespace TipBot
{
    public class Settings
    {
        public void Initialize(TextFileConfiguration configReader)
        {
            this.BotToken = configReader.GetOrDefault<string>("token", "NDY4MDI1ODM0NTE5NjU4NDk2.DizKmA.pBifJbNeB0OlIJ5yZxF2kkJSaI8");

            // To run stratis daemon that supports RPC use "dotnet exec ...\netcoreapp2.1\Stratis.StratisD.dll -rpcuser=user -rpcpassword=4815162342 -rpcport=17771 -server=1"
            this.DaemonUrl = configReader.GetOrDefault<string>("daemonUrl", "http://127.0.0.1:17771/");
            this.RpcUsername = configReader.GetOrDefault<string>("rpcUsername", "user");
            this.RpcPassword = configReader.GetOrDefault<string>("rpcPassword", "4815162342");
            this.WalletPassword = configReader.GetOrDefault<string>("walletPassword", "walletPass");
            this.RpcRequestTimeoutInSeconds = configReader.GetOrDefault<short>("rpcTimeout", 20);
        }

        public string BotToken { get; private set; }

        public string LogoUrl { get; } = "https://stratisplatform.com/wp-content/uploads/2016/08/Stratis_Logo_Gradient.png";

        public string Ticker { get; } = "STRAT";

        // RPC
        public string DaemonUrl { get; private set; }
        public string RpcUsername { get; private set; }
        public string RpcPassword { get; private set; }
        public string WalletPassword { get; private set; }
        public short RpcRequestTimeoutInSeconds { get; private set; }
    }
}
