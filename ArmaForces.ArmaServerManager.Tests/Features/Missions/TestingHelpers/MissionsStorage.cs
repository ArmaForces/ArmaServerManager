using System.Collections.Generic;
using ArmaForces.ArmaServerManager.Features.Missions.DTOs;
using ArmaForces.ArmaServerManager.Tests.Helpers.Missions;

namespace ArmaForces.ArmaServerManager.Tests.Features.Missions.TestingHelpers
{
    public class MissionsStorage
    {
        public IReadOnlyList<WebMission> Missions { get; }

        public MissionsStorage()
        {
            Missions = CreateMissions();
        }

        private static List<WebMission> CreateMissions(int count = 10) => WebMissionsHelper.CreateWebMissions(count);
    }
}
