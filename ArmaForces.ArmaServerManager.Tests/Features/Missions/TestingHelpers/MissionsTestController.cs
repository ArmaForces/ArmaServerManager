using System;
using ArmaForces.ArmaServerManager.Features.Missions.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ArmaForces.ArmaServerManager.Tests.Features.Missions.TestingHelpers
{
    [ApiController]
    [Route("api/missions")]
    public class MissionsTestController : ControllerBase
    {
        // TODO: Add some implementation here
        [HttpGet]
        public ActionResult<WebMission> GetMissions(bool? includeArchive = false, DateTime? fromDateTime = null)
        {
            return Ok();
        }
    }
}
