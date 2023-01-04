using System;
using System.Collections.Generic;
using ArmaForces.ArmaServerManager.Features.Jobs.Models;

namespace ArmaForces.ArmaServerManager.Features.Jobs.Persistence.Models
{
    /// <summary>
    /// Details of the job like state, name or parameters.
    /// </summary>
    public class JobDetails
    {
        /// <summary>
        /// Unique ID of the job.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Name of the job.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// When the job was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// When the job is/was scheduled to run.
        /// Applicable only to non-dependant jobs.
        /// </summary>
        public DateTime? ScheduledAt { get; set; }
        
        /// <summary>
        /// When the job was enqueued.
        /// </summary>
        public DateTime? EnqueuedAt { get; set; }
        
        /// <summary>
        /// When the job started processing.
        /// </summary>
        public DateTime? StartedAt { get; set; }
        
        /// <summary>
        /// When the job was finished.
        /// </summary>
        public DateTime? FinishedAt { get; set; }

        /// <summary>
        /// Current status of the job.
        /// </summary>
        public JobStatus JobStatus { get; set; }

        /// <summary>
        /// Job parameters.
        /// </summary>
        public List<KeyValuePair<string, object>> Parameters { get; set; } = new List<KeyValuePair<string, object>>();

        /// <summary>
        /// Optional state history of the job.
        /// </summary>
        public List<JobStateHistory>? StateHistory { get; set; }
    }
}
