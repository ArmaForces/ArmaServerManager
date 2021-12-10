using System;
using System.Collections.Generic;

namespace ArmaForces.ArmaServerManager.Features.Hangfire.Helpers
{
    public class JobDetails
    {
        public string Name { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; }

        public JobStatus JobStatus { get; set; }

        public List<KeyValuePair<string, object>> Parameters { get; set; } = new List<KeyValuePair<string, object>>();
    }
}
