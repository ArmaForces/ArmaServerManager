using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Constants;
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

        protected async Task<Result<T>> HttpGetAsync<T>(string requestUrl)
        {
            var httpResponseMessage = await _httpClient.GetAsync(requestUrl);
            
            var responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
            
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<T>(responseBody, JsonOptions.Default) ??
                       Result.Failure<T>($"Failed to deserialize response: {responseBody}");
            }

            var error = string.IsNullOrWhiteSpace(responseBody)
                ? httpResponseMessage.ReasonPhrase
                : responseBody;
                
            return Result.Failure<T>(error);
        }

        protected async Task<Result> HttpPostAsync<T>(string? requestUrl, T content)
        {
            var stringContent = new StringContent(JsonSerializer.Serialize(content, JsonSerializerOptions.Web), Encoding.UTF8, "application/json");
            var httpResponseMessage = await _httpClient.PostAsync(requestUrl, stringContent);
            
            var responseBody = await httpResponseMessage.Content.ReadAsStringAsync();

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return Result.Success();
            }

            var error = string.IsNullOrWhiteSpace(responseBody)
                ? httpResponseMessage.ReasonPhrase
                : responseBody;
            
            return Result.Failure(error);
        }
    }
}
