using System;
using ArmaForces.ArmaServerManager.Features.Jobs.Models;
using LiteDB;

namespace ArmaForces.ArmaServerManager.Features.Jobs.Persistence.Models
{
    internal class JobStateHistoryDataModel
    {
        [BsonField("Name")]
        public JobStatus StateName { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        // public DateTime ExpireAt { get; set; }

        /// <summary>
        /// Additional state specific data.
        /// </summary>
        public JobHistoryData Data { get; set; }
    }

    /// <summary>
    /// State specific data.
    /// </summary>
    internal class JobHistoryData
    {
        /// <summary>
        /// When job was deleted.
        /// </summary>
        public DateTime? DeletedAt { get; set; }
        
        /// <summary>
        /// When job was enqueued.
        /// </summary>
        public DateTime? EnqueuedAt { get; set; }
        
        /// <summary>
        /// When job should be enqueued.
        /// </summary>
        public DateTime? EnqueueAt { get; set; }
        
        public string? Queue { get; set; }
        
        public string? ServerId { get; set; }
        
        public string? WorkerId { get; set; }
        
        /// <summary>
        /// When job was created/scheduled.
        /// </summary>
        public DateTime? ScheduledAt { get; set; }
        
        /// <summary>
        /// When job was started.
        /// </summary>
        public DateTime? StartedAt { get; set; }
        
        /// <summary>
        /// When job was succeeded.
        /// </summary>
        public DateTime? SucceededAt { get; set; }
        
        public string PerformanceDuration { get; set; }
        
        public string Latency { get; set; }
        
        public string Result { get; set; }
    }
}