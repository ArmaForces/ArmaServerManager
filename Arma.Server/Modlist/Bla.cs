using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace Arma.Server.Modlist {
    public static class Bla {
        public static HttpClient _httpClient = new HttpClient();

        private static HttpResponseMessage getHttpResponseMessage(string requestUri) {
            var response = _httpClient.GetAsync(requestUri).Result;
            response.EnsureSuccessStatusCode();
            return response;
        }

        private static string ApiModlistsId(string id) {
            var requestUri = String.Join("", "https://dev.armaforces.com/api/mod-lists/", id);
            return getHttpResponseMessage(requestUri).Content.ReadAsStringAsync().Result;
        }

        private static string ApiModlistsName(string name) {
            var requestUri = String.Join("", "https://dev.armaforces.com/api/mod-lists/by-name/", name);
            return getHttpResponseMessage(requestUri).Content.ReadAsStringAsync().Result;
        }

        private static string ApiModlists() {
            var requestUri = "https://dev.armaforces.com/api/mod-lists?page=1";
            return getHttpResponseMessage(requestUri).Content.ReadAsStringAsync().Result;
        }

        public static List<Modlist> GetModlists()
            => JsonConvert.DeserializeObject<List<Modlist>>(ApiModlists());

        public static Modlist GetModlistData(Modlist modlist)
            => GetModlistData(modlist.Id);

        public static Modlist GetModlistData(string modlistId)
            => JsonConvert.DeserializeObject<Modlist>(ApiModlistsId(modlistId));
    }
}