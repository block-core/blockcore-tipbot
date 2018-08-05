using System;
using System.Linq;
using System.Text;
using Discord;
using TipBot.Database;
using TipBot.Database.Models;
using TipBot.Helpers;
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
            for (var i = 0; i < 5000; i++)
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

        [Fact]
        public void GetActiveQuizes_ReturnsActiveQuizes()
        {
            using (BotDbContext dbContext = this.testContext.CreateDbContext())
            {
                Assert.Equal(0, dbContext.ActiveQuizes.Count());
            }

            Assert.Empty(this.testContext.CommandsManager.GetActiveQuizes());

            this.testContext.CommandsManager.StartQuiz(this.caller, 2, Cryptography.Hash("1"), 2, string.Empty);
            this.testContext.CommandsManager.StartQuiz(this.caller, 2, Cryptography.Hash("2"), 2, string.Empty);

            using (BotDbContext dbContext = this.testContext.CreateDbContext())
            {
                Assert.Equal(2, dbContext.ActiveQuizes.Count());
            }

            Assert.Equal(2, this.testContext.CommandsManager.GetActiveQuizes().Count);
        }

        [Fact]
        public void AnswerToQuiz_FailsIfAnswerTooLong()
        {
            AnswerToQuizResponseModel answer = this.testContext.CommandsManager.AnswerToQuiz(this.caller, RandomStringGenerator.RandomString(1025));
            Assert.False(answer.Success);
        }

        [Fact]
        public void AnswerToQuiz_FailsIfHashNotCorrect()
        {
            this.testContext.CommandsManager.StartQuiz(this.caller, 2, Cryptography.Hash("1"), 2, string.Empty);

            AnswerToQuizResponseModel answer = this.testContext.CommandsManager.AnswerToQuiz(this.caller, "2");
            Assert.False(answer.Success);
        }

        [Fact]
        public void AnswerToQuiz_FailsIfTimeIsCorrectButExpired()
        {
            this.testContext.CommandsManager.StartQuiz(this.caller, 2, Cryptography.Hash("1"), 2, string.Empty);

            using (BotDbContext dbContext = this.testContext.CreateDbContext())
            {
                QuizModel quiz = dbContext.ActiveQuizes.First();
                quiz.CreationTime = DateTime.Now - TimeSpan.FromMinutes(20);
                dbContext.Update(quiz);
                dbContext.SaveChanges();
            }

            AnswerToQuizResponseModel answer = this.testContext.CommandsManager.AnswerToQuiz(this.caller, "1");
            Assert.False(answer.Success);
        }

        [Fact]
        public void AnswerToQuiz_AnsweredCorrectly()
        {
            this.testContext.CommandsManager.StartQuiz(this.caller, 2, Cryptography.Hash("1"), 2, string.Empty);
            this.testContext.CommandsManager.StartQuiz(this.caller, 2, Cryptography.Hash("2"), 2, string.Empty);

            using (BotDbContext dbContext = this.testContext.CreateDbContext())
            {
                Assert.Equal(2, dbContext.ActiveQuizes.Count());
            }

            AnswerToQuizResponseModel answer = this.testContext.CommandsManager.AnswerToQuiz(this.caller, "1");
            Assert.True(answer.Success);

            using (BotDbContext dbContext = this.testContext.CreateDbContext())
            {
                Assert.Equal(1, dbContext.ActiveQuizes.Count());
            }
        }
    }
}
