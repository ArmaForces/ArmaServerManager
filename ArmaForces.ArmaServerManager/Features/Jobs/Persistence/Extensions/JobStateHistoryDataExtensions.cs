using System.Collections.Generic;
using System.Linq;
using ArmaForces.ArmaServerManager.Features.Jobs.Models;
using ArmaForces.ArmaServerManager.Features.Jobs.Persistence.Models;

namespace ArmaForces.ArmaServerManager.Features.Jobs.Persistence.Extensions
{
    internal static class JobStateHistoryDataExtensions
    {
        public static EnqueuedJobStateHistoryData? GetLastEnqueuedState(this List<JobStateHistory> histories)
            => GetLastJobState<EnqueuedJobStateHistoryData>(histories, JobStatus.Enqueued);

        public static ProcessingJobStateHistoryData? GetLastProcessingState(this List<JobStateHistory> histories)
            => GetLastJobState<ProcessingJobStateHistoryData>(histories, JobStatus.Processing);

        public static ScheduledJobStateHistoryData? GetLastScheduledState(this List<JobStateHistory> histories)
            => GetLastJobState<ScheduledJobStateHistoryData>(histories, JobStatus.Scheduled);

        public static SucceededJobStateHistoryData? GetLastSucceededState(this List<JobStateHistory> histories)
            => GetLastJobState<SucceededJobStateHistoryData>(histories, JobStatus.Succeeded);
        
        public static FailedJobStateHistoryData? GetLastFailedState(this List<JobStateHistory> histories)
            => GetLastJobState<FailedJobStateHistoryData>(histories, JobStatus.Failed);

        private static T? GetLastJobState<T>(this List<JobStateHistory> histories, JobStatus status)
                where T : JobStateHistoryData
            => histories?.LastOrDefault(x => x.Name == status)?.Data as T;
    }
}