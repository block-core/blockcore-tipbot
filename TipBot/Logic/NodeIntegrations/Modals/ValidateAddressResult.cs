namespace TipBot.Logic.NodeIntegrations.Modals
{
    public class ValidateAddressResult
    {
        public bool isvalid { get; set; }
        public string address { get; set; }
        public string scriptPubKey { get; set; }
        public bool isscript { get; set; }
        public bool iswitness { get; set; }
    }
}
