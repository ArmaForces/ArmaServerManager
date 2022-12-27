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
        
        public DateTime ExpireAt { get; set; }

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
        /// When job was deleted, in milliseconds epoch.
        /// </summary>
        public ulong? DeletedAt { get; set; }
        
        /// <summary>
        /// When job was enqueued, in milliseconds epoch.
        /// </summary>
        public ulong? EnqueuedAt { get; set; }
        
        /// <summary>
        /// When job should be enqueued, in milliseconds epoch.
        /// </summary>
        public ulong? EnqueueAt { get; set; }
        
        public ulong? FailedAt { get; set; }
        
        public string? Queue { get; set; }
        
        public string? ServerId { get; set; }
        
        public string? WorkerId { get; set; }
        
        /// <summary>
        /// When job was created/scheduled, in milliseconds epoch.
        /// </summary>
        public ulong? ScheduledAt { get; set; }
        
        /// <summary>
        /// When job was started, in milliseconds epoch.
        /// </summary>
        public ulong? StartedAt { get; set; }
        
        /// <summary>
        /// When job was succeeded, in milliseconds epoch.
        /// </summary>
        public ulong? SucceededAt { get; set; }
        
        public ulong? PerformanceDuration { get; set; }
        
        public ulong? Latency { get; set; }
        
        public string? Result { get; set; }
        public string? NextState { get; set; }
        public string? Options { get; set; }
        public string? ParentId { get; set; }
        
        public string? ExceptionType { get; set; }
        public string? ExceptionMessage { get; set; }
        public string? ExceptionDetails { get; set; }
    }
}