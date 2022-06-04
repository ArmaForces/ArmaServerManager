using ArmaForces.ArmaServerManager.Features.Jobs.Models;
using Newtonsoft.Json;

namespace ArmaForces.ArmaServerManager.Api.Jobs.DTOs
{
    /// <summary>
    /// Simple job status response.
    /// </summary>
    public class JobStatusResponseDto
    {
        /// <summary>
        /// Id of the job.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string JobId { get; set; } = string.Empty;

        /// <summary>
        /// Status of the job.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public JobStatus Status { get; set; }
    }
}
