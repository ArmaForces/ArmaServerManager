using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ArmaForces.ArmaServerManager.Extensions;
using ArmaForces.ArmaServerManager.Features.Jobs.Helpers;
using ArmaForces.ArmaServerManager.Features.Jobs.Persistence;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace ArmaForces.ArmaServerManager.Features.Jobs
{
    /// <inheritdoc cref="IJobsScheduler" />
    internal class JobsScheduler : IJobsScheduler
    {
        private readonly TimeSpan _defaultPrecision = TimeSpan.FromMinutes(15);
        
        private readonly IHangfireBackgroundJobClientWrapper _backgroundJobClientWrapper;
        private readonly IJobsRepository _jobsStorage;
        private readonly ILogger<JobsScheduler> _logger;

        public JobsScheduler(
            IHangfireBackgroundJobClientWrapper backgroundJobClientWrapper,
            IJobsRepository jobsStorage,
            ILogger<JobsScheduler> logger)
        {
            _backgroundJobClientWrapper = backgroundJobClientWrapper;
            _jobsStorage = jobsStorage;
            _logger = logger;
        }

        /// <inheritdoc cref="IJobsScheduler" />
        public Result<string> ScheduleJob<T>(Expression<Func<T, Task>> func, DateTime? dateTime = null) where T : class
            => dateTime.HasValue
                ? ScheduleAt(func, dateTime.Value)
                : EnqueueImmediately(func);

        /// <inheritdoc cref="IJobsScheduler"/>
        public Result<string> ContinueJobWith<T>(string parentId, Expression<Func<T, Task>> func) where T : class
            => _backgroundJobClientWrapper.ContinueWith(parentId, func)
                .Tap(_ => _logger.LogDebug("Scheduled continuation for job {JobId}", parentId));

        /// <summary>
        ///     Schedules job for execution at <paramref name="dateTime" />.
        ///     If similar job is already scheduled around that time, nothing is done.
        /// </summary>
        private Result<string> ScheduleAt<T>(Expression<Func<T, Task>> func, DateTime dateTime) where T : class
        {
            var scheduledJobs = _jobsStorage.GetSimilarScheduledJobs(func)
                .Where(x => x.EnqueueAt.IsCloseTo(dateTime, _defaultPrecision));

            var queuedJobs = _jobsStorage.GetSimilarQueuedJobs(func);

            if (scheduledJobs.Any() || queuedJobs.Any())
                // TODO: Return the similarJobId
                return Result.Success(string.Empty)
                    .Tap(_ => _logger.LogDebug("There is similar job scheduled at {DateTime}", dateTime));

            return _backgroundJobClientWrapper.Schedule(func, dateTime)
                .Tap(jobId => _logger.LogDebug("Scheduled job {JobId} at {DateTime}", jobId, dateTime));
        }

        /// <summary>
        ///     Schedules job for immediate execution.
        ///     If similar job is already scheduled or in queue now, nothing is done.
        /// </summary>
        private Result<string> EnqueueImmediately<T>(Expression<Func<T, Task>> func) where T : class
        {
            var scheduledJobs = _jobsStorage.GetSimilarScheduledJobs(func)
                .Where(x => x.EnqueueAt.IsCloseTo(DateTime.Now, _defaultPrecision));

            var queuedJobs = _jobsStorage.GetSimilarQueuedJobs(func);

            if (scheduledJobs.Any() || queuedJobs.Any())
                return Result.Success(string.Empty)
                    .Tap(_ => _logger.LogDebug("There is similar job queued in less than {Precision}", _defaultPrecision));

            return _backgroundJobClientWrapper.Enqueue(func)
                .Tap(jobId => _logger.LogDebug("Enqueued job {JobId}", jobId));
        }
    }
}
