using System;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Servers.DTOs;
using ArmaForces.ArmaServerManager.Api.Servers.DTOs;
using ArmaForces.ArmaServerManager.Features.Hangfire;
using ArmaForces.ArmaServerManager.Infrastructure.Authentication;
using ArmaForces.ArmaServerManager.Providers.Server;
using ArmaForces.ArmaServerManager.Services;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;

namespace ArmaForces.ArmaServerManager.Api.Servers
{
    [Route("api/server")]
    [ApiController]
    public class ServersController : ControllerBase
    {
        private readonly IHangfireManager _hangfireManager;
        private readonly IServerProvider _serverProvider;

        public ServersController(
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

        [Obsolete("Use {port}/start route.")]
        [HttpPost]
        [Route("Start")]
        [ApiKey]
        public IActionResult StartServer([FromBody] ServerStartRequestDto startRequestDto)
            => StartServer(startRequestDto.Port, startRequestDto);

        [HttpPost]
        [Route("{port}/start")]
        [ApiKey]
        public IActionResult StartServer(int port, [FromBody] ServerStartRequestDto serverStartRequestDto)
        {
            var serverShutdownJob = _hangfireManager
                .ScheduleJob<ServerStartupService>(
                    x => x.ShutdownServer(
                        port,
                        serverStartRequestDto.ForceRestart ?? false,
                        CancellationToken.None),
                    serverStartRequestDto.ScheduleAt);

            if (serverShutdownJob.IsFailure) return Problem(serverShutdownJob.Error);

            var result = _hangfireManager.ContinueJobWith<ServerStartupService>(
                serverShutdownJob.Value,
                x => x.StartServer(serverStartRequestDto.ModsetName, CancellationToken.None));

            return result.Match(
                onSuccess: Accepted,
                onFailure: error => (IActionResult)BadRequest(error));
        }

        [HttpPost]
        [Route("{port}/restart")]
        [ApiKey]
        public IActionResult RestartServer(int port, ServerRestartRequestDto serverRestartRequestDto)
        {
            var server = _serverProvider.GetServer(port);

            if (server is null) return BadRequest("Server is not running and cannot be restarted.");

            var modsetName = server.Modset;

            var serverShutdownJob = _hangfireManager
                .ScheduleJob<ServerStartupService>(
                    x => x.ShutdownServer(
                        port,
                        serverRestartRequestDto.ForceRestart ?? false,
                        CancellationToken.None),
                    serverRestartRequestDto.ScheduleAt);

            if (serverShutdownJob.IsFailure) return Problem(serverShutdownJob.Error);

            var result = _hangfireManager.ContinueJobWith<ServerStartupService>(
                serverShutdownJob.Value,
                x => x.StartServer(modsetName, CancellationToken.None));

            return result.Match(
                onSuccess: Accepted,
                onFailure: error => (IActionResult)BadRequest(error));
        }

        [HttpPost]
        [Route("{port}/shutdown")]
        [ApiKey]
        public IActionResult ShutdownServer(int port)
        {
            var serverShutdownJob = _hangfireManager
                .ScheduleJob<ServerStartupService>(
                    x => x.ShutdownServer(
                        port,
                        false,
                        CancellationToken.None));

            return serverShutdownJob.IsSuccess
                ? Accepted()
                : Problem(serverShutdownJob.Error);
        }
    }
}
