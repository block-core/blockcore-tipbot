using System;
using TipBot.Database;

namespace TipBot.Logic.NodeIntegrations
{
    public interface INodeIntegration : IDisposable
    {
        /// <summary>Initializes the class.</summary>
        /// <remarks>Generates addresses and loads them to <see cref="BotDbContext.UnusedAddresses"/> if <see cref="BotDbContext.UnusedAddresses"/> is empty.</remarks>
        void Initialize();

        /// <summary>Withdraws specified amount or money to specified address.</summary>
        /// <remarks>Address will be validated prior to withdrawal.</remarks>
        void Withdraw(decimal amount, string address);
    }

    public class InvalidAddressException : Exception
    {
        public InvalidAddressException() : base("Address specified is invalid.") { }
    }
}
