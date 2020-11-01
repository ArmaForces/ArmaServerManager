using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Arma.Server.Config
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

        private readonly IConfig _serverConfig;
        private readonly IFileSystem _fileSystem;
        private readonly ISettings _settings;

        public ModsetConfig(
            IConfig serverConfig,
            ISettings settings,
            string modsetName,
            IFileSystem fileSystem = null)
        {
            _serverConfig = serverConfig;
            _settings = settings;
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
            using (StreamWriter file = _fileSystem.File.CreateText(ConfigJson))
            {
                var serializerOptions = new JsonSerializerOptions();
                serializerOptions.WriteIndented = true;
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
                .AddJsonFile(_serverConfig.ConfigJson)
                .AddJsonFile(ConfigJson)
                .Build();

            // Process configuration files for modset
            var configs = new List<string> { "server", "basic" };
            foreach (var config in configs)
            {
                Console.WriteLine($"Loading {config}.cfg for {ModsetName} modset.");

                var somePath = Path.Join($"{_serverConfig.DirectoryPath}", $"{config}.cfg");

                var cfgFile = ConfigFileCreator.FillCfg(
                    _fileSystem.File.ReadAllText(somePath),
                    modsetConfig.GetSection(config));

                _fileSystem.File.WriteAllText(Path.Join(DirectoryPath, $"{config}.cfg"), cfgFile);
                Console.WriteLine($"{config}.cfg successfully exported to {DirectoryPath}");
            }

            return Result.Success();
        }
    }
}