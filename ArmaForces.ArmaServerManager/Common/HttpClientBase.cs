using System;
using System.Net.Http;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Newtonsoft.Json;

namespace ArmaForces.ArmaServerManager.Common
{
    internal class HttpClientBase
    {
        private readonly HttpClient _httpClient;

        protected HttpClientBase(
            IHttpClientFactory httpClientFactory,
            // TODO: Handle ApiModsets and ApiMissions not required
            string? baseAddress,
            string? clientName = null)
        {
            if (baseAddress is null)
            {
                throw new NotSupportedException(
                    "Missions and modsets APIs are required. Both must be a valid url.");
            }
            _httpClient = httpClientFactory.CreateClient(clientName);
            _httpClient.BaseAddress = new Uri(baseAddress);
        }

        protected async Task<Result<T>> HttpGetAsync<T>(string requestUri)
        {
            var httpResponseMessage = await _httpClient.GetAsync(requestUri);
            
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
                return Result.Success(JsonConvert.DeserializeObject<T>(responseBody));
            }
            else
            {
                return Result.Failure<T>($"{await httpResponseMessage.Content.ReadAsStringAsync()}");
            }
        }
    }
}
