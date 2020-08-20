using System.Collections.Generic;
using XCloud.GetPeers.Api.Model;

namespace XCloud.GetPeers.Api.Persistance
{
    public interface IPeerListRepository
    {
        void AddPeers(List<Peer> peers);

        List<Peer> GetPeers(string version, bool all = false);
    }
}
