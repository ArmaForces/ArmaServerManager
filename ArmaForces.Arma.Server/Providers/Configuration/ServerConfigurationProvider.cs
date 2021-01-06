using ArmaForces.Arma.Server.Config;
using Microsoft.Extensions.Logging;

namespace ArmaForces.Arma.Server.Providers.Configuration
{
    public class ServerConfigurationProvider : IServerConfigurationProvider
    {
        private readonly ISettings _settings;
        private readonly ConfigFileCreator _configFileCreator;
        private readonly ILogger<ModsetConfig> _modsetConfigLogger;

        public ServerConfigurationProvider(ISettings settings, ConfigFileCreator configFileCreator, ILogger<ModsetConfig> modsetConfigLogger)
        {
            _settings = settings;
            _configFileCreator = configFileCreator;
            _modsetConfigLogger = modsetConfigLogger;
        }

        public IModsetConfig GetModsetConfig(string modsetName)
        {
            var serverConfig = new ServerConfig(_settings);
            serverConfig.LoadConfig();
            
            var modsetConfig = new ModsetConfig(
                serverConfig,
                _settings,
                modsetName,
                _configFileCreator,
                _modsetConfigLogger);
            modsetConfig.LoadConfig();

            return modsetConfig;
        }
    }
}
