using System;
using System.Collections.Generic;
using ArmaForces.ArmaServerManager.Features.Jobs.Models;

namespace ArmaForces.ArmaServerManager.Features.Jobs.Persistence.Models
{
    public class JobDetails
    {
        public string Name { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; }

        public JobStatus JobStatus { get; set; }

        public List<KeyValuePair<string, object>> Parameters { get; set; } = new List<KeyValuePair<string, object>>();
    }
}
