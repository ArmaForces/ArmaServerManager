using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Hangfire.Common;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using HangfireJobStorage = Hangfire.JobStorage;

namespace ArmaForces.ArmaServerManager.Features.Hangfire.Helpers
{
    internal class JobStorage : IJobStorage
    {
        private IMonitoringApi MonitoringApi { get; } = HangfireJobStorage.Current.GetMonitoringApi();

        private IStorageConnection StorageConnection { get; } = HangfireJobStorage.Current.GetConnection();

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

        public Result<JobDetails> GetJobDetails(string jobId)
        {
            var jobData = StorageConnection.GetJobData(jobId);

            return jobData != null
                ? CreateJobDetails(jobData)
                : Result.Failure<JobDetails>("Job does not exist.");
        }

        private static JobDetails CreateJobDetails(JobData jobData) => new JobDetails
        {
            Name = jobData.Job.ToString().Split('.').Last(),
            CreatedAt = jobData.CreatedAt,
            JobStatus = JobStatusParser.ParseJobStatus(jobData.State),
            Parameters = jobData.Job.Method.GetParameters()
                .Zip(jobData.Job.Args, (parameterInfo, parameterValue) => new KeyValuePair<string, object>(parameterInfo.Name ?? "unknown", parameterValue))
                .SkipLast(1)
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
            => MonitoringApi
                .EnqueuedJobs(
                    queue,
                    from,
                    perPage)
                .Select(x => x.Value);


        private IEnumerable<ScheduledJobDto> GetScheduledJobs(int from = 0, int count = 50)
            => MonitoringApi
                .ScheduledJobs(from, count)
                .Select(x => x.Value);
    }
}
