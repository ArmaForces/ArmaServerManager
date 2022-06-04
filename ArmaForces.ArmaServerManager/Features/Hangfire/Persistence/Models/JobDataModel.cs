using System;
using System.Collections.Generic;
using LiteDB;

namespace ArmaForces.ArmaServerManager.Features.Hangfire.Persistence.Models
{
    internal class JobDataModel
    {
        [BsonField("_id")]
        public int Id { get; set; }
        
        [BsonField("StateName")]
        public JobStatus JobStatus { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public List<JobStateHistoryDataModel> StateHistory { get; set; }
    }
}