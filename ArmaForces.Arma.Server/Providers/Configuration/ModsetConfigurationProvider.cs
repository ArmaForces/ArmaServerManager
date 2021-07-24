﻿using ArmaForces.Arma.Server.Config;
using Microsoft.Extensions.Logging;

namespace ArmaForces.Arma.Server.Providers.Configuration
{
    public class ModsetConfigurationProvider : IModsetConfigurationProvider
    {
        private readonly ISettings _settings;
        private readonly ConfigFileCreator _configFileCreator;
        private readonly ILogger<ModsetConfig> _modsetConfigLogger;

        public ModsetConfigurationProvider(ISettings settings, ConfigFileCreator configFileCreator, ILogger<ModsetConfig> modsetConfigLogger)
        {
            _settings = settings;
            _configFileCreator = configFileCreator;
            _modsetConfigLogger = modsetConfigLogger;
        }

        public IModsetConfig GetModsetConfig(string modsetName)
            => new ModsetConfig(
                GetServerConfig(),
                _settings,
                modsetName,
                _configFileCreator,
                _modsetConfigLogger);

        private IConfig GetServerConfig() => new ServerConfig(_settings);
    }
}