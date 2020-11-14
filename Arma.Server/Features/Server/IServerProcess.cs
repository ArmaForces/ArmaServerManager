using CSharpFunctionalExtensions;

namespace Arma.Server.Features.Server
{
    public interface IServerProcess
    {
        bool IsStopped { get; }

        bool IsStarted { get; }

        bool IsStarting { get; }

        Result Start();

        Result Shutdown();
    }
}
