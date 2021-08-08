using System.Net.Http;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Newtonsoft.Json;

namespace ArmaForces.ArmaServerManager.Common
{
    internal class HttpClientBase
    {
        private readonly HttpClient _httpClient;

        protected HttpClientBase(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        protected async Task<Result<T>> HttpGetAsync<T>(string requestUrl)
        {
            var httpResponseMessage = await _httpClient.GetAsync(requestUrl);
            
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
                return Result.Success(JsonConvert.DeserializeObject<T>(responseBody));
            }
            else
            {
                var responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
                var error = string.IsNullOrWhiteSpace(responseBody)
                    ? httpResponseMessage.ReasonPhrase
                    : responseBody;
                
                return Result.Failure<T>(error);
            }
        }
    }
}
