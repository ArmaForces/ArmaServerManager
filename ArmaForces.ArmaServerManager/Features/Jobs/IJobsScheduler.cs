using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Common.Errors;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Jobs
{
    /// <summary>
    /// Allows advanced jobs scheduling.
    /// </summary>
    public interface IJobsScheduler
    {
        /// <summary>
        /// Schedule job expressed as <paramref name="func"/> for execution at <paramref name="dateTime"/>.
        /// 
        /// </summary>
        /// <typeparam name="T">Class used in Job expression.</typeparam>
        /// <param name="func">Expression to execute as job.</param>
        /// <param name="dateTime">When job should be executed.</param>
        /// <returns>Successful <see cref="Result"/> if job was correctly scheduled or was scheduled before around given <paramref name="dateTime"/>.</returns>
        Result<int, IError> ScheduleJob<T>(Expression<Func<T, Task>> func, DateTime? dateTime = null) where T : class;

        /// <summary>
        /// Schedule job expressed as <paramref name="func"/> for execution after <paramref name="parentId"/> job is finished.
        /// </summary>
        /// <typeparam name="T">Class used in Job expression.</typeparam>
        /// <param name="parentId">Parent job id which needs to Succeed for newly scheduled job to start.</param>
        /// <param name="func">Expression to execute as job.</param>
        /// <returns>Successful <see cref="Result"/> if job was correctly scheduled.</returns>
        Result<int, IError> ContinueJobWith<T>(int parentId, Expression<Func<T, Task>> func) where T : class;
    }
}
