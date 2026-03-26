using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Common.Errors;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Features.Processes;
using ArmaForces.Arma.Server.Features.Servers;
using ArmaForces.Arma.Server.Features.Servers.DTOs;
using AutoFixture;
using CSharpFunctionalExtensions;

namespace ArmaForces.Arma.Server.Tests.Helpers
{
    public class TestDedicatedServer : IDedicatedServer
    {
        public TestDedicatedServer()
        {
        }

        public TestDedicatedServer(Modset modset)
        {
            Modset = modset;
        }

        public void Dispose()
        {
            Shutdown();
            OnServerShutdown?.Invoke(this);
        }

        public int Port { get; set; } = 2302;

        public int SteamQueryPort => Port + 1;

        public Modset Modset { get; set; } = ModsetHelpers.CreateEmptyModset(new Fixture());

        public int HeadlessClientsConnected { get; set; } = 0;

        public bool IsServerStopped { get; set; }

        public DateTimeOffset? StartTime { get; set; }

        public async Task<ServerStatus> GetServerStatusAsync(CancellationToken cancellationToken)
        {
            return new ServerStatus();
        }

        public UnitResult<IError> Start()
        {
            if (!IsServerStopped) return new Error("Server already running.", TestErrorCode.ServerAlreadyRunning);

            IsServerStopped = false;
            return UnitResult.Success<IError>();
        }

        public async Task<UnitResult<IError>> Shutdown()
        {
            if (IsServerStopped) return new Error("Server not running.", ManagerErrorCode.ServerStopped);

            IsServerStopped = true;
            return UnitResult.Success<IError>();
        }

        public UnitResult<IError> AddAndStartHeadlessClients(IEnumerable<IArmaProcess> headlessClients)
            => UnitResult.Success<IError>();

        public Task<UnitResult<IError>> RemoveHeadlessClients(int headlessClientsToRemove)
            => Task.FromResult(UnitResult.Success<IError>());

        public event Func<IDedicatedServer, Task> OnServerShutdown = null!;

        public event Func<IDedicatedServer, Task> OnServerRestarted = null!;
    }
}
