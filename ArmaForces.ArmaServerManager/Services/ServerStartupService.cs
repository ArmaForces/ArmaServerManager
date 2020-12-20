using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Modset;
using ArmaForces.ArmaServerManager.Clients.Missions;
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

        public async Task<Result> StartServerForMission(string missionTitle, CancellationToken cancellationToken)
        {
            var mission = _apiMissionsClient.GetUpcomingMissions()
                .Single(x => x.Title == missionTitle);

            return await StartServer(mission.Modlist, cancellationToken);
        }

        public async Task<Result> StartServer(string modsetName, CancellationToken cancellationToken)
        {
            return await _modsetProvider.GetModsetByName(modsetName)
                .Bind(modset => StartServer(modset, cancellationToken));
        }

        public async Task<Result> StartServer(IModset modset, CancellationToken cancellationToken)
        {
            await _modsUpdateService.UpdateModset(modset, cancellationToken);

            var server = _serverProvider.GetServer(Port, modset);

            return server.IsServerStopped
                ? server.Start()
                : server.Modset == modset 
                    ? Result.Success()
                    : Result.Failure($"Server is already running with {server.Modset.Name} modset on port {Port}.");
        }

        public async Task<Result> ShutdownServer(int port, bool force, CancellationToken cancellationToken)
        {
            var server = _serverProvider.GetServer(port);

            if (server is null) return Result.Success();

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
