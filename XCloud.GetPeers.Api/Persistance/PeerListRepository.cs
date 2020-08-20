using System;
using System.Collections.Generic;
using System.Linq;
using XCloud.GetPeers.Api.Model;

namespace XCloud.GetPeers.Api.Persistance
{
    public class PeerListRepository : IPeerListRepository
    {
        private Dictionary<string, List<Peer>> _peers;

        public PeerListRepository()
        {
            _peers = new Dictionary<string, List<Peer>>();
        }
        public void AddPeers(List<Peer> peers)
        {
            string version;
            var yesterday = DateTime.Now.AddDays(-1);
            peers.ForEach(p =>
            {
                version = p.Version;

                if (!_peers.ContainsKey(version))
                    _peers.Add(version, new List<Peer>() { p });
                else
                {
                    var currentPeers = _peers[version];

                    currentPeers.RemoveAll(p => p.LastSeen < yesterday);

                    var currentPeer = currentPeers.FirstOrDefault(newPeer => p.Address == newPeer.Address);
                    if (currentPeer != null)
                        currentPeer.LastSeen = p.LastSeen;

                    if (!currentPeers.Any(peer => peer.Address.Equals(p.Address)))
                        currentPeers.Add(p);
                }
            });
        }

        public List<Peer> GetPeers(string version, bool all = false)
        {
            if (_peers.ContainsKey(version))
                return _peers[version];

            if (all)
            {
                var peers = new List<Peer>();
                foreach (var entry in _peers)
                {
                    peers.AddRange(entry.Value);
                }
                return peers;
            }

            return new List<Peer>();
        }
    }
}
