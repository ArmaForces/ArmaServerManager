using ArmaForces.Arma.Server.Features.Keys;
using ArmaForces.Arma.Server.Features.Processes;
using ArmaForces.Arma.Server.Providers.Configuration;
using Microsoft.Extensions.Logging;

namespace ArmaForces.Arma.Server.Features.Servers
{
    public class ServerBuilderFactory : IServerBuilderFactory
    {
        private readonly IServerStatusFactory _serverStatusFactory;
        private readonly IKeysPreparer _keysPreparer;
        private readonly IModsetConfigurationProvider _modsetConfigurationProvider;
        private readonly IArmaProcessManager _armaProcessManager;
        private readonly IArmaProcessFactory _armaProcessFactory;
        private readonly ILogger<ServerBuilder> _serverBuilderLogger;
        private readonly ILogger<DedicatedServer> _dedicatedServerLogger;

        public ServerBuilderFactory(
            IServerStatusFactory serverStatusFactory,
            IKeysPreparer keysPreparer,
            IModsetConfigurationProvider modsetConfigurationProvider,
            IArmaProcessManager armaProcessManager,
            IArmaProcessFactory armaProcessFactory,
            ILogger<ServerBuilder> serverBuilderLogger,
            ILogger<DedicatedServer> dedicatedServerLogger)
        {
            _serverStatusFactory = serverStatusFactory;
            _keysPreparer = keysPreparer;
            _modsetConfigurationProvider = modsetConfigurationProvider;
            _armaProcessManager = armaProcessManager;
            _armaProcessFactory = armaProcessFactory;
            _serverBuilderLogger = serverBuilderLogger;
            _dedicatedServerLogger = dedicatedServerLogger;
        }

        public IServerBuilder CreateServerBuilder()
        {
            return new ServerBuilder(
                _keysPreparer,
                _modsetConfigurationProvider,
                _serverStatusFactory,
                _armaProcessManager,
                _armaProcessFactory,
                _serverBuilderLogger,
                _dedicatedServerLogger);
        }
    }
}
