using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Arma.Server.Manager.Clients.Modsets.Entities;
using Newtonsoft.Json;

namespace Arma.Server.Manager.Clients.Modsets {
    public class ApiModsetClient : IApiModsetClient {
        public HttpClient HttpClient = new HttpClient();

        public ApiModsetClient(string baseUrl) : this(new Uri(baseUrl)) {}

        public ApiModsetClient(Uri baseUri) {
            HttpClient.BaseAddress = baseUri;
            HttpClient.DefaultRequestHeaders
                .Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private HttpResponseMessage GetHttpResponseMessage(string requestUri) {
            var response = HttpClient.GetAsync(requestUri).Result;
            response.EnsureSuccessStatusCode();
            return response;
        }

        private string ApiModsetById(string id) {
            var requestUri = $"api/mod-lists/{id}";
            return GetHttpResponseMessage(requestUri).Content.ReadAsStringAsync().Result;
        }

        private string ApiModsetByName(string name) {
            var requestUri = $"api/mod-lists/by-name/{name}";
            return GetHttpResponseMessage(requestUri).Content.ReadAsStringAsync().Result;
        }

        private string ApiModsets() {
            var requestUri = "api/mod-lists";
            return GetHttpResponseMessage(requestUri).Content.ReadAsStringAsync().Result;
        }

        public List<WebModset> GetModsets()
            => JsonConvert.DeserializeObject<List<WebModset>>(ApiModsets());

        public WebModset GetModsetDataByName(string name)
            => JsonConvert.DeserializeObject<WebModset>(ApiModsetByName(name));

        public WebModset GetModsetDataByModset(WebModset webModset)
            => GetModsetDataById(webModset.Id);

        public WebModset GetModsetDataById(string id)
            => JsonConvert.DeserializeObject<WebModset>(ApiModsetById(id));
    }
}