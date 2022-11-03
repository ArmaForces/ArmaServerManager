using System.Linq;
using ArmaForces.ArmaServerManager.Api.Jobs.Mappers;
using ArmaForces.ArmaServerManager.Api.Servers;
using ArmaForces.ArmaServerManager.Api.Status.DTOs;
using ArmaForces.ArmaServerManager.Features.Status.Models;

namespace ArmaForces.ArmaServerManager.Api.Status.Mappers
{
    internal static class StatusMapper
    {
        public static AppStatusDetailsDto Map(AppStatusDetails appStatusDetails)
            => new AppStatusDetailsDto
            {
                Status = appStatusDetails.Status,
                CurrentJob = appStatusDetails.CurrentJob is null
                    ? null
                    : JobsMapper.Map(appStatusDetails.CurrentJob),
                QueuedJobs = appStatusDetails.QueuedJobs?
                    .Select(JobsMapper.Map)
                    .ToList(),
                Servers = appStatusDetails.Servers?
                    .Select(ServerStatusMapper.Map)
                    .ToList()
            };
    }
}