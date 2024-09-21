using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Common.Errors;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.ArmaServerManager.Api.Servers.DTOs;
using ArmaForces.ArmaServerManager.Api.Status.DTOs;
using ArmaForces.ArmaServerManager.Features.Missions;
using ArmaForces.ArmaServerManager.Features.Missions.DTOs;
using ArmaForces.ArmaServerManager.Features.Modsets;
using ArmaForces.ArmaServerManager.Features.Servers;
using ArmaForces.ArmaServerManager.Features.Status;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace ArmaForces.ArmaServerManager.Services
{
    /// <summary>
    /// TODO: create documentation
    /// </summary>
    public class ServerStartupService : IServerStartupService
    {
        private const int Port = 2302;

        private readonly IAppStatusStore _appStatusStore;
        private readonly IApiMissionsClient _apiMissionsClient;
        private readonly IModsetProvider _modsetProvider;
        private readonly IServerCommandLogic _serverCommandLogic;
        private readonly IModsUpdateService _modsUpdateService;
        private readonly ILogger<ServerStartupService> _logger;

        public ServerStartupService(
            IAppStatusStore appStatusStore,
            IApiMissionsClient apiMissionsClient,
            IModsetProvider modsetProvider,
            IServerCommandLogic serverCommandLogic,
            IModsUpdateService modsUpdateService,
            ILogger<ServerStartupService> logger)
        {
            _appStatusStore = appStatusStore;
            _apiMissionsClient = apiMissionsClient;
            _modsetProvider = modsetProvider;
            _serverCommandLogic = serverCommandLogic;
            _modsUpdateService = modsUpdateService;
            _logger = logger;
        }

        // TODO: Add port
        public async Task<UnitResult<IError>> StartServerForMission(string missionTitle, CancellationToken cancellationToken)
        {
            return await _apiMissionsClient.GetUpcomingMissions()
                .Bind(upcomingMissions => GetMissionWithTitle(missionTitle, upcomingMissions))
                .Bind(x => StartServer(x.Modlist, ServerStartRequestDto.DefaultHeadlessClients, cancellationToken))
                .TapError(error => Result.Failure($"Could not start server for mission {missionTitle}, error: {error}"));
        }

        // TODO: Add port

        public async Task<UnitResult<IError>> StartServer(
            string modsetName,
            int headlessClients,
            CancellationToken cancellationToken)
        {
            return await _modsetProvider.GetModsetByName(modsetName)
                .Bind(modset => StartServer(modset, headlessClients, cancellationToken));
        }

        // TODO: Add port

        public async Task<UnitResult<IError>> StartServer(Modset modset, int headlessClients, CancellationToken cancellationToken)
        {
            IDisposable? appStatusChanges = null;

            try
            {
                return await ShutdownServer(
                        Port,
                        false,
                        cancellationToken)
                    .Tap(() => appStatusChanges = _appStatusStore.SetAppStatus(AppStatus.UpdatingMods, $"Updating mods for server with '{modset.Name}' modset"))
                    .Bind(() => _modsUpdateService.UpdateModset(modset, cancellationToken))
                    .Tap(() => appStatusChanges?.Dispose())
                    .Bind(() => _serverCommandLogic.StartServer(Port, headlessClients, modset))
                    .Tap(() => _logger.LogInformation("Successfully started server on {Port} port with {ModsetName} modset", Port, modset.Name));
            }
            finally
            {
                appStatusChanges?.Dispose();
            }
        }

        public async Task<UnitResult<IError>> ShutdownServer(
            int port,
            bool force = false,
            CancellationToken? cancellationToken = null)
            => await _serverCommandLogic.ShutdownServer(port, force, cancellationToken);

        public async Task<UnitResult<IError>> ShutdownAllServers(bool force = false, CancellationToken? cancellationToken = null)
            => await _serverCommandLogic.ShutdownAllServers(force, cancellationToken);

        private static Result<WebMission, IError> GetMissionWithTitle(string missionTitle, IReadOnlyCollection<WebMission> upcomingMissions)
        {
            return upcomingMissions.SingleOrDefault(x => x.Title == missionTitle) ??
                Result.Failure<WebMission, IError>(new Error ($"Mission {missionTitle} not found.", ManagerErrorCode.MissionNotFound));
        }
    }
}
