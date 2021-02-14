using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Providers.Configuration;
using Microsoft.Extensions.Logging;

namespace ArmaForces.Arma.Server.Features.Server
{
    public class ServerBuilderFactory : IServerBuilderFactory
    {
        private readonly ISettings _settings;
        private readonly IServerConfigurationProvider _serverConfigurationProvider;
        private readonly IServerProcessFactory _serverProcessFactory;
        private readonly ILogger<DedicatedServer> _dedicatedServerLogger;

        public ServerBuilderFactory(
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

        public IServerBuilder CreateServerBuilder()
        {
            return new ServerBuilder(
                _settings,
                _serverConfigurationProvider,
                _serverProcessFactory,
                _dedicatedServerLogger);
        }
    }
}
