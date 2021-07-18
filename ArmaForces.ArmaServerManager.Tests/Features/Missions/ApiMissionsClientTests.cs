using System.Collections.Generic;
using System.Linq;
using System.Net;
using ArmaForces.ArmaServerManager.Features.Missions;
using ArmaForces.ArmaServerManager.Features.Missions.DTOs;
using ArmaForces.ArmaServerManager.Features.Modsets.DTOs;
using ArmaForces.ArmaServerManager.Tests.Helpers.Extensions;
using AutoFixture;
using FluentAssertions;
using Moq;
using RestSharp;
using Xunit;

namespace ArmaForces.ArmaServerManager.Tests.Features.Missions {
    [Trait("Category", "Unit")]
    public class ApiMissionsClientTests {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void GetUpcomingMissions_StatusOk_MissionsRetrieved() {
            var expectedMissions = new List<WebMission> { _fixture.Create<WebMission>() };
            var restClientMock = new Mock<IRestClient>();
            restClientMock.SetupResponse(HttpStatusCode.OK, expectedMissions);
            var apiClient = new ApiMissionsClient(restClientMock.Object);

            var result = apiClient.GetUpcomingMissions();

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(expectedMissions);
        }

        [Fact]
        public void GetUpcomingMissionsModsets_StatusOk_ModsetsRetrieved() {
            const int modsetsCount = 3;
            const int missionsCount = 6;
            var modsets = _fixture.CreateMany<WebModset>(modsetsCount).ToList();
            var expectedModsets = modsets.Select(x => x.Name);
            var missions = PrepareMissions(missionsCount, modsets);

            var restClientMock = new Mock<IRestClient>();
            restClientMock.SetupResponse(HttpStatusCode.OK, missions);
            var apiClient = new ApiMissionsClient(restClientMock.Object);

            var result = apiClient.GetUpcomingMissionsModsetsNames();

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(expectedModsets);
        }

        private List<WebMission> PrepareMissions(int missionsCount, IReadOnlyList<WebModset> modsets) {
            var missions = _fixture.CreateMany<WebMission>(missionsCount).ToList();
            var j = 0;
            foreach (var webMission in missions) {
                webMission.Modlist = modsets[j].Name;
                j = j < modsets.Count - 1
                    ? j + 1
                    : 0;
            }

            return missions;
        }
    }
}