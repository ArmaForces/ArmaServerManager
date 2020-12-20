using System;
using System.Collections.Generic;
using ArmaForces.Arma.Server.Config;
using ArmaForces.ArmaServerManager.Clients.Extensions;
using ArmaForces.ArmaServerManager.Clients.Modsets.Entities;
using RestSharp;

namespace ArmaForces.ArmaServerManager.Clients.Modsets {
    /// <inheritdoc />
    public class ApiModsetClient : IApiModsetClient {
        private readonly IRestClient _restClient;

        /// <inheritdoc cref="ApiModsetClient" />
        /// <param name="restClient">SteamClient used for connections.</param>
        public ApiModsetClient(IRestClient restClient) {
            _restClient = restClient;
        }

        public ApiModsetClient(ISettings settings) : this(settings.ApiModsetsBaseUrl)
        {
        }

        /// <inheritdoc cref="ApiModsetClient" />
        /// <param name="baseUrl">Base API url.</param>
        public ApiModsetClient(string baseUrl) : this(new Uri(baseUrl)) {
        }

        /// <inheritdoc cref="ApiModsetClient" />
        /// /// <param name="baseUri">Base API uri.</param>
        public ApiModsetClient(Uri baseUri) {
            _restClient = new RestClient(baseUri);
        }

        /// <inheritdoc />
        public List<WebModset> GetModsets()
            => ApiModsets();

        /// <inheritdoc />
        public WebModset GetModsetDataByName(string name)
            => ApiModsetByName(name);

        /// <inheritdoc />
        public WebModset GetModsetDataByModset(WebModset webModset)
            => GetModsetDataById(webModset.Id);

        /// <inheritdoc />
        public WebModset GetModsetDataById(string id)
            => ApiModsetById(id);

        private WebModset ApiModsetById(string id) {
            var requestUri = $"api/mod-lists/{id}";
            var request = new RestRequest(requestUri, Method.GET, DataFormat.Json);
            return _restClient.ExecuteAndReturnData<WebModset>(request);
        }

        private WebModset ApiModsetByName(string name) {
            var requestUri = $"api/mod-lists/by-name/{name}";
            var request = new RestRequest(requestUri, Method.GET, DataFormat.Json);
            return _restClient.ExecuteAndReturnData<WebModset>(request);
        }

        private List<WebModset> ApiModsets() {
            var requestUri = "api/mod-lists";
            var request = new RestRequest(requestUri, Method.GET, DataFormat.Json);
            return _restClient.ExecuteAndReturnData<List<WebModset>>(request);
        }
    }
}