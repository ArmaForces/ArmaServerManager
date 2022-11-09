using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Jobs.Helpers
{
    internal interface IHangfireBackgroundJobClientWrapper
    {
        Result<int> ContinueWith<T>(int parentId, Expression<Func<T, Task>> methodCall);
        
        Result Delete(int jobId);

        Result<int> Enqueue<T>(Expression<Func<T, Task>> methodCall);
        
        Result Requeue(int jobId);

        Result<int> Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset dateTimeOffset);
    }
}
