namespace TipBot.Database.Models
{
    public class DiscordUserModel
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public ulong DiscordUserId { get; set; }

        public decimal Balance { get; set; }

        public string DepositAddress { get; set; } = null;

        /// <summary>How much money was received in total by <see cref="DepositAddress"/> by the time last check happened.</summary>
        public decimal LastCheckedReceivedAmountByAddress { get; set; } = 0;

        public override string ToString()
        {
            return $"{nameof(this.Id)}:{this.Id},{nameof(this.Username)}:{this.Username},{nameof(this.DiscordUserId)}:{this.DiscordUserId},{nameof(this.Balance)}:{this.Balance})";
        }
    }
}
