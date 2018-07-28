using System;
using Discord.Commands;

namespace TipBot.CommandModules
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandWithHelpAttribute : CommandAttribute
    {
        public readonly string HelpInfo;

        public readonly string UsageExample;

        public CommandWithHelpAttribute(string command, string helpInfo, string usageExample = null) : base(command)
        {
            this.HelpInfo = helpInfo;
            this.UsageExample = usageExample;
        }
    }
}
