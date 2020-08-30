using Blocknet.Lib.Services.Coins.Cryptocoin;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using XCloud.GetPeers.Api.Model;
using XCloud.GetPeers.Api.Persistance;

namespace XCloud.GetPeers.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PeersController : ControllerBase
    {
        private readonly IEnumerable<ICryptocoinService> _cryptoCoinServices;
        private readonly IPeerListRepository _repository;

        public PeersController(IEnumerable<ICryptocoinService> cryptoCoinServices, IPeerListRepository repository)
        {
            _cryptoCoinServices = cryptoCoinServices;
            _repository = repository;
        }

        [HttpGet("[action]")]
        public IActionResult GetPeerList(string coinName, string version)
        {
            if (string.IsNullOrEmpty(coinName))
                return BadRequest("No coin name provided");

            if (string.IsNullOrEmpty(version))
                return BadRequest("No coin version provided");

            var cryptoService = _cryptoCoinServices.FirstOrDefault(svc => svc.Parameters.CoinShortName.Equals(coinName));

            if (cryptoService == null)
                return BadRequest($"This service node does not have {coinName} configured");

            var peerInfo = cryptoService.GetPeerInfo();

            if (!version.Equals("0"))
                peerInfo = peerInfo.Where(p => p.SubVer.Equals("/" + cryptoService.Parameters.CoinLongName + ":" + version + "/")).ToList();

            var addresses = peerInfo.Select(p => p.Addr.Split(":")[0]).ToList();

            if (addresses.Count.Equals(0))
                return Ok($"No peers of version {version} connected to my {coinName} wallet");

            string peerssResponse = string.Empty;

            addresses.ForEach(ad =>
            {
                peerssResponse += "addnode=" + ad + Environment.NewLine;
            });

            return Ok(peerssResponse);
        }

        [HttpGet("[action]")]
        public IActionResult GetDailyPeerList(string coinName, string version)
        {
            if (string.IsNullOrEmpty(coinName))
                return BadRequest("No coin name provided");

            if (string.IsNullOrEmpty(version))
                return BadRequest("No coin version provided");

            if (!_cryptoCoinServices.Any(svc => svc.Parameters.CoinShortName.Equals(coinName)))
                return BadRequest($"This service node does not have {coinName} configured");

            try
            {
                List<Peer> peers;
                if (version.Equals("0"))
                    peers = _repository.GetPeers(coinName, version, all: true);

                else
                    peers = _repository.GetPeers(coinName, version);

                if (peers.Count.Equals(0))
                    return Ok($"No peers of version {version} connected to my {coinName} wallet");

                string peerssResponse = string.Empty;

                peers.ForEach(ad =>
                {
                    peerssResponse += "addnode=" + ad.Address + Environment.NewLine;
                });

                return Ok(peerssResponse);
            }
            catch (Exception)
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
