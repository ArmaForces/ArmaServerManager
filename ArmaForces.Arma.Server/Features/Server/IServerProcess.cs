using ArmaForces.Arma.Server.Features.Parameters;
using CSharpFunctionalExtensions;

namespace ArmaForces.Arma.Server.Features.Server
{
    public interface IServerProcess
    {
        ServerParameters Parameters { get; }

        bool IsStopped { get; }
        
        bool IsStartingOrStarted { get; }

        Result Start();

        Result Shutdown();
    }
}
