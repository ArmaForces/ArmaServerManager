using System.Collections.Generic;
using System.Linq;
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
                .Tap(x => _logger.LogTrace("Successfully retrieved details for job {JobId}", jobId));

        public Result<List<JobDetails>> GetJobs(IEnumerable<JobStatus>? jobStatusEnumerable = null)
            => _jobsRepository.GetQueuedJobs()
                .Tap(x => _logger.LogTrace("Found {Count} queued jobs", x.Count))
                .Map(x => FilterByJobStatus(x, jobStatusEnumerable))
                .Map(x => x.ToList());

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

        private static List<JobDetails> FilterByJobStatus(IEnumerable<JobDetails> jobs, IEnumerable<JobStatus>? jobStatusEnumerable)
            => jobStatusEnumerable is null
                ? jobs.ToList()
                : jobs.Where(x => jobStatusEnumerable.Contains(x.JobStatus))
                    .ToList();
    }
}
