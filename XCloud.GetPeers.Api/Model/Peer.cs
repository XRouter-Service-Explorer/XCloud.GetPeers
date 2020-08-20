using System;

namespace XCloud.GetPeers.Api.Model
{
    public class Peer
    {
        public string Address { get; set; }
        public DateTime LastSeen { get; set; }
        public string Version { get; set; }
    }
}
