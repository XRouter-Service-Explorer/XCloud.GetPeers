﻿using Blocknet.Lib.ExceptionHandling.Rpc;
using Blocknet.Lib.Responses;
using Blocknet.Lib.Services.Coins.Cryptocoin;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
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
        private readonly IEnumerable<ICryptocoinService> _cryptoCoinServices;

        public PeerListService(
            IPeerListRepository repository,
            IEnumerable<ICryptocoinService> cryptoCoinServices
            )
        {
            _repository = repository;
            _cryptoCoinServices = cryptoCoinServices;
        }

        public void Dispose()
        {
            _timer.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero,
            TimeSpan.FromSeconds(20));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            foreach (var cryptoService in _cryptoCoinServices)
            {
                List<GetPeerInfoResponse> peerInfoRespone;
                try
                {
                    peerInfoRespone = cryptoService.GetPeerInfo();

                }
                catch (RpcException e)
                {
                    Console.WriteLine(e.StackTrace);
                    return;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                    return;
                }

                var peers = peerInfoRespone
                    .Where(p => !string.IsNullOrWhiteSpace(p.SubVer))
                    .Select(p =>
                    {
                        string version = p.SubVer.Split(":")[1];
                        version = version.Remove(version.Length - 1);

                        var uri = new Uri("http://" + p.Addr);
                        string addr = uri.Host;
                        addr = addr.Replace("[", "");
                        addr = addr.Replace("]", "");
                        return new Peer
                        {
                            Address = addr,
                            Version = version,
                            LastSeen = DateTime.Now
                        };
                    })
                    .ToList();

                _repository.AddPeers(cryptoService.Parameters.CoinShortName, peers);
            }
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }
    }
}
