using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ArmaForces.ArmaServerManager.Api.Jobs.DTOs;
using ArmaForces.ArmaServerManager.Api.Servers.DTOs;
using ArmaForces.ArmaServerManager.Features.Jobs.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace ArmaForces.ArmaServerManager.Api.Status.DTOs
{
    /// <summary>
    /// Application status details, optionally including jobs and servers details.
    /// </summary>
    public class AppStatusDetailsDto
    {
        /// <summary>
        /// Short description what the application is doing currently.
        /// </summary>
        [SwaggerSchema(Nullable = false)]
        [Required]
        public required AppStatus Status { get; set; } = AppStatus.Idle;

        /// <summary>
        /// Longer description if needed.
        /// </summary>
        public string? StatusLong { get; set; }
        
        /// <summary>
        /// Currently processed job, if any.
        /// </summary>
        public JobDetailsDto? CurrentJob { get; set; }
        
        // TODO: Enums have full name in redoc.
        /// <summary>
        /// Jobs in <see cref="JobStatus.Awaiting"/>, <see cref="JobStatus.Enqueued"/> or <see cref="JobStatus.Scheduled"/> state.
        /// </summary>
        public List<JobDetailsDto>? QueuedJobs { get; set; }

        /// <summary>
        /// Currently running servers details.
        /// </summary>
        public List<ServerStatusDto>? Servers { get; set; }
    }
}