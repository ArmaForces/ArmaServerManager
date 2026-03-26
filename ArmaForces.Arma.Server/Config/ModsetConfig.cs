using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Text.Json;
using ArmaForces.Arma.Server.Common.Errors;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using JsonSerializer = System.Text.Json.JsonSerializer;

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

        // TODO: Create factory, it will be easier to get rid of nullable warnings
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public ModsetConfig(
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
            IConfig serverConfig,
            ISettings settings,
            string modsetName,
            ConfigFileCreator configFileCreator,
            ILogger<ModsetConfig> logger,
            IFileSystem? fileSystem = null)
        {
            _serverConfig = serverConfig;
            _settings = settings;
            _configFileCreator = configFileCreator;
            _logger = logger;
            ModsetName = modsetName;
            _fileSystem = fileSystem ?? new FileSystem();
            SetProperties();
        }

        public UnitResult<IError> CopyConfigFiles()
        {
            return _serverConfig.CopyConfigFiles()
                .Bind(GetOrCreateModsetConfigDir)
                .Bind(PrepareModsetConfig);
        }

        private void SetProperties()
        {
            DirectoryPath = Path.Join(_serverConfig.DirectoryPath, _settings.ModsetConfigDirectoryName, ModsetName);
            ConfigJson = Path.Join(DirectoryPath, "config.json");
            BasicCfg = Path.Join(DirectoryPath, "basic.cfg");
            ServerCfg = Path.Join(DirectoryPath, "server.cfg");
            HCProfileDirectory = Path.Join(DirectoryPath, "profiles", "HC");
            ServerProfileDirectory = Path.Join(DirectoryPath, "profiles", "Server");
            ServerPassword = LoadServerPassword();
        }

        private string LoadServerPassword()
        {
            if (_fileSystem.File.Exists(ConfigJson))
            {
                var modsetConfigModel =
                    JsonSerializer.Deserialize<ConfigSimpleModel>(_fileSystem.File.ReadAllText(ConfigJson));
                if (modsetConfigModel.Server?.Password != null)
                {
                    return modsetConfigModel.Server.Password;
                }
            }

            if (!_fileSystem.File.Exists(_serverConfig.ConfigJson)) return string.Empty;

            var serverConfigModel =
                JsonSerializer.Deserialize<ConfigSimpleModel>(
                    _fileSystem.File.ReadAllText(_serverConfig.ConfigJson));

            return serverConfigModel.Server?.Password ?? string.Empty;
        }

        /// <summary>
        /// Prepares config directory and files for current modset
        /// </summary>
        /// <returns>path to modsetConfig</returns>
        private UnitResult<IError> GetOrCreateModsetConfigDir()
        {
            // Check for directory if present
            if (!_fileSystem.Directory.Exists(DirectoryPath))
            {
                _fileSystem.Directory.CreateDirectory(DirectoryPath);
            }

            return _fileSystem.File.Exists(ConfigJson)
                ? UnitResult.Success<IError>()
                : CreateConfigJson();
        }

        private UnitResult<IError> CreateConfigJson()
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
            return UnitResult.Success<IError>();
        }

        /// <summary>
        /// Prepares modset cfg files for server to load.
        /// </summary>
        private UnitResult<IError> PrepareModsetConfig()
        {
            // Apply modset config on top of default config
            var modsetConfig = new ConfigurationBuilder()
                .AddJsonStream(_fileSystem.FileStream.New(_serverConfig.ConfigJson, FileMode.Open))
                .AddJsonStream(_fileSystem.FileStream.New(ConfigJson, FileMode.Open))
                .Build();

            ServerPassword = modsetConfig["server:password"] ?? string.Empty;

            CreateConfigFiles(_serverConfig.BasicCfg, BasicCfg, modsetConfig);
            CreateConfigFiles(_serverConfig.ServerCfg, ServerCfg, modsetConfig);

            return UnitResult.Success<IError>();
        }

        private void CreateConfigFiles(
            string serverConfigFilePath,
            string modsetConfigFilePath,
            IConfiguration modsetConfig)
        {
            var fileName = Path.GetFileNameWithoutExtension(serverConfigFilePath);
            _logger.LogDebug("Loading {FileName} for {ModsetName} modset", fileName, ModsetName);

            var cfgFile = _configFileCreator.FillCfg(
                _fileSystem.File.ReadAllText(serverConfigFilePath),
                modsetConfig.GetSection(fileName));

            _fileSystem.File.WriteAllText(modsetConfigFilePath, cfgFile);
            _logger.LogDebug("{FileName} successfully exported to {Directory}", fileName, DirectoryPath);
        }
    }
}