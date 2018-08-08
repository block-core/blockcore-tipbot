using TipBot.Helpers;

namespace TipBot
{
    public class Settings
    {
        public void Initialize(TextFileConfiguration configReader)
        {
            this.ConfigReader = configReader;

            this.BotToken = configReader.GetOrDefault<string>("token", "NDc2ODU5MDc1MzM1MDI4NzM4.DkztLQ.lWroAup2WOK8VZAyhGjG_E33ENY");

            this.EnableMigrations = configReader.GetOrDefault<bool>("enableMigrations", true);

            this.ConnectionString = configReader.GetOrDefault<string>("connectionString", @"Data Source=(LocalDb)\testDb;Initial Catalog=testBotDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
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

        /// <summary>DB connection string. Local db will be used if it's null.</summary>
        public string ConnectionString { get; private set; }
    }
}
