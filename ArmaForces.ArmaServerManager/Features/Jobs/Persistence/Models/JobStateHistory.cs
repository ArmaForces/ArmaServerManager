using System;
using ArmaForces.ArmaServerManager.Features.Jobs.Models;

namespace ArmaForces.ArmaServerManager.Features.Jobs.Persistence.Models
{
    public class JobStateHistory
    {
        public int JobId { get; set;}
        
        public JobStatus Name { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
}