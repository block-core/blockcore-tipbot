using System.Collections.Generic;

namespace TipBot.Logic.NodeIntegrations.Modals
{
    public class Balance
    {
        public string address { get; set; }
        public decimal balance { get; set; }
    }

    public class GetAddressBalanceResult
    {
        public List<Balance> balances { get; set; }
        public string reason { get; set; }
    }


}
