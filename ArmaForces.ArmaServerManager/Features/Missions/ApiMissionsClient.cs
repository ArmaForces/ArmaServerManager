using System;
using System.Collections.Generic;
using System.Linq;
using ArmaForces.Arma.Server.Config;
using ArmaForces.ArmaServerManager.Extensions;
using ArmaForces.ArmaServerManager.Features.Missions.DTOs;
using ArmaForces.ArmaServerManager.Features.Modsets.DTOs;
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

        public ApiMissionsClient(ISettings settings):this(settings.ApiMissionsBaseUrl){}

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
        public IEnumerable<WebMission> GetUpcomingMissions()
            => ApiMissionsUpcoming();

        /// <inheritdoc />
        public ISet<WebModset> GetUpcomingMissionsModsets()
            => GetUpcomingMissions()
                .GroupBy(x => x.Modlist)
                .Select(x => x.First())
                .Select(x => new WebModset{ Name = x.Modlist })
                .ToHashSet();

        private IEnumerable<WebMission> ApiMissionsUpcoming() {
            var requestUri = string.Format(MissionsUpcomingResourceFormat, DateTime.Today);
            var request = new RestRequest(requestUri);
            return _restClient.ExecuteAndReturnData<List<WebMission>>(request);
        }
    }
}