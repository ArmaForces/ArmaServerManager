using System;
using ArmaForces.ArmaServerManager.Infrastructure.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace ArmaForces.ArmaServerManager.Api.Jobs
{
    [Route("api/jobs")]
    [ApiController]
    [ApiKey]
    public class JobsController : ControllerBase
    {
        [HttpGet]
        [Route("{jobId}")]
        public IActionResult GetJobStatus(string jobId) => throw new NotImplementedException("Jobs are not implemented yet.");
    }
}
