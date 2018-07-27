using TipBot.Helpers;

namespace TipBot
{
    public class Settings
    {
        public void Initialize(TextFileConfiguration configReader)
        {
            this.BotToken = configReader.GetOrDefault<string>("token", "NDY4MDI1ODM0NTE5NjU4NDk2.DizKmA.pBifJbNeB0OlIJ5yZxF2kkJSaI8");
        }

        public string BotToken { get; private set; }

        public string LogoUrl { get; } = "https://stratisplatform.com/wp-content/uploads/2016/08/Stratis_Logo_Gradient.png";

        public string Ticker { get; } = "STRAT";
    }
}
