using System.Linq;
using ArmaForces.ArmaServerManager.Api.Jobs.Mappers;
using ArmaForces.ArmaServerManager.Api.Servers;
using ArmaForces.ArmaServerManager.Api.Status.DTOs;
using ArmaForces.ArmaServerManager.Features.Status.Models;

namespace ArmaForces.ArmaServerManager.Api.Status.Mappers
{
    internal static class StatusMapper
    {
        public static AppStatusDto Map(AppStatus appStatus)
            => new AppStatusDto
            {
                Status = appStatus.Status,
                CurrentJob = appStatus.CurrentJob is null
                    ? null
                    : JobsMapper.Map(appStatus.CurrentJob),
                QueuedJobs = appStatus.QueuedJobs
                    .Select(JobsMapper.Map)
                    .ToList(),
                Servers = appStatus.Servers?
                    .Select(ServerStatusMapper.Map)
                    .ToList()
            };
    }
}