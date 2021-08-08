using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Extensions;
using ArmaForces.Arma.Server.Tests.Helpers.Extensions;
using ArmaForces.ArmaServerManager.Features.Modsets;
using ArmaForces.ArmaServerManager.Features.Modsets.DTOs;
using ArmaForces.ArmaServerManager.Tests.Features.Modsets.TestingHelpers;
using AutoFixture;
using Xunit;

namespace ArmaForces.ArmaServerManager.Tests.Features.Modsets
{
    [Trait("Category", "Integration")]
    public class ApiModsetClientTests : IClassFixture<ModsetsTestApiFixture>
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly HttpClient _httpClient;
        private readonly ModsetsStorage _modsetsStorage;

        public ApiModsetClientTests(ModsetsTestApiFixture modsetsTestApiFixture)
        {
            _httpClient = modsetsTestApiFixture.HttpClient;
            _modsetsStorage = modsetsTestApiFixture.ModsetsStorage;
        }
        
        [Fact]
        public async Task GetModsets_StatusOk_ModsetsRetrieved()
        {
            var expectedModsets = _modsetsStorage.Modsets.ToList();
            var apiClient = new ApiModsetClient(_httpClient);

            var result = await apiClient.GetModsets();
            
            result.ShouldBeSuccess(expectedModsets);
        }

        [Fact]
        public async Task GetModsetDataByName_ModsetWithNameExists_ReturnsModset()
        {
            var modset = _modsetsStorage.Modsets.First();
             
            var apiClient = new ApiModsetClient(_httpClient);

            var result = await apiClient.GetModsetDataByName(modset.Name);
            
            result.ShouldBeSuccess(modset);
        }

        [Fact]
        public async Task GetModsetDataByName_ModsetNotExists_ReturnsNotFound()
        {
            var modset = _fixture.Create<WebModset>();
            var apiClient = new ApiModsetClient(_httpClient);
            
            var result = await apiClient.GetModsetDataByName(modset.Name);

            result.ShouldBeFailure(ModsetsTestController.ModsetWithNameDoesNotExistMessage);
        }

        [Fact]
        public async Task GetModsetDataById_ModsetWithIdExists_ReturnsModset()
        {
            var modset = _modsetsStorage.Modsets.First();
            
            var apiClient = new ApiModsetClient(_httpClient);

            var result = await apiClient.GetModsetDataById(modset.Id);

            result.ShouldBeSuccess(modset);
        }

        [Fact]
        public async Task GetModsetDataById_ModsetNotExists_ReturnsNotFound()
        {
            var modset = _fixture.Create<WebModset>();
            
            var apiClient = new ApiModsetClient(_httpClient);

            var result = await apiClient.GetModsetDataById(modset.Id);

            result.ShouldBeFailure(ModsetsTestController.ModsetWithIdNotExistsMessage);
        }
    }
}