using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.ArmaServerManager.Features.Missions;
using ArmaForces.ArmaServerManager.Features.Missions.Extensions;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Services
{
    public class MissionPreparationService : IMissionPreparationService
    {
        private readonly IApiMissionsClient _apiMissionsClient;
        private readonly IServerStartupService _serverStartupService;
        private readonly IModsUpdateService _modsUpdateService;

        public MissionPreparationService(
            IApiMissionsClient apiMissionsClient,
            IServerStartupService serverStartupService,
            IModsUpdateService modsUpdateService)
        {
            _apiMissionsClient = apiMissionsClient;
            _serverStartupService = serverStartupService;
            _modsUpdateService = modsUpdateService;
        }

        /// <inheritdoc />
        public async Task<Result> PrepareForUpcomingMissions(CancellationToken cancellationToken)
        {
            var modsetsToPrepare = _apiMissionsClient.GetUpcomingMissionsModsets();
            foreach (var webModset in modsetsToPrepare)
            {
                await _modsUpdateService.UpdateModset(webModset.Name, cancellationToken);
            }

            return Result.Success();
        }

        /// <inheritdoc />
        public async Task<Result> StartServerForNearestMission(CancellationToken cancellationToken)
        {
            var upcomingMission = _apiMissionsClient
                .GetUpcomingMissions()
                .GetNearestMission();
            
            if (upcomingMission is null) return Result.Success();

            return await _serverStartupService.StartServer(upcomingMission.Modlist, cancellationToken);
        }
    }
}
