using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Servers.DTOs;
using ArmaForces.ArmaServerManager.Features.Hangfire;
using ArmaForces.ArmaServerManager.Features.Hangfire.Helpers;
using ArmaForces.ArmaServerManager.Features.Status.Models;
using ArmaForces.ArmaServerManager.Providers.Server;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Status
{
    public class StatusProvider
    {
        private readonly IJobService _jobService;
        private readonly IServerProvider _serverProvider;

        public StatusProvider(IJobService jobService, IServerProvider serverProvider)
        {
            _jobService = jobService;
            _serverProvider = serverProvider;
        }

        public async Task<Result<AppStatus>> GetAppStatus(bool includeJobs, bool includeServers) =>
            includeJobs && includeServers
                ? await CreateFullAppStatus()
                : !includeJobs && !includeServers
                    ? await CreateSimpleAppStatus()
                    : includeJobs
                        ? await CreateJobsStatus()
                        : await CreateServersStatus();

        private async Task<AppStatus> CreateJobsStatus()
            => new AppStatus
            {
                Status = await GetCurrentStatus(),
                CurrentJob = await GetCurrentJobDetails()
            };

        private async Task<AppStatus> CreateServersStatus()
            => new AppStatus
            {
                Status = await GetCurrentStatus(),
                Servers = await GetServersStatus()
            };

        private async Task<AppStatus> CreateSimpleAppStatus()
            => new AppStatus
            {
                Status = await GetCurrentStatus(),
            };

        private async Task<AppStatus> CreateFullAppStatus()
            => new AppStatus
            {
                Status = await GetCurrentStatus(),
                CurrentJob = await GetCurrentJobDetails(),
                Servers = await GetServersStatus()
            };

        private async Task<string> GetCurrentStatus()
        {
            return "Idle";
        }

        private async Task<JobDetails?> GetCurrentJobDetails()
        {
            return _jobService.GetCurrentJob()
                .Match(
                    onSuccess: x => x,
                    onFailure: null);
        }

        private async Task<List<ServerStatus>> GetServersStatus()
            => (await Task.WhenAll(
                    _serverProvider.GetServers()
                        .Select(x => x.GetServerStatusAsync(CancellationToken.None))))
                .ToList();
    }
}