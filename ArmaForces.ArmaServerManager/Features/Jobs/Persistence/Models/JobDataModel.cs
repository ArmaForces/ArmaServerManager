using System;
using System.Collections.Generic;
using ArmaForces.ArmaServerManager.Features.Jobs.Models;
using LiteDB;

namespace ArmaForces.ArmaServerManager.Features.Jobs.Persistence.Models
{
    internal class JobDataModel
    {
        [BsonField("_id")]
        public int Id { get; set; }
        
        [BsonField("StateName")]
        public JobStatus JobStatus { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public string InvocationData { get; set; }
        
        public string Arguments { get; set; }
    }

    internal class JobDataModelWithHistory : JobDataModel
    {
        public List<JobStateHistoryDataModel> StateHistory { get; set; }
    }
}