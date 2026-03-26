using System.Collections.Generic;
using System.Linq;
using ArmaForces.Arma.Server.Common.Errors;
using ArmaForces.ArmaServerManager.Common.Errors;
using ArmaForces.ArmaServerManager.Features.Missions.DTOs;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Missions.Extensions
{
    internal static class WebMissionsCollectionExtensions
    {
        public const string NoNearestMissionError = "No nearest mission found.";
        
        public static Result<WebMission, IError> GetNearestMission(this IEnumerable<WebMission> missions)
        {
            var nearestMission = missions
                .OrderBy(x => x.Date)
                .FirstOrDefault();

            return nearestMission is null
                ? new Error(NoNearestMissionError, ManagerErrorCode.MissionNotFound)
                : nearestMission;
        }
    }
}
