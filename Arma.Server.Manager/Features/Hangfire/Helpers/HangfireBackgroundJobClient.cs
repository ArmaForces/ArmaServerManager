using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hangfire;

namespace Arma.Server.Manager.Features.Hangfire.Helpers
{
    public class HangfireBackgroundJobClient : IHangfireBackgroundJobClient
    {
        private readonly IBackgroundJobClient _backgroundJobClient = new BackgroundJobClient();

        public string Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset dateTimeOffset)
            => _backgroundJobClient.Schedule(methodCall, dateTimeOffset);

        public string Enqueue<T>(Expression<Func<T, Task>> methodCall)
            => _backgroundJobClient.Enqueue(methodCall);
    }
}
