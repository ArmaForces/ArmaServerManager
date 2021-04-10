﻿using System.Collections.Generic;
using System.Linq;
using ArmaForces.ArmaServerManager.Features.Missions.DTOs;
using ArmaForces.ArmaServerManager.Features.Missions.Extensions;
using ArmaForces.ArmaServerManager.Tests.Helpers.Missions;
using FluentAssertions;
using Xunit;

namespace ArmaForces.ArmaServerManager.Tests.Features.Missions.Extensions
{
    public class WebMissionsCollectionExtensionsTests
    {
        [Fact]
        public void GetNearestMission_CollectionEmpty_ReturnsNull()
        {
            var missions = new List<WebMission>();

            var result = missions.GetNearestMission();

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void GetNearestMission_CollectionNotEmpty_ReturnsNearestMission()
        {
            var missions = WebMissionsHelper.CreateWebMissions();

            var expectedMission = missions
                .OrderBy(x => x.Date)
                .First();

            var result = missions.GetNearestMission();

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(expectedMission);
        }
    }
}
