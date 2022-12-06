using System;
using ArmaForces.ArmaServerManager.Features.Jobs.Models;

namespace ArmaForces.ArmaServerManager.Api.Jobs.DTOs
{
    /// <summary>
    /// Job state history.
    /// </summary>
    public class JobStateHistoryDto
    {
        /// <summary>
        /// Job state name.
        /// </summary>
        public JobStatus StateName { get; set; }
        
        /// <summary>
        /// Time when given job state was entered.
        /// </summary>
        public DateTime EnteredAt { get; set; }
        
        /// <summary>
        /// Time when given job started processing.
        /// </summary>
        public DateTime? StartedAt { get; set; }
        
        /// <summary>
        /// Time when given job was successfully completed.
        /// </summary>
        public DateTime? SucceededAt { get; set; }
        
        /// <summary>
        /// Time when given job was moved into queue.
        /// </summary>
        public DateTime? EnqueuedAt { get; set; }
        
        /// <summary>
        /// Time when given job was scheduled.
        /// </summary>
        public DateTime? ScheduledAt { get; set; }
        
        /// <summary>
        /// Time when given job should be enqueued.
        /// </summary>
        public DateTime? EnqueueAt { get; set; }
        
        /// <summary>
        /// Result of a finished job.
        /// </summary>
        public string? Result { get; set; }
    }
}