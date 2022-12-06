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

    public record CanceledJobStateHistoryData : JobStateHistoryData
    {
        public DateTime CanceledAt { get; init; }
    }
    
    public record AwaitingJobStateHistoryData : JobStateHistoryData
    {
        public string ParentId { get; init; }
        
        public string NextState { get; init; }
        
        public string Options { get; init; }
    }
    
    public record FailedJobStateHistoryData : JobStateHistoryData
    {
        // TODO: Fill here
    }
    
    public record DeletedJobStateHistoryData : JobStateHistoryData
    {
        public DateTime DeletedAt { get; init; }
    }
    
    public record SucceededJobStateHistoryData : JobStateHistoryData
    {
        public DateTime SucceededAt { get; init; }
        
        public ulong PerformanceDuration { get; init; }
        
        public ulong Latency { get; init; }
        
        public string Result { get; init; }
    }
    
    public record EnqueuedJobStateHistoryData : JobStateHistoryData
    {
        public DateTime EnqueuedAt { get; init; }
        
        public string Queue { get; init; }
    }
    
    public record ProcessingJobStateHistoryData : JobStateHistoryData
    {
        public DateTime StartedAt { get; init; }
        
        public string ServerId { get; init; }
        
        public string WorkerId { get; init; }
    }
    
    public record ScheduledJobStateHistoryData : JobStateHistoryData
    {
        public DateTime EnqueueAt { get; init; }
        
        public DateTime ScheduledAt { get; init; }
    }
}