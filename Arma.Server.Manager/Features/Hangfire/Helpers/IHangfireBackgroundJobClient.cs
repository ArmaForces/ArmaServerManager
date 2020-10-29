using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Arma.Server.Manager.Features.Hangfire.Helpers
{
    public interface IHangfireBackgroundJobClient
    {
        string Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset dateTimeOffset);

        string Enqueue<T>(Expression<Func<T, Task>> methodCall);
    }
}
