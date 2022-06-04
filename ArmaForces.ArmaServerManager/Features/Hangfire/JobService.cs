using System.Collections.Generic;
using System.Linq;
using ArmaForces.ArmaServerManager.Features.Hangfire.Models;
using ArmaForces.ArmaServerManager.Features.Hangfire.Persistence;
using ArmaForces.ArmaServerManager.Features.Hangfire.Persistence.Models;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace ArmaForces.ArmaServerManager.Features.Hangfire
{
    internal class JobService : IJobService
    {
        private readonly IJobStorage _jobStorage;
        private readonly ILogger<JobService> _logger;

        public JobService(IJobStorage jobStorage, ILogger<JobService> logger)
        {
            _jobStorage = jobStorage;
            _logger = logger;
        }

        public Result<JobDetails> GetJobDetails(string jobId)
            => _jobStorage.GetJobDetails(jobId)
                .Tap(x => _logger.LogTrace("Successfully retrieved details for job {JobId}", jobId));

        public Result<List<JobDetails>> GetQueuedJobs()
            => GetJobs(new List<JobStatus>
            {
                JobStatus.Scheduled,
                JobStatus.Awaiting,
                JobStatus.Enqueued
            });

        public Result<List<JobDetails>> GetJobs(IEnumerable<JobStatus>? jobStatusEnumerable = null)
            => _jobStorage.GetQueuedJobs()
                .Tap(x => _logger.LogTrace("Found {Count} queued jobs", x.Count))
                .Map(x => FilterByJobStatus(x, jobStatusEnumerable))
                .Map(x => x.ToList());

        public Result<JobDetails?> GetCurrentJob()
            => _jobStorage.GetCurrentJob();

        private static List<JobDetails> FilterByJobStatus(IEnumerable<JobDetails> jobs, IEnumerable<JobStatus>? jobStatusEnumerable)
            => jobStatusEnumerable is null
                ? jobs.ToList()
                : jobs.Where(x => jobStatusEnumerable.Contains(x.JobStatus))
                    .ToList();
    }
}
