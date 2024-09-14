using ArmaForces.ArmaServerManager.Features.Jobs.Models;

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
        public required string JobId { get; set; } = string.Empty;

        /// <summary>
        /// Status of the job.
        /// </summary>
        public required JobStatus Status { get; set; }
    }
}
