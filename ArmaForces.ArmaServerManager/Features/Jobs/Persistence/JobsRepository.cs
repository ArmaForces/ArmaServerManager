using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ArmaForces.ArmaServerManager.Features.Jobs.Helpers;
using ArmaForces.ArmaServerManager.Features.Jobs.Models;
using ArmaForces.ArmaServerManager.Features.Jobs.Persistence.Extensions;
using ArmaForces.ArmaServerManager.Features.Jobs.Persistence.Models;
using CSharpFunctionalExtensions;
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

        public JobsRepository(
            IHangfireBackgroundJobClientWrapper backgroundJobClientWrapper,
            IJobsDataAccess jobsDataAccess,
            IMonitoringApi monitoringApi)
        {
            _backgroundJobClientWrapper = backgroundJobClientWrapper;
            _jobsDataAccess = jobsDataAccess;
            _monitoringApi = monitoringApi;
        }

        public Result DeleteJob(int jobId)
            => _backgroundJobClientWrapper.Delete(jobId);

        public Result DeleteJobs(IEnumerable<int> jobIds)
            => jobIds.Select(jobId => _backgroundJobClientWrapper.Delete(jobId)
                    .OnFailureCompensate(_ => _backgroundJobClientWrapper.Delete(jobId)))
                .Combine();

        public Result<JobDetails?> GetCurrentJob()
        {
            // TODO: Handle manager process restart (it can cause job to be stuck in processing state for a while)
            var currentJobId = _monitoringApi.ProcessingJobs(0, 1)
                .Select(x => x.Key)
                .SingleOrDefault();
            
            // Disable Nullability warning as compiler cannot handle this case properly
#pragma warning disable CS8619
            return currentJobId is null
                ? Result.Success<JobDetails?>(null)
                : GetJobDetails(int.Parse(currentJobId));
#pragma warning restore CS8619
        }

        public Result<JobDetails> GetJobDetails(int jobId, bool includeHistory = false)
            => includeHistory
                ? _jobsDataAccess.GetJob<JobDataModelWithHistory>(jobId)
                    .Map(CreateJobDetails)
                : _jobsDataAccess.GetJob<JobDataModel>(jobId)
                    .Map(CreateJobDetails);

        public Result<List<JobDetails>> GetJobs(IEnumerable<int> jobIds, bool includeHistory = false)
            => includeHistory
                ? GetJobs<JobDataModelWithHistory>(x => jobIds.Contains(x.Id))
                : GetJobs<JobDataModel>(x => jobIds.Contains(x.Id));

        public Result<List<JobDetails>> GetJobs(ISet<JobStatus> includeStatuses, bool includeHistory = false)
            => includeHistory
                ? GetJobs<JobDataModelWithHistory>(x => includeStatuses.Contains(x.JobStatus))
                : GetJobs<JobDataModel>(x => includeStatuses.Contains(x.JobStatus));

        public Result RequeueJob(int jobId)
            => _backgroundJobClientWrapper.Requeue(jobId);

        public IEnumerable<EnqueuedJobDto> GetSimilarQueuedJobs<T>(
            Expression<Func<T, Task>> func,
            string queue = "default",
            int from = 0,
            int perPage = 50)
            => GetQueuedJobs(queue, from, perPage)
                .Where(x => JobMatchesMethod(x.Job, func));

        public IEnumerable<ScheduledJobDto> GetSimilarScheduledJobs<T>(
            Expression<Func<T, Task>> func,
            int from = 0,
            int count = 50)
            => GetScheduledJobs(from, count)
                .Where(x => JobMatchesMethod(x.Job, func));
        
        private Result<List<JobDetails>> GetJobs<T>(Expression<Func<T, bool>> filterExpression) where T : JobDataModel
            => _jobsDataAccess.GetJobs(filterExpression)
                .Select(CreateJobDetails)
                .ToList();
        
        private static JobDetails CreateJobDetails(JobDataModel jobDataModel)
        {
            var job = SerializationHelper
                .Deserialize<InvocationData>(jobDataModel.InvocationData, SerializationOption.User)
                .DeserializeJob();

            var history = jobDataModel is JobDataModelWithHistory jobDataModelWithHistory
                ? jobDataModelWithHistory.StateHistory
                    .Select(x => new JobStateHistory
                    {
                        JobId = jobDataModel.Id,
                        Name = x.StateName,
                        CreatedAt = x.CreatedAt,
                        Data = GetJobStateHistoryData(x)
                    }).ToList()
                : null;
            
            return new JobDetails
            {
                Id = jobDataModel.Id,
                Name = job.ToString().Split('.').Last(),
                CreatedAt = jobDataModel.CreatedAt,
                EnqueuedAt = history?.GetLastEnqueuedState()?.EnqueuedAt,
                ScheduledAt = history?.GetLastScheduledState()?.EnqueueAt,
                StartedAt = history?.GetLastProcessingState()?.StartedAt,
                FinishedAt = history?.GetLastSucceededState()?.SucceededAt
                             ?? history?.GetLastFailedState()?.FailedAt,
                JobStatus = jobDataModel.JobStatus,
                Parameters = job.Method.GetParameters()
                    .Zip(job.Args,
                        (parameterInfo, parameterValue) =>
                            new KeyValuePair<string, object>(parameterInfo.Name ?? "unknown", parameterValue))
                    .Where(x => x.Key != "cancellationToken")
                    .ToList(),
                StateHistory = history
            };
        }

        private static JobStateHistoryData GetJobStateHistoryData(JobStateHistoryDataModel jobStateHistoryDataModel)
        {
            DateTime ConvertFromEpoch(ulong? epoch) => epoch.HasValue
                    ? DateTimeOffset.FromUnixTimeMilliseconds((long) epoch).DateTime
                    // TODO: Change exception type
                    : throw new InvalidDataException("Expected epoch time, but found none.");

            return jobStateHistoryDataModel.StateName switch
            {
                JobStatus.Scheduled => new ScheduledJobStateHistoryData
                {
                    EnqueueAt = ConvertFromEpoch(jobStateHistoryDataModel.Data.EnqueueAt)
                },
                JobStatus.Awaiting => new AwaitingJobStateHistoryData
                {
                    NextState = jobStateHistoryDataModel.Data.NextState!,
                    Options = jobStateHistoryDataModel.Data.Options!,
                    ParentId = jobStateHistoryDataModel.Data.ParentId!
                },
                JobStatus.Deleted => new DeletedJobStateHistoryData
                {
                    DeletedAt  = ConvertFromEpoch(jobStateHistoryDataModel.Data.DeletedAt)
                },
                JobStatus.Enqueued => new EnqueuedJobStateHistoryData
                {
                    EnqueuedAt = ConvertFromEpoch(jobStateHistoryDataModel.Data.EnqueuedAt),
                    Queue = jobStateHistoryDataModel.Data.Queue!
                },
                JobStatus.Failed => new FailedJobStateHistoryData
                {
                    FailedAt = ConvertFromEpoch(jobStateHistoryDataModel.Data.FailedAt),
                    ExceptionType = jobStateHistoryDataModel.Data.ExceptionType,
                    ExceptionMessage = jobStateHistoryDataModel.Data.ExceptionMessage,
                    ExceptionDetails = jobStateHistoryDataModel.Data.ExceptionDetails
                },
                JobStatus.Succeeded => new SucceededJobStateHistoryData
                {
                    SucceededAt = ConvertFromEpoch(jobStateHistoryDataModel.Data.SucceededAt),
                    Result = jobStateHistoryDataModel.Data.Result!,
                    Latency = jobStateHistoryDataModel.Data.Latency!.Value,
                    PerformanceDuration = jobStateHistoryDataModel.Data.PerformanceDuration!.Value
                },
                JobStatus.Processing => new ProcessingJobStateHistoryData
                {
                    ServerId = jobStateHistoryDataModel.Data.ServerId!,
                    WorkerId = jobStateHistoryDataModel.Data.WorkerId!,
                    StartedAt = ConvertFromEpoch(jobStateHistoryDataModel.Data.StartedAt)
                },
                _ => new JobStateHistoryData()
            };
        }

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
                .EnqueuedJobs(queue, from, perPage)
                .Select(x => x.Value);


        private IEnumerable<ScheduledJobDto> GetScheduledJobs(int from = 0, int count = 50)
            => _monitoringApi
                .ScheduledJobs(from, count)
                .Select(x => x.Value);
    }
}
