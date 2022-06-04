using System;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Servers.DTOs;
using ArmaForces.ArmaServerManager.Api.Servers.DTOs;
using ArmaForces.ArmaServerManager.Features.Jobs;
using ArmaForces.ArmaServerManager.Infrastructure.Authentication;
using ArmaForces.ArmaServerManager.Providers.Server;
using ArmaForces.ArmaServerManager.Services;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ArmaForces.ArmaServerManager.Api.Servers
{
    /// <summary>
    /// Allows server startup, shutdown and status retrieval.
    /// </summary>
    [Route("api/server")]
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    public class ServersController : ControllerBase
    {
        private readonly IJobsScheduler _jobsScheduler;
        private readonly IServerProvider _serverProvider;

        /// <inheritdoc />
        public ServersController(
            IJobsScheduler jobsScheduler,
            IServerProvider serverProvider)
        {
            _jobsScheduler = jobsScheduler;
            _serverProvider = serverProvider;
        }

        /// <summary>Get Server Status</summary>
        /// <remarks>Retrieves status of server on given <paramref name="port"/>.</remarks>
        /// <param name="port">Port of the server which status will be retrieved.</param>
        [HttpGet("{port:int}", Name = nameof(GetServerStatus))]
        [ProducesResponseType(typeof(ServerStatus), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetServerStatus(int port = 2302)
        {
            var server = _serverProvider.GetServer(port);

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            var serverStatus = server is null
                ? new ServerStatus()
                : await server.GetServerStatusAsync(cancellationTokenSource.Token);

            return Ok(ServerStatusMapper.Map(serverStatus));
        }

        /// <summary>StartServer</summary>
        /// <remarks>Starts server according to <paramref name="startRequestDto"/>.</remarks>
        [Obsolete("Use {port}/start route.")]
        [HttpPost("Start", Name = "StartServerObsolete")]
        [ProducesResponseType(typeof(string), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [ApiKey]
        public IActionResult StartServer([FromBody] ServerStartRequestDto startRequestDto)
            => StartServer(startRequestDto.Port, startRequestDto);

        /// <summary>StartServer</summary>
        /// <remarks>Starts server on given <paramref name="port"/>.</remarks>
        [HttpPost("{port}/start", Name = nameof(StartServer))]
        [ProducesResponseType(typeof(string), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [ApiKey]
        public IActionResult StartServer(int port, [FromBody] ServerStartRequestDto serverStartRequestDto)
        {
            var serverShutdownJob = _jobsScheduler
                .ScheduleJob<ServerStartupService>(
                    x => x.ShutdownServer(
                        port,
                        serverStartRequestDto.ForceRestart ?? false,
                        CancellationToken.None),
                    serverStartRequestDto.ScheduleAt);

            if (serverShutdownJob.IsFailure) return Problem(serverShutdownJob.Error);

            var result = _jobsScheduler.ContinueJobWith<ServerStartupService>(
                serverShutdownJob.Value,
                x => x.StartServer(serverStartRequestDto.ModsetName, CancellationToken.None));

            return result.Match(
                onSuccess: Accepted,
                onFailure: error => (IActionResult)BadRequest(error));
        }

        /// <summary>Start Headless Client</summary>
        /// <remarks>Starts headless client for server on given <paramref name="port"/>. Not implemented.</remarks>
        /// <param name="port">Port of the server to start headless client for.</param>
        [HttpPost("{port:int}/headless/start", Name = nameof(StartHeadlessClient))]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        [ApiKey]
        public IActionResult StartHeadlessClient(int port) => throw new NotImplementedException("Starting headless client is not supported yet.");

        /// <summary>Restart Server</summary>
        /// <remarks>Restarts server on given <paramref name="port"/>.</remarks>
        /// <param name="port">Port of the server to restart.</param>
        /// <param name="serverRestartRequestDto">Additional details.</param>
        [HttpPost("{port:int}/restart", Name = nameof(RestartServer))]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [ApiKey]
        public IActionResult RestartServer(int port, ServerRestartRequestDto serverRestartRequestDto)
        {
            var server = _serverProvider.GetServer(port);

            if (server is null) return BadRequest("Server is not running and cannot be restarted.");

            var modsetName = server.Modset;

            var serverShutdownJob = _jobsScheduler
                .ScheduleJob<ServerStartupService>(
                    x => x.ShutdownServer(
                        port,
                        serverRestartRequestDto.ForceRestart ?? false,
                        CancellationToken.None),
                    serverRestartRequestDto.ScheduleAt);

            if (serverShutdownJob.IsFailure) return Problem(serverShutdownJob.Error);

            var result = _jobsScheduler.ContinueJobWith<ServerStartupService>(
                serverShutdownJob.Value,
                x => x.StartServer(modsetName, CancellationToken.None));

            return result.Match(
                onSuccess: Accepted,
                onFailure: error => (IActionResult)BadRequest(error));
        }

        /// <summary>Shutdown Headless Client</summary>
        /// <remarks>Shutdowns all headless clients for server on given <paramref name="port"/>. Not implemented.</remarks>
        /// <param name="port">Port of the server to shutdown headless client for.</param>
        [HttpPost("{port:int}/headless/shutdown", Name = nameof(ShutdownHeadlessClients))]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        [ApiKey]
        public IActionResult ShutdownHeadlessClients(int port)
            => throw new NotImplementedException("Shutting down headless clients is not supported yet.");

        /// <summary>Shutdown Server</summary>
        /// <remarks>Shutdowns server on given <paramref name="port"/>.</remarks>
        /// <param name="port">Port of the server to shutdown.</param>
        [HttpPost("{port:int}/shutdown", Name = nameof(ShutdownServer))]
        [ProducesResponseType(typeof(string), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [ApiKey]
        public IActionResult ShutdownServer(int port)
        {
            var serverShutdownJob = _jobsScheduler
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
