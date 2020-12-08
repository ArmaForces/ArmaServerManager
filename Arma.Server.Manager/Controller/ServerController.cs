using System.Net;
using System.Net.Sockets;
using System.Threading;
using Arma.Server.Features.Server;
using Arma.Server.Features.Server.DTOs;
using Arma.Server.Manager.Features.Hangfire;
using Arma.Server.Manager.Features.Server.DTOs;
using Arma.Server.Manager.Features.Steam;
using Arma.Server.Manager.Infrastructure.Authentication;
using Arma.Server.Manager.Providers.Server;
using Arma.Server.Manager.Services;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Arma.Server.Manager.Controller
{
    [Route("api/Server")]
    [ApiController]
    public class ServerController : ControllerBase
    {
        private readonly IHangfireManager _hangfireManager;
        private readonly IServerProvider _serverProvider;

        public ServerController(
            IHangfireManager hangfireManager,
            IServerProvider serverProvider)
        {
            _hangfireManager = hangfireManager;
            _serverProvider = serverProvider;
        }

        [HttpGet]
        [Route("{port}")]
        public IActionResult GetServerStatus(int port = 2302)
        {
            var server = _serverProvider.GetServer(port);

            var serverStatus = server?.ServerStatus ?? new ServerStatus();

            return Ok(serverStatus);
        }

        [HttpPost]
        [Route("Start")]
        [ApiKey]
        public IActionResult StartServer([FromBody] ServerStartRequest startRequest)
        {
            var result = _hangfireManager.ScheduleJob<ServerStartupService>(
                x => x.StartServer(startRequest.ModsetName, CancellationToken.None),
                startRequest.ScheduleAt);

            return result.Match(
                onSuccess: Ok,
                onFailure: error => (IActionResult) BadRequest(error));
        }
    }
}
