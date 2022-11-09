using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ArmaForces.ArmaServerManager.Features.Jobs.Models;

namespace ArmaForces.ArmaServerManager.Api.Jobs.DTOs
{
    /// <summary>
    /// Details of the job.
    /// </summary>
    public class JobDetailsDto
    {
        /// <summary>
        /// ID of the job. Used for job related actions.
        /// </summary>
        [Required]
        public int Id { get; set; }
        
        /// <summary>
        /// Name of the job. Corresponds to action.
        /// </summary>
        [Required]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Time when job was created.
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Job status.
        /// </summary>
        [Required]
        public JobStatus JobStatus { get; set; }

        /// <summary>
        /// Parameters used when processing the job.
        /// </summary>
        [Required]
        public List<KeyValuePair<string, object>> Parameters { get; set; } = new List<KeyValuePair<string, object>>();
        
        /// <summary>
        /// Detailed job state history.
        /// </summary>
        public List<JobStateHistoryDto>? StateHistory { get; set; }
    }
}