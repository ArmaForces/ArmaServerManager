using System;
using System.Collections.Generic;
using System.Linq;
using ArmaForces.Arma.Server.Config;
using ArmaForces.ArmaServerManager.Extensions;
using ArmaForces.ArmaServerManager.Features.Missions.DTOs;
using CSharpFunctionalExtensions;
using RestSharp;

namespace ArmaForces.ArmaServerManager.Features.Missions {
    /// <inheritdoc />
    public class ApiMissionsClient : IApiMissionsClient {
        // TODO: it should not be like this
        private const string MissionsUpcomingResourceFormat = @"api/missions?includeArchive=true&fromDateTime={0}";

        private readonly IRestClient _restClient;

        /// <inheritdoc cref="ApiMissionsClient"/>
        /// <param name="restClient">SteamClient used for connections.</param>
        public ApiMissionsClient(IRestClient restClient)
        {
            _restClient = restClient;
        }

        // TODO: Handle no missions URL
        public ApiMissionsClient(ISettings settings):this(settings.ApiMissionsBaseUrl!){}

        /// <inheritdoc cref="ApiMissionsClient"/>
        /// <param name="baseUrl">Base API url.</param>
        private ApiMissionsClient(string baseUrl) : this(new Uri(baseUrl)) {
        }

        /// <inheritdoc cref="ApiMissionsClient"/>
        /// <param name="baseUri">Base API uri.</param>
        private ApiMissionsClient(Uri baseUri) {
            _restClient = new RestClient(baseUri);
        }

        /// <inheritdoc />
        public Result<List<WebMission>> GetUpcomingMissions()
            => ApiMissionsUpcoming();

        /// <inheritdoc />
        public Result<ISet<string>> GetUpcomingMissionsModsetsNames()
        {
            var upcomingMissionsResult = GetUpcomingMissions();
            if (upcomingMissionsResult.IsFailure)
                return Result.Failure<ISet<string>>("Missions could not be retrieved.");

            return upcomingMissionsResult.Value
                .GroupBy(x => x.Modlist)
                .Select(x => x.First())
                .Select(x => x.Modlist)
                .ToHashSet();
        }

        private Result<List<WebMission>> ApiMissionsUpcoming() {
            var requestUri = string.Format(MissionsUpcomingResourceFormat, DateTime.Today.ToString("s"));
            var request = new RestRequest(requestUri);
            return _restClient.ExecuteAndReturnResult<List<WebMission>>(request);
        }
    }
}