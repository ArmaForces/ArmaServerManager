using System;
using System.Collections.Generic;
using ArmaForces.ArmaServerManager.Features.Missions.DTOs;
using AutoFixture;

namespace ArmaForces.ArmaServerManager.Tests.Helpers.Missions
{
    public static class WebMissionsHelper
    {
        public static List<WebMission> CreateWebMissions(int missionsCount = 5)
            => CreateWebMissions(new Fixture(), missionsCount);

        public static List<WebMission> CreateWebMissions(Fixture fixture, int missionsCount = 5)
        {
            var modsList = new List<WebMission>();

            for (var i = 0; i < missionsCount; i++)
            {
                modsList.Add(CreateWebMission(fixture));
            }

            return modsList;
        }

        private static WebMission CreateWebMission(Fixture fixture)
        {
            var date = fixture.Create<DateTime>();
            var closeDate = date.AddHours(-fixture.Create<int>());

            return new WebMission
            {
                Title = fixture.Create<string>(),
                Description = fixture.Create<string>(),
                Modlist = fixture.Create<string>(),
                Date = date,
                CloseDate = closeDate,
                Archive = DateTime.Now < closeDate
            };
        } 
    }
}
