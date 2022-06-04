using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ArmaForces.ArmaServerManager.Features.Jobs.Persistence.Models;
using CSharpFunctionalExtensions;
using Hangfire.Storage.Monitoring;

namespace ArmaForces.ArmaServerManager.Features.Jobs.Persistence
{
    internal interface IJobsRepository
    {
        Result<JobDetails> GetJobDetails(string jobId);
        
        IEnumerable<EnqueuedJobDto> GetSimilarQueuedJobs<T>(
            Expression<Func<T, Task>> func,
            string queue = "default",
            int from = 0,
            int perPage = 50);
        
        IEnumerable<ScheduledJobDto> GetSimilarScheduledJobs<T>(
            Expression<Func<T, Task>> func,
            int from = 0,
            int count = 50);

        Result<List<JobDetails>> GetQueuedJobs();
        
        Result<JobDetails?> GetCurrentJob();
    }
}
