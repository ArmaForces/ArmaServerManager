using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arma.Server.Manager.Clients.Missions;
using CSharpFunctionalExtensions;

namespace Arma.Server.Manager.Services
{
    public class MissionPreparationService
    {
        private readonly IApiMissionsClient _apiMissionsClient;
        private readonly ServerStartupService _serverStartupService;
        private readonly IModsUpdateService _modsUpdateService;

        public MissionPreparationService(
            IApiMissionsClient apiMissionsClient,
            ServerStartupService serverStartupService,
            IModsUpdateService modsUpdateService)
        {
            _apiMissionsClient = apiMissionsClient;
            _serverStartupService = serverStartupService;
            _modsUpdateService = modsUpdateService;
        }

        public async Task<Result> PrepareForUpcomingMissions(CancellationToken cancellationToken)
        {
            var modsetsToPrepare = _apiMissionsClient.GetUpcomingMissionsModsets();
            foreach (var webModset in modsetsToPrepare)
            {
                await _modsUpdateService.UpdateModset(webModset.Name, cancellationToken);
            }

            return Result.Success();
        }

        public async Task<Result> StartServerForNearestMission(CancellationToken cancellationToken)
        {
            var upcomingMission = _apiMissionsClient.GetUpcomingMissions().FirstOrDefault();
            
            if (upcomingMission is null) return Result.Success();

            return await _serverStartupService.StartServer(upcomingMission.Modlist, cancellationToken);
        }
    }
}
