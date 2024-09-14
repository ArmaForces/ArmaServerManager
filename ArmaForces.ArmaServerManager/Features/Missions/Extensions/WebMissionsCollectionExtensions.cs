using System.Collections.Generic;
using System.Linq;
using ArmaForces.ArmaServerManager.Features.Missions.DTOs;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Missions.Extensions
{
    internal static class WebMissionsCollectionExtensions
    {
        public const string NoNearestMissionError = "No nearest mission found.";
        
        public static Result<WebMission> GetNearestMission(this IEnumerable<WebMission> missions)
        {
            var nearestMission = missions
                .OrderBy(x => x.Date)
                .FirstOrDefault();

            return nearestMission is null
                ? Result.Failure<WebMission>(NoNearestMissionError)
                : Result.Success(nearestMission);
        }
    }
}
