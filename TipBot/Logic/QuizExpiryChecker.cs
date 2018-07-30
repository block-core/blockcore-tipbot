using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using TipBot.Database;
using TipBot.Database.Models;

namespace TipBot.Logic
{
    /// <summary>Checks if quizes are expired and should be canceled.</summary>
    public class QuizExpiryChecker : IDisposable
    {
        private readonly IContextFactory contextFactory;
        private readonly CancellationTokenSource cancellation;
        private readonly Logger logger;

        private Task quizCheckingTask;

        public QuizExpiryChecker(IContextFactory contextFactory)
        {
            this.contextFactory = contextFactory;
            this.logger = LogManager.GetCurrentClassLogger();
            this.cancellation = new CancellationTokenSource();
        }

        public void Initialize()
        {
            this.logger.Trace("()");

            this.quizCheckingTask = Task.Run(async () =>
            {
                try
                {
                    while (!this.cancellation.IsCancellationRequested)
                    {
                        this.CheckQuizesExpired();

                        await Task.Delay(60 * 1000, this.cancellation.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception exception)
                {
                    this.logger.Fatal(exception.ToString);
                    throw;
                }
            });

            this.logger.Trace("(-)");
        }

        /// <summary>Checks quizes and removes those that are expired.</summary>
        private void CheckQuizesExpired()
        {
            this.logger.Trace("()");

            using (BotDbContext context = this.contextFactory.CreateContext())
            {
                foreach (QuizModel quiz in context.ActiveQuizes.ToList())
                {
                    if (DateTime.Now > (quiz.CreationTime + TimeSpan.FromMinutes(quiz.DurationMinutes)))
                    {
                        // Quiz expired. Return money to creator and remove the quiz.
                        this.logger.Info("Quiz {0} expired.", quiz.Id);

                        DiscordUserModel quizCreator = context.Users.Single(x => x.DiscordUserId == quiz.CreatorDiscordUserId);
                        quizCreator.Balance += quiz.Reward;
                        context.Update(quizCreator);

                        context.ActiveQuizes.Remove(quiz);
                        context.SaveChanges();
                    }
                }
            }

            this.logger.Trace("(-)");
        }

        public void Dispose()
        {
            this.cancellation.Cancel();
            this.quizCheckingTask?.GetAwaiter().GetResult();
        }
    }
}
