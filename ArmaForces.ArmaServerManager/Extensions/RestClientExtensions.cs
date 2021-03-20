using System.Net;
using System.Net.Http;
using CSharpFunctionalExtensions;
using RestSharp;

namespace ArmaForces.ArmaServerManager.Extensions {
    internal static class RestClientExtensions {
        /// <summary>
        /// Executes REST request and converts response content to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Expected response content.</typeparam>
        /// <param name="restClient"><seealso cref="IRestClient"/> used.</param>
        /// <param name="request">Request to execute.</param>
        /// <exception cref="HttpRequestException">Thrown when status code is not OK.</exception>
        /// <returns><typeparamref name="T"/></returns>
        public static T ExecuteAndReturnData<T>(this IRestClient restClient, IRestRequest request) where T : new()
        {
            var response = restClient.Execute<T>(request);
            return response.StatusCode == HttpStatusCode.OK
                ? response.Data
                : throw new HttpRequestException(response.ErrorMessage + "|" + response.ErrorException + "|" + response.Content);
        }

        public static Result<T> ExecuteAndReturnResult<T>(this IRestClient restClient, IRestRequest request)
            where T : new()
        {
            var response = restClient.Execute<T>(request);
            return response.IsSuccessful
                ? Result.Success(response.Data)
                : Result.Failure<T>(response.ErrorMessage);
        }
    }
}