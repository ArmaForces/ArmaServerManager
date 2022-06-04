using System;
using System.Threading;
using ArmaForces.ArmaServerManager.Api.Jobs.DTOs;
using ArmaForces.ArmaServerManager.Api.Mods.DTOs;
using ArmaForces.ArmaServerManager.Features.Jobs;
using ArmaForces.ArmaServerManager.Infrastructure.Authentication;
using ArmaForces.ArmaServerManager.Providers;
using ArmaForces.ArmaServerManager.Services;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;

namespace ArmaForces.ArmaServerManager.Api.Mods
{
    [Route("api/mods")]
    [ApiController]
    [ApiKey]
    public class ModsController : ControllerBase
    {
        private readonly IJobsScheduler _jobsScheduler;
        private readonly IModsetProvider _modsetProvider;

        public ModsController(
            IJobsScheduler jobsScheduler,
            IModsetProvider modsetProvider)
        {
            _jobsScheduler = jobsScheduler;
            _modsetProvider = modsetProvider;
        }

        [HttpPost]
        [Route("update")]
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

        [HttpPost]
        [Route("verify")]
        public IActionResult VerifyMods([FromBody] ModsVerificationRequestDto modsVerificationRequestDto)
        {
            throw new NotImplementedException("Mods verification is not implemented yet.");
        }

        [HttpPost]
        [Route("{modsetName}/update")]
        public IActionResult UpdateModset(string modsetName, [FromBody] JobScheduleRequestDto jobScheduleRequestDto)
        {
            return UpdateMods(
                new ModsUpdateRequestDto
                {
                    ModsetName = modsetName,
                    ScheduleAt = jobScheduleRequestDto.ScheduleAt
                });
        }

        [HttpPost]
        [Route("{modsetName}/verify")]
        public IActionResult VerifyModset(string modsetName, [FromBody] JobScheduleRequestDto jobScheduleRequestDto)
        {
            throw new NotImplementedException("Modset verification is not implemented yet.");
        }
    }
}
