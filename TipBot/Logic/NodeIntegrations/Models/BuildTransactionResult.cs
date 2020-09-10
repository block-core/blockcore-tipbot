using Newtonsoft.Json;

namespace TipBot.Logic.NodeIntegrations.Models
{
    public class BuildTransactionResult
    {
        [JsonProperty(PropertyName = "fee")]
        public string Fee { get; set; }

        [JsonProperty(PropertyName = "hex")]
        public string Hex { get; set; }

        [JsonProperty(PropertyName = "transactionId")]
        public string TransactionId { get; set; }
    }
}
