using System;
using ArmaForces.ArmaServerManager.Features.Jobs.Models;

namespace ArmaForces.ArmaServerManager.Features.Jobs.Persistence.Models
{
    public class JobStateHistory
    {
        public int JobId { get; set;}
        
        public JobStatus Name { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public JobStateHistoryData Data { get; set; }
    }

    public record JobStateHistoryData
    {
        
    }

    public record ScheduledJobStateHistoryData : JobStateHistoryData
    {
        public DateTime EnqueueAt { get; init; }
        
        public DateTime ScheduledAt { get; init; }
    }
}