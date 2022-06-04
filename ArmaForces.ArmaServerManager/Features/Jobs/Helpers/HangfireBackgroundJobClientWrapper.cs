using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Hangfire;

namespace ArmaForces.ArmaServerManager.Features.Jobs.Helpers
{
    internal class HangfireBackgroundJobClientWrapper : IHangfireBackgroundJobClientWrapper
    {
        private readonly IBackgroundJobClient _backgroundJobClient;

        public HangfireBackgroundJobClientWrapper(IBackgroundJobClient backgroundJobClient)
        {
            _backgroundJobClient = backgroundJobClient;
        }

        public Result<string> ContinueWith<T>(string parentId, Expression<Func<T, Task>> methodCall)
            => _backgroundJobClient.ContinueJobWith(parentId, methodCall);

        public Result Delete(string jobId)
            => _backgroundJobClient.Delete(jobId)
                ? Result.Success()
                : Result.Failure($"Could not delete job {jobId}. This could be caused by job state change, expiration or completion.");

        public Result<string> Enqueue<T>(Expression<Func<T, Task>> methodCall)
            => _backgroundJobClient.Enqueue(methodCall);

        public Result Requeue(string jobId)
            => _backgroundJobClient.Requeue(jobId)
                ? Result.Success()
                : Result.Failure($"Could not requeue job {jobId}. This could be caused by job state change, expiration or completion.");

        public Result<string> Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset dateTimeOffset)
            => _backgroundJobClient.Schedule(methodCall, dateTimeOffset);
    }
}
