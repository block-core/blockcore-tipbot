using System;

namespace TipBot.Database.Models
{
    public class QuizModel
    {
        public int Id { get; set; }

        public ulong CreatorDiscordUserId { get; set; }

        public string AnswerHash { get; set; }

        public string Question { get; set; }

        public decimal Reward { get; set; }

        public DateTime CreationTime { get; set; }

        public int DurationMinutes { get; set; }
    }
}
