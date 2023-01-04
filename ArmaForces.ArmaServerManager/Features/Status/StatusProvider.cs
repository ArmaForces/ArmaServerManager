using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Servers.DTOs;
using ArmaForces.ArmaServerManager.Api.Status.DTOs;
using ArmaForces.ArmaServerManager.Features.Jobs;
using ArmaForces.ArmaServerManager.Features.Jobs.Extensions;
using ArmaForces.ArmaServerManager.Features.Jobs.Persistence.Models;
using ArmaForces.ArmaServerManager.Features.Servers.Providers;
using ArmaForces.ArmaServerManager.Features.Status.Models;
using ArmaForces.ArmaServerManager.Services;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Status
{
    internal class StatusProvider : IStatusProvider
    {
        private readonly IJobsService _jobsService;
        private readonly IServerProvider _serverProvider;

        public StatusProvider(IJobsService jobsService, IServerProvider serverProvider)
        {
            _jobsService = jobsService;
            _serverProvider = serverProvider;
        }

        public async Task<Result<AppStatusDetails>> GetAppStatus(IEnumerable<AppStatusIncludes> include)
        {
            var status = CreateSimpleAppStatus(GetCurrentJobDetails());
            
            var appStatusIncludesEnumerable = include as AppStatusIncludes[] ?? include.ToArray();
            if (appStatusIncludesEnumerable.Contains(AppStatusIncludes.Jobs))
            {
                status = status with
                {
                    QueuedJobs = GetQueuedJobDetails()
                };
            }

            if (appStatusIncludesEnumerable.Contains(AppStatusIncludes.Servers))
            {
                status = status with
                {
                    Servers = await GetServersStatus()
                };
            }

            return status;
        }

        private static AppStatusDetails CreateSimpleAppStatus(JobDetails? currentJobDetails)
            => GetCurrentStatus(currentJobDetails) with
            {
                CurrentJob = currentJobDetails
            };

        private List<JobDetails> GetQueuedJobDetails()
        {
            return _jobsService.GetQueuedJobs()
                .Match(
                    onSuccess: x => x,
                    onFailure: null)
                .ToList();
        }
        
        private JobDetails? GetCurrentJobDetails()
        {
            return _jobsService.GetCurrentJob()
                .Match(
                    onSuccess: x => x,
                    onFailure: null);
        }

        private async Task<List<ServerStatus>> GetServersStatus()
            => (await Task.WhenAll(
                    _serverProvider.GetServers()
                        .Select(x => x.GetServerStatusAsync(CancellationToken.None))))
                .ToList();

        private static AppStatusDetails GetCurrentStatus(JobDetails? currentJobDetails)
        {
            if (currentJobDetails is null) return new AppStatusDetails();

            return currentJobDetails.Name switch
            {
                nameof(ModsUpdateService.UpdateAllMods) => new AppStatusDetails
                {
                    Status = AppStatus.UpdatingMods,
                    LongStatus = "Updating all mods"
                },
                nameof(ModsUpdateService.UpdateModset) => new AppStatusDetails
                {
                    Status = AppStatus.UpdatingMods,
                    LongStatus = $"Updating mods for {currentJobDetails.GetParameterValue("modsetName")}"
                },
                nameof(ServerStartupService.StartServerForMission) => new AppStatusDetails
                {
                    Status = AppStatus.StartingServer,
                    LongStatus = $"Starting server for {currentJobDetails.GetParameterValue("missionTitle")}"
                },
                nameof(ServerStartupService.StartServer) => new AppStatusDetails
                {
                    Status = AppStatus.StartingServer,
                    LongStatus = $"Starting server on {currentJobDetails.GetParameterValue("modsetName")}"
                },
                _ => new AppStatusDetails
                {
                    Status = AppStatus.Busy,
                    LongStatus = $"Operation {currentJobDetails.Name} in progress"
                }
            };
        }
    }
}