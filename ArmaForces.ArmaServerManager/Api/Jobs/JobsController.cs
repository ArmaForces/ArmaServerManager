using System;
using System.Collections.Generic;
using System.Linq;
using ArmaForces.ArmaServerManager.Api.Jobs.DTOs;
using ArmaForces.ArmaServerManager.Features.Hangfire;
using ArmaForces.ArmaServerManager.Features.Hangfire.Helpers;
using ArmaForces.ArmaServerManager.Infrastructure.Authentication;
using ArmaForces.ArmaServerManager.Services;
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
        private readonly IJobScheduler _jobScheduler;

        public JobsController(
            IJobService jobService,
            IJobScheduler jobScheduler)
        {
            _jobService = jobService;
            _jobScheduler = jobScheduler;
        }
        
        [HttpGet("queued")]
        public IActionResult GetQueuedJobs()
            => _jobService.GetQueuedJobs()
                .Map(Map)
                .Match(
                    onSuccess: Ok,
                    onFailure: error => (IActionResult) Problem());

        [HttpGet]
        public IActionResult GetJobs([FromQuery] IEnumerable<JobStatus>? jobStatus = null)
            => _jobService.GetJobs(jobStatus)
                .Map(Map)
                .Match(
                    onSuccess: Ok,
                    onFailure: error => (IActionResult) NotFound(error));

        [HttpGet]
        [Route("{jobId}")]
        public IActionResult GetJob(string jobId)
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

        private static List<JobDetailsDto> Map(IEnumerable<JobDetails> jobDetails)
            => jobDetails.Select(Map).ToList();
    }
}
