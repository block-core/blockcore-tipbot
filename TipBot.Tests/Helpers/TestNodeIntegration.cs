using System;
using System.Collections.Generic;
using System.Text;
using TipBot.Logic.NodeIntegrations;

namespace TipBot.Tests.Helpers
{
    public class TestNodeIntegration : INodeIntegration
    {
        public void Initialize()
        {
        }

        public void Withdraw(decimal amount, string address)
        {
        }

        public void Dispose()
        {
        }
    }
}
