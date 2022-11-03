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

        public JobHistoryData Data { get; set; }
    }

    internal class JobHistoryData
    {
        // public DateTime? StartedAt { get; set; }
        
        // public DateTime? EnqueuedAt { get; set; }
        
        public string? Queue { get; set; }
        
        public string? ServerId { get; set; }
        
        public string? WorkerId { get; set; }
        
        // public DateTime? SucceededAt { get; set; }
        
        public string PerformanceDuration { get; set; }
        
        public string Latency { get; set; }
        
        public string Result { get; set; }
    }
}