using System;
using System.Collections.Generic;
using ArmaForces.Arma.Server.Common.Errors;
using ArmaForces.ArmaServerManager.Features.Jobs.Models;
using ArmaForces.ArmaServerManager.Features.Jobs.Persistence.Models;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Jobs
{
    public interface IJobsService
    {
        UnitResult<IError> DeleteJob(int jobId);
        
        Result<JobDetails, IError> GetJobDetails(int jobId, bool includeHistory = false);

        Result<List<JobDetails>, IError> GetQueuedJobs();

        Result<List<JobDetails>, IError> GetJobs(
            IEnumerable<int> jobIds,
            IEnumerable<JobStatus> statusFilter,
            bool includeHistory = false);

        Result<JobDetails?, IError> GetCurrentJob();

        UnitResult<IError> RequeueJob(int jobId);
        
        UnitResult<IError> DeleteJobs(DateTime deleteFrom, DateTime deleteTo);
    }
}
