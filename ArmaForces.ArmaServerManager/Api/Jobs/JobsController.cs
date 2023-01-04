using System.Collections.Generic;
using ArmaForces.ArmaServerManager.Api.Jobs.DTOs;
using ArmaForces.ArmaServerManager.Api.Jobs.Mappers;
using ArmaForces.ArmaServerManager.Features.Jobs;
using ArmaForces.ArmaServerManager.Features.Jobs.Models;
using ArmaForces.ArmaServerManager.Infrastructure.Authentication;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ArmaForces.ArmaServerManager.Api.Jobs
{
    /// <summary>
    /// Allows jobs details retrieval.
    /// </summary>
    [Route("api/jobs")]
    [ApiController]
    [ApiKey]
    public class JobsController : ManagerControllerBase
    {
        private readonly IJobsService _jobsService;

        /// <inheritdoc />
        public JobsController(IJobsService jobsService)
        {
            _jobsService = jobsService;
        }

        /// <summary>Delete Job</summary>
        /// <remarks>
        /// Attempts deletion (cancellation) of job with given <paramref name="jobId"/>.
        /// Delete may fail if job changes state, is completed or expires.
        /// </remarks>
        /// <param name="jobId">Id of the job to delete.</param>
        [HttpDelete("{jobId:int}", Name = nameof(DeleteJob))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status422UnprocessableEntity)]
        [ApiKey]
        public IActionResult DeleteJob(int jobId)
        {
            return _jobsService.DeleteJob(jobId)
                .Match(
                    onSuccess: NoContent,
                    onFailure: error => error.Contains("not exist")
                        ? NotFound(error)
                        : (IActionResult) UnprocessableEntity(error));
        }

        /// <summary>Delete Jobs</summary>
        /// <remarks>
        /// Attempts deletion (cancellation) of all jobs.
        /// Delete may fail if job changes state, is completed or expires.
        /// </remarks>
        /// <param name="jobsDeleteRequestDto"></param>
        [HttpDelete(Name = nameof(DeleteJobs))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ApiKey]
        public IActionResult DeleteJobs([FromBody] JobsDeleteRequestDto jobsDeleteRequestDto)
        {
            return jobsDeleteRequestDto.DeleteFrom is not null && jobsDeleteRequestDto.DeleteTo is not null
                ? _jobsService.DeleteJobs(jobsDeleteRequestDto.DeleteFrom.Value, jobsDeleteRequestDto.DeleteTo.Value)
                    .Match(
                    onSuccess: NoContent,
                    onFailure: error => (IActionResult) BadRequest(error))
                : BadRequest($"Both {nameof(JobsDeleteRequestDto.DeleteFrom)} and {nameof(JobsDeleteRequestDto.DeleteTo)} must be specified.");
        }

        /// <summary>Get Job</summary>
        /// <remarks>Retrieves job by id.</remarks>
        /// <param name="jobId">Id of the job to retrieve.</param>
        /// <param name="includeHistory">Include job status history.</param>
        [HttpGet("{jobId:int}", Name = nameof(GetJob))]
        [ProducesResponseType(typeof(JobDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
        public IActionResult GetJob(int jobId, [FromQuery] bool includeHistory = false)
        {
            return _jobsService.GetJobDetails(jobId, includeHistory)
                .Map(JobsMapper.Map)
                .Match(
                    onSuccess: Ok,
                    onFailure: error => (IActionResult) NotFound(error));
        }

        /// <summary>Get Jobs</summary>
        /// <remarks>
        /// Returns jobs with given <paramref name="jobStatus"/>.
        /// Only one of <paramref name="jobIds"/> and <paramref name="jobStatus"/> query parameters can be specified.
        /// </remarks>
        /// <param name="jobIds">Ids of jobs to retrieve.</param>
        /// <param name="jobStatus">Allowed job statuses.</param>
        /// <param name="includeHistory">Include job status history.</param>
        [HttpGet(Name = nameof(GetJobs))]
        [ProducesResponseType(typeof(List<JobDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public IActionResult GetJobs(
            [FromQuery] IEnumerable<int> jobIds,
            [FromQuery] IEnumerable<JobStatus> jobStatus,
            [FromQuery] bool includeHistory = false)
            => _jobsService.GetJobs(jobIds, jobStatus, includeHistory)
                .Map(JobsMapper.Map)
                .Match(
                    onSuccess: Ok,
                    onFailure: error => (IActionResult) Problem(error));

        /// <summary>Get Queued Jobs</summary>
        /// <remarks>Returns enqueued, scheduled and awaiting jobs.</remarks>
        [HttpGet("queued", Name = nameof(GetQueuedJobs))]
        [ProducesResponseType(typeof(List<JobDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public IActionResult GetQueuedJobs()
            => _jobsService.GetQueuedJobs()
                .Map(JobsMapper.Map)
                .Match(
                    onSuccess: Ok,
                    onFailure: error => (IActionResult) Problem(error));

        /// <summary>Requeue Job</summary>
        /// <remarks>
        /// Attempts requeue of job with given <paramref name="jobId"/>.
        /// Requeue may fail if job changes state, is completed or expires.
        /// </remarks>
        /// <param name="jobId">Id of the job to requeue.</param>
        [HttpPost("{jobId:int}/requeue", Name = nameof(RequeueJob))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status422UnprocessableEntity)]
        [ApiKey]
        public IActionResult RequeueJob(int jobId)
        {
            return _jobsService.RequeueJob(jobId)
                .Match(
                    onSuccess: NoContent,
                    onFailure: error => error.Contains("not exists")
                        ? NotFound()
                        : (IActionResult) UnprocessableEntity(error));
        }
    }
}
