using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Jobs.Helpers
{
    internal interface IHangfireBackgroundJobClientWrapper
    {
        Result<string> ContinueWith<T>(string parentId, Expression<Func<T, Task>> methodCall);
        
        Result Delete(string jobId);

        Result<string> Enqueue<T>(Expression<Func<T, Task>> methodCall);
        
        Result Requeue(string jobId);

        Result<string> Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset dateTimeOffset);
    }
}
