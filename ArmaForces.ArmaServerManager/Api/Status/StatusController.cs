﻿using System.Net.Mime;
using System.Threading.Tasks;
using ArmaForces.ArmaServerManager.Api.Status.DTOs;
using ArmaForces.ArmaServerManager.Api.Status.Mappers;
using ArmaForces.ArmaServerManager.Features.Status;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ArmaForces.ArmaServerManager.Api.Status
{
    /// <summary>
    /// Allows manager service status retrieval.
    /// </summary>
    [Route("api/status")]
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly IStatusProvider _statusProvider;

        public StatusController(IStatusProvider statusProvider)
        {
            _statusProvider = statusProvider;
        }
        
        /// <summary>Get Status</summary>
        /// <remarks>
        /// Returns current application status and currently processing job.
        /// Optionally, can also include jobs queue and server status.
        /// </remarks>
        /// <param name="includeJobs">Include jobs queue</param>
        /// <param name="includeServers">Include servers status</param>
        [HttpGet(Name = nameof(GetStatus))]
        [ProducesResponseType(typeof(AppStatusDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStatus(
            [FromQuery] bool includeJobs = false,
            [FromQuery] bool includeServers = false)
            => await _statusProvider.GetAppStatus(includeJobs, includeServers)
                .Map(StatusMapper.Map)
                .Match(
                    onSuccess: Ok,
                    onFailure: error => (IActionResult) NotFound(error));
    }
}