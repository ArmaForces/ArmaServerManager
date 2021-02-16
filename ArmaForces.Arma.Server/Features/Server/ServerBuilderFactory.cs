using ArmaForces.Arma.Server.Providers.Configuration;
using ArmaForces.Arma.Server.Providers.Keys;
using Microsoft.Extensions.Logging;

namespace ArmaForces.Arma.Server.Features.Server
{
    public class ServerBuilderFactory : IServerBuilderFactory
    {
        private readonly IKeysProvider _keysProvider;
        private readonly IServerConfigurationProvider _serverConfigurationProvider;
        private readonly IArmaProcessFactory _armaProcessFactory;
        private readonly ILogger<DedicatedServer> _dedicatedServerLogger;

        public ServerBuilderFactory(
            IKeysProvider keysProvider,
            IServerConfigurationProvider serverConfigurationProvider,
            IArmaProcessFactory armaProcessFactory,
            ILogger<DedicatedServer> dedicatedServerLogger)
        {
            _keysProvider = keysProvider;
            _serverConfigurationProvider = serverConfigurationProvider;
            _armaProcessFactory = armaProcessFactory;
            _dedicatedServerLogger = dedicatedServerLogger;
        }

        public IServerBuilder CreateServerBuilder()
        {
            return new ServerBuilder(
                _keysProvider,
                _serverConfigurationProvider,
                _armaProcessFactory,
                _dedicatedServerLogger);
        }
    }
}
