using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace Arma.Server.Modlist {
    public class ApiModlistDataService : IApiModlistDataService {
        public HttpClient HttpClient = new HttpClient();
        
        private HttpResponseMessage GetHttpResponseMessage(string requestUri) {
            var response = HttpClient.GetAsync(requestUri).Result;
            response.EnsureSuccessStatusCode();
            return response;
        }

        private string ApiModlistById(string id) {
            var requestUri = String.Join("", "https://dev.armaforces.com/api/mod-lists/", id);
            return GetHttpResponseMessage(requestUri).Content.ReadAsStringAsync().Result;
        }

        private string ApiModlistByName(string name) {
            var requestUri = String.Join("", "https://dev.armaforces.com/api/mod-lists/by-name/", name);
            return GetHttpResponseMessage(requestUri).Content.ReadAsStringAsync().Result;
        }

        private string ApiModlists() {
            var requestUri = "https://dev.armaforces.com/api/mod-lists?page=1";
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