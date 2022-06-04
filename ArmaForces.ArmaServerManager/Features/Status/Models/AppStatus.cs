using System.Collections.Generic;
using ArmaForces.Arma.Server.Features.Servers.DTOs;
using ArmaForces.ArmaServerManager.Features.Hangfire.Persistence.Models;

namespace ArmaForces.ArmaServerManager.Features.Status.Models
{
    public class AppStatus
    {
        public string Status { get; set; } = string.Empty;
        
        public JobDetails? CurrentJob { get; set; }

        public List<JobDetails> QueuedJobs { get; set; } = new List<JobDetails>();

        public List<ServerStatus>? Servers { get; set; }
    }
}