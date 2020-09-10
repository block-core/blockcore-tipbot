using System.Collections.Generic;

namespace TipBot.Logic.NodeIntegrations.Modals
{
    public class Outpoint
    {
        public string transactionId { get; set; }
        public int index { get; set; }
    }

    public class Recipient
    {
        public string destinationAddress { get; set; }
        public string amount { get; set; }
    }

    public class BuildTransactionRequest
    {
        public string feeAmount { get; set; }
        public string password { get; set; }
        public bool segwitChangeAddress { get; set; }
        public string walletName { get; set; }
        public string accountName { get; set; }
        public List<Outpoint> outpoints { get; set; }
        public List<Recipient> recipients { get; set; }
        public string opReturnData { get; set; }
        public string opReturnAmount { get; set; }
        public string feeType { get; set; }
        public bool allowUnconfirmed { get; set; }
        public bool shuffleOutputs { get; set; }
        public string changeAddress { get; set; }
    }


}
