using System;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Common.Errors;
using ArmaForces.Arma.Server.Features.Parameters;
using CSharpFunctionalExtensions;

namespace ArmaForces.Arma.Server.Features.Processes
{
    public interface IArmaProcess
    {
        ProcessParameters Parameters { get; }

        ArmaProcessType ProcessType { get; }

        bool IsStopped { get; }
        
        bool IsStartingOrStarted { get; }

        DateTimeOffset? StartTime { get; }

        UnitResult<IError> Start();

        Task<UnitResult<IError>> Shutdown();

        public event Func<IArmaProcess, Task> OnProcessShutdown;
    }
}
