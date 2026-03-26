using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Common.Errors;
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
        
        Modset Modset { get; }

        int HeadlessClientsConnected { get; }

        bool IsServerStopped { get; }

        DateTimeOffset? StartTime { get; }

        Task<ServerStatus> GetServerStatusAsync(CancellationToken cancellationToken);

        UnitResult<IError> Start();

        Task<UnitResult<IError>> Shutdown();

        UnitResult<IError> AddAndStartHeadlessClients(IEnumerable<IArmaProcess> headlessClients);

        Task<UnitResult<IError>> RemoveHeadlessClients(int headlessClientsToRemove);

        public event Func<IDedicatedServer, Task> OnServerShutdown;

        public event Func<IDedicatedServer, Task> OnServerRestarted;
    }
}
