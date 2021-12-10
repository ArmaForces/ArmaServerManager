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
    }
}
