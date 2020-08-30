using System;
using System.Collections.Generic;
using System.Linq;
using XCloud.GetPeers.Api.Model;

namespace XCloud.GetPeers.Api.Persistance
{
    public class PeerListRepository : IPeerListRepository
    {
        private Dictionary<string, Dictionary<string, List<Peer>>> _peers;

        public PeerListRepository()
        {
            _peers = new Dictionary<string, Dictionary<string, List<Peer>>>();
        }
        public void AddPeers(string coin, List<Peer> peers)
        {
            if (!_peers.ContainsKey(coin))
                _peers.Add(coin, new Dictionary<string, List<Peer>>());

            var peersCoin = _peers[coin];

            string version;
            var yesterday = DateTime.Now.AddDays(-1);
            peers.ForEach(p =>
            {
                version = p.Version;

                if (!peersCoin.ContainsKey(version))
                    peersCoin.Add(version, new List<Peer>() { p });
                else
                {
                    var currentPeers = peersCoin[version];

                    currentPeers.RemoveAll(p => p.LastSeen < yesterday);

                    var currentPeer = currentPeers.FirstOrDefault(newPeer => p.Address == newPeer.Address);
                    if (currentPeer != null)
                        currentPeer.LastSeen = p.LastSeen;

                    if (!currentPeers.Any(peer => peer.Address.Equals(p.Address)))
                        currentPeers.Add(p);
                }
            });
        }

        public List<Peer> GetPeers(string coin, string version, bool all = false)
        {
            if (!_peers.ContainsKey(coin))
                _peers.Add(coin, new Dictionary<string, List<Peer>>());

            var peersCoin = _peers[coin];

            if (peersCoin.ContainsKey(version))
                return peersCoin[version];

            if (all)
            {
                var peers = new List<Peer>();
                foreach (var entry in peersCoin)
                {
                    peers.AddRange(entry.Value);
                }
                return peers;
            }

            return new List<Peer>();
        }
    }
}
