using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Features.Server.DTOs;
using ArmaForces.Arma.Server.Providers.Configuration;
using ArmaForces.Arma.Server.Providers.Keys;
using ArmaForces.Arma.Server.Providers.Parameters;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("ArmaForces.Arma.Server.Tests")]

namespace ArmaForces.Arma.Server.Features.Server
{
    public class DedicatedServer : IDedicatedServer
    {
        private readonly ILogger<DedicatedServer> _logger;
        private readonly ILogger<ServerProcess> _serverProcessLogger;
        private readonly IServerConfigurationProvider _serverConfigurationProvider;
        private readonly ISettings _settings;
        private readonly IKeysProvider _keysProvider;

        private IServerProcess _headlessProcess;

        private IServerProcess _serverProcess;
        
        public DedicatedServer(
            int port,
            ISettings settings,
            IModset modset,
            IKeysProvider keysProvider,
            IServerConfigurationProvider serverConfigurationProvider,
            ILogger<DedicatedServer> logger,
            ILogger<ServerProcess> serverProcessLogger,
            IServerProcess serverProcess) : this(
            port,
            settings,
            modset,
            keysProvider,
            serverConfigurationProvider,
            logger,
            serverProcessLogger)
        {
            _serverProcess = serverProcess;
        }

        public DedicatedServer(
            int port,
            ISettings settings,
            IModset modset,
            IKeysProvider keysProvider,
            IServerConfigurationProvider serverConfigurationProvider,
            ILogger<DedicatedServer> logger,
            ILogger<ServerProcess> serverProcessLogger)
        {
            Port = port;
            _settings = settings;
            _keysProvider = keysProvider;
            Modset = modset;
            _serverConfigurationProvider = serverConfigurationProvider;
            _logger = logger;
            _serverProcessLogger = serverProcessLogger;
        }

        public int Port { get; }

        public IModset Modset { get; }

        public int HeadlessClientsConnected => _headlessProcess is null ? 0 : 1;

        public bool IsServerStopped => _serverProcess?.IsStopped ?? true;
        
        public event EventHandler Disposed;

        public void Dispose()
        {
            Shutdown();

            _serverProcess = null;
            _headlessProcess = null;

            Disposed?.Invoke(this, EventArgs.Empty);
        }

        public Result Start()
        {
            _serverProcess = CreateDedicatedServerProcess();
            _headlessProcess = CreateHeadlessServerProcess();

            _logger.LogTrace("Starting server on port {port} with {modsetName} modset.", Port, Modset.Name);

            return _keysProvider.PrepareKeysForModset(Modset)
                .Bind(() => _serverProcess.Start())
                .Bind(() => _headlessProcess.Start());
        }

        public Result Shutdown()
        {
            _serverProcess?.Shutdown();
            _headlessProcess?.Shutdown();

            _logger.LogTrace("Server shutdown completed");

            return Result.Success();
        }

        public async Task<ServerStatus> GetServerStatusAsync(CancellationToken cancellationToken) 
            => await ServerStatus.GetServerStatus(this, cancellationToken);

        private ServerProcess CreateDedicatedServerProcess()
        {
            var serverParametersProvider = new ServerParametersProvider(
                Port,
                Modset,
                _serverConfigurationProvider.GetModsetConfig(Modset.Name));

            return CreateServerProcess(serverParametersProvider);
        }

        private ServerProcess CreateHeadlessServerProcess()
        {
            var headlessParametersProvider = new HeadlessParametersProvider(
                Port,
                Modset,
                _serverConfigurationProvider.GetModsetConfig(Modset.Name));

            return CreateServerProcess(headlessParametersProvider);
        }

        private ServerProcess CreateServerProcess(IParametersProvider parametersProvider) 
            => new ServerProcess(
                _settings.ServerExecutable,
                parametersProvider.GetStartupParams(),
                _serverProcessLogger);
    }
}
