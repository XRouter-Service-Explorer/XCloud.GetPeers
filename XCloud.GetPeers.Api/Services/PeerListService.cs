using Blocknet.Lib.Services.Coins.Cryptocoin;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XCloud.GetPeers.Api.Model;
using XCloud.GetPeers.Api.Persistance;

namespace XCloud.GetPeers.Api.Services
{
    public class PeerListService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IPeerListRepository _repository;
        private readonly ICryptocoinService _cryptoCoinService;

        public PeerListService(
            IPeerListRepository repository,
            ICryptocoinService cryptoCoinService
            )
        {
            _repository = repository;
            _cryptoCoinService = cryptoCoinService;
        }

        public void Dispose()
        {
            _timer.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero,
            TimeSpan.FromSeconds(15));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            var peerInfo = _cryptoCoinService.GetPeerInfo();

            var peers = peerInfo.Select(p =>
            {
                string version = p.SubVer.Split(":")[1];
                version = version.Remove(version.Length - 1);
                return new Peer
                {
                    Address = p.Addr.Split(":")[0],
                    Version = version,
                    LastSeen = DateTime.Now
                };
            }).ToList();

            _repository.AddPeers(peers);

        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }
    }
}
