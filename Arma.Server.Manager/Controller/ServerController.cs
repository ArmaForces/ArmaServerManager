using System;
using System.Threading;
using Arma.Server.Manager.Features.Hangfire;
using Arma.Server.Manager.Services;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Arma.Server.Manager.Controller
{
    [Route("api/ServerController")]
    [ApiController]
    public class ServerController : ControllerBase
    {
        private readonly IHangfireManager _hangfireManager;

        public ServerController(IHangfireManager hangfireManager)
        {
            _hangfireManager = hangfireManager;
        }

        // POST api/<ServerController>
        [HttpPost]
        [Route("StartServer")]
        public IActionResult StartServer([FromQuery] string modsetName, DateTime? dateTime = null)
        {
            var result = _hangfireManager.ScheduleJob<ServerStartupService>(
                x => x.StartServer(modsetName, CancellationToken.None),
                dateTime);

            return result.Match(
                onSuccess: Ok,
                onFailure: error => (IActionResult) BadRequest(error));
        }
    }
}
