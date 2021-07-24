﻿using ArmaForces.Arma.Server.Features.Keys;
using ArmaForces.Arma.Server.Features.Processes;
using ArmaForces.Arma.Server.Providers.Configuration;
using Microsoft.Extensions.Logging;

namespace ArmaForces.Arma.Server.Features.Servers
{
    public class ServerBuilderFactory : IServerBuilderFactory
    {
        private readonly IKeysPreparer _keysPreparer;
        private readonly IModsetConfigurationProvider _modsetConfigurationProvider;
        private readonly IArmaProcessManager _armaProcessManager;
        private readonly IArmaProcessFactory _armaProcessFactory;
        private readonly ILogger<DedicatedServer> _dedicatedServerLogger;

        public ServerBuilderFactory(
            IKeysPreparer keysPreparer,
            IModsetConfigurationProvider modsetConfigurationProvider,
            IArmaProcessManager armaProcessManager,
            IArmaProcessFactory armaProcessFactory,
            ILogger<DedicatedServer> dedicatedServerLogger)
        {
            _keysPreparer = keysPreparer;
            _modsetConfigurationProvider = modsetConfigurationProvider;
            _armaProcessManager = armaProcessManager;
            _armaProcessFactory = armaProcessFactory;
            _dedicatedServerLogger = dedicatedServerLogger;
        }

        public IServerBuilder CreateServerBuilder()
        {
            return new ServerBuilder(
                _keysPreparer,
                _modsetConfigurationProvider,
                _armaProcessManager,
                _armaProcessFactory,
                _dedicatedServerLogger);
        }
    }
}
