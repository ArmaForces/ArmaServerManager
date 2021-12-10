using System;
using System.Collections.Generic;
using ArmaForces.ArmaServerManager.Features.Hangfire;
using ArmaForces.ArmaServerManager.Features.Hangfire.Helpers;
using ArmaForces.ArmaServerManager.Infrastructure.Authentication;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;

namespace ArmaForces.ArmaServerManager.Api.Jobs
{
    [Route("api/jobs")]
    [ApiController]
    [ApiKey]
    public class JobsController : ControllerBase
    {
        private readonly IJobService _jobService;
        public JobsController(IJobService jobService)
        {
            _jobService = jobService;
        }

        [HttpGet]
        [Route("{jobId}")]
        public IActionResult GetJobStatus(string jobId)
        {
            return _jobService.GetJobDetails(jobId)
                .Map(Map)
                .Match(
                    onSuccess: Ok,
                    onFailure: error => (IActionResult) NotFound(error));
        }

        private static JobDetailsDto Map(JobDetails jobDetails)
        {
            return new JobDetailsDto
            {
                Name = jobDetails.Name,
                CreatedAt = jobDetails.CreatedAt,
                JobStatus = jobDetails.JobStatus,
                Parameters = jobDetails.Parameters
            };
        }
    }

    public class JobDetailsDto
    {
        public string Name { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; }
        
        public JobStatus JobStatus { get; set; }

        public List<KeyValuePair<string, object>> Parameters { get; set; } = new List<KeyValuePair<string, object>>();
    }
}
