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
        private readonly ICryptocoinService _cryptoCoinService;
        private readonly IPeerListRepository _repository;

        public PeersController(ICryptocoinService cryptoCoinService, IPeerListRepository repository)
        {
            _cryptoCoinService = cryptoCoinService;
            _repository = repository;
        }

        [HttpGet("[action]")]
        public IActionResult GetPeerList(string version)
        {
            var peerInfo = _cryptoCoinService.GetPeerInfo();

            if (!version.Equals("0"))
                peerInfo = peerInfo.Where(p => p.SubVer.Equals("/" + _cryptoCoinService.Parameters.CoinLongName + ":" + version + "/")).ToList();

            var addresses = peerInfo.Select(p => p.Addr.Split(":")[0]).ToList();

            if (addresses.Count.Equals(0))
                return Ok($"No peers of version {version} connected to my wallet");

            string peerssResponse = string.Empty;

            addresses.ForEach(ad =>
            {
                peerssResponse += "addnode=" + ad + Environment.NewLine;
            });
            return Ok(peerssResponse);
        }

        [HttpGet("[action]")]
        public IActionResult GetDailyPeerList(string version)
        {
            if (string.IsNullOrEmpty(version))
                return BadRequest();

            try
            {
                List<Peer> peers;
                if (version.Equals("0"))
                    peers = _repository.GetPeers(version, all: true);

                else
                    peers = _repository.GetPeers(version);

                if (peers.Count.Equals(0))
                    return Ok($"No peers of version {version} connected to my wallet");

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
