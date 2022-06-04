using System;
using System.Linq;
using System.Threading.Tasks;
using ArmaForces.ArmaServerManager.Api.Jobs.DTOs;
using ArmaForces.ArmaServerManager.Api.Status.DTOs;
using ArmaForces.ArmaServerManager.Features.Hangfire;
using ArmaForces.ArmaServerManager.Features.Status;
using ArmaForces.ArmaServerManager.Features.Status.Models;
using ArmaForces.ArmaServerManager.Providers.Server;
using ArmaForces.ArmaServerManager.Services;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;

namespace ArmaForces.ArmaServerManager.Api.Status
{
   
    [Route("api/status")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly StatusProvider _statusProvider;

        public StatusController(
            IJobService jobService,
            IServerProvider serverProvider)
        {
            _statusProvider = new StatusProvider(jobService, serverProvider);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetStatus(
            [FromQuery] bool includeJobs = false,
            [FromQuery] bool includeServers = false)
            => await _statusProvider.GetAppStatus(includeJobs, includeServers)
                .Map(Map)
                .Match(
                    onSuccess: Ok,
                    onFailure: error => (IActionResult) NotFound(error));

        private static AppStatusDto Map(AppStatus appStatus)
            => new AppStatusDto
            {
                Status = appStatus.Status,
                CurrentJob = appStatus.CurrentJob is null
                    ? null
                    : new JobDetailsDto
                    {
                        Name = appStatus.CurrentJob.Name,
                        JobStatus = appStatus.CurrentJob.JobStatus,
                        CreatedAt = appStatus.CurrentJob.CreatedAt,
                        Parameters = appStatus.CurrentJob.Parameters
                    },
                QueuedJobs = appStatus.QueuedJobs.Select(x => new JobDetailsDto
                {
                    Name = x.Name,
                    JobStatus = x.JobStatus,
                    CreatedAt = x.CreatedAt,
                    Parameters = x.Parameters
                }).ToList(),
                Servers = appStatus.Servers
            };
    }
}