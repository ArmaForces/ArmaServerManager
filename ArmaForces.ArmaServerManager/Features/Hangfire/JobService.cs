using System.Collections.Generic;
using System.Linq;
using ArmaForces.ArmaServerManager.Features.Hangfire.Helpers;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace ArmaForces.ArmaServerManager.Features.Hangfire
{
    internal class JobService : IJobService
    {
        private readonly IJobScheduler _jobScheduler;
        private readonly IJobStorage _jobStorage;
        private readonly ILogger<JobService> _logger;

        public JobService(
            IJobScheduler jobScheduler,
            IJobStorage jobStorage,
            ILogger<JobService> logger)
        {
            _jobScheduler = jobScheduler;
            _jobStorage = jobStorage;
            _logger = logger;
        }

        public Result<JobDetails> GetJobDetails(string jobId)
            => _jobStorage.GetJobDetails(jobId);

        public Result<List<JobDetails>> GetJobs(IEnumerable<JobStatus>? jobStatusEnumerable = null)
            => _jobStorage.GetQueuedJobs()
                .Map(x => FilterByJobStatus(x, jobStatusEnumerable))
                .Map(x => x.ToList());

        private static List<JobDetails> FilterByJobStatus(IEnumerable<JobDetails> jobs, IEnumerable<JobStatus>? jobStatusEnumerable)
            => jobStatusEnumerable is null
                ? jobs.ToList()
                : jobs.Where(x => jobStatusEnumerable.Contains(x.JobStatus))
                    .ToList();
    }
}
