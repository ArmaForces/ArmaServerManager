using System;

namespace ArmaForces.ArmaServerManager.Features.Hangfire
{
    public static class JobStatusParser
    {
        public static JobStatus ParseJobStatus(string jobStatusString)
        {
            return jobStatusString switch
            {
                "Enqueued" => JobStatus.Enqueued,
                "Scheduled" => JobStatus.Scheduled,
                "Awaiting" => JobStatus.Awaiting,
                "Deleted" => JobStatus.Deleted,
                "Failed" => JobStatus.Failed,
                "Processing" => JobStatus.Processing,
                "Succeeded" => JobStatus.Succeeded,
                _ => throw new ArgumentOutOfRangeException(nameof(JobStatus))
            };
        }
    }
}
