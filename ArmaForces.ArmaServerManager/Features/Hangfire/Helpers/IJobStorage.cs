using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Hangfire.Storage.Monitoring;

namespace ArmaForces.ArmaServerManager.Features.Hangfire.Helpers
{
    public interface IJobStorage
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
    }
}
