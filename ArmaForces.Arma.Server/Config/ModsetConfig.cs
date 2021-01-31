using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Text.Json;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ArmaForces.Arma.Server.Config
{
    public class ModsetConfig : IModsetConfig
    {
        public string BasicCfg { get; protected set; }
        public string ConfigJson { get; protected set; }
        public string DirectoryPath { get; protected set; }
        public string HCProfileDirectory { get; protected set; }
        public string ModsetName { get; protected set; }
        public string ServerCfg { get; protected set; }
        public string ServerProfileDirectory { get; protected set; }
        public string ServerPassword { get; protected set; }

        private readonly IConfig _serverConfig;
        private readonly IFileSystem _fileSystem;
        private readonly ISettings _settings;
        private readonly ConfigFileCreator _configFileCreator;
        private readonly ILogger<ModsetConfig> _logger;

        // TODO: Create factory
        public ModsetConfig(
            IConfig serverConfig,
            ISettings settings,
            string modsetName,
            ConfigFileCreator configFileCreator,
            ILogger<ModsetConfig> logger,
            IFileSystem fileSystem = null)
        {
            _serverConfig = serverConfig;
            _settings = settings;
            _configFileCreator = configFileCreator;
            _logger = logger;
            ModsetName = modsetName;
            _fileSystem = fileSystem ?? new FileSystem();
        }

        public Result LoadConfig()
        {
            return SetProperties()
                .Bind(GetOrCreateModsetConfigDir)
                .Bind(PrepareModsetConfig);
        }

        private Result SetProperties()
        {
            DirectoryPath = Path.Join(_serverConfig.DirectoryPath, _settings.ModsetConfigDirectoryName, ModsetName);
            ConfigJson = Path.Join(DirectoryPath, "config.json");
            BasicCfg = Path.Join(DirectoryPath, "basic.cfg");
            ServerCfg = Path.Join(DirectoryPath, "server.cfg");
            HCProfileDirectory = Path.Join(DirectoryPath, "profiles", "HC");
            ServerProfileDirectory = Path.Join(DirectoryPath, "profiles", "Server");
            return Result.Success();
        }

        /// <summary>
        /// Prepares config directory and files for current modset
        /// </summary>
        /// <returns>path to modsetConfig</returns>
        private Result GetOrCreateModsetConfigDir()
        {
            // Check for directory if present
            if (!_fileSystem.Directory.Exists(DirectoryPath))
            {
                _fileSystem.Directory.CreateDirectory(DirectoryPath);
            }

            return _fileSystem.File.Exists(ConfigJson)
                ? Result.Success()
                : CreateConfigJson();
        }

        private Result CreateConfigJson()
        {
            // Set hostName according to pattern
            var sampleServer = new Dictionary<string, string>();
            sampleServer.Add("hostName", $"ArmaForces {ModsetName} edition");
            var sampleJSON = new Dictionary<string, Dictionary<string, string>>();
            sampleJSON.Add("server", sampleServer);
            // Write to file
            using (var file = _fileSystem.File.CreateText(ConfigJson))
            {
                var serializerOptions = new JsonSerializerOptions {WriteIndented = true};
                var serializedJson = JsonSerializer.Serialize(sampleJSON, serializerOptions);
                file.Write(serializedJson);
            }
            return Result.Success();
        }

        /// <summary>
        /// Prepares modset cfg files for server to load.
        /// </summary>
        private Result PrepareModsetConfig()
        {
            // Apply modset config on top of default config
            var modsetConfig = new ConfigurationBuilder()
                .AddJsonStream(_fileSystem.FileStream.Create(_serverConfig.ConfigJson, FileMode.Open))
                .AddJsonStream(_fileSystem.FileStream.Create(ConfigJson, FileMode.Open))
                .Build();

            ServerPassword = modsetConfig["server:password"] ?? string.Empty;

            CreateConfigFiles(_serverConfig.BasicCfg, BasicCfg, modsetConfig);
            CreateConfigFiles(_serverConfig.ServerCfg, ServerCfg, modsetConfig);

            return Result.Success();
        }

        private void CreateConfigFiles(
            string serverConfigFilePath,
            string modsetConfigFilePath,
            IConfiguration modsetConfig)
        {
            var fileName = Path.GetFileNameWithoutExtension(serverConfigFilePath);
            _logger.LogDebug("Loading {fileName} for {modsetName} modset.", fileName, ModsetName);

            var cfgFile = _configFileCreator.FillCfg(
                _fileSystem.File.ReadAllText(serverConfigFilePath),
                modsetConfig.GetSection(fileName));

            _fileSystem.File.WriteAllText(modsetConfigFilePath, cfgFile);
            _logger.LogDebug("{fileName} successfully exported to {directoryPath}", fileName, DirectoryPath);
        }
    }
}