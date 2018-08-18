using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;

namespace TipBot.Services
{
    public class ErrorMessageCreator
    {
        public string CreateErrorMessage(IResult commandResult)
        {
            if (commandResult.IsSuccess || !commandResult.Error.HasValue)
                throw new Exception("No error to create error message for.");

            var answer = ":no_entry: ";

            switch (commandResult.Error.Value)
            {
                case CommandError.UnknownCommand:
                    answer += "Unknown command. You've probably typed command name incorrectly.";
                    break;
                case CommandError.ParseFailed:
                    answer += "Failed to recognize command argument. Most likely argument type is different (for example: bot expected a number but you supplied a string).";
                    break;
                case CommandError.BadArgCount:
                    answer += "Bad arguments count. You've supplied more or less arguments than invoked command requires.";
                    break;
                case CommandError.ObjectNotFound:
                case CommandError.MultipleMatches:
                case CommandError.UnmetPrecondition:
                case CommandError.Exception:
                case CommandError.Unsuccessful:
                    answer += $"Error: {commandResult.Error}. {commandResult.ErrorReason}";
                    break;
            }

            answer += Environment.NewLine + "Use `help` command to display all supported commands and their usage examples.";

            return answer;
        }
    }
}
