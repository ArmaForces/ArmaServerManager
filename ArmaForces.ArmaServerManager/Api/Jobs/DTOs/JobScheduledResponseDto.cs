using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
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
        [JsonProperty(Required = Required.Always)]
        [SwaggerSchema(Nullable = false)]
        [Required]
        public string JobId { get; set; } = string.Empty;
    }
}
