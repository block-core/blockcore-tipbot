using TipBot.Helpers;

namespace TipBot
{
    public class Settings
    {
        public void Initialize(TextFileConfiguration configReader)
        {
            this.ConfigReader = configReader;

            this.BotToken = configReader.GetOrDefault<string>("token", "NDY4MDI1ODM0NTE5NjU4NDk2.DizKmA.pBifJbNeB0OlIJ5yZxF2kkJSaI8");

            this.EnableMigrations = configReader.GetOrDefault<bool>("enableMigrations", true);
        }

        public TextFileConfiguration ConfigReader { get; private set; }

        public string BotToken { get; private set; }

        public string Ticker { get; } = "STRAT";

        public uint PregeneratedAddressesCount { get; } = 8000;

        public int MinConfirmationsForDeposit { get; } = 16;

        public decimal MinWithdrawAmount { get; } = 0.1m;

        public decimal MinQuizAmount { get; } = 0.1m;

        public decimal MinMakeItRainTipAmount { get; } = 0.1m;

        public decimal MinTipAmount { get; } = 0.001m;

        public int MaxChartUsersCount { get; } = 3;

        public int MaxDaysChartCount { get; } = 30;

        /// <summary>Specifies if bit should attempt to update the database on startup.</summary>
        public bool EnableMigrations { get; private set; }

        /// <summary>Nickname of a user that will receive a message in case of a fatal error.</summary>
        public string SupportUsername { get; } = "NoEscape0";
        public string SupportDiscriminator { get; } = "5537";
    }
}
