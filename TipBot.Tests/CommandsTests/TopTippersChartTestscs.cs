using System;
using System.Collections.Generic;
using System.Text;

namespace TipBot.Tests.CommandsTests
{
    public class TopTippersChartTestscs
    {
        //TODO
        /*
         * /// <summary>
        /// Gets top users (up to <paramref name="amountOfUsersToReturn"/> for each nomination) who tipped
        /// the most and were tipped the most over the last <paramref name="periodDays"/> days.
        /// </summary>
        /// <exception cref="CommandExecutionException">Thrown when user supplied invalid input data.</exception>
        public TippingChartsModel GetTopTippers(int periodDays, int amountOfUsersToReturn)
        {
            this.logger.Trace("({0}:{1},{2}:{3})", nameof(periodDays), periodDays, nameof(amountOfUsersToReturn), amountOfUsersToReturn);

            if (periodDays > this.settings.MaxDaysChartCount)
            {
                this.logger.Trace("(-)[PERIOD_TOO_LONG]");
                throw new CommandExecutionException($"Period in days can't be longer than {this.settings.MaxDaysChartCount}.");
            }

            if (periodDays < 1)
            {
                this.logger.Trace("(-)[PERIOD_TOO_SHORT]");
                throw new CommandExecutionException($"Period in days can't be shorter than 1 day.");
            }

            var bestTippers = new Dictionary<ulong, decimal>();
            var bestBeingTipped = new Dictionary<ulong, decimal>();

            using (BotDbContext context = this.contextFactory.CreateContext())
            {
                DateTime earliestCreationDate = DateTime.Now - TimeSpan.FromDays(periodDays);

                foreach (TipModel tip in context.TipsHistory.Where(x => x.CreationTime > earliestCreationDate))
                {
                    if (!bestTippers.ContainsKey(tip.SenderDiscordUserId))
                        bestTippers.Add(tip.SenderDiscordUserId, 0);

                    if (!bestBeingTipped.ContainsKey(tip.ReceiverDiscordUserId))
                        bestBeingTipped.Add(tip.ReceiverDiscordUserId, 0);

                    bestTippers[tip.SenderDiscordUserId] += tip.Amount;
                    bestBeingTipped[tip.ReceiverDiscordUserId] += tip.Amount;
                }
            }

            var model = new TippingChartsModel()
            {
                BestTippers = bestTippers.OrderBy(x => x.Value).Take(amountOfUsersToReturn).ToList(),
                BestBeingTipped = bestBeingTipped.OrderBy(x => x.Value).Take(amountOfUsersToReturn).ToList(),
            };

            this.logger.Trace("(-)");
            return model;
        }
         */
    }
}
