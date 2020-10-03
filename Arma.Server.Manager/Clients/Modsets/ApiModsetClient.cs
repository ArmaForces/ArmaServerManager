using Arma.Server.Manager.Clients.Modsets.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using RestSharp;

namespace Arma.Server.Manager.Clients.Modsets {
    /// <inheritdoc />
    public class ApiModsetClient : IApiModsetClient {
        private readonly IRestClient _restClient;

        /// <inheritdoc cref="ApiModsetClient" />
        /// <param name="restClient">Client used for connections.</param>
        public ApiModsetClient(IRestClient restClient) {
            _restClient = restClient;
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

        /// <summary>
        /// Executes REST request and converts response content to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Expected response content.</typeparam>
        /// <param name="request">Request to execute.</param>
        /// <exception cref="HttpRequestException">Thrown when status code is not OK.</exception>
        /// <returns><typeparamref name="T"/></returns>
        internal T ExecuteRequest<T>(IRestRequest request) where T : new() {
            var response = _restClient.Execute<T>(request);
            return response.StatusCode == HttpStatusCode.OK
                ? response.Data
                : throw new HttpRequestException(response.StatusCode.ToString());
        }

        private WebModset ApiModsetById(string id) {
            var requestUri = $"api/mod-lists/{id}";
            var request = new RestRequest(requestUri, Method.GET, DataFormat.Json);
            return ExecuteRequest<WebModset>(request);
        }

        private WebModset ApiModsetByName(string name) {
            var requestUri = $"api/mod-lists/by-name/{name}";
            var request = new RestRequest(requestUri, Method.GET, DataFormat.Json);
            return ExecuteRequest<WebModset>(request);
        }

        private List<WebModset> ApiModsets() {
            var requestUri = "api/mod-lists";
            var request = new RestRequest(requestUri, Method.GET, DataFormat.Json);
            return ExecuteRequest<List<WebModset>>(request);
        }
    }
}