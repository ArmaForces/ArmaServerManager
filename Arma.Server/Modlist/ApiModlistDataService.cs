using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace Arma.Server.Modlist {
    public class ApiModlistDataService : IApiModlistDataService {
        public HttpClient HttpClient = new HttpClient();

        public ApiModlistDataService(string baseUrl) : this(new Uri(baseUrl)) {}

        public ApiModlistDataService(Uri baseUri) {
            HttpClient.BaseAddress = baseUri;
            HttpClient.DefaultRequestHeaders
                .Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private HttpResponseMessage GetHttpResponseMessage(string requestUri) {
            var response = HttpClient.GetAsync(requestUri).Result;
            response.EnsureSuccessStatusCode();
            return response;
        }

        private string ApiModlistById(string id) {
            var requestUri = $"api/mod-lists/{id}";
            return GetHttpResponseMessage(requestUri).Content.ReadAsStringAsync().Result;
        }

        private string ApiModlistByName(string name) {
            var requestUri = $"api/mod-lists/by-name/{name}";
            return GetHttpResponseMessage(requestUri).Content.ReadAsStringAsync().Result;
        }

        private string ApiModlists() {
            var requestUri = "api/mod-lists";
            return GetHttpResponseMessage(requestUri).Content.ReadAsStringAsync().Result;
        }

        public List<Modlist> GetModlists()
            => JsonConvert.DeserializeObject<List<Modlist>>(ApiModlists());

        public Modlist GetModlistDataByName(string name)
            => JsonConvert.DeserializeObject<Modlist>(ApiModlistByName(name));

        public Modlist GetModlistDataByModlist(Modlist modlist)
            => GetModlistDataById(modlist.Id);

        public Modlist GetModlistDataById(string id)
            => JsonConvert.DeserializeObject<Modlist>(ApiModlistById(id));
    }
}