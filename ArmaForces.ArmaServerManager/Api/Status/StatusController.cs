using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Servers.DTOs;
using ArmaForces.ArmaServerManager.Api.Jobs.DTOs;
using ArmaForces.ArmaServerManager.Features.Hangfire;
using ArmaForces.ArmaServerManager.Providers.Server;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ArmaForces.ArmaServerManager.Api.Status
{
   
    [Route("api/status")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly IJobService _jobService;
        private readonly IServerProvider _serverProvider;

        public StatusController(IJobService jobService, IServerProvider serverProvider)
        {
            _jobService = jobService;
            _serverProvider = serverProvider;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetStatus(
            [FromQuery] bool includeJobs = false,
            [FromQuery] bool includeServers = false)
            => await GetAppStatus(includeJobs, includeServers)
                .Map(Map)
                .Match(
                    onSuccess: Ok,
                    onFailure: error => (IActionResult) NotFound(error));

        private async Task<Result<AppStatus>> GetAppStatus(bool includeJobs, bool includeServers)
        {
            var serversStatus = includeServers
                ? await GetServersStatus()
                : null;

            var currentJob = includeJobs
                ? _jobService.GetCurrentJob()
                : null;

            return new AppStatus
            {
                Status = string.Empty,
                CurrentJob = currentJob,
                Servers = serversStatus
            };
        }

        private async Task<List<ServerStatus>> GetServersStatus()
            => (await Task.WhenAll(
                _serverProvider.GetServers()
                    .Select(x => x.GetServerStatusAsync(CancellationToken.None))))
            .ToList();

        private static AppStatusDto Map(AppStatus appStatus)
            => new AppStatusDto();
    }

    public class AppStatus
    {
        public string Status { get; set; }
        
        public JobDetailsDto? CurrentJob { get; set; }
        
        public List<ServerStatus>? Servers { get; set; }
    }
    
    public class AppStatusDto
    {
        [JsonProperty(Required = Required.Always)]
        public string Status { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JobDetailsDto? CurrentJob { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<ServerStatus>? Servers { get; set; }
    }
}