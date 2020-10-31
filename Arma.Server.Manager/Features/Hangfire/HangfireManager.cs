using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Arma.Server.Manager.Extensions;
using Arma.Server.Manager.Features.Hangfire.Helpers;
using CSharpFunctionalExtensions;
using Hangfire.Common;
using Hangfire.Storage.Monitoring;

namespace Arma.Server.Manager.Features.Hangfire
{
    /// <inheritdoc cref="IHangfireManager"/>
    public class HangfireManager : IHangfireManager
    {
        private readonly TimeSpan _defaultPrecision = TimeSpan.FromMinutes(15);

        private readonly IHangfireBackgroundJobClient _backgroundJobClient;
        private readonly IHangfireJobStorage _hangfireJobStorage;

        public HangfireManager(IHangfireBackgroundJobClient backgroundJobClient, IHangfireJobStorage hangfireJobStorage)
        {
            _backgroundJobClient = backgroundJobClient;
            _hangfireJobStorage = hangfireJobStorage;
        }

        /// <inheritdoc cref="IHangfireManager"/>
        public Result ScheduleJob<T>(Expression<Func<T, Task>> func, DateTime? dateTime = null) where T : new()
            => dateTime.HasValue
                ? ScheduleAt(func, dateTime.Value)
                : EnqueueImmediately(func);

        /// <summary>
        /// Checks if given <paramref name="job"/> will execute using <typeparamref name="T"/>
        /// and uses the same method as in <paramref name="func"/>.
        /// </summary>
        private static bool JobMatchesMethod<T>(Job job, Expression<Func<T, Task>> func)
            => job.Type == typeof(T) && func.Body.ToString().Contains(job.Method.Name);

        /// <summary>
        /// Schedules job for execution at <paramref name="dateTime"/>.
        /// If similar job is already scheduled around that time, nothing is done.
        /// </summary>
        private Result ScheduleAt<T>(Expression<Func<T, Task>> func, DateTime dateTime) where T : new()
        {
            var scheduledJobs = GetSimilarScheduledJobs(func)
                .Where(x => x.EnqueueAt.IsCloseTo(dateTime, _defaultPrecision));

            var queuedJobs = GetSimilarQueuedJobs(func);

            if (scheduledJobs.Any() || queuedJobs.Any())
                return Result.Success();

            _backgroundJobClient.Schedule(func, dateTime);

            return Result.Success();
        }

        /// <summary>
        /// Schedules job for immediate execution.
        /// If similar job is already scheduled or in queue now, nothing is done.
        /// </summary>
        private Result EnqueueImmediately<T>(Expression<Func<T, Task>> func) where T : new()
        {
            var scheduledJobs = GetSimilarScheduledJobs(func)
                .Where(x => x.EnqueueAt.IsCloseTo(DateTime.Now, _defaultPrecision));

            var queuedJobs = GetSimilarQueuedJobs(func);

            if (scheduledJobs.Any() || queuedJobs.Any())
                return Result.Success();

            _backgroundJobClient.Enqueue(func);

            return Result.Success();
        }

        private IEnumerable<EnqueuedJobDto> GetSimilarQueuedJobs<T>(
            Expression<Func<T, Task>> func,
            string queue = "default",
            int from = 0,
            int perPage = 50)
            => GetQueuedJobs(
                    queue,
                    from,
                    perPage)
                .Where(x => JobMatchesMethod(x.Job, func));

        private IEnumerable<ScheduledJobDto> GetSimilarScheduledJobs<T>(
            Expression<Func<T, Task>> func,
            int from = 0,
            int count = 50)
            => GetScheduledJobs(from, count)
                .Where(x => JobMatchesMethod(x.Job, func));

        private IEnumerable<EnqueuedJobDto> GetQueuedJobs(
            string queue = "default",
            int from = 0,
            int perPage = 50)
            => _hangfireJobStorage.MonitoringApi
                .EnqueuedJobs(
                    queue,
                    from,
                    perPage)
                .Select(x => x.Value);


        private IEnumerable<ScheduledJobDto> GetScheduledJobs(int from = 0, int count = 50)
            => _hangfireJobStorage.MonitoringApi
                .ScheduledJobs(from, count)
                .Select(x => x.Value);
    }
}
