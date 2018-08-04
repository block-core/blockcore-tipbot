using System;
using System.Collections.Generic;
using System.Text;

namespace TipBot.Tests.CommandsTests
{
    public class RandomlyTipUsersTests
    {
        //TODO
        /*
         * public List<DiscordUserModel> RandomlyTipUsers(IUser caller, List<IUser> onlineUsers, decimal totalAmount, decimal tipAmount)
        {
            this.logger.Trace("({0}:{1},{2}.{3}:{4},{5}:{6})", nameof(caller), caller.Id, nameof(onlineUsers), nameof(onlineUsers.Count), onlineUsers.Count, nameof(totalAmount), totalAmount);

            this.AssertAmountPositive(totalAmount);

            if (tipAmount < this.settings.MinMakeItRainTipAmount)
            {
                this.logger.Trace("(-)[TIP_SIZE_TOO_SMALL]'");
                throw new CommandExecutionException($"Tip amount can't be less than {this.settings.MinMakeItRainTipAmount}.");
            }

            if (totalAmount < tipAmount)
            {
                this.logger.Trace("(-)[AMOUNT_TOO_SMALL]'");
                throw new CommandExecutionException("Total amount for tipping can't be less than specified tip amount.");
            }

            if (onlineUsers.Count == 0)
            {
                this.logger.Trace("(-)[NO_USERS_ONLINE]'");
                throw new CommandExecutionException("There are no users online!");
            }

            var tipsCount = (int)(totalAmount / tipAmount);

            if (tipsCount > onlineUsers.Count)
            {
                this.logger.Trace("Coins to tip was set to amount of users.");
                tipsCount = onlineUsers.Count;
            }

            decimal amountToSpend = tipAmount * tipsCount;

            using (BotDbContext context = this.contextFactory.CreateContext())
            {
                DiscordUserModel discordUserCreator = this.GetOrCreateUser(context, caller);

                this.AssertBalanceIsSufficient(discordUserCreator, amountToSpend);

                var chosenDiscordUsers = new List<DiscordUserModel>(tipsCount);

                for (int i = 0; i < tipsCount; i++)
                {
                    int userIndex = this.random.Next(onlineUsers.Count);

                    IUser chosenUser = onlineUsers[userIndex];
                    onlineUsers.Remove(chosenUser);

                    DiscordUserModel chosenDiscordUser = this.GetOrCreateUser(context, chosenUser);
                    chosenDiscordUsers.Add(chosenDiscordUser);
                }

                discordUserCreator.Balance -= amountToSpend;
                context.Update(discordUserCreator);

                foreach (DiscordUserModel discordUser in chosenDiscordUsers)
                {
                    discordUser.Balance += tipAmount;
                    context.Update(discordUser);

                    this.AddTipToHistory(context, tipAmount, discordUser.DiscordUserId, discordUserCreator.DiscordUserId);

                    this.logger.Debug("User '{0}' was randomly tipped.", discordUser);
                }

                this.logger.Debug("User '{0}' tipped {1} users {2} coins each.", discordUserCreator, chosenDiscordUsers.Count, tipAmount);

                context.SaveChanges();

                this.logger.Trace("(-)");
                return chosenDiscordUsers;
            }
        }
         */
    }
}
