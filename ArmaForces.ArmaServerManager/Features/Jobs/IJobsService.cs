using System.Collections.Generic;
using ArmaForces.ArmaServerManager.Features.Jobs.Models;
using ArmaForces.ArmaServerManager.Features.Jobs.Persistence.Models;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Jobs
{
    public interface IJobsService
    {
        Result DeleteJob(int jobId);
        
        Result<JobDetails> GetJobDetails(int jobId, bool includeHistory = false);

        Result<List<JobDetails>> GetQueuedJobs();

        Result<List<JobDetails>> GetJobs(
            IEnumerable<int> jobIds,
            IEnumerable<JobStatus> statusFilter,
            bool includeHistory = false);

        Result<JobDetails?> GetCurrentJob();

        Result RequeueJob(int jobId);
    }
}
