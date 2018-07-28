using System;
using System.Threading.Tasks;
using BitcoinLib.Services;
using BitcoinLib.Services.Coins.Base;
using BitcoinLib.Services.Coins.Bitcoin;

namespace TipBot
{
    internal class Program
    {
        private Logic.TipBot bot;

        private static void Main(string[] args)
        {
            new Program().MainAsync(args).GetAwaiter().GetResult();
        }

        private async Task MainAsync(string[] args)
        {
            Console.CancelKeyPress += this.ShutdownHandler;

            this.bot = new Logic.TipBot();
            await this.bot.StartAsync(args);

            await Task.Delay(-1);
        }

        /// <summary>Shutdown the handler. Executed when user presses CTRL+C on console.</summary>
        private void ShutdownHandler(object sender, ConsoleCancelEventArgs args)
        {
            this.bot.Dispose();

            args.Cancel = true;
            Environment.Exit(0);
        }
    }
}
