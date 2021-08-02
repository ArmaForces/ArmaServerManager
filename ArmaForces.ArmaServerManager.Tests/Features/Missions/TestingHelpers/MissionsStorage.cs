using System.Collections.Generic;
using ArmaForces.ArmaServerManager.Features.Missions.DTOs;

namespace ArmaForces.ArmaServerManager.Tests.Features.Missions.TestingHelpers
{
    public class MissionsStorage
    {
        public List<WebMission> Missions { get; set; } = new List<WebMission>();
    }
}
