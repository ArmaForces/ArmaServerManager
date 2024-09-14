using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace ArmaForces.ArmaServerManager.Api.Jobs.DTOs
{
    /// <summary>
    /// Response when job is successfully scheduled.
    /// </summary>
    public class JobScheduledResponseDto
    {
        /// <summary>
        /// Id of the job.
        /// Can be used to retrieve the job details later.
        /// </summary>
        [SwaggerSchema(Nullable = false)]
        [Required]
        public required string JobId { get; set; } = string.Empty;
    }
}
