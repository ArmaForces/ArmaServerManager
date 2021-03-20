using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Exceptions;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Features.Processes;
using ArmaForces.Arma.Server.Features.Servers.DTOs;
using ArmaForces.Arma.Server.Providers.Keys;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("ArmaForces.Arma.Server.Tests")]

namespace ArmaForces.Arma.Server.Features.Servers
{
    public class DedicatedServer : IDedicatedServer
    {
        private readonly IModsetConfig _modsetConfig;
        private readonly ILogger<DedicatedServer> _logger;
        private readonly IKeysProvider _keysProvider;
        private readonly IArmaProcessManager _armaProcessManager;

        private readonly List<IArmaProcess> _headlessProcesses;
        private IArmaProcess _armaProcess;

        public DedicatedServer(
            int port,
            IModset modset,
            IModsetConfig modsetConfig,
            IKeysProvider keysProvider,
            IArmaProcessManager armaProcessManager,
            IArmaProcess armaProcess,
            IEnumerable<IArmaProcess> headlessClients,
            ILogger<DedicatedServer> logger)
        {
            Port = port;
            _keysProvider = keysProvider;
            _armaProcessManager = armaProcessManager;
            Modset = modset;
            _modsetConfig = modsetConfig;
            _armaProcess = armaProcess;
            _headlessProcesses = headlessClients.ToList();
            _logger = logger;
        }

        public int Port { get; }

        public IModset Modset { get; }

        public int HeadlessClientsConnected => _headlessProcesses.Count(x => x.IsStartingOrStarted);

        public bool IsServerStopped => _armaProcess?.IsStopped ?? true;
        
        public event Func<IDedicatedServer, Task> OnServerShutdown;

        public event Func<IDedicatedServer, Task> OnServerRestarted;

        public void Dispose() => Task.Run(Shutdown);

        public Result Start()
        {
            if (!IsServerStopped) throw new ServerRunningException();

            _logger.LogTrace("Starting server on port {port} with {modsetName} modset.", Port, Modset.Name);

            return _modsetConfig.CopyConfigFiles()
                .Bind(() => _keysProvider.PrepareKeysForModset(Modset))
                .Bind(() => _armaProcess.Start())
                .Tap(() => _armaProcess.OnProcessShutdown += OnServerProcessShutdown)
                .Bind(() => _headlessProcesses.Select(x => x.Start())
                    .Combine());
        }

        private async Task OnServerProcessShutdown(IArmaProcess armaProcess)
        {
            _logger.LogTrace("Detected server process shutdown on port {port}.", Port);

            await (await _armaProcessManager.CheckServerIsRestarting(armaProcess))
                .Match(
                    onSuccess: async newArmaProcess =>
                    {
                        _logger.LogDebug("Server restart detected.");
                        _armaProcess = newArmaProcess;
                        await InvokeOnServerRestarted();
                    },
                    onFailure: async _ =>
                    {
                        _logger.LogDebug("Server process shutdown itself.");
                        ShutdownHeadlessClients();
                        await InvokeOnServerShutdown();
                    });
        }

        private async Task InvokeOnServerRestarted()
        {
            _logger.LogTrace("Invoking OnServerRestarted event on port {port}.", Port);
            if (OnServerRestarted != null) await OnServerRestarted.Invoke(this);
        }

        public async Task<Result> Shutdown()
        {
            return await _armaProcess.Shutdown()
                .Tap(() => _logger.LogTrace("Server shutdown completed."))
                .Finally(_ => ShutdownHeadlessClients())
                .Finally(_ => InvokeOnServerShutdown());
        }

        public async Task<ServerStatus> GetServerStatusAsync(CancellationToken cancellationToken) 
            => await ServerStatus.GetServerStatus(this, cancellationToken);

        private Result ShutdownHeadlessClients()
        {
            return _headlessProcesses
                .Select(x => x.Shutdown())
                .Combine()
                .Tap(() => _logger.LogTrace("Headless clients shutdown completed."));
        }

        private async Task<Result> InvokeOnServerShutdown()
        {
            _logger.LogTrace("Invoking OnServerShutdown event on port {port}.", Port);
            if (OnServerShutdown != null) await OnServerShutdown.Invoke(this);
            return Result.Success();
        }
    }
}
