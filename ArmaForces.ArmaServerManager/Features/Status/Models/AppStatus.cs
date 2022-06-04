using System.Collections.Generic;
using ArmaForces.Arma.Server.Features.Servers.DTOs;
using ArmaForces.ArmaServerManager.Api.Jobs.DTOs;
using ArmaForces.ArmaServerManager.Features.Hangfire.Helpers;

namespace ArmaForces.ArmaServerManager.Features.Status.Models
{
    public class AppStatus
    {
        public string Status { get; set; } = string.Empty;
        
        public JobDetails? CurrentJob { get; set; }
        
        public List<ServerStatus>? Servers { get; set; }
    }
}