using System;
using System.Collections.Generic;
using ArmaForces.ArmaServerManager.Features.Jobs.Models;

namespace ArmaForces.ArmaServerManager.Api.Jobs.DTOs
{
    /// <summary>
    /// Details of the job.
    /// </summary>
    public class JobDetailsDto
    {
        /// <summary>
        /// Name of the job. Corresponds to action.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Time when job was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Job status.
        /// </summary>
        public JobStatus JobStatus { get; set; }

        /// <summary>
        /// Parameters used when processing the job.
        /// </summary>
        public List<KeyValuePair<string, object>> Parameters { get; set; } = new List<KeyValuePair<string, object>>();
    }
}