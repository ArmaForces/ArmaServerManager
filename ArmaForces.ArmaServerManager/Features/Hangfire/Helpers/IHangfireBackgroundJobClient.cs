using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ArmaForces.ArmaServerManager.Features.Hangfire.Helpers
{
    public interface IHangfireBackgroundJobClient
    {
        string Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset dateTimeOffset);

        string Enqueue<T>(Expression<Func<T, Task>> methodCall);

        string ContinueWith<T>(string parentId, Expression<Func<T, Task>> methodCall);
    }
}
