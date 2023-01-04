using System.Collections.Generic;
using System.Linq;
using ArmaForces.ArmaServerManager.Api.Jobs.DTOs;
using ArmaForces.ArmaServerManager.Features.Jobs.Persistence.Models;

namespace ArmaForces.ArmaServerManager.Api.Jobs.Mappers
{
    internal static class JobsMapper
    {
        public static JobDetailsDto Map(JobDetails jobDetails)
        {
            return new JobDetailsDto
            {
                Id = jobDetails.Id,
                Name = jobDetails.Name,
                CreatedAt = jobDetails.CreatedAt,
                EnqueuedAt = jobDetails.EnqueuedAt,
                StartedAt = jobDetails.StartedAt,
                FinishedAt = jobDetails.FinishedAt,
                JobStatus = jobDetails.JobStatus,
                Parameters = jobDetails.Parameters,
                StateHistory = Map(jobDetails.StateHistory)
            };
        }

        public static List<JobDetailsDto> Map(IEnumerable<JobDetails> jobDetails)
            => jobDetails.Select(Map).ToList();

        private static List<JobStateHistoryDto>? Map(IEnumerable<JobStateHistory>? history)
            => history?.Select(x => new JobStateHistoryDto
            {
                StateName = x.Name,
                EnteredAt = x.CreatedAt,
                // TODO: Consider removing all times if they add no value as EnteredAt is equal to most of them
                EnqueuedAt = (x.Data as EnqueuedJobStateHistoryData)?.EnqueuedAt,
                StartedAt = (x.Data as ProcessingJobStateHistoryData)?.StartedAt,
                EnqueueAt = (x.Data as ScheduledJobStateHistoryData)?.EnqueueAt,
                ScheduledAt = (x.Data as ScheduledJobStateHistoryData)?.ScheduledAt,
                SucceededAt = (x.Data as SucceededJobStateHistoryData)?.SucceededAt,
                Result = (x.Data as SucceededJobStateHistoryData)?.Result
            }).ToList();
    }
}