using System.Net;
using Moq;
using RestSharp;

namespace Arma.Server.Manager.Test.Helpers.Extensions {
    public static class RestClientMockExtensions {
        /// <summary>
        /// Setups <see cref="IRestClient"/> response for any <see cref="IRestRequest"/>.
        /// </summary>
        /// <typeparam name="T">Type of response data.</typeparam>
        /// <param name="restClientMock">Mock of rest client.</param>
        /// <param name="statusCode">Response status code.</param>
        /// <param name="data">Response data.</param>
        public static void SetupResponse<T>(
            this Mock<IRestClient> restClientMock,
            HttpStatusCode statusCode,
            T data)
        {
            var response = new RestResponse<T> { Data = data, StatusCode = statusCode };
            restClientMock.Setup(x => x.Execute<T>(It.IsAny<IRestRequest>())).Returns(response);
        }
    }
}