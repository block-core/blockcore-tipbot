using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TipBot.Database.Models
{
    public class TipModel
    {
        public int Id { get; set; }

        [Column(TypeName = "decimal(18,8)")]
        public decimal Amount { get; set; }

        public DateTime CreationTime { get; set; }

        public ulong SenderDiscordUserId { get; set; }

        public string SenderDiscordUserName { get; set; }

        public ulong ReceiverDiscordUserId { get; set; }

        public string ReceiverDiscordUserName { get; set; }
    }
}
