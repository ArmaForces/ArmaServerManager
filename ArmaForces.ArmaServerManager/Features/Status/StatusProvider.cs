﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Servers.DTOs;
using ArmaForces.ArmaServerManager.Features.Hangfire;
using ArmaForces.ArmaServerManager.Features.Hangfire.Extensions;
using ArmaForces.ArmaServerManager.Features.Hangfire.Helpers;
using ArmaForces.ArmaServerManager.Features.Status.Models;
using ArmaForces.ArmaServerManager.Providers.Server;
using ArmaForces.ArmaServerManager.Services;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Status
{
    internal class StatusProvider : IStatusProvider
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
                ? await CreateFullAppStatus(GetCurrentJobDetails())
                : !includeJobs && !includeServers
                    ? CreateSimpleAppStatus(GetCurrentJobDetails())
                    : includeJobs
                        ? CreateJobsStatus(GetCurrentJobDetails())
                        : await CreateServersStatus(GetCurrentJobDetails());

        private AppStatus CreateJobsStatus(JobDetails? currentJobDetails)
            => new AppStatus
            {
                Status = GetCurrentStatus(currentJobDetails),
                CurrentJob = GetCurrentJobDetails(),
                QueuedJobs = _jobService.GetQueuedJobs()
                    .Match(x => x, null)
            };

        private async Task<AppStatus> CreateServersStatus(JobDetails? currentJobDetails)
            => new AppStatus
            {
                Status = GetCurrentStatus(currentJobDetails),
                CurrentJob = currentJobDetails,
                Servers = await GetServersStatus()
            };

        private AppStatus CreateSimpleAppStatus(JobDetails? currentJobDetails)
            => new AppStatus
            {
                Status = GetCurrentStatus(currentJobDetails),
                CurrentJob = currentJobDetails
            };

        private async Task<AppStatus> CreateFullAppStatus(JobDetails? currentJobDetails)
            => new AppStatus
            {
                Status = GetCurrentStatus(currentJobDetails),
                CurrentJob = currentJobDetails,
                QueuedJobs = _jobService.GetQueuedJobs()
                    .Match(x => x, null),
                Servers = await GetServersStatus()
            };

        private JobDetails? GetCurrentJobDetails()
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

        private static string GetCurrentStatus(JobDetails? currentJobDetails)
        {
            if (currentJobDetails is null) return "Idle";

            return currentJobDetails.Name switch
            {
                nameof(ModsUpdateService.UpdateAllMods) => $"Updating all mods",
                nameof(ModsUpdateService.UpdateModset) =>
                    $"Updating mods for {currentJobDetails.GetParameterValue("modsetName")}",
                nameof(ServerStartupService.StartServerForMission) =>
                    $"Starting server for {currentJobDetails.GetParameterValue("missionTitle")}",
                nameof(ServerStartupService.StartServer) =>
                    $"Starting server on {currentJobDetails.GetParameterValue("modsetName")}",
                _ => $"Operation {currentJobDetails.Name} in progress"
            };
        }
    }
}