using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ArmaForces.ArmaServerManager.Extensions;
using ArmaForces.ArmaServerManager.Features.Hangfire.Helpers;
using CSharpFunctionalExtensions;
using Hangfire.Common;
using Hangfire.Storage.Monitoring;
using Microsoft.Extensions.Logging;

namespace ArmaForces.ArmaServerManager.Features.Hangfire
{
    /// <inheritdoc cref="IJobScheduler" />
    internal class JobScheduler : IJobScheduler
    {
        private readonly TimeSpan _defaultPrecision = TimeSpan.FromMinutes(15);
        
        private readonly IHangfireBackgroundJobClient _backgroundJobClient;
        private readonly IJobStorage _jobStorage;
        private readonly ILogger<JobScheduler> _logger;

        public JobScheduler(
            IHangfireBackgroundJobClient backgroundJobClient,
            IJobStorage jobStorage,
            ILogger<JobScheduler> logger)
        {
            _backgroundJobClient = backgroundJobClient;
            _jobStorage = jobStorage;
            _logger = logger;
        }

        /// <inheritdoc cref="IJobScheduler" />
        public Result<string> ScheduleJob<T>(Expression<Func<T, Task>> func, DateTime? dateTime = null) where T : class
            => dateTime.HasValue
                ? ScheduleAt(func, dateTime.Value)
                : EnqueueImmediately(func);

        /// <inheritdoc cref="IJobScheduler"/>
        public Result<string> ContinueJobWith<T>(string parentId, Expression<Func<T, Task>> func) where T : class
            => _backgroundJobClient.ContinueWith(parentId, func)
                .Tap(_ => _logger.LogDebug("Scheduled continuation for job {JobId}", parentId));

        /// <summary>
        ///     Schedules job for execution at <paramref name="dateTime" />.
        ///     If similar job is already scheduled around that time, nothing is done.
        /// </summary>
        private Result<string> ScheduleAt<T>(Expression<Func<T, Task>> func, DateTime dateTime) where T : class
        {
            var scheduledJobs = _jobStorage.GetSimilarScheduledJobs(func)
                .Where(x => x.EnqueueAt.IsCloseTo(dateTime, _defaultPrecision));

            var queuedJobs = _jobStorage.GetSimilarQueuedJobs(func);

            if (scheduledJobs.Any() || queuedJobs.Any())
                // TODO: Return the similarJobId
                return Result.Success(string.Empty)
                    .Tap(_ => _logger.LogDebug("There is similar job scheduled at {DateTime}", dateTime));

            return _backgroundJobClient.Schedule(func, dateTime)
                .Tap(jobId => _logger.LogDebug("Scheduled job {JobId} at {DateTime}", jobId, dateTime));
        }

        /// <summary>
        ///     Schedules job for immediate execution.
        ///     If similar job is already scheduled or in queue now, nothing is done.
        /// </summary>
        private Result<string> EnqueueImmediately<T>(Expression<Func<T, Task>> func) where T : class
        {
            var scheduledJobs = _jobStorage.GetSimilarScheduledJobs(func)
                .Where(x => x.EnqueueAt.IsCloseTo(DateTime.Now, _defaultPrecision));

            var queuedJobs = _jobStorage.GetSimilarQueuedJobs(func);

            if (scheduledJobs.Any() || queuedJobs.Any())
                return Result.Success(string.Empty)
                    .Tap(_ => _logger.LogDebug("There is similar job queued in less than {Precision}", _defaultPrecision));

            return _backgroundJobClient.Enqueue(func)
                .Tap(jobId => _logger.LogDebug("Enqueued job {JobId}", jobId));
        }
    }
}
