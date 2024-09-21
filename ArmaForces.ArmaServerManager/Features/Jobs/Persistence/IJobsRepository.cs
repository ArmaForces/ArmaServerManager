using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Common.Errors;
using ArmaForces.ArmaServerManager.Features.Jobs.Models;
using ArmaForces.ArmaServerManager.Features.Jobs.Persistence.Models;
using CSharpFunctionalExtensions;
using Hangfire.Storage.Monitoring;

namespace ArmaForces.ArmaServerManager.Features.Jobs.Persistence
{
    internal interface IJobsRepository
    {
        UnitResult<IError> DeleteJob(int jobId);
        
        UnitResult<IError> DeleteJobs(IEnumerable<int> jobIds);

        Result<JobDetails?, IError> GetCurrentJob();
        
        Result<JobDetails, IError> GetJobDetails(int jobId, bool includeHistory = false);
        
        Result<List<JobDetails>, IError> GetJobs(IEnumerable<int> jobIds, bool includeHistory = false);
        
        Result<List<JobDetails>, IError> GetJobs(ISet<JobStatus> includeStatuses, bool includeHistory = false);

        UnitResult<IError> RequeueJob(int jobId);

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
