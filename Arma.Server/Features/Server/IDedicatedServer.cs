using System;
using Arma.Server.Modset;
using CSharpFunctionalExtensions;

namespace Arma.Server.Features.Server
{
    public interface IDedicatedServer : IDisposable
    {
        int Port { get; }
        
        IModset Modset { get; }

        int HeadlessClientsConnected { get; }

        bool IsServerStarted { get; }

        bool IsServerStopped { get; }

        Result Start();

        Result Shutdown();

        event EventHandler Disposed;
    }
}
