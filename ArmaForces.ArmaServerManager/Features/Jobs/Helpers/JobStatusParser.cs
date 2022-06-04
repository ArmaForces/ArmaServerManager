using System;
using ArmaForces.ArmaServerManager.Features.Jobs.Models;

namespace ArmaForces.ArmaServerManager.Features.Jobs.Helpers
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
                _ => throw new ArgumentOutOfRangeException(nameof(jobStatusString))
            };
        }
    }
}
