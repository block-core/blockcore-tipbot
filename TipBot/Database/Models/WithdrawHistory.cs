using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TipBot.Database.Models
{
    public class WithdrawHistory
    {
        public int Id { get; set; }

        public ulong DiscordUserId { get; set; }

        public string ToAddress { get; set; }

        [Column(TypeName = "decimal(18,8)")]
        public decimal Amount { get; set; }

        public string TransactionId { get; set; }

        public DateTime WithdrawTime { get; set; }
    }
}
