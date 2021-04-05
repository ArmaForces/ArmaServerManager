using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.ArmaServerManager.Features.Missions;
using ArmaForces.ArmaServerManager.Features.Missions.Extensions;
using ArmaForces.ArmaServerManager.Features.Mods;
using ArmaForces.ArmaServerManager.Features.Modsets;
using ArmaForces.ArmaServerManager.Features.Modsets.DTOs;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Services
{
    public class MissionPreparationService : IMissionPreparationService
    {
        private readonly IApiMissionsClient _apiMissionsClient;
        private readonly IApiModsetClient _apiModsetClient;
        private readonly IWebModsetMapper _webModsetMapper;
        private readonly IServerStartupService _serverStartupService;
        private readonly IModsUpdateService _modsUpdateService;

        public MissionPreparationService(
            IApiMissionsClient apiMissionsClient,
            IApiModsetClient apiModsetClient,
            IWebModsetMapper webModsetMapper,
            IServerStartupService serverStartupService,
            IModsUpdateService modsUpdateService)
        {
            _apiMissionsClient = apiMissionsClient;
            _apiModsetClient = apiModsetClient;
            _webModsetMapper = webModsetMapper;
            _serverStartupService = serverStartupService;
            _modsUpdateService = modsUpdateService;
        }

        /// <inheritdoc />
        public async Task<Result> PrepareForUpcomingMissions(CancellationToken cancellationToken)
        {
            return await _apiMissionsClient.GetUpcomingMissionsModsetsNames()
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

        private Result<ISet<IMod>> GetModsListFromModsets(IEnumerable<string> modsetsNames)
        {
            return GetModsetsData(modsetsNames)
                .Select(x => _webModsetMapper.MapWebModsetToCacheModset(x))
                .Select(x => x.Mods)
                .SelectMany(x => x)
                .ToHashSet(new ModEqualityComparer());
        }

        private ISet<WebModset> GetModsetsData(IEnumerable<string> modsetsNames)
        {
            return modsetsNames
                .Select(modsetName => _apiModsetClient.GetModsetDataByName(modsetName))
                .ToHashSet();
        }
    }
}
