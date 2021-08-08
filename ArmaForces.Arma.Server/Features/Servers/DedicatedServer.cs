using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Exceptions;
using ArmaForces.Arma.Server.Features.Keys;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Features.Processes;
using ArmaForces.Arma.Server.Features.Servers.DTOs;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("ArmaForces.Arma.Server.Tests")]

namespace ArmaForces.Arma.Server.Features.Servers
{
    public class DedicatedServer : IDedicatedServer
    {
        private readonly IModsetConfig _modsetConfig;
        private readonly IServerStatusFactory _serverStatusFactory;
        private readonly ILogger<DedicatedServer> _logger;
        private readonly IKeysPreparer _keysPreparer;
        private readonly IArmaProcessManager _armaProcessManager;

        private readonly List<IArmaProcess> _headlessProcesses;
        private IArmaProcess _armaProcess;

        public DedicatedServer(
            int port,
            IModset modset,
            IModsetConfig modsetConfig,
            IServerStatusFactory serverStatusFactory,
            IKeysPreparer keysPreparer,
            IArmaProcessManager armaProcessManager,
            IArmaProcess armaProcess,
            IEnumerable<IArmaProcess> headlessClients,
            ILogger<DedicatedServer> logger)
        {
            Port = port;
            SteamQueryPort = port + 1;
            _keysPreparer = keysPreparer;
            _armaProcessManager = armaProcessManager;
            Modset = modset;
            _modsetConfig = modsetConfig;
            _serverStatusFactory = serverStatusFactory;
            _armaProcess = InitializeArmaProcess(armaProcess);

            _headlessProcesses = headlessClients.ToList();
            _logger = logger;
        }
        
        public int Port { get; }
        
        public int SteamQueryPort { get; }

        public IModset Modset { get; }

        public int HeadlessClientsConnected => _headlessProcesses.Count(x => x.IsStartingOrStarted);

        public bool IsServerStopped => _armaProcess.IsStopped;

        public event Func<IDedicatedServer, Task>? OnServerShutdown;

        public event Func<IDedicatedServer, Task>? OnServerRestarted;

        public void Dispose() => Task.Run(Shutdown);

        public Result Start()
        {
            if (!IsServerStopped) throw new ServerRunningException();

            _logger.LogInformation("Starting server on port {Port} with {ModsetName} modset", Port, Modset.Name);
            
            return _modsetConfig.CopyConfigFiles()
                .Bind(() => _keysPreparer.PrepareKeysForModset(Modset))
                .Bind(() => _armaProcess.Start())
                .Tap(() => _armaProcess.OnProcessShutdown += OnServerProcessShutdown)
                .Bind(() => _headlessProcesses.Select(x => x.Start())
                    .Combine());
        }

        public async Task<Result> Shutdown()
        {
            return await _armaProcess.Shutdown()
                .Tap(() => _logger.LogInformation("Server shutdown completed on port {Port}", Port))
                .Finally(_ => ShutdownHeadlessClients())
                .Finally(_ => InvokeOnServerShutdown());
        }

        public async Task<ServerStatus> GetServerStatusAsync(CancellationToken cancellationToken) 
            => await _serverStatusFactory.GetServerStatus(this, cancellationToken);

        private IArmaProcess InitializeArmaProcess(IArmaProcess armaProcess)
        {
            if (armaProcess.IsStartingOrStarted)
            {
                armaProcess.OnProcessShutdown += OnServerProcessShutdown;
            }

            return armaProcess;
        }

        private async Task OnServerProcessShutdown(IArmaProcess armaProcess)
        {
            _logger.LogDebug("Detected server process shutdown on port {Port}", Port);

            await (await _armaProcessManager.CheckServerIsRestarting(armaProcess))
                .Match(
                    onSuccess: async newArmaProcess =>
                    {
                        _logger.LogDebug("Server restart detected");
                        _armaProcess = newArmaProcess;
                        _armaProcess.OnProcessShutdown += OnServerProcessShutdown;
                        await InvokeOnServerRestarted();
                    },
                    onFailure: async _ =>
                    {
                        _logger.LogDebug("Server process shutdown itself");
                        ShutdownHeadlessClients();
                        await InvokeOnServerShutdown();
                    });
        }

        private async Task InvokeOnServerRestarted()
        {
            _logger.LogDebug("Invoking OnServerRestarted event on port {Port}", Port);
            if (OnServerRestarted != null) await OnServerRestarted.Invoke(this);
        }

        private Result ShutdownHeadlessClients()
        {
            return _headlessProcesses
                .Select(x => x.Shutdown())
                .Combine()
                .Tap(() => _logger.LogDebug("Headless clients shutdown completed"));
        }

        private async Task<Result> InvokeOnServerShutdown()
        {
            _logger.LogDebug("Invoking OnServerShutdown event on port {Port}", Port);
            if (OnServerShutdown != null) await OnServerShutdown.Invoke(this);
            return Result.Success();
        }
    }
}
