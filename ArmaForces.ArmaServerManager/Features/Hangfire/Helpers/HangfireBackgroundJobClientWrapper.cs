using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Hangfire;

namespace ArmaForces.ArmaServerManager.Features.Hangfire.Helpers
{
    public class HangfireBackgroundJobClientWrapper : IHangfireBackgroundJobClientWrapper
    {
        private readonly IBackgroundJobClient _backgroundJobClient = new BackgroundJobClient();

        public Result<string> Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset dateTimeOffset)
            => _backgroundJobClient.Schedule(methodCall, dateTimeOffset);

        public Result<string> Enqueue<T>(Expression<Func<T, Task>> methodCall)
            => _backgroundJobClient.Enqueue(methodCall);

        public Result<string> ContinueWith<T>(string parentId, Expression<Func<T, Task>> methodCall)
            => _backgroundJobClient.ContinueJobWith(parentId, methodCall);
    }
}
