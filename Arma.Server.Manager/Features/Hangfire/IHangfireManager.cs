using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace Arma.Server.Manager.Features.Hangfire {
    /// <summary>
    /// Allows advanced jobs scheduling.
    /// </summary>
    public interface IHangfireManager {
        /// <summary>
        /// Schedule job expressed as <paramref name="func"/> for execution at <paramref name="dateTime"/>.
        /// 
        /// </summary>
        /// <typeparam name="T">Class used in Job expression.</typeparam>
        /// <param name="func">Expression to execute as job.</param>
        /// <param name="dateTime">When job should be executed.</param>
        /// <returns>Successful <see cref="Result"/> if job was correctly scheduled or was scheduled before around given <paramref name="dateTime"/>.</returns>
        Result ScheduleJob<T>(Expression<Func<T, Task>> func, DateTime? dateTime = null) where T : class;
    }
}
