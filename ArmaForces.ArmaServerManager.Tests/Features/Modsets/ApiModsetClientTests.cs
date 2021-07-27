using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using ArmaForces.ArmaServerManager.Features.Modsets;
using ArmaForces.ArmaServerManager.Features.Modsets.DTOs;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace ArmaForces.ArmaServerManager.Tests.Features.Modsets
{
    [Trait("Category", "Unit")]
    public class ApiModsetClientTests
    {
        private readonly Fixture _fixture = new Fixture();

        // TODO: Create fixture similar to Mission API Tests
        [Fact(Skip = "HttpClient not handled properly")]
        public void GetModsets_StatusOk_ModsetsRetrieved()
        {
            var expectedModsets = new List<WebModset>{_fixture.Create<WebModset>()};
            var apiClient = new ApiModsetClient(new HttpClient());

            var retrievedModsets = apiClient.GetModsets();

            retrievedModsets.Should().BeEquivalentTo(expectedModsets);
        }

        [Fact(Skip = "HttpClient not handled properly")]
        public void GetModsets_StatusNotFound_ThrowsHttpRequestException()
        {
            var apiClient = new ApiModsetClient(new HttpClient());

            Action action = () => apiClient.GetModsets();

            action.Should().Throw<HttpRequestException>(HttpStatusCode.NotFound.ToString());
        }

        [Fact(Skip = "HttpClient not handled properly")]
        public void GetModsets_StatusInternalError_ThrowsHttpRequestException()
        {
            var apiClient = new ApiModsetClient(new HttpClient());

            Action action = () => apiClient.GetModsets();

            action.Should().Throw<HttpRequestException>(HttpStatusCode.InternalServerError.ToString());
        }

        [Fact(Skip = "HttpClient not handled properly")]
        public void GetModsetDataByName_StatusNotFound_ThrowsHttpRequestException()
        {
            var apiClient = new ApiModsetClient(new HttpClient());

            Action action = () => apiClient.GetModsetDataByName(string.Empty);

            action.Should().Throw<HttpRequestException>(HttpStatusCode.NotFound.ToString());
        }

        [Fact(Skip = "HttpClient not handled properly")]
        public void GetModsetDataById_StatusNotFound_ThrowsHttpRequestException()
        {
            var apiClient = new ApiModsetClient(new HttpClient());

            Action action = () => apiClient.GetModsetDataById(string.Empty);

            action.Should().Throw<HttpRequestException>(HttpStatusCode.NotFound.ToString());
        }

        [Fact(Skip = "HttpClient not handled properly")]
        public void GetModsetDataByModset_StatusNotFound_ThrowsHttpRequestException()
        {
            var modset = _fixture.Create<WebModset>();
            var apiClient = new ApiModsetClient(new HttpClient());

            Action action = () => apiClient.GetModsetDataByModset(modset);

            action.Should().Throw<HttpRequestException>(HttpStatusCode.NotFound.ToString());
        }
    }
}