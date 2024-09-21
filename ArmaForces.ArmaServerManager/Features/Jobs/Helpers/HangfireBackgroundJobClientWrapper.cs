using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Common.Errors;
using CSharpFunctionalExtensions;
using Hangfire;

namespace ArmaForces.ArmaServerManager.Features.Jobs.Helpers
{
    internal class HangfireBackgroundJobClientWrapper : IHangfireBackgroundJobClientWrapper
    {
        private const string JobIdParseError = "Could not parse job id to int.";
        
        private readonly IBackgroundJobClient _backgroundJobClient;

        public HangfireBackgroundJobClientWrapper(IBackgroundJobClient backgroundJobClient)
        {
            _backgroundJobClient = backgroundJobClient;
        }

        public Result<int, IError> ContinueWith<T>(int parentId, Expression<Func<T, Task>> methodCall)
        {
            var result = _backgroundJobClient.ContinueJobWith(parentId.ToString(), methodCall);
            
            return result != string.Empty
                ? TryParseJobId(result)
                : new Error($"Continuation job creation failed for parent job {parentId}.", ManagerErrorCode.JobContinuationCreationFailed);
        }

        public UnitResult<IError> Delete(int jobId)
            => _backgroundJobClient.Delete(jobId.ToString())
                ? UnitResult.Success<IError>()
                : new Error($"Could not delete job {jobId}. This could be caused by job state change, expiration or completion.", ManagerErrorCode.JobDeletionFailed);

        public Result<int, IError> Enqueue<T>(Expression<Func<T, Task>> methodCall)
        {
            var result = _backgroundJobClient.Enqueue(methodCall);
            
            return result != string.Empty
                ? TryParseJobId(result)
                : new Error("Failed to enqueue job.", ManagerErrorCode.JobEnqueueFailed);
        }

        public UnitResult<IError> Requeue(int jobId)
            => _backgroundJobClient.Requeue(jobId.ToString())
                ? UnitResult.Success<IError>()
                : new Error($"Could not requeue job {jobId}. This could be caused by job state change, expiration or completion.", ManagerErrorCode.JobEnqueueFailed);

        public Result<int, IError> Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset dateTimeOffset)
        {
            var result = _backgroundJobClient.Schedule(methodCall, dateTimeOffset);

            return result != string.Empty
                ? TryParseJobId(result)
                : new Error("Job schedule failed.", ManagerErrorCode.JobScheduleFailed);
        }

        private static Result<int, IError> TryParseJobId(string jobId)
            => int.TryParse(jobId, out var intJobId)
                ? intJobId
                : new Error(JobIdParseError, ManagerErrorCode.ParseError);
    }
}
