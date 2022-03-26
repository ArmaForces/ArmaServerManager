using System.Collections.Generic;
using ArmaForces.ArmaServerManager.Features.Hangfire.Helpers;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Hangfire
{
    public interface IJobService
    {
        Result<JobDetails> GetJobDetails(string jobId);
        
        Result<List<JobDetails>> GetJobs(IEnumerable<JobStatus>? jobStatusEnumerable);
        Result<JobDetails?> GetCurrentJob();
    }
}
