using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ArmaForces.Arma.Server.Common.Errors;
using ArmaForces.Arma.Server.Extensions;
using ArmaForces.ArmaServerManager.Common.Errors;
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

        public UnitResult<IError> DeleteJob(int jobId)
            => _jobsRepository.GetJobDetails(jobId)
                .Bind(CheckJobCanBeDeleted)
                .Bind(() => _jobsRepository.DeleteJob(jobId));

        public Result<JobDetails?, IError> GetCurrentJob()
            => _jobsRepository.GetCurrentJob();

        public Result<JobDetails, IError> GetJobDetails(int jobId, bool includeHistory = false)
            => _jobsRepository.GetJobDetails(jobId, includeHistory)
                .Tap(_ => _logger.LogDebug("Successfully retrieved details for job {JobId}", jobId));

        public Result<List<JobDetails>, IError> GetJobs(
            IEnumerable<int> jobIds,
            IEnumerable<JobStatus> statusFilter,
            bool includeHistory = false)
        {
            var jobIdsList = jobIds.ToList();
            var statusSet = statusFilter.ToHashSet();
            
            if (jobIdsList.Any() && statusSet.Any())
            {
                return new Error("Only one of jobIds and statusFilter can be specified.", ManagerErrorCode.ArgumentError);
            }

            return jobIdsList.Any()
                ? GetJobs(jobIdsList, includeHistory)
                : GetJobs(statusSet, includeHistory);
        }

        public Result<List<JobDetails>, IError> GetQueuedJobs()
            => GetJobs(new List<JobStatus>
            {
                JobStatus.Scheduled,
                JobStatus.Awaiting,
                JobStatus.Enqueued
            });

        public UnitResult<IError> RequeueJob(int jobId)
            => _jobsRepository.GetJobDetails(jobId)
                .Bind(CheckJobCanBeRequeued)
                .Bind(() => _jobsRepository.RequeueJob(jobId));

        public UnitResult<IError> DeleteJobs(DateTime deleteFrom, DateTime deleteTo)
            => GetQueuedJobs()
                .Bind(jobs => jobs
                    .Where(job => job.ScheduledAt >= deleteFrom && job.ScheduledAt < deleteTo)
                    .Select(job => DeleteJob(job.Id))
                    .Combine());

        private static UnitResult<IError> CheckJobCanBeDeleted(JobDetails jobDetails)
            => jobDetails.JobStatus == JobStatus.Deleted
                ? new Error("Cannot delete deleted job.", ManagerErrorCode.JobDeletionFailed)
                : UnitResult.Success<IError>();

        private static UnitResult<IError> CheckJobCanBeRequeued(JobDetails jobDetails)
            => jobDetails.JobStatus == JobStatus.Enqueued
                ? new Error("Job is already enqueued", ManagerErrorCode.SimilarJobFound)
                : UnitResult.Success<IError>();

        private Result<List<JobDetails>, IError> GetJobs(
            IEnumerable<int> jobIds,
            bool includeHistory = false)
        {
            return _jobsRepository.GetJobs(jobIds, includeHistory)
                .Tap(x => _logger.LogDebug("Found {Count} jobs out of requested list {List}", x.Count, jobIds))
                .Map(x => x.ToList());
        }

        private Result<List<JobDetails>, IError> GetJobs(
            IEnumerable<JobStatus> statusFilter,
            bool includeHistory = false)
        {
            var statusSet = statusFilter.ToHashSet();
            ISet<JobStatus> includeStatuses = statusSet.IsEmpty() ? AnyStatus : statusSet;
            
            return _jobsRepository.GetJobs(includeStatuses, includeHistory)
                .Tap(x => _logger.LogDebug("Found {Count} jobs matching status {List}", x.Count, includeStatuses))
                .Map(x => x.ToList());
        }

        private static ImmutableHashSet<JobStatus> AnyStatus { get; } = new HashSet<JobStatus>
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
