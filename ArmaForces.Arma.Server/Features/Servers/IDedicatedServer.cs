using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Features.Processes;
using ArmaForces.Arma.Server.Features.Servers.DTOs;
using CSharpFunctionalExtensions;

namespace ArmaForces.Arma.Server.Features.Servers
{
    public interface IDedicatedServer : IDisposable
    {
        int Port { get; }
        
        int SteamQueryPort { get; }
        
        IModset Modset { get; }

        int HeadlessClientsConnected { get; }

        bool IsServerStopped { get; }

        DateTimeOffset? StartTime { get; }

        Task<ServerStatus> GetServerStatusAsync(CancellationToken cancellationToken);

        Result Start();

        Task<Result> Shutdown();

        Result AddAndStartHeadlessClients(IEnumerable<IArmaProcess> headlessClients);

        Task<Result> RemoveHeadlessClients(int headlessClientsToRemove);

        public event Func<IDedicatedServer, Task> OnServerShutdown;

        public event Func<IDedicatedServer, Task> OnServerRestarted;
    }
}
