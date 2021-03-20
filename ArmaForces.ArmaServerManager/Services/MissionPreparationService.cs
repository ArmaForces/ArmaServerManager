using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.ArmaServerManager.Features.Missions;
using ArmaForces.ArmaServerManager.Features.Missions.Extensions;
using ArmaForces.ArmaServerManager.Features.Mods;
using ArmaForces.ArmaServerManager.Features.Modsets.DTOs;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Services
{
    public class MissionPreparationService : IMissionPreparationService
    {
        private readonly IApiMissionsClient _apiMissionsClient;
        private readonly IWebModsetMapper _webModsetMapper;
        private readonly IServerStartupService _serverStartupService;
        private readonly IModsUpdateService _modsUpdateService;

        public MissionPreparationService(
            IApiMissionsClient apiMissionsClient,
            IWebModsetMapper webModsetMapper,
            IServerStartupService serverStartupService,
            IModsUpdateService modsUpdateService)
        {
            _apiMissionsClient = apiMissionsClient;
            _webModsetMapper = webModsetMapper;
            _serverStartupService = serverStartupService;
            _modsUpdateService = modsUpdateService;
        }

        /// <inheritdoc />
        public async Task<Result> PrepareForUpcomingMissions(CancellationToken cancellationToken)
        {
            return await _apiMissionsClient.GetUpcomingMissionsModsets()
                .Bind(GetModsListFromModsets)
                .Tap(x => _modsUpdateService.UpdateMods(x, cancellationToken))
                .Bind(_ => Result.Success());
        }

        /// <inheritdoc />
        public async Task<Result> StartServerForNearestMission(CancellationToken cancellationToken)
        {
            return await _apiMissionsClient
                .GetUpcomingMissions()
                .Bind(x => Result.Success(x.GetNearestMission()))
                .Bind(nearestMission => _serverStartupService.StartServer(nearestMission.Modlist, cancellationToken));
        }

        private Result<ISet<IMod>> GetModsListFromModsets(IEnumerable<WebModset> modsets)
        {
            return modsets
                .Select(x => _webModsetMapper.MapWebModsetToCacheModset(x))
                .Select(x => x.Mods)
                .SelectMany(x => x)
                .ToHashSet(new ModEqualityComparer());
        }
    }
}
