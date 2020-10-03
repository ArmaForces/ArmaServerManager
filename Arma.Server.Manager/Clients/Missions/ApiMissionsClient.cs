using Arma.Server.Manager.Clients.Missions.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Arma.Server.Manager.Clients.Extensions;
using Arma.Server.Manager.Clients.Modsets.Entities;
using RestSharp;
using RestSharp.Deserializers;

namespace Arma.Server.Manager.Clients.Missions {
    /// <inheritdoc />
    public class ApiMissionsClient : IApiMissionsClient {
        private const string MissionsUpcomingResourceFormat = @"api/missions?includeArchive=true&fromDateTime={0}";

        private IRestClient _restClient;

        /// <inheritdoc cref="ApiMissionsClient"/>
        /// <param name="restClient">Client used for connections.</param>
        public ApiMissionsClient(IRestClient restClient)
        {
            _restClient = restClient;
        }

        /// <inheritdoc cref="ApiMissionsClient"/>
        /// <param name="baseUrl">Base API url.</param>
        public ApiMissionsClient(string baseUrl) : this(new Uri(baseUrl)) {
        }

        /// <inheritdoc cref="ApiMissionsClient"/>
        /// <param name="baseUri">Base API uri.</param>
        public ApiMissionsClient(Uri baseUri) {
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