﻿using System;
using System.Runtime.CompilerServices;
using Arma.Server.Config;
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
        private readonly IModset _modset;
        private readonly IServerConfigurationProvider _serverConfigurationProvider;
        private readonly ISettings _settings;
        private IServerProcess _headlessProcess;

        private IServerProcess _serverProcess;

        public DedicatedServer(
            int port,
            ISettings settings,
            IModset modset,
            IServerConfigurationProvider serverConfigurationProvider,
            ILogger<DedicatedServer> logger)
        {
            Port = port;
            _settings = settings;
            _modset = modset;
            _serverConfigurationProvider = serverConfigurationProvider;
            _logger = logger;
        }

        public int Port { get; }

        public bool IsServerStarted => _serverProcess?.IsStarted ?? false;

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

            _logger.LogTrace("Starting server on port {port} with {modsetName} modset.", Port, _modset.Name);

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
                _modset,
                _serverConfigurationProvider.GetModsetConfig(_modset.Name));

            return new ServerProcess(
                _settings.ServerExecutable,
                serverParametersProvider.GetStartupParams(),
                null);
        }

        private ServerProcess CreateHeadlessServerProcess()
        {
            var headlessParametersProvider = new HeadlessParametersProvider(
                Port,
                _modset,
                _serverConfigurationProvider.GetModsetConfig(_modset.Name));

            return new ServerProcess(
                _settings.ServerExecutable,
                headlessParametersProvider.GetStartupParams(),
                null);
        }
    }
}
