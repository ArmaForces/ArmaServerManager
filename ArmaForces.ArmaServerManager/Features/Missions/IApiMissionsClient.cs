﻿using System.Collections.Generic;
using ArmaForces.ArmaServerManager.Features.Missions.DTOs;
using ArmaForces.ArmaServerManager.Features.Modsets.DTOs;

namespace ArmaForces.ArmaServerManager.Features.Missions {
    /// <summary>
    /// Handles connection and retrieving missions data from web API.
    /// </summary>
    public interface IApiMissionsClient {
        /// <summary>
        /// Retrieves all upcoming missions counting from today.
        /// </summary>
        /// <returns><see cref="IEnumerable{T}"/> of <see cref="WebMission"/></returns>
        IEnumerable<WebMission> GetUpcomingMissions();

        /// <summary>
        /// Prepares <seealso cref="ISet{T}"/> of <see cref="WebModset"/> for all upcoming missions counting from today.
        /// </summary>
        /// <returns><seealso cref="ISet{T}"/> of <see cref="WebModset"/></returns>
        ISet<WebModset> GetUpcomingMissionsModsets();
    }
}