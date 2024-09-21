using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Common.Errors;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Jobs.Helpers
{
    internal interface IHangfireBackgroundJobClientWrapper
    {
        Result<int, IError> ContinueWith<T>(int parentId, Expression<Func<T, Task>> methodCall);
        
        UnitResult<IError> Delete(int jobId);

        Result<int, IError> Enqueue<T>(Expression<Func<T, Task>> methodCall);
        
        UnitResult<IError> Requeue(int jobId);

        Result<int, IError> Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset dateTimeOffset);
    }
}
