﻿using System.Collections.Generic;
using System.Linq;
using ArmaForces.ArmaServerManager.Api.Jobs.DTOs;
using ArmaForces.ArmaServerManager.Features.Hangfire.Helpers;

namespace ArmaForces.ArmaServerManager.Api.Jobs.Mappers
{
    public static class JobsMapper
    {
        public static JobDetailsDto Map(JobDetails jobDetails)
        {
            return new JobDetailsDto
            {
                Name = jobDetails.Name,
                CreatedAt = jobDetails.CreatedAt,
                JobStatus = jobDetails.JobStatus,
                Parameters = jobDetails.Parameters
            };
        }

        public static List<JobDetailsDto> Map(IEnumerable<JobDetails> jobDetails)
            => jobDetails.Select(Map).ToList();
    }
}