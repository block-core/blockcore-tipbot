using System.Collections.Generic;

namespace TipBot.Logic.NodeIntegrations.Models
{
    public class OutboundPeer
    {
        public string version { get; set; }
        public string remoteSocketEndpoint { get; set; }
        public int tipHeight { get; set; }
    }

    public class FeaturesData
    {
        public string @namespace { get; set; }
        public string state { get; set; }
    }

    public class GetNodeStatusResult
    {
        public string agent { get; set; }
        public string version { get; set; }
        public string externalAddress { get; set; }
        public string network { get; set; }
        public string coinTicker { get; set; }
        public int processId { get; set; }
        public int consensusHeight { get; set; }
        public int blockStoreHeight { get; set; }
        public int bestPeerHeight { get; set; }
        public List<object> inboundPeers { get; set; }
        public List<OutboundPeer> outboundPeers { get; set; }
        public List<FeaturesData> featuresData { get; set; }
        public string dataDirectoryPath { get; set; }
        public string runningTime { get; set; }
        public double difficulty { get; set; }
        public int protocolVersion { get; set; }
        public bool testnet { get; set; }
        public double relayFee { get; set; }
        public string state { get; set; }
    }
}
