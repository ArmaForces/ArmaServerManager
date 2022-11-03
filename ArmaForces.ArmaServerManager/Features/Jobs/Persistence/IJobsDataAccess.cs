using System.Collections.Generic;
using ArmaForces.ArmaServerManager.Features.Jobs.Models;
using ArmaForces.ArmaServerManager.Features.Jobs.Persistence.Models;
using CSharpFunctionalExtensions;
using Hangfire.Storage;

namespace ArmaForces.ArmaServerManager.Features.Jobs.Persistence
{
    internal interface IJobsDataAccess
    {
        List<JobDataModel> GetJobs(ISet<JobStatus> includeStatuses);
        
        Result<JobDataModel> GetJob(int jobId);
    }
}