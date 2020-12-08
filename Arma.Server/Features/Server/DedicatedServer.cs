﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Arma.Server.Config;
using Arma.Server.Features.Server.DTOs;
using Arma.Server.Modset;
using Arma.Server.Providers.Configuration;
using Arma.Server.Providers.Parameters;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("Arma.Server.Test")]

namespace Arma.Server.Features.Server
{
    public class DedicatedServer : IDedicatedServer
    {
        private readonly ILogger<DedicatedServer> _logger;
        private readonly ILogger<ServerProcess> _serverProcessLogger;
        private readonly IServerConfigurationProvider _serverConfigurationProvider;
        private readonly ISettings _settings;

        private IServerProcess _headlessProcess;

        private IServerProcess _serverProcess;
        
        public DedicatedServer(
            int port,
            ISettings settings,
            IModset modset,
            IServerConfigurationProvider serverConfigurationProvider,
            ILogger<DedicatedServer> logger,
            ILogger<ServerProcess> serverProcessLogger,
            IServerProcess serverProcess) : this(port, settings, modset, serverConfigurationProvider, logger, serverProcessLogger)
        {
            _serverProcess = serverProcess;
        }

        public DedicatedServer(
            int port,
            ISettings settings,
            IModset modset,
            IServerConfigurationProvider serverConfigurationProvider,
            ILogger<DedicatedServer> logger,
            ILogger<ServerProcess> serverProcessLogger)
        {
            Port = port;
            _settings = settings;
            Modset = modset;
            _serverConfigurationProvider = serverConfigurationProvider;
            _logger = logger;
            _serverProcessLogger = serverProcessLogger;
        }

        public int Port { get; }

        public IModset Modset { get; }

        public ServerStatus ServerStatus => new ServerStatus(this);

        public int HeadlessClientsConnected => _headlessProcess is null ? 0 : 1;

        public bool IsServerStarted => ServerStatus.IsServerRunning;

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

            return _serverProcess.Start()
                .Bind(() => _headlessProcess.Start());
        }

        public Result Shutdown()
        {
            _serverProcess?.Shutdown();
            _headlessProcess?.Shutdown();

            _logger.LogTrace("Server shutdown completed");

            return Result.Success();
        }

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
