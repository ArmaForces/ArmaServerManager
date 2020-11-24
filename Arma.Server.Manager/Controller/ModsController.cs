using System.Threading;
using Arma.Server.Manager.Features.Hangfire;
using Arma.Server.Manager.Features.Server.DTOs;
using Arma.Server.Manager.Providers;
using Arma.Server.Manager.Services;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Arma.Server.Manager.Controller
{
    [Route("api/Mods")]
    [ApiController]
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
            var result = modsUpdateRequest.ModsetName is null
                ? _hangfireManager.ScheduleJob<ModsUpdateService>(
                    x => x.UpdateAllMods(CancellationToken.None),
                    modsUpdateRequest.ScheduleAt)
                : _hangfireManager.ScheduleJob<ModsUpdateService>(
                    x => x.UpdateModset(modsUpdateRequest.ModsetName, CancellationToken.None),
                    modsUpdateRequest.ScheduleAt);

            return result.Match(
                onSuccess: Ok,
                onFailure: error => (IActionResult)BadRequest(error));
        }
    }
}
