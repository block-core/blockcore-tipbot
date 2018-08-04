using TipBot.Helpers;

namespace TipBot
{
    public class Settings
    {
        public void Initialize(TextFileConfiguration configReader)
        {
            this.ConfigReader = configReader;

            this.BotToken = configReader.GetOrDefault<string>("token", "NDY4MDI1ODM0NTE5NjU4NDk2.DizKmA.pBifJbNeB0OlIJ5yZxF2kkJSaI8");
        }

        public TextFileConfiguration ConfigReader { get; private set; }

        public string BotToken { get; private set; }

        public string Ticker { get; } = "STRAT";

        public uint PregeneratedAddressesCount { get; } = 8000;

        public int MinConfirmationsForDeposit { get; } = 5; //TODO set to 16. 5 is for testing

        public decimal MinWithdrawAmount { get; } = 0.1m;

        public decimal MinQuizAmount { get; } = 0.1m;

        public decimal MinMakeItRainTipAmount { get; } = 0.1m;

        public int MaxChartUsersCount { get; } = 3;

        public int MaxDaysChartCount { get; } = 30;
    }
}
