using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ArmaForces.ArmaServerManager.Common;
using ArmaForces.ArmaServerManager.Features.Modsets.DTOs;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Modsets
{
    /// <inheritdoc cref="IApiModsetClient" />
    internal class ApiModsetClient : HttpClientBase, IApiModsetClient
    {
        /// <inheritdoc cref="ApiModsetClient" />
        public ApiModsetClient(HttpClient httpClient) : base(httpClient) { }

        /// <inheritdoc />
        public async Task<Result<List<WebModset>>> GetModsets()
            => await ApiModsets();

        /// <inheritdoc />
        public async Task<Result<WebModset>> GetModsetDataByName(string name)
            => await ApiModsetByName(name);

        /// <inheritdoc />
        public async Task<Result<WebModset>> GetModsetDataByModset(WebModset webModset)
            => await GetModsetDataById(webModset.Id);

        /// <inheritdoc />
        public async Task<Result<WebModset>> GetModsetDataById(string id)
            => await ApiModsetById(id);

        private async Task<Result<WebModset>> ApiModsetById(string id) {
            var requestUri = $"api/mod-lists/{id}";
            return await HttpGetAsync<WebModset>(requestUri);
        }

        private async Task<Result<WebModset>> ApiModsetByName(string name) {
            var requestUri = $"api/mod-lists/by-name/{name}";
            return await HttpGetAsync<WebModset>(requestUri);
        }

        private async Task<Result<List<WebModset>>> ApiModsets() {
            var requestUri = "api/mod-lists";
            return await HttpGetAsync<List<WebModset>>(requestUri);
        }
    }
}