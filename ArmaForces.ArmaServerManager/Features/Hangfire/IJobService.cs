using System.Collections.Generic;
using ArmaForces.ArmaServerManager.Features.Hangfire.Models;
using ArmaForces.ArmaServerManager.Features.Hangfire.Persistence.Models;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Hangfire
{
    public interface IJobService
    {
        Result<JobDetails> GetJobDetails(string jobId);

        Result<List<JobDetails>> GetQueuedJobs();
        
        Result<List<JobDetails>> GetJobs(IEnumerable<JobStatus>? jobStatusEnumerable);
        
        Result<JobDetails?> GetCurrentJob();
    }
}
