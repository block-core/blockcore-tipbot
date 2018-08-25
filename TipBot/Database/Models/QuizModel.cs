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

    public class QuizViewModel : QuizModel
    {
        public QuizViewModel(QuizModel from)
        {
            this.Id = from.Id;
            this.CreatorDiscordUserId = from.CreatorDiscordUserId;
            this.AnswerHash = from.AnswerHash;
            this.Question = from.Question;
            this.Reward = from.Reward;
            this.CreationTime = from.CreationTime;
            this.DurationMinutes = from.DurationMinutes;
        }

        public string DiscordUserName { get; set; }
    }
}
