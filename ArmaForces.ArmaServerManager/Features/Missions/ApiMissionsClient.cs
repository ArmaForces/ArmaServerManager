using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Config;
using ArmaForces.ArmaServerManager.Common;
using ArmaForces.ArmaServerManager.Extensions;
using ArmaForces.ArmaServerManager.Features.Missions.DTOs;
using CSharpFunctionalExtensions;
using RestSharp;

namespace ArmaForces.ArmaServerManager.Features.Missions
{
    /// <inheritdoc cref="IApiMissionsClient" />
    internal class ApiMissionsClient : HttpClientBase, IApiMissionsClient
    {
        // TODO: it should not be like this
        private const string ClientName = "MissionsClient";
        private const string MissionsUpcomingResourceFormat = @"api/missions?includeArchive=true&fromDateTime={0}";

        /// <inheritdoc cref="ApiMissionsClient"/>
        public ApiMissionsClient(
            IHttpClientFactory httpClientFactory,
            ISettings settings)
            : base(
                httpClientFactory,
                // TODO: Handle no missions URL
                settings.ApiMissionsBaseUrl,
                ClientName)
        {
        }

        /// <inheritdoc />
        public async Task<Result<List<WebMission>>> GetUpcomingMissions()
            => await ApiMissionsUpcoming();

        /// <inheritdoc />
        public async Task<Result<HashSet<string>>> GetUpcomingMissionsModsetsNames()
        {
            return await GetUpcomingMissions()
                .Bind(GetMissionModsets)
                .OnFailure(error => Result.Failure<ISet<string>>($"Upcoming missions modsets names could not be retrieved, error: {error}"));
        }

        private Result<HashSet<string>> GetMissionModsets(IReadOnlyCollection<WebMission> missions)
            => missions
                .GroupBy(x => x.Modlist)
                .Select(x => x.First())
                .Select(x => x.Modlist)
                .ToHashSet();

        private async Task<Result<List<WebMission>>> ApiMissionsUpcoming() {
            var requestUri = string.Format(MissionsUpcomingResourceFormat, DateTime.Today.ToString("s"));
            return await HttpGetAsync<List<WebMission>>(requestUri);
        }
    }
}