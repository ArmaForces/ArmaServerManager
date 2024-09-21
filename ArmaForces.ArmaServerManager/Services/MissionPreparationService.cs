using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Common.Errors;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.ArmaServerManager.Api.Servers.DTOs;
using ArmaForces.ArmaServerManager.Extensions;
using ArmaForces.ArmaServerManager.Features.Missions;
using ArmaForces.ArmaServerManager.Features.Missions.Extensions;
using ArmaForces.ArmaServerManager.Features.Mods;
using ArmaForces.ArmaServerManager.Features.Modsets.Client;
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
        private readonly IModsVerificationService _modsVerificationService;

        public MissionPreparationService(
            IApiMissionsClient apiMissionsClient,
            IApiModsetClient apiModsetClient,
            IWebModsetMapper webModsetMapper,
            IServerStartupService serverStartupService,
            IModsUpdateService modsUpdateService,
            IModsVerificationService modsVerificationService)
        {
            _apiMissionsClient = apiMissionsClient;
            _apiModsetClient = apiModsetClient;
            _webModsetMapper = webModsetMapper;
            _serverStartupService = serverStartupService;
            _modsUpdateService = modsUpdateService;
            _modsVerificationService = modsVerificationService;
        }

        /// <inheritdoc />
        public async Task<UnitResult<IError>> PrepareForUpcomingMissions(CancellationToken cancellationToken)
        {
            return await _apiMissionsClient.GetUpcomingMissionsModsetsNames()
                .Bind(GetModsListFromModsets)
                .Tap(x => _modsVerificationService.VerifyMods(x, cancellationToken))
                .Bind(_ => Result.Success<IError>());
        }

        /// <inheritdoc />
        public async Task<UnitResult<IError>> StartServerForNearestMission(CancellationToken cancellationToken)
        {
            return await _apiMissionsClient
                .GetUpcomingMissions()
                .Bind(x => x.GetNearestMission())
                .Bind(nearestMission => _serverStartupService.StartServer(nearestMission.Modlist, ServerStartRequestDto.DefaultHeadlessClients, cancellationToken))
                .Compensate(error => error.Code.Is(ManagerErrorCode.MissionNotFound) 
                    ? UnitResult.Success<IError>()
                    : UnitResult.Failure(error));
        }

        private async Task<Result<HashSet<Mod>, IError>> GetModsListFromModsets(IReadOnlyCollection<string> modsetsNames)
        {
            return await GetModsetsData(modsetsNames)
                .Bind(MapModsets);
        }

        private Result<HashSet<Mod>, IError> MapModsets(ISet<WebModset> modsets) => modsets
            .Select(webModset => _webModsetMapper.MapWebModsetToCacheModset(webModset))
            .Select(modset => modset.ActiveMods)
            .SelectMany(x => x)
            .ToHashSet(new ModEqualityComparer());

        private async Task<Result<HashSet<WebModset>, IError>> GetModsetsData(IReadOnlyCollection<string> modsetsNames)
        {
            return (await modsetsNames
                .ToAsyncEnumerable()
                .SelectAwait(async modsetName => await _apiModsetClient.GetModsetDataByName(modsetName))
                .ToHashSetAsync())
                .Combine()
                .Bind(x => x.ToHashSet().ToResult());
        }
    }
}
