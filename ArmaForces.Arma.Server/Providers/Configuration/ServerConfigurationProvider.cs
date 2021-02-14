using ArmaForces.Arma.Server.Config;

namespace ArmaForces.Arma.Server.Providers.Configuration
{
    public class ServerConfigurationProvider : IServerConfigurationProvider
    {
        private readonly ISettings _settings;

        public ServerConfigurationProvider(ISettings settings)
        {
            _settings = settings;
        }

        public IModsetConfig GetModsetConfig(string modsetName)
            => new ModsetConfig(
                GetServerConfig(),
                _settings,
                modsetName);

        private IConfig GetServerConfig() => new ServerConfig(_settings);
    }
}
