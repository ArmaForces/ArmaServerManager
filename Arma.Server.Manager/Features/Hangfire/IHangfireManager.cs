using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace Arma.Server.Manager.Features.Hangfire {
    public interface IHangfireManager {
        Result ScheduleJob<T>(Expression<Func<T, Task>> func, DateTime? dateTime = null) where T : new();
    }
}
