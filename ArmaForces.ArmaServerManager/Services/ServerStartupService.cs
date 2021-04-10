using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.ArmaServerManager.Features.Missions;
using ArmaForces.ArmaServerManager.Providers;
using ArmaForces.ArmaServerManager.Providers.Server;
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
            var upcomingMissions = _apiMissionsClient.GetUpcomingMissions();
            if (upcomingMissions.IsFailure) return Result.Failure("Could not retrieve upcoming missions.");

            var mission = upcomingMissions.Value
                .Single(x => x.Title == missionTitle);

            return await StartServer(mission.Modlist, cancellationToken);
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

        public async Task<Result> ShutdownServer(int port, bool force, CancellationToken cancellationToken)
        {
            var server = _serverProvider.GetServer(port);

            if (server is null || server.IsServerStopped) return Result.Success();

            var serverStatus = await server.GetServerStatusAsync(cancellationToken);

            if (serverStatus.Players != 0 && !force)
            {
                return Result.Failure($"Server cannot be shut down, there are {serverStatus.Players} online.");
            }

            server.Shutdown();
            return Result.Success();
        }
    }
}
