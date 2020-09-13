using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TipBot
{
    public class Worker : IHostedService
    {
        private Logic.TipBot bot;

        private Settings settings;

        public Worker(IOptionsMonitor<Settings> options, Logic.TipBot bot)
        {
            this.settings = options.CurrentValue;
            this.bot = bot;

            // Make sure it is possible to edit the API keys while running.
            options.OnChange(config =>
            {
                this.settings = config;
            });
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await this.bot.StartAsync(cancellationToken).ConfigureAwait(false);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.bot.Dispose();

            return Task.CompletedTask;
        }
    }
}
