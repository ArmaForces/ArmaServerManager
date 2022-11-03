using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ArmaForces.ArmaServerManager.Features.Jobs.Models;
using ArmaForces.ArmaServerManager.Features.Jobs.Persistence.Models;
using CSharpFunctionalExtensions;
using Hangfire.Storage.Monitoring;

namespace ArmaForces.ArmaServerManager.Features.Jobs.Persistence
{
    internal interface IJobsRepository
    {
        Result DeleteJob(int jobId);

        Result<JobDetails?> GetCurrentJob();
        
        Result<JobDetails> GetJobDetails(int jobId, bool includeHistory = false);
        
        Result<List<JobDetails>> GetJobs(ISet<JobStatus> includeStatuses, bool includeHistory = false);

        Result RequeueJob(int jobId);

        IEnumerable<EnqueuedJobDto> GetSimilarQueuedJobs<T>(
            Expression<Func<T, Task>> func,
            string queue = "default",
            int from = 0,
            int perPage = 50);

        IEnumerable<ScheduledJobDto> GetSimilarScheduledJobs<T>(
            Expression<Func<T, Task>> func,
            int from = 0,
            int count = 50);
    }
}
