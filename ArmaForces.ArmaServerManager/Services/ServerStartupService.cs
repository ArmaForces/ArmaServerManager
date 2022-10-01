using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.ArmaServerManager.Features.Missions;
using ArmaForces.ArmaServerManager.Features.Missions.DTOs;
using ArmaForces.ArmaServerManager.Features.Modsets;
using ArmaForces.ArmaServerManager.Features.Servers;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Services
{
    /// <summary>
    /// TODO: create documentation
    /// </summary>
    public class ServerStartupService : IServerStartupService
    {
        private const int Port = 2302;

        private readonly IApiMissionsClient _apiMissionsClient;
        private readonly IModsetProvider _modsetProvider;
        private readonly IServerProvider _serverProvider;
        private readonly IModsUpdateService _modsUpdateService;

        public ServerStartupService(
            IApiMissionsClient apiMissionsClient,
            IModsetProvider modsetProvider,
            IServerProvider serverProvider,
            IModsUpdateService modsUpdateService)
        {
            _apiMissionsClient = apiMissionsClient;
            _modsetProvider = modsetProvider;
            _serverProvider = serverProvider;
            _modsUpdateService = modsUpdateService;
        }

        // TODO: Add port
        public async Task<Result> StartServerForMission(string missionTitle, CancellationToken cancellationToken)
        {
            return await _apiMissionsClient.GetUpcomingMissions()
                .Bind(upcomingMissions => GetMissionWithTitle(missionTitle, upcomingMissions))
                .Bind(x => StartServer(x.Modlist, cancellationToken))
                .OnFailure(error => Result.Failure($"Could not start server for mission {missionTitle}, error: {error}"));
        }

        // TODO: Add port

        public async Task<Result> StartServer(string modsetName, CancellationToken cancellationToken)
        {
            return await _modsetProvider.GetModsetByName(modsetName)
                .Bind(modset => StartServer(modset, cancellationToken));
        }

        // TODO: Add port

        public async Task<Result> StartServer(IModset modset, CancellationToken cancellationToken)
        {
            return await ShutdownServer(
                Port,
                false,
                cancellationToken)
                //.Bind(() => _modsUpdateService.UpdateModset(modset, cancellationToken))
                .Bind(() => _serverProvider.GetServer(Port, modset).Start());
        }

        public async Task<Result> ShutdownServer(
            int port,
            bool force = false,
            CancellationToken? cancellationToken = null)
        {
            var server = _serverProvider.GetServer(port);

            if (server is null || server.IsServerStopped) return Result.Success();

            var serverStatus = await server.GetServerStatusAsync(cancellationToken ?? CancellationToken.None);

            if (serverStatus.Players.HasValue && serverStatus.Players != 0 && !force)
            {
                return Result.Failure($"Server cannot be shut down, there are {serverStatus.Players} online.");
            }

            return await server.Shutdown();
        }

        private static Result<WebMission> GetMissionWithTitle(string missionTitle, IReadOnlyCollection<WebMission> upcomingMissions)
        {
            return upcomingMissions.SingleOrDefault(x => x.Title == missionTitle) ??
                Result.Failure<WebMission>($"Mission {missionTitle} not found.");
        }
    }
}
