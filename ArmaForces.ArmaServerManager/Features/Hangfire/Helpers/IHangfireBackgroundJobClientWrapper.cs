using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Hangfire.Helpers
{
    public interface IHangfireBackgroundJobClientWrapper
    {
        Result<string> Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset dateTimeOffset);

        Result<string> Enqueue<T>(Expression<Func<T, Task>> methodCall);

        Result<string> ContinueWith<T>(string parentId, Expression<Func<T, Task>> methodCall);
    }
}
