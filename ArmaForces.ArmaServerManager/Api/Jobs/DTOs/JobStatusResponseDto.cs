using ArmaForces.ArmaServerManager.Features.Jobs.Models;
using Newtonsoft.Json;

namespace ArmaForces.ArmaServerManager.Api.Jobs.DTOs
{
    public class JobStatusResponseDto
    {
        [JsonProperty(Required = Required.Always)]
        public string JobId { get; set; } = string.Empty;

        [JsonProperty(Required = Required.Always)]
        public JobStatus Status { get; set; }
    }
}
