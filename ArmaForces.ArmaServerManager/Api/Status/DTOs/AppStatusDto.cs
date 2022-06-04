using System.Collections.Generic;
using ArmaForces.Arma.Server.Features.Servers.DTOs;
using ArmaForces.ArmaServerManager.Api.Jobs.DTOs;
using Newtonsoft.Json;

namespace ArmaForces.ArmaServerManager.Api.Status.DTOs
{
    public class AppStatusDto
    {
        [JsonProperty(Required = Required.Always)]
        public string Status { get; set; } = string.Empty;
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JobDetailsDto? CurrentJob { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<JobDetailsDto> QueuedJobs { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<ServerStatus>? Servers { get; set; }
    }
}