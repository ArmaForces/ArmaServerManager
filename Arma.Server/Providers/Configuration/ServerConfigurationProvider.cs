using System;
using Arma.Server.Config;

namespace Arma.Server.Providers.Configuration
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
            
            return new ModsetConfig(serverConfig, _settings, modsetName);
        }
    }
}
