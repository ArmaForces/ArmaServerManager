using System;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Server.DTOs;
using ArmaForces.Arma.Server.Modset;
using CSharpFunctionalExtensions;

namespace ArmaForces.Arma.Server.Features.Server
{
    public interface IDedicatedServer : IDisposable
    {
        int Port { get; }
        
        IModset Modset { get; }

        int HeadlessClientsConnected { get; }

        bool IsServerStopped { get; }

        Task<ServerStatus> GetServerStatusAsync(CancellationToken cancellationToken);

        Result Start();

        Result Shutdown();
        
        event EventHandler Disposed;
    }
}
