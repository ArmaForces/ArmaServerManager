using ArmaForces.Arma.Server.Providers.Configuration;
using ArmaForces.Arma.Server.Providers.Keys;
using Microsoft.Extensions.Logging;

namespace ArmaForces.Arma.Server.Features.Server
{
    public class ServerBuilderFactory : IServerBuilderFactory
    {
        private readonly IKeysProvider _keysProvider;
        private readonly IServerConfigurationProvider _serverConfigurationProvider;
        private readonly IArmaProcessManager _armaProcessManager;
        private readonly IArmaProcessFactory _armaProcessFactory;
        private readonly ILogger<DedicatedServer> _dedicatedServerLogger;

        public ServerBuilderFactory(
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

        public IServerBuilder CreateServerBuilder()
        {
            return new ServerBuilder(
                _keysProvider,
                _serverConfigurationProvider,
                _armaProcessManager,
                _armaProcessFactory,
                _dedicatedServerLogger);
        }
    }
}
