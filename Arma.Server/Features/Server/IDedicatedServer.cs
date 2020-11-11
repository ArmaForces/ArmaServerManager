using System;
using CSharpFunctionalExtensions;

namespace Arma.Server.Features.Server
{
    public interface IDedicatedServer : IDisposable
    {
        int Port { get; }

        bool IsServerStarted { get; }

        bool IsServerStopped { get; }

        Result Start();

        Result Shutdown();

        event EventHandler Disposed;
    }
}
