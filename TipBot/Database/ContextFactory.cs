using System;
using System.Collections.Generic;
using System.Text;

namespace TipBot.Database
{
    public class ContextFactory : IContextFactory
    {
        public BotDbContext CreateContext()
        {
            return new BotDbContext();
        }
    }

    public interface IContextFactory
    {
        BotDbContext CreateContext();
    }
}
