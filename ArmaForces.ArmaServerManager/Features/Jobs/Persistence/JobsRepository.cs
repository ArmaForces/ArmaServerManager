using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ArmaForces.ArmaServerManager.Features.Jobs.Helpers;
using ArmaForces.ArmaServerManager.Features.Jobs.Models;
using ArmaForces.ArmaServerManager.Features.Jobs.Persistence.Models;
using CSharpFunctionalExtensions;
using Hangfire;
using Hangfire.Common;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;

namespace ArmaForces.ArmaServerManager.Features.Jobs.Persistence
{
    internal class JobsRepository : IJobsRepository
    {
        private readonly IHangfireBackgroundJobClientWrapper _backgroundJobClientWrapper;
        private readonly IJobsDataAccess _jobsDataAccess;
        private readonly IMonitoringApi _monitoringApi;
        private readonly IStorageConnection _storageConnection;

        public JobsRepository(
            IHangfireBackgroundJobClientWrapper backgroundJobClientWrapper,
            IJobsDataAccess jobsDataAccess,
            IMonitoringApi monitoringApi,
            IStorageConnection storageConnection)
        {
            _backgroundJobClientWrapper = backgroundJobClientWrapper;
            _jobsDataAccess = jobsDataAccess;
            _monitoringApi = monitoringApi;
            _storageConnection = storageConnection;
        }

        public Result DeleteJob(string jobId)
            => _backgroundJobClientWrapper.Delete(jobId);

        public Result<JobDetails?> GetCurrentJob()
        {
            // TODO: Handle manager process restart (it can cause job to be stuck in processing state for a while)
            var currentJobId = _monitoringApi.ProcessingJobs(0, 1)
                .Select(x => x.Key)
                .SingleOrDefault();
            
            return currentJobId is null
                ? Result.Success<JobDetails?>(null)
                : GetJobDetails(currentJobId);
        }

        public Result<JobDetails> GetJobDetails(string jobId)
        {
            var jobData = _storageConnection.GetJobData(jobId);

            return jobData != null
                ? CreateJobDetails(jobData)
                : Result.Failure<JobDetails>("Job does not exist.");
        }

        public Result<List<JobDetails>> GetQueuedJobs()
        {
            return _jobsDataAccess.GetJobs(new List<JobStatus>
                {
                    JobStatus.Awaiting,
                    JobStatus.Enqueued,
                    JobStatus.Scheduled
                })
                .Select(GetJobDetails)
                .Combine()
                .Map(x => x.ToList());
        }

        public Result RequeueJob(string jobId)
            => _backgroundJobClientWrapper.Requeue(jobId);

        public IEnumerable<EnqueuedJobDto> GetSimilarQueuedJobs<T>(
            Expression<Func<T, Task>> func,
            string queue = "default",
            int from = 0,
            int perPage = 50)
            => GetQueuedJobs(
                    queue,
                    from,
                    perPage)
                .Where(x => JobMatchesMethod(x.Job, func));

        public IEnumerable<ScheduledJobDto> GetSimilarScheduledJobs<T>(
            Expression<Func<T, Task>> func,
            int from = 0,
            int count = 50)
            => GetScheduledJobs(from, count)
                .Where(x => JobMatchesMethod(x.Job, func));

        private Result<JobDetails> GetJobDetails(JobDataModel jobDataModel)
            => GetJobDetails(jobDataModel.Id.ToString());

        private static JobDetails CreateJobDetails(JobData jobData) => new JobDetails
        {
            Name = jobData.Job.ToString().Split('.').Last(),
            CreatedAt = jobData.CreatedAt,
            JobStatus = JobStatusParser.ParseJobStatus(jobData.State),
            Parameters = jobData.Job.Method.GetParameters()
                .Zip(jobData.Job.Args, (parameterInfo, parameterValue) => new KeyValuePair<string, object>(parameterInfo.Name ?? "unknown", parameterValue))
                .Where(x => x.Key != "cancellationToken")
                .ToList()
        };

        /// <summary>
        ///     Checks if given <paramref name="job" /> will execute using <typeparamref name="T" />
        ///     and uses the same method as in <paramref name="func" />.
        /// </summary>
        private static bool JobMatchesMethod<T>(Job job, Expression<Func<T, Task>> func)
            => job.Type == typeof(T) && func.Body.ToString().Contains(job.Method.Name);
        
        private IEnumerable<EnqueuedJobDto> GetQueuedJobs(
            string queue = "default",
            int from = 0,
            int perPage = 50)
            => _monitoringApi
                .EnqueuedJobs(
                    queue,
                    from,
                    perPage)
                .Select(x => x.Value);


        private IEnumerable<ScheduledJobDto> GetScheduledJobs(int from = 0, int count = 50)
            => _monitoringApi
                .ScheduledJobs(from, count)
                .Select(x => x.Value);
    }
}
