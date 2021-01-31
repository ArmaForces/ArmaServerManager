using System.Threading;
using ArmaForces.ArmaServerManager.Features.Hangfire;
using ArmaForces.ArmaServerManager.Features.Server.DTOs;
using ArmaForces.ArmaServerManager.Infrastructure.Authentication;
using ArmaForces.ArmaServerManager.Providers;
using ArmaForces.ArmaServerManager.Services;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ArmaForces.ArmaServerManager.Controller
{
    [Route("api/Mods")]
    [ApiController]
    [ApiKey]
    public class ModsController : ControllerBase
    {
        private readonly IHangfireManager _hangfireManager;
        private readonly IModsetProvider _modsetProvider;

        public ModsController(
            IHangfireManager hangfireManager,
            IModsetProvider modsetProvider)
        {
            _hangfireManager = hangfireManager;
            _modsetProvider = modsetProvider;
        }

        [HttpPost]
        [Route("Update")]
        public IActionResult UpdateMods([FromBody] ModsUpdateRequest modsUpdateRequest)
        {
            var serverShutdownJob = _hangfireManager
                .ScheduleJob<ServerStartupService>(
                    x => x.ShutdownServer(2302, false, CancellationToken.None),
                    modsUpdateRequest.ScheduleAt);

            if (serverShutdownJob.IsFailure)
                return Problem(serverShutdownJob.Error);

            var result = modsUpdateRequest.ModsetName is null
                ? _hangfireManager.ContinueJobWith<ModsUpdateService>(
                    serverShutdownJob.Value,
                    x => x.UpdateAllMods(CancellationToken.None))
                : _hangfireManager.ContinueJobWith<ModsUpdateService>(
                    serverShutdownJob.Value,
                    x => x.UpdateModset(modsUpdateRequest.ModsetName, CancellationToken.None));

            return result.Match(
                onSuccess: Accepted,
                onFailure: error => (IActionResult)BadRequest(error));
        }
    }
}
