using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Servers.DTOs;
using ArmaForces.ArmaServerManager.Api.Servers.DTOs;
using ArmaForces.ArmaServerManager.Common;
using ArmaForces.ArmaServerManager.Features.Jobs;
using ArmaForces.ArmaServerManager.Features.Servers;
using ArmaForces.ArmaServerManager.Infrastructure.Authentication;
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
    [ApiController]
    public class ServersController : ManagerControllerBase
    {
        private readonly IJobsScheduler _jobsScheduler;
        private readonly IServerQueryLogic _serverQueryLogic;
        private readonly IServerCommandLogic _serverCommandLogic;

        /// <inheritdoc />
        public ServersController(
            IJobsScheduler jobsScheduler,
            IServerQueryLogic serverQueryLogic,
            IServerCommandLogic serverCommandLogic)
        {
            _jobsScheduler = jobsScheduler;
            _serverQueryLogic = serverQueryLogic;
            _serverCommandLogic = serverCommandLogic;
        }

        /// <summary>Get Servers Status</summary>
        /// <remarks>Retrieves status of all servers.</remarks>
        [HttpGet(Name = nameof(GetServersStatus))]
        [ProducesResponseType(typeof(ServerStatus), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetServersStatus()
        {
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            var serversStatus = (await _serverQueryLogic.GetAllServersStatus(cancellationTokenSource.Token))
                .Select(ServerStatusMapper.Map)
                .ToList();
            
            return Ok(serversStatus);
        }

        /// <summary>Get Server Status</summary>
        /// <remarks>Retrieves status of server on given <paramref name="port"/>.</remarks>
        /// <param name="port">Port of the server which status will be retrieved.</param>
        [HttpGet("{port:int}", Name = nameof(GetServerStatus))]
        [ProducesResponseType(typeof(ServerStatus), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetServerStatus(int port = 2302)
        {
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            
            var serverStatus = await _serverQueryLogic.GetServerStatus(port, cancellationTokenSource.Token);

            return Ok(ServerStatusMapper.Map(serverStatus));
        }

        /// <summary>StartServer</summary>
        /// <remarks>Starts server according to <paramref name="startRequestDto"/>.</remarks>
        [Obsolete("Use {port}/start route.")]
        [HttpPost("Start", Name = "StartServerObsolete")]
        [ProducesResponseType(typeof(int), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [ApiKey]
        public IActionResult StartServer([FromBody] ServerStartRequestDto startRequestDto)
            => StartServer(startRequestDto.Port, startRequestDto);

        /// <summary>StartServer</summary>
        /// <remarks>Starts server on given <paramref name="port"/>.</remarks>
        [HttpPost("{port}/start", Name = nameof(StartServer))]
        [ProducesResponseType(typeof(int), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(string), StatusCodesExtended.Status425TooEarly)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [ApiKey]
        public IActionResult StartServer(int port, [FromBody] ServerStartRequestDto serverStartRequestDto)
        {
            var result = _jobsScheduler.ScheduleJob<ServerStartupService>(
                    x => x.ShutdownServer(
                        port,
                        serverStartRequestDto.ForceRestart ?? false,
                        CancellationToken.None),
                    serverStartRequestDto.ScheduleAt)
                .Bind(shutdownJobId => _jobsScheduler.ContinueJobWith<ServerStartupService>(
                    shutdownJobId,
                    x => x.StartServer(serverStartRequestDto.ModsetName, serverStartRequestDto.HeadlessClients, CancellationToken.None)));
            
            return result.Match(
                onSuccess: JobAccepted,
                onFailure: TooEarly);
        }

        /// <summary>Set Headless Clients</summary>
        /// <remarks>Starts or stops headless clients for server on given <paramref name="port"/> to match desired count.
        /// Not implemented.</remarks>
        /// <param name="port">Port of the server to start/stop headless clients for.</param>
        /// <param name="headlessSetRequestDto">TODO</param>
        [HttpPatch("{port:int}/headless", Name = nameof(SetHeadlessClients))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [ApiKey]
        public async Task<IActionResult> SetHeadlessClients(int port, HeadlessSetRequestDto headlessSetRequestDto)
        {
            var result = await _serverCommandLogic.SetHeadlessClients(port, headlessSetRequestDto.Count);

            return result.Match(
                onSuccess: NoContent,
                onFailure: error => (IActionResult) BadRequest(error));
        }

        /// <summary>Restart Server</summary>
        /// <remarks>Restarts server on given <paramref name="port"/>.</remarks>
        /// <param name="port">Port of the server to restart.</param>
        /// <param name="serverRestartRequestDto">Additional details.</param>
        [HttpPost("{port:int}/restart", Name = nameof(RestartServer))]
        [ProducesResponseType(typeof(int), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(string), StatusCodesExtended.Status425TooEarly)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [ApiKey]
        public IActionResult RestartServer(int port, ServerRestartRequestDto serverRestartRequestDto)
        {
            var serverGetResult = _serverQueryLogic.GetServer(port);

            if (serverGetResult.IsFailure) return BadRequest("Server is not running and cannot be restarted.");

            var server = serverGetResult.Value;
            
            var modsetName = server.Modset;

            var result = _jobsScheduler
                .ScheduleJob<ServerStartupService>(
                    x => x.ShutdownServer(
                        port,
                        serverRestartRequestDto.ForceRestart ?? false,
                        CancellationToken.None),
                    serverRestartRequestDto.ScheduleAt)
                .Bind(shutdownJobId => _jobsScheduler.ContinueJobWith<ServerStartupService>(
                    shutdownJobId,
                    x => x.StartServer(modsetName, server.HeadlessClientsConnected, CancellationToken.None)));

            return result.Match(
                onSuccess: JobAccepted,
                onFailure: TooEarly);
        }

        /// <summary>Shutdown Server</summary>
        /// <remarks>Shutdowns server on given <paramref name="port"/>.</remarks>
        /// <param name="port">Port of the server to shutdown.</param>
        [HttpPost("{port:int}/shutdown", Name = nameof(ShutdownServer))]
        [ProducesResponseType(typeof(int), StatusCodes.Status202Accepted)]
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
