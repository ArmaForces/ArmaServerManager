using System.Collections.Generic;
using System.Linq;
using ArmaForces.ArmaServerManager.Features.Missions.DTOs;

namespace ArmaForces.ArmaServerManager.Features.Missions.Extensions
{
    public static class WebMissionsCollectionExtensions
    {
        public static WebMission GetNearestMission(this IEnumerable<WebMission> missions)
            => missions
                .OrderBy(x => x.Date)
                .FirstOrDefault();
    }
}
