using System.Collections.Generic;
using ArmaForces.Arma.Server.Features.Servers.DTOs;
using ArmaForces.ArmaServerManager.Api.Status.DTOs;
using ArmaForces.ArmaServerManager.Features.Jobs.Persistence.Models;

namespace ArmaForces.ArmaServerManager.Features.Status.Models
{
    public record AppStatusDetails
    {
        public AppStatus Status { get; set; }
        
        public string? LongStatus { get; set; }
        
        public JobDetails? CurrentJob { get; set; }

        public List<JobDetails>? QueuedJobs { get; set; }

        public List<ServerStatus>? Servers { get; set; }
    }
}