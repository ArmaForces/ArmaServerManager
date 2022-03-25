using System;
using System.Collections.Generic;
using ArmaForces.ArmaServerManager.Features.Hangfire;

namespace ArmaForces.ArmaServerManager.Api.Jobs.DTOs
{
    public class JobDetailsDto
    {
        public string Name { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; }
        
        public JobStatus JobStatus { get; set; }

        public List<KeyValuePair<string, object>> Parameters { get; set; } = new List<KeyValuePair<string, object>>();
    }
}