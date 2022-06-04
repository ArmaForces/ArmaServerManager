using System.Collections.Generic;
using ArmaForces.ArmaServerManager.Features.Jobs.Models;
using ArmaForces.ArmaServerManager.Features.Jobs.Persistence.Models;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Jobs
{
    public interface IJobsService
    {
        Result<JobDetails> GetJobDetails(string jobId);

        Result<List<JobDetails>> GetQueuedJobs();
        
        Result<List<JobDetails>> GetJobs(IEnumerable<JobStatus>? jobStatusEnumerable);
        
        Result<JobDetails?> GetCurrentJob();
    }
}
