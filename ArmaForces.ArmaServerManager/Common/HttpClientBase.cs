using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Common.Errors;
using ArmaForces.Arma.Server.Constants;
using ArmaForces.ArmaServerManager.Common.Errors;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Common
{
    internal class HttpClientBase
    {
        private readonly HttpClient _httpClient;

        protected HttpClientBase(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        protected async Task<Result<T, IError>> HttpGetAsync<T>(string requestUrl)
        {
            var httpResponseMessage = await _httpClient.GetAsync(requestUrl);
            
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
                
                return JsonSerializer.Deserialize<T>(responseBody, JsonOptions.Default) ??
                       Result.Failure<T, IError>(new Error($"Failed to deserialize response: {responseBody}", ManagerErrorCode.InternalServerError));
            }
            else
            {
                var responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
                var errorMessage = string.IsNullOrWhiteSpace(responseBody)
                    ? httpResponseMessage.ReasonPhrase
                    : responseBody;
                
                return new Error(errorMessage ?? $"GET request {requestUrl} failed with error", httpResponseMessage.StatusCode);
            }
        }
    }
}
