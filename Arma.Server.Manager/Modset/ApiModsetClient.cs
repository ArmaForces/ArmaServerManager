using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace Arma.Server.Manager.Modset {
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

        public List<Modset> GetModsets()
            => JsonConvert.DeserializeObject<List<Modset>>(ApiModsets());

        public Modset GetModsetDataByName(string name)
            => JsonConvert.DeserializeObject<Modset>(ApiModsetByName(name));

        public Modset GetModsetDataByModset(Modset modset)
            => GetModsetDataById(modset.Id);

        public Modset GetModsetDataById(string id)
            => JsonConvert.DeserializeObject<Modset>(ApiModsetById(id));
    }
}