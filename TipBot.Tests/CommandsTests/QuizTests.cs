using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using TipBot.Database;
using TipBot.Database.Models;
using TipBot.Logic;
using TipBot.Tests.Helpers;
using Xunit;

namespace TipBot.Tests.CommandsTests
{
    public class QuizTests
    {
        private readonly TestContext testContext;

        private readonly IUser caller;

        private readonly string hash = "489cd5dbc708c7e541de4d7cd91ce6d0f1613573b7fc5b40d3942ccb9555cf35";

        public QuizTests()
        {
            this.testContext = new TestContext();

            this.caller = this.testContext.SetupUser(1, "caller");

            // That will create a user in db.
            this.testContext.CommandsManager.GetUserBalance(this.caller);

            using (BotDbContext dbContext = this.testContext.CreateDbContext())
            {
                DiscordUserModel user = dbContext.Users.First();
                user.Balance = 10;
                dbContext.Update(user);
                dbContext.SaveChanges();
            }
        }

        [Fact]
        public void StartQuiz_AssertsAmountPositive()
        {
            Assert.Throws<CommandExecutionException>(() => this.testContext.CommandsManager.StartQuiz(this.caller, 0, this.hash, 1, string.Empty));
        }

        [Fact]
        public void StartQuiz_AssertsHashNotBogus()
        {
            Assert.Throws<CommandExecutionException>(() => this.testContext.CommandsManager.StartQuiz(this.caller, 1, "qwe", 1, string.Empty));
        }

        [Fact]
        public void StartQuiz_AssertsDurationIsMoreOrEqualThanOne()
        {
            Assert.Throws<CommandExecutionException>(() => this.testContext.CommandsManager.StartQuiz(this.caller, 1, this.hash, -1, string.Empty));
        }

        [Fact]
        public void StartQuiz_AssertsQuestionIsNotTooLong()
        {
            var builder = new StringBuilder();
            for (int i = 0; i < 5000; i++)
                builder.Append("qwe");

            Assert.Throws<CommandExecutionException>(() => this.testContext.CommandsManager.StartQuiz(this.caller, 1, this.hash, 2, builder.ToString()));
        }

        [Fact]
        public void StartQuiz_AssertsSameHashIsNotUsedTwice()
        {
            this.testContext.CommandsManager.StartQuiz(this.caller, 1, this.hash, 2, string.Empty);

            Assert.Throws<CommandExecutionException>(() => this.testContext.CommandsManager.StartQuiz(this.caller, 1, this.hash, 2, string.Empty));
        }

        [Fact]
        public void StartQuiz_AssertsBalanceIsSufficient()
        {
            this.testContext.CommandsManager.StartQuiz(this.caller, 6, this.hash, 2, string.Empty);

            Assert.Throws<CommandExecutionException>(() => this.testContext.CommandsManager.StartQuiz(this.caller, 5, this.hash, 2, string.Empty));
        }

        [Fact]
        public void StartQuiz_AssertsAmountIsMoreThanMin()
        {
            Assert.Throws<CommandExecutionException>(() => this.testContext.CommandsManager.StartQuiz(this.caller, this.testContext.Settings.MinQuizAmount/2, this.hash, 2, string.Empty));
        }

        [Fact]
        public void StartQuiz_QuizCreated()
        {
            this.testContext.CommandsManager.StartQuiz(this.caller, 6, this.hash, 2, string.Empty);

            using (BotDbContext dbContext = this.testContext.CreateDbContext())
            {
                QuizModel quiz = dbContext.ActiveQuizes.First();

                Assert.Equal(this.hash, quiz.AnswerHash);
                Assert.Equal(string.Empty, quiz.Question);
                Assert.Equal(this.caller.Id, quiz.CreatorDiscordUserId);
                Assert.Equal(2, quiz.DurationMinutes);
                Assert.Equal(6, quiz.Reward);

                Assert.Equal(4, dbContext.Users.First().Balance);
            }
        }

        //TODO
        /*
         *

        public List<QuizModel> GetActiveQuizes()
        {
            this.logger.Trace("()");

            using (BotDbContext context = this.contextFactory.CreateContext())
            {
                List<QuizModel> quizes = context.ActiveQuizes.ToList();

                this.logger.Trace("(-):{0}", quizes.Count);
                return quizes;
            }
        }

        public AnswerToQuizResponseModel AnswerToQuiz(IUser user, string answer)
        {
            this.logger.Trace("({0}:'{1}')", nameof(answer), answer);

            if (answer.Length > 1024)
            {
                // We don't want to hash big strings.
                this.logger.Trace("(-)[ANSWER_TOO_LONG]");
                return new AnswerToQuizResponseModel() { Success = false };
            }

            string answerHash = Cryptography.Hash(answer);

            using (BotDbContext context = this.contextFactory.CreateContext())
            {
                foreach (QuizModel quiz in context.ActiveQuizes.ToList())
                {
                    if (DateTime.Now > (quiz.CreationTime + TimeSpan.FromMinutes(quiz.DurationMinutes)))
                    {
                        // Quiz expired but just not deleted yet.
                        continue;
                    }

                    if (quiz.AnswerHash == answerHash)
                    {
                        DiscordUserModel winner = this.GetOrCreateUser(context, user);

                        winner.Balance += quiz.Reward;
                        context.Update(winner);

                        context.ActiveQuizes.Remove(quiz);
                        context.SaveChanges();

                        this.logger.Debug("User {0} solved quiz with hash {1} and received a reward of {2}.", winner, quiz.AnswerHash, quiz.Reward);

                        var response = new AnswerToQuizResponseModel()
                        {
                            Success = true,
                            Reward = quiz.Reward,
                            QuizCreatorDiscordUserId = quiz.CreatorDiscordUserId,
                            QuizQuestion = quiz.Question
                        };

                        this.logger.Trace("(-)");
                        return response;
                    }
                }
            }

            this.logger.Trace("(-)[QUIZ_NOT_FOUND]");
            return new AnswerToQuizResponseModel() { Success = false };
        }
         */
    }
}
