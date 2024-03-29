﻿using System;
using System.Linq;
using ArmaForces.ArmaServerManager.Features.Missions.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ArmaForces.ArmaServerManager.Tests.Features.Missions.TestingHelpers
{
    [ApiController]
    [Route("api/missions")]
    public class MissionsTestController : ControllerBase
    {
        private readonly MissionsStorage _missionsStorage;

        public MissionsTestController(MissionsStorage missionsStorage)
        {
            _missionsStorage = missionsStorage;
        }

        [HttpGet]
        public ActionResult<WebMission> GetMissions(
            bool includeArchive = false,
            DateTime? fromDateTime = null,
            DateTime? toDateTime = null)
        {
            fromDateTime ??= DateTime.MinValue;
            toDateTime ??= DateTime.MaxValue;

            return Ok(_missionsStorage.Missions
                .Where(x => !x.Archive || includeArchive)
                .Where(x => x.Date >= fromDateTime)
                .Where(x => x.Date <= toDateTime)
                .ToList());
        }
    }
}
