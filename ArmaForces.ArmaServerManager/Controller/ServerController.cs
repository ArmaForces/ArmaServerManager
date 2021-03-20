using System;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Servers.DTOs;
using ArmaForces.ArmaServerManager.Features.Hangfire;
using ArmaForces.ArmaServerManager.Features.Server.DTOs;
using ArmaForces.ArmaServerManager.Infrastructure.Authentication;
using ArmaForces.ArmaServerManager.Providers.Server;
using ArmaForces.ArmaServerManager.Services;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ArmaForces.ArmaServerManager.Controller
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
        public async Task<IActionResult> GetServerStatus(int port = 2302)
        {
            var server = _serverProvider.GetServer(port);

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            var serverStatus = server is null
                ? new ServerStatus()
                : await server.GetServerStatusAsync(cancellationTokenSource.Token);

            return Ok(serverStatus);
        }

        [HttpPost]
        [Route("Start")]
        [ApiKey]
        public IActionResult StartServer([FromBody] ServerStartRequest startRequest)
        {
            var serverShutdownJob = _hangfireManager
                .ScheduleJob<ServerStartupService>(
                    x => x.ShutdownServer(
                        2302,
                        false,
                        CancellationToken.None),
                    startRequest.ScheduleAt);

            if (serverShutdownJob.IsFailure)
                return Problem(serverShutdownJob.Error);

            var result = _hangfireManager.ContinueJobWith<ServerStartupService>(
                serverShutdownJob.Value,
                x => x.StartServer(startRequest.ModsetName, CancellationToken.None));

            return result.Match(
                onSuccess: Accepted,
                onFailure: error => (IActionResult) BadRequest(error));
        }
    }
}
