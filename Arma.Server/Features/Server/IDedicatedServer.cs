using System;
using System.Threading;
using System.Threading.Tasks;
using Arma.Server.Features.Server.DTOs;
using Arma.Server.Modset;
using CSharpFunctionalExtensions;

namespace Arma.Server.Features.Server
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
