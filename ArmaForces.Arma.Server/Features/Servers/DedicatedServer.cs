using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Common.Errors;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Exceptions;
using ArmaForces.Arma.Server.Extensions;
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

        private readonly ConcurrentStack<IArmaProcess> _headlessProcesses;
        private IArmaProcess _serverProcess;

        private bool _serverWasStarted;

        public DedicatedServer(
            int port,
            Modset modset,
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
            _serverProcess = InitializeArmaProcess(armaProcess);

            _headlessProcesses = new ConcurrentStack<IArmaProcess>(
                headlessClients.Where(x => x.ProcessType == ArmaProcessType.HeadlessClient));
            _logger = logger;
        }
        
        public int Port { get; }
        
        public int SteamQueryPort { get; }

        public Modset Modset { get; }

        // TODO: This will be called frequently due to server status requests. Consider caching this (or whole server state).
        public int HeadlessClientsConnected => _headlessProcesses.Count(x => x.IsStartingOrStarted);

        public bool IsServerStopped => _serverProcess.IsStopped;

        public DateTimeOffset? StartTime => _serverProcess.StartTime;

        public event Func<IDedicatedServer, Task>? OnServerShutdown;

        public event Func<IDedicatedServer, Task>? OnServerRestarted;

        public void Dispose() => Task.Run(Shutdown);

        public UnitResult<IError> Start()
        {
            if (!IsServerStopped) throw new ServerRunningException();
            if (_serverWasStarted) throw new InvalidOperationException("Cannot start a previously stopped server.");

            _logger.LogInformation("Starting server on port {Port} with {ModsetName} modset", Port, Modset.Name);
            
            return _modsetConfig.CopyConfigFiles()
                .Bind(() => _keysPreparer.PrepareKeysForModset(Modset))
                .Bind(() => _serverProcess.Start())
                .Tap(() => _serverWasStarted = true)
                .Tap(() => _serverProcess.OnProcessShutdown += OnServerProcessShutdown)
                .Bind(() => _headlessProcesses.Select(x => x.Start())
                    .Combine());
        }

        public async Task<UnitResult<IError>> Shutdown()
        {
            return await _serverProcess.Shutdown()
                .Tap(() => _logger.LogInformation("Server shutdown completed on port {Port}", Port))
                .Finally(_ => ShutdownHeadlessClients(_headlessProcesses))
                .Finally(_ => InvokeOnServerShutdown());
        }

        public async Task<ServerStatus> GetServerStatusAsync(CancellationToken cancellationToken) 
            => await _serverStatusFactory.GetServerStatus(this, cancellationToken);

        public UnitResult<IError> AddAndStartHeadlessClients(IEnumerable<IArmaProcess> headlessClients)
            => IsServerStopped && _serverWasStarted
                ? new Error("The server has been shut down.", ManagerErrorCode.ServerStopped)
                : AddAndStartHeadlessClientsInternal(headlessClients).Combine();

        public async Task<UnitResult<IError>> RemoveHeadlessClients(int headlessClientsToRemove)
        {
            if (IsServerStopped && _serverWasStarted)
                return new Error("The server has been shut down.", ManagerErrorCode.ServerStopped);
            
            var poppedClients = new IArmaProcess[headlessClientsToRemove];
            _headlessProcesses.TryPopRange(poppedClients, startIndex: 0, headlessClientsToRemove);

            return await ShutdownHeadlessClients(poppedClients);
        }

        private IEnumerable<UnitResult<IError>> AddAndStartHeadlessClientsInternal(IEnumerable<IArmaProcess> headlessClients)
        {
            foreach (var headlessClient in headlessClients
                         .Where(x => x.ProcessType == ArmaProcessType.HeadlessClient))
            {
                _headlessProcesses.Push(headlessClient);
                if (!IsServerStopped)
                {
                    yield return headlessClient.Start();
                }
            }
        }

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
                        _serverProcess = newArmaProcess;
                        _serverProcess.OnProcessShutdown += OnServerProcessShutdown;
                        await InvokeOnServerRestarted();
                    },
                    onFailure: async _ =>
                    {
                        _logger.LogDebug("Server process shutdown itself");
                        await ShutdownHeadlessClients(_headlessProcesses);
                        await InvokeOnServerShutdown();
                    });
        }

        private async Task InvokeOnServerRestarted()
        {
            _logger.LogDebug("Invoking OnServerRestarted event on port {Port}", Port);
            if (OnServerRestarted != null) await OnServerRestarted.Invoke(this);
        }

        private async Task<UnitResult<IError>> ShutdownHeadlessClients(IEnumerable<IArmaProcess> headlessProcesses)
        {
            return await headlessProcesses
                .AsParallel()
                .Select(x => x.Shutdown())
                .Combine()
                .Tap(() => _logger.LogDebug("Headless clients shutdown completed"));
        }

        private async Task<UnitResult<IError>> InvokeOnServerShutdown()
        {
            _logger.LogDebug("Invoking OnServerShutdown event on port {Port}", Port);
            if (OnServerShutdown != null) await OnServerShutdown.Invoke(this);
            return UnitResult.Success<IError>();
        }
    }
}