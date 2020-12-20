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
        {
            var serverConfig = new ServerConfig(_settings);
            serverConfig.LoadConfig();
            
            var modsetConfig = new ModsetConfig(serverConfig, _settings, modsetName);
            modsetConfig.LoadConfig();

            return modsetConfig;
        }
    }
}
