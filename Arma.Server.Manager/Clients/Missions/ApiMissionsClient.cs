using Arma.Server.Manager.Clients.Missions.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Arma.Server.Manager.Clients.Modsets.Entities;

namespace Arma.Server.Manager.Clients.Missions {
    public class ApiMissionsClient : IApiMissionsClient
    {
        public HttpClient HttpClient = new HttpClient();

        /// <inheritdoc cref="ApiMissionsClient"/>
        /// <param name="baseUrl">Base API url.</param>
        public ApiMissionsClient(string baseUrl) : this(new Uri(baseUrl)) {
        }

        /// <inheritdoc cref="ApiMissionsClient"/>
        /// <param name="baseUri">Base API uri.</param>
        public ApiMissionsClient(Uri baseUri) {
            HttpClient.BaseAddress = baseUri;
            HttpClient.DefaultRequestHeaders
                .Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <inheritdoc />
        public IEnumerable<WebMission> GetUpcomingMissions()
            => JsonConvert.DeserializeObject<IEnumerable<WebMission>>(ApiMissionsUpcoming());

        /// <inheritdoc />
        public ISet<WebModset> GetUpcomingMissionsModsets()
            => GetUpcomingMissions()
                .Select(x => new WebModset { Name = x.Modlist })
                .ToHashSet();

        private string ApiMissionsUpcoming() {
            var requestUri = $"api/missions?includeArchive=true&fromDateTime={DateTime.Today}";
            return GetHttpResponseMessage(requestUri).Content.ReadAsStringAsync().Result;
        }

        private HttpResponseMessage GetHttpResponseMessage(string requestUri) {
            var response = HttpClient.GetAsync(requestUri).Result;
            response.EnsureSuccessStatusCode();
            return response;
        }
    }
}