using System.Collections.Generic;
using ArmaForces.ArmaServerManager.Features.Jobs.Models;
using ArmaForces.ArmaServerManager.Features.Jobs.Persistence.Models;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Jobs
{
    public interface IJobsService
    {
        Result DeleteJob(string jobId);
        
        Result<JobDetails> GetJobDetails(string jobId);

        Result<List<JobDetails>> GetQueuedJobs();

        Result<List<JobDetails>> GetJobs(IEnumerable<JobStatus> statusFilter);

        Result<JobDetails?> GetCurrentJob();

        Result RequeueJob(string jobId);
    }
}
