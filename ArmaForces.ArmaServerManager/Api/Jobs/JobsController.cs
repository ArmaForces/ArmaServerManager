using System;
using System.Collections.Generic;
using System.Net.Mime;
using ArmaForces.ArmaServerManager.Api.Jobs.DTOs;
using ArmaForces.ArmaServerManager.Api.Jobs.Mappers;
using ArmaForces.ArmaServerManager.Features.Hangfire;
using ArmaForces.ArmaServerManager.Features.Hangfire.Helpers;
using ArmaForces.ArmaServerManager.Infrastructure.Authentication;
using ArmaForces.ArmaServerManager.Services;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ArmaForces.ArmaServerManager.Api.Jobs
{
    /// <summary>
    /// Allows jobs details retrieval.
    /// </summary>
    [Route("api/jobs")]
    [Produces(MediaTypeNames.Application.Json)]
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
        
        /// <summary>Get Queued Jobs</summary>
        /// <remarks>Returns enqueued, scheduled and awaiting jobs.</remarks>
        [HttpGet("queued", Name = nameof(GetQueuedJobs))]
        [ProducesResponseType(typeof(List<JobDetailsDto>), StatusCodes.Status200OK)]
        public IActionResult GetQueuedJobs()
            => _jobService.GetQueuedJobs()
                .Map(JobsMapper.Map)
                .Match(
                    onSuccess: Ok,
                    onFailure: error => (IActionResult) Problem());

        /// <summary>Get Jobs</summary>
        /// <remarks>Returns jobs with given <paramref name="jobStatus"/>.</remarks>
        /// <param name="jobStatus">Allowed job statuses.</param>
        [HttpGet(Name = nameof(GetJobs))]
        [ProducesResponseType(typeof(List<JobDetailsDto>), StatusCodes.Status200OK)]
        public IActionResult GetJobs([FromQuery] IEnumerable<JobStatus>? jobStatus = null)
            => _jobService.GetJobs(jobStatus)
                .Map(JobsMapper.Map)
                .Match(
                    onSuccess: Ok,
                    onFailure: error => (IActionResult) NotFound(error));

        /// <summary>Get Job</summary>
        /// <remarks>Retrieves job by id.</remarks>
        /// <param name="jobId">Id of the job to retrieve.</param>
        [HttpGet]
        [Route("{jobId}", Name = nameof(GetJob))]
        [ProducesResponseType(typeof(JobDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
        public IActionResult GetJob(string jobId)
        {
            return _jobService.GetJobDetails(jobId)
                .Map<JobDetails, JobDetailsDto>(JobsMapper.Map)
                .Match(
                    onSuccess: Ok,
                    onFailure: error => (IActionResult) NotFound(error));
        }
    }
}
