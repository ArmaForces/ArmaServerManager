using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Tests.Helpers.Extensions;
using ArmaForces.ArmaServerManager.Features.Missions;
using ArmaForces.ArmaServerManager.Tests.Features.Missions.TestingHelpers;
using Xunit;

namespace ArmaForces.ArmaServerManager.Tests.Features.Missions
{
    [Trait("Category", "Integration")]
    public class ApiMissionsClientTests : IClassFixture<MissionsTestApiFixture>
    {
        private readonly HttpClient _httpClient;
        private readonly MissionsStorage _missionsStorage;

        public ApiMissionsClientTests(MissionsTestApiFixture missionsTestApiFixture)
        {
            _httpClient = missionsTestApiFixture.HttpClient;
            _missionsStorage = missionsTestApiFixture.MissionsStorage;
        }

        [Fact]
        public async Task GetUpcomingMissions_StatusOk_MissionsRetrieved()
        {
            var expectedMissions = _missionsStorage.Missions
                .Where(x => x.Date > DateTime.Today)
                .ToList();
            var apiClient = new ApiMissionsClient(_httpClient);

            var result = await apiClient.GetUpcomingMissions();

            result.ShouldBeSuccess(expectedMissions);
        }

        [Fact]
        public async Task GetUpcomingMissionsModsets_StatusOk_ModsetsRetrieved()
        {
            var expectedModsets = _missionsStorage.Missions
                .Where(x => x.Date > DateTime.Today)
                .Select(x => x.Modlist)
                .ToHashSet();

            var apiClient = new ApiMissionsClient(_httpClient);

            var result = await apiClient.GetUpcomingMissionsModsetsNames();

            result.ShouldBeSuccess(expectedModsets);
        }
    }
}