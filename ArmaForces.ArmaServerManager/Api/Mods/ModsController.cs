using System;
using System.Net.Mime;
using System.Threading;
using ArmaForces.ArmaServerManager.Api.Jobs.DTOs;
using ArmaForces.ArmaServerManager.Api.Mods.DTOs;
using ArmaForces.ArmaServerManager.Common;
using ArmaForces.ArmaServerManager.Features.Jobs;
using ArmaForces.ArmaServerManager.Features.Modsets;
using ArmaForces.ArmaServerManager.Infrastructure.Authentication;
using ArmaForces.ArmaServerManager.Services;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ArmaForces.ArmaServerManager.Api.Mods
{
    /// <summary>
    /// Allows triggering and scheduling mods update/verification.
    /// </summary>
    [Route("api/mods")]
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [ApiKey]
    public class ModsController : ControllerBase
    {
        private readonly IJobsScheduler _jobsScheduler;
        private readonly IModsetProvider _modsetProvider;

        /// <inheritdoc />
        public ModsController(
            IJobsScheduler jobsScheduler,
            IModsetProvider modsetProvider)
        {
            _jobsScheduler = jobsScheduler;
            _modsetProvider = modsetProvider;
        }

        /// <summary>Update Mods</summary>
        /// <remarks>Triggers or schedules update of given mods.</remarks>
        [HttpPost("update", Name = nameof(UpdateMods))]
        [ProducesResponseType(typeof(string), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateMods([FromBody] ModsUpdateRequestDto modsUpdateRequestDto)
        {
            var serverShutdownJob = _jobsScheduler
                .ScheduleJob<ServerStartupService>(
                    // TODO: Shutdown all servers
                    x => x.ShutdownServer(2302, false, CancellationToken.None),
                    modsUpdateRequestDto.ScheduleAt);

            if (serverShutdownJob.IsFailure) return Problem(serverShutdownJob.Error);

            var result = modsUpdateRequestDto.ModsetName is null
                ? _jobsScheduler.ContinueJobWith<ModsUpdateService>(
                    serverShutdownJob.Value,
                    x => x.UpdateAllMods(CancellationToken.None))
                : _jobsScheduler.ContinueJobWith<ModsUpdateService>(
                    serverShutdownJob.Value,
                    x => x.UpdateModset(modsUpdateRequestDto.ModsetName, CancellationToken.None));

            return result.Match(
                onSuccess: Accepted,
                onFailure: error => (IActionResult)BadRequest(error));
        }

        /// <summary>Verify Mods</summary>
        /// <remarks>Triggers or schedules verification of given mods. <b>Not implemented.</b></remarks>
        [HttpPost("verify")]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public IActionResult VerifyMods([FromBody] ModsVerificationRequestDto modsVerificationRequestDto)
        {
            throw new NotImplementedException("Mods verification is not implemented yet.");
        }

        /// <summary>Update Modset</summary>
        /// <remarks>Triggers or schedules update of given modset.</remarks>
        /// <param name="modsetName">Name of modset to update.</param>
        /// <param name="modsetUpdateRequestDto">Optional job schedule details.</param>
        [HttpPost("{modsetName}/update", Name = nameof(UpdateModset))]
        [ProducesResponseType(typeof(string), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateModset(string modsetName, [FromBody] ModsetUpdateRequestDto modsetUpdateRequestDto)
        {
            var result = _jobsScheduler
                .ScheduleJob<ServerStartupService>(
                    x => x.ShutdownAllServers(modsetUpdateRequestDto.Force, CancellationToken.None),
                    modsetUpdateRequestDto.ScheduleAt)
                .Bind(shutdownJobId => _jobsScheduler.ContinueJobWith<ModsUpdateService>(
                    shutdownJobId,
                    x => x.UpdateModset(modsetName, CancellationToken.None)));

            return result.Match(
                onSuccess: Accepted,
                onFailure: TooEarly);
        }

        /// <summary>Verify Modset</summary>
        /// <remarks>Triggers or schedules verification of given modset. <b>Not implemented.</b></remarks>
        /// <param name="modsetName">Name of modset to verify.</param>
        /// <param name="jobScheduleRequestDto">Optional job schedule details.</param>
        [HttpPost("{modsetName}/verify", Name = nameof(VerifyModset))]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public IActionResult VerifyModset(string modsetName, [FromBody] JobScheduleRequestDto jobScheduleRequestDto)
        {
            throw new NotImplementedException("Modset verification is not implemented yet.");
        }
        
        private ObjectResult TooEarly(string error)
            => StatusCode(StatusCodesExtended.Status425TooEarly,
                $"Similar request is already in processing. Please wait. Error: {error}");
    }
}
