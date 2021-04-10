using System;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Modsets;
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

        public TestDedicatedServer(IModset modset)
        {
            Modset = modset;
        }

        public void Dispose()
        {
            Shutdown();
            OnServerShutdown?.Invoke(this);
        }

        public int Port { get; set; } = 2302;

        public IModset Modset { get; set; } = ModsetHelpers.CreateEmptyModset(new Fixture());

        public int HeadlessClientsConnected { get; set; } = 0;

        public bool IsServerStopped { get; set; }

        public async Task<ServerStatus> GetServerStatusAsync(CancellationToken cancellationToken)
        {
            return new ServerStatus();
        }

        public Result Start()
        {
            if (!IsServerStopped) return Result.Failure("Server already running.");

            IsServerStopped = false;
            return Result.Success();
        }

        public async Task<Result> Shutdown()
        {
            if (IsServerStopped) return Result.Failure("Server not running.");

            IsServerStopped = true;
            return Result.Success();
        }

        public event Func<IDedicatedServer, Task> OnServerShutdown = null!;

        public event Func<IDedicatedServer, Task> OnServerRestarted = null!;
    }
}
