using Blocknet.Lib.Services.Coins.Cryptocoin;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using XCloud.GetPeers.Api.Model;
using XCloud.GetPeers.Api.Persistance;

namespace XCloud.GetPeers.Api.Controllers
{
    [ApiController]
    [Route("xrs")]
    public class PeersController : ControllerBase
    {
        private readonly IEnumerable<ICryptocoinService> _cryptoCoinServices;
        private readonly IPeerListRepository _repository;

        public PeersController(IEnumerable<ICryptocoinService> cryptoCoinServices, IPeerListRepository repository)
        {
            _cryptoCoinServices = cryptoCoinServices;
            _repository = repository;
        }

        [HttpPost("[action]")]
        public IActionResult GetPeerList([FromBody] string[] parameters)
        {
            string coinName;
            string version;
            if (string.IsNullOrEmpty(parameters[0]))
                return BadRequest("No coin name provided");
            else
                coinName = parameters[0].ToUpper();

            if (string.IsNullOrEmpty(parameters[1]))
                return BadRequest("No coin version provided");
            else
                version = parameters[1];

            var cryptoService = _cryptoCoinServices.FirstOrDefault(svc => svc.Parameters.CoinShortName.Equals(coinName));

            if (cryptoService == null)
                return BadRequest($"This service node does not have {coinName} configured");

            var peerInfo = cryptoService.GetPeerInfo();

            if (!version.Equals("0"))
                peerInfo = peerInfo.Where(p => p.SubVer.Equals("/" + cryptoService.Parameters.CoinLongName + ":" + version + "/")).ToList();

            var addresses = peerInfo.Select(p =>
            {
                var uri = new Uri("http://" + p.Addr);
                string host = uri.Host;
                host = host.Replace("[", "");
                host = host.Replace("]", "");
                return host;
            })
            .Where(addr => !string.IsNullOrEmpty(addr))
            .ToList();

            if (addresses.Count.Equals(0))
                return Ok($"No peers of version {version} connected to my {coinName} wallet");

            string peerssResponse = string.Empty;

            addresses.ForEach(ad =>
            {
                peerssResponse += "addnode=" + Regex.Replace(ad, @"\s+", "") + Environment.NewLine;
            });

            return Content(peerssResponse);
        }

        [HttpPost("[action]")]
        public IActionResult GetDailyPeerList([FromBody] string[] parameters)
        {
            string coinName;
            string version;
            if (string.IsNullOrEmpty(parameters[0]))
                return BadRequest("No coin name provided");
            else
                coinName = parameters[0].ToUpper();

            if (string.IsNullOrEmpty(parameters[1]))
                return BadRequest("No coin version provided");
            else
                version = parameters[1];

            if (!_cryptoCoinServices.Any(svc => svc.Parameters.CoinShortName.Equals(coinName)))
                return BadRequest($"This service node does not have {coinName} configured");

            try
            {
                List<Peer> peers;
                if (parameters[0].Equals("0"))
                    peers = _repository.GetPeers(coinName, version, all: true);

                else
                    peers = _repository.GetPeers(coinName, version);

                if (peers.Count.Equals(0))
                    return Ok($"No peers of version {version} connected to my {coinName} wallet");

                string peerssResponse = string.Empty;

                peers.ForEach(ad =>
                {
                    peerssResponse += "addnode=" + Regex.Replace(ad.Address, @"\s+", "") + Environment.NewLine;
                });

                return Content(peerssResponse);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
