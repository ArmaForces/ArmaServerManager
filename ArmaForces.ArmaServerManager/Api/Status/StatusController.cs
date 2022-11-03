using System.Collections.Generic;
using System.Net.Mime;
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

        /// <inheritdoc />
        public StatusController(IStatusProvider statusProvider)
        {
            _statusProvider = statusProvider;
        }
        
        /// <summary>Get Status</summary>
        /// <remarks>
        /// Returns current application status and currently processing job.
        /// Optionally, can also include jobs queue and server status.
        /// </remarks>
        /// <param name="includeJobs">Includes jobs queue details when true.</param>
        /// <param name="includeServers">Includes servers status when true.</param>
        [HttpGet(Name = nameof(GetStatus))]
        [ProducesResponseType(typeof(AppStatusDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppStatusDetailsDto), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStatus([FromQuery] IEnumerable<AppStatusIncludes> include)
            => await _statusProvider.GetAppStatus(include)
                .Map(StatusMapper.Map)
                .Match(
                    onSuccess: Ok,
                    onFailure: error => (IActionResult) Problem(error));
    }
}