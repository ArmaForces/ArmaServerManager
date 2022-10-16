using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ArmaForces.Arma.Server.Extensions;
using ArmaForces.ArmaServerManager.Features.Jobs.Models;
using ArmaForces.ArmaServerManager.Features.Jobs.Persistence;
using ArmaForces.ArmaServerManager.Features.Jobs.Persistence.Models;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace ArmaForces.ArmaServerManager.Features.Jobs
{
    internal class JobsService : IJobsService
    {
        private readonly IJobsRepository _jobsRepository;
        private readonly ILogger<JobsService> _logger;

        public JobsService(IJobsRepository jobsRepository, ILogger<JobsService> logger)
        {
            _jobsRepository = jobsRepository;
            _logger = logger;
        }

        public Result DeleteJob(string jobId)
            => _jobsRepository.GetJobDetails(jobId)
                .Bind(CheckJobCanBeDeleted)
                .Bind(() => _jobsRepository.DeleteJob(jobId));

        public Result<JobDetails?> GetCurrentJob()
            => _jobsRepository.GetCurrentJob();

        public Result<JobDetails> GetJobDetails(string jobId)
            => _jobsRepository.GetJobDetails(jobId)
                .Tap(x => _logger.LogDebug("Successfully retrieved details for job {JobId}", jobId));

        public Result<List<JobDetails>> GetJobs(IEnumerable<JobStatus> statusFilter)
        {
            var statusSet = statusFilter.ToHashSet();
            ISet<JobStatus> includeStatuses = statusSet.IsEmpty() ? AnyStatus : statusSet;
            
            return _jobsRepository.GetJobs(includeStatuses)
                .Tap(x => _logger.LogDebug("Found {Count} jobs matching status {List}", x.Count, statusFilter))
                .Map(x => x.ToList());
        }

        public Result<List<JobDetails>> GetQueuedJobs()
            => GetJobs(new List<JobStatus>
            {
                JobStatus.Scheduled,
                JobStatus.Awaiting,
                JobStatus.Enqueued
            });

        public Result RequeueJob(string jobId)
            => _jobsRepository.GetJobDetails(jobId)
                .Bind(CheckJobCanBeRequeued)
                .Bind(() => _jobsRepository.RequeueJob(jobId));

        private static Result CheckJobCanBeDeleted(JobDetails jobDetails)
            => jobDetails.JobStatus == JobStatus.Deleted
                ? Result.Failure("Cannot delete deleted job.")
                : Result.Success();

        private static Result CheckJobCanBeRequeued(JobDetails jobDetails)
            => jobDetails.JobStatus == JobStatus.Enqueued
                ? Result.Failure("Job is already enqueued")
                : Result.Success();

        private static ImmutableHashSet<JobStatus> AnyStatus { get; set; } = new HashSet<JobStatus>
        {
            JobStatus.Awaiting,
            JobStatus.Deleted,
            JobStatus.Enqueued,
            JobStatus.Failed,
            JobStatus.Processing,
            JobStatus.Scheduled,
            JobStatus.Succeeded
        }.ToImmutableHashSet();
    }
}
