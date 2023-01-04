using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
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

        public Result<int> ContinueWith<T>(int parentId, Expression<Func<T, Task>> methodCall)
        {
            var result = _backgroundJobClient.ContinueJobWith(parentId.ToString(), methodCall);
            
            return result != string.Empty
                ? TryParseJobId(result)
                : Result.Failure<int>($"Continuation job creation failed for parent job {parentId}.");
        }

        public Result Delete(int jobId)
            => _backgroundJobClient.Delete(jobId.ToString())
                ? Result.Success()
                : Result.Failure($"Could not delete job {jobId}. This could be caused by job state change, expiration or completion.");

        public Result<int> Enqueue<T>(Expression<Func<T, Task>> methodCall)
        {
            var result = _backgroundJobClient.Enqueue(methodCall);
            
            return result != string.Empty
                ? TryParseJobId(result)
                : Result.Failure<int>("Failed to enqueue job.");
        }

        public Result Requeue(int jobId)
            => _backgroundJobClient.Requeue(jobId.ToString())
                ? Result.Success()
                : Result.Failure($"Could not requeue job {jobId}. This could be caused by job state change, expiration or completion.");

        public Result<int> Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset dateTimeOffset)
        {
            var result = _backgroundJobClient.Schedule(methodCall, dateTimeOffset);

            return result != string.Empty
                ? TryParseJobId(result)
                : Result.Failure<int>("Job schedule failed.");
        }

        private static Result<int> TryParseJobId(string jobId)
            => int.TryParse(jobId, out var intJobId)
                ? intJobId
                : Result.Failure<int>(JobIdParseError);
    }
}
