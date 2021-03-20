﻿using System.Collections.Generic;
using System.Linq;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Features.Processes;
using ArmaForces.Arma.Server.Providers.Configuration;
using ArmaForces.Arma.Server.Providers.Keys;
using Microsoft.Extensions.Logging;

namespace ArmaForces.Arma.Server.Features.Servers
{
    public class ServerBuilder : IServerBuilder
    {
        private readonly IKeysProvider _keysProvider;
        private readonly IServerConfigurationProvider _serverConfigurationProvider;
        private readonly IArmaProcessManager _armaProcessManager;
        private readonly IArmaProcessFactory _armaProcessFactory;
        private readonly ILogger<DedicatedServer> _dedicatedServerLogger;

        private int _port;
        private IModset _modset;
        private IArmaProcess _armaProcess;
        private int _numberOfHeadlessClients;
        private IReadOnlyList<IArmaProcess> _headlessClients;

        public ServerBuilder(
            IKeysProvider keysProvider,
            IServerConfigurationProvider serverConfigurationProvider,
            IArmaProcessManager armaProcessManager,
            IArmaProcessFactory armaProcessFactory,
            ILogger<DedicatedServer> dedicatedServerLogger)
        {
            _keysProvider = keysProvider;
            _serverConfigurationProvider = serverConfigurationProvider;
            _armaProcessManager = armaProcessManager;
            _armaProcessFactory = armaProcessFactory;
            _dedicatedServerLogger = dedicatedServerLogger;
        }

        public IServerBuilder OnPort(int port)
        {
            _port = port;
            return this;
        }

        public IServerBuilder WithModset(IModset modset)
        {
            _modset = modset;
            return this;
        }

        public IServerBuilder WithServerProcess(IArmaProcess armaProcess)
        {
            _armaProcess = armaProcess;
            return this;
        }

        public IServerBuilder WithHeadlessClients(IEnumerable<IArmaProcess> headlessClients)
        {
            _headlessClients = headlessClients?.ToList();
            return this;
        }

        public IServerBuilder WithHeadlessClients(int numberOfHeadlessClients)
        {
            _numberOfHeadlessClients = numberOfHeadlessClients;
            return this;
        }

        public IDedicatedServer Build()
        {
            var modsetConfig = _serverConfigurationProvider.GetModsetConfig(_modset.Name);

            var serverProcess = _armaProcess
                                ?? _armaProcessFactory.CreateServerProcess(
                                    _port,
                                    _modset,
                                    modsetConfig);

            var headlessClients = _headlessClients
                                  ?? _armaProcessFactory.CreateHeadlessClients(
                                      _port,
                                      _modset,
                                      modsetConfig,
                                      _numberOfHeadlessClients);

            return new DedicatedServer(
                _port,
                _modset,
                modsetConfig,
                _keysProvider,
                _armaProcessManager,
                serverProcess,
                headlessClients,
                _dedicatedServerLogger);
        }
    }
}