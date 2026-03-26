using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Common.Errors;
using ArmaForces.ArmaServerManager.Common;
using ArmaForces.ArmaServerManager.Features.Modsets.DTOs;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Modsets.Client
{
    /// <inheritdoc cref="IApiModsetClient" />
    internal class ApiModsetClient : HttpClientBase, IApiModsetClient
    {
        /// <inheritdoc cref="ApiModsetClient" />
        public ApiModsetClient(HttpClient httpClient) : base(httpClient) { }

        /// <inheritdoc />
        public async Task<Result<List<WebModset>, IError>> GetModsets()
            => await ApiModsets();

        /// <inheritdoc />
        public async Task<Result<WebModset, IError>> GetModsetDataByName(string name)
            => await ApiModsetByName(name);

        /// <inheritdoc />
        public async Task<Result<WebModset, IError>> GetModsetDataByModset(WebModset webModset)
            => await GetModsetDataById(webModset.Id);

        /// <inheritdoc />
        public async Task<Result<WebModset, IError>> GetModsetDataById(string id)
            => await ApiModsetById(id);

        private async Task<Result<WebModset, IError>> ApiModsetById(string id) {
            var requestUri = $"api/mod-lists/{id}";
            return await HttpGetAsync<WebModset>(requestUri);
        }

        private async Task<Result<WebModset, IError>> ApiModsetByName(string name) {
            var requestUri = $"api/mod-lists/by-name/{name}";
            return await HttpGetAsync<WebModset>(requestUri);
        }

        private async Task<Result<List<WebModset>, IError>> ApiModsets() {
            var requestUri = "api/mod-lists";
            return await HttpGetAsync<List<WebModset>>(requestUri);
        }
    }
}