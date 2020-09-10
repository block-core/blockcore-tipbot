using System.Collections.Generic;

namespace TipBot.Logic.NodeIntegrations.Models
{
    public class GetAddressesHistory
    {
        public List<History> history { get; set; }
    }

    public class Payment
    {
        public string destinationAddress { get; set; }
        public long amount { get; set; }
    }

    public class TransactionsHistory
    {
        public string type { get; set; }
        public string id { get; set; }
        public decimal amount { get; set; }
        public List<Payment> payments { get; set; }
        public int fee { get; set; }
        public int confirmedInBlock { get; set; }
        public string timestamp { get; set; }
        public string toAddress { get; set; }
        public int? blockIndex { get; set; }
    }

    public class History
    {
        public string accountName { get; set; }
        public string accountHdPath { get; set; }
        public int coinType { get; set; }
        public List<TransactionsHistory> transactionsHistory { get; set; }
    }
}
