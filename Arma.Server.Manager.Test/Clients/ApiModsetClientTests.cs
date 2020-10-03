using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Arma.Server.Config;
using Arma.Server.Manager.Clients.Modsets;
using Arma.Server.Manager.Clients.Modsets.Entities;
using AutoFixture;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using RestSharp;
using Xunit;

namespace Arma.Server.Manager.Test.Clients {
    public class ApiModsetClientTests {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void GetModsets_StatusOk_ModsetsRetrieved() {
            var expectedModsets = new List<WebModset>{_fixture.Create<WebModset>()};
            var restClientMock = new Mock<IRestClient>();
            SetupResponse(restClientMock, HttpStatusCode.OK, expectedModsets);
            var apiClient = new ApiModsetClient(restClientMock.Object);

            var retrievedModsets = apiClient.GetModsets();

            retrievedModsets.Should().BeEquivalentTo(expectedModsets);
        }

        [Fact]
        public void GetModsets_StatusNotFound_ThrowsHttpRequestException() {
            var restClientMock = new Mock<IRestClient>();
            SetupResponse(restClientMock, HttpStatusCode.NotFound, new List<WebModset>());
            var apiClient = new ApiModsetClient(restClientMock.Object);

            Action action = () => apiClient.GetModsets();

            action.Should().Throw<HttpRequestException>(HttpStatusCode.NotFound.ToString());
        }

        [Fact]
        public void GetModsets_StatusInternalError_ThrowsHttpRequestException() {
            var restClientMock = new Mock<IRestClient>();
            SetupResponse(restClientMock, HttpStatusCode.InternalServerError, new List<WebModset>());
            var apiClient = new ApiModsetClient(restClientMock.Object);

            Action action = () => apiClient.GetModsets();

            action.Should().Throw<HttpRequestException>(HttpStatusCode.InternalServerError.ToString());
        }

        [Fact]
        public void GetModsetDataByName_StatusNotFound_ThrowsHttpRequestException() {
            var restClientMock = new Mock<IRestClient>();
            SetupResponse(restClientMock, HttpStatusCode.NotFound, new WebModset());
            var apiClient = new ApiModsetClient(restClientMock.Object);

            Action action = () => apiClient.GetModsetDataByName("");

            action.Should().Throw<HttpRequestException>(HttpStatusCode.NotFound.ToString());
        }

        [Fact]
        public void GetModsetDataById_StatusNotFound_ThrowsHttpRequestException() {
            var restClientMock = new Mock<IRestClient>();
            SetupResponse(restClientMock, HttpStatusCode.NotFound, new WebModset());
            var apiClient = new ApiModsetClient(restClientMock.Object);

            Action action = () => apiClient.GetModsetDataById("");

            action.Should().Throw<HttpRequestException>(HttpStatusCode.NotFound.ToString());
        }

        [Fact]
        public void GetModsetDataByModset_StatusNotFound_ThrowsHttpRequestException() {
            var modset = _fixture.Create<WebModset>();
            var restClientMock = new Mock<IRestClient>();
            SetupResponse(restClientMock, HttpStatusCode.NotFound, new WebModset());
            var apiClient = new ApiModsetClient(restClientMock.Object);

            Action action = () => apiClient.GetModsetDataByModset(modset);

            action.Should().Throw<HttpRequestException>(HttpStatusCode.NotFound.ToString());
        }

        private static void SetupResponse<T>(
            Mock<IRestClient> restClientMock,
            HttpStatusCode statusCode,
            T data) {
            var response = new RestResponse<T> { Data = data, StatusCode = statusCode };
            restClientMock.Setup(x => x.Execute<T>(It.IsAny<IRestRequest>())).Returns(response);
        }
    }
}