using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Tests.Helpers.Extensions;
using ArmaForces.ArmaServerManager.Features.Missions;
using ArmaForces.ArmaServerManager.Features.Missions.DTOs;
using ArmaForces.ArmaServerManager.Features.Modsets.DTOs;
using ArmaForces.ArmaServerManager.Tests.Features.Missions.TestingHelpers;
using AutoFixture;
using Xunit;

namespace ArmaForces.ArmaServerManager.Tests.Features.Missions
{
    [Trait("Category", "Integration")]
    public class ApiMissionsClientTests : IClassFixture<MissionsTestApiFixture>
    {
        private readonly Fixture _fixture = new Fixture();
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
            var expectedMissions = new List<WebMission> { _fixture.Create<WebMission>() };
            _missionsStorage.Missions = expectedMissions;
            var apiClient = new ApiMissionsClient(_httpClient);

            var result = await apiClient.GetUpcomingMissions();

            result.ShouldBeSuccess(expectedMissions);
        }

        [Fact]
        public async Task GetUpcomingMissionsModsets_StatusOk_ModsetsRetrieved()
        {
            const int modsetsCount = 3;
            const int missionsCount = 6;
            var modsets = _fixture.CreateMany<WebModset>(modsetsCount).ToList();
            var expectedModsets = modsets
                .Select(x => x.Name)
                .ToHashSet();
            var missions = PrepareMissions(missionsCount, modsets);
            _missionsStorage.Missions = missions;

            var apiClient = new ApiMissionsClient(_httpClient);

            var result = await apiClient.GetUpcomingMissionsModsetsNames();

            result.ShouldBeSuccess(expectedModsets);
        }

        private List<WebMission> PrepareMissions(int missionsCount, IReadOnlyList<WebModset> modsets)
        {
            var missions = _fixture.CreateMany<WebMission>(missionsCount).ToList();
            var j = 0;
            foreach (var webMission in missions)
            {
                webMission.Modlist = modsets[j].Name;
                j = j < modsets.Count - 1
                    ? j + 1
                    : 0;
            }

            return missions;
        }
    }
}