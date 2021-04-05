using System.Collections.Generic;
using System.Linq;
using ArmaForces.ArmaServerManager.Features.Missions.DTOs;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Missions.Extensions
{
    public static class WebMissionsCollectionExtensions
    {
        public static Result<WebMission> GetNearestMission(this IEnumerable<WebMission> missions)
        {
            var nearestMission = missions
                .OrderBy(x => x.Date)
                .FirstOrDefault();

            return nearestMission is null
                ? Result.Failure<WebMission>("No nearest mission found.")
                : Result.Success(nearestMission);
        }
    }
}
