using System;

namespace ArmaForces.ArmaServerManager.Features.Hangfire.Persistence.Models
{
    internal class JobStateHistoryDataModel
    {
        public int JobId { get; set; }
        
        public JobStatus Name { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
}