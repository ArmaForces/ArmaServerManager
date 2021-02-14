using System.Collections.Generic;
using System.Linq;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Providers.Configuration;
using Microsoft.Extensions.Logging;

namespace ArmaForces.Arma.Server.Features.Server
{
    public class ServerBuilder : IServerBuilder
    {
        private readonly ISettings _settings;
        private readonly IServerConfigurationProvider _serverConfigurationProvider;
        private readonly IServerProcessFactory _serverProcessFactory;
        private readonly ILogger<DedicatedServer> _dedicatedServerLogger;

        private int _port;
        private IModset _modset;
        private IServerProcess _serverProcess;
        private int _numberOfHeadlessClients;
        private IReadOnlyList<IServerProcess> _headlessClients;

        public ServerBuilder(
            ISettings settings,
            IServerConfigurationProvider serverConfigurationProvider,
            IServerProcessFactory serverProcessFactory,
            ILogger<DedicatedServer> dedicatedServerLogger)
        {
            _settings = settings;
            _serverConfigurationProvider = serverConfigurationProvider;
            _serverProcessFactory = serverProcessFactory;
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

        public IServerBuilder WithServerProcess(IServerProcess serverProcess)
        {
            _serverProcess = serverProcess;
            return this;
        }

        public IServerBuilder WithHeadlessClients(IEnumerable<IServerProcess> headlessClients)
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

            var serverProcess = _serverProcess
                                ?? _serverProcessFactory.CreateServerProcess(
                                    _port,
                                    _modset,
                                    modsetConfig);

            var headlessClients = _headlessClients
                                  ?? _serverProcessFactory.CreateHeadlessClients(
                                      _port,
                                      _modset,
                                      modsetConfig,
                                      _numberOfHeadlessClients);

            return new DedicatedServer(
                _port,
                _modset,
                modsetConfig,
                _dedicatedServerLogger,
                serverProcess,
                headlessClients);
        }
    }
}
