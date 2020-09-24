using Arma.Server.Manager.Clients.Missions.Entities;
using System.Collections.Generic;

namespace Arma.Server.Manager.Clients.Missions {
    public interface IApiMissionsClient {
        IEnumerable<WebMission> GetUpcomingMissions();
    }
}