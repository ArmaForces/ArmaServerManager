using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Arma.Server.Config {
    public class ModlistConfig : IModlistConfig {
        public string BasicCfg { get; protected set; }
        public string ConfigJson { get; protected set; }
        public string DirectoryPath { get; protected set; }
        public string HCProfileDirectory { get; protected set; }
        public string ModlistName { get; protected set; }
        public string ServerCfg { get; protected set; }
        public string ServerProfileDirectory { get; protected set; }

        private readonly IConfig _serverConfig;
        private readonly ISettings _settings;

        public ModlistConfig(ISettings settings, string modlistName) {
            _settings = settings;
            _serverConfig = new ServerConfig(_settings);
            ModlistName = modlistName;
        }

        public Result LoadConfig() {
            return _serverConfig.LoadConfig()
                .OnFailure(e => {})
                .Tap(() => SetProperties())
                .Bind(GetOrCreateModlistConfigDir)
                .Bind(PrepareModlistConfig);
        }

        private Result SetProperties() {
            DirectoryPath = Path.Join(_serverConfig.DirectoryPath, _settings.ModlistConfigDirectoryName, ModlistName);
            ConfigJson = Path.Join(DirectoryPath, "config.json");
            BasicCfg = Path.Join(DirectoryPath, "basic.cfg");
            ServerCfg = Path.Join(DirectoryPath, "server.cfg");
            HCProfileDirectory = Path.Join(DirectoryPath, "profiles", "HC");
            ServerProfileDirectory = Path.Join(DirectoryPath, "profiles", "Server");
            return Result.Success();
        }

        /// <summary>
        /// Prepares config directory and files for current modlist
        /// </summary>
        /// <returns>path to modlistConfig</returns>
        private Result GetOrCreateModlistConfigDir() {
            // Check for directory if present
            if (!Directory.Exists(DirectoryPath)) {
                Directory.CreateDirectory(DirectoryPath);
            }

            return File.Exists(ConfigJson)
                ? Result.Success()
                : CreateConfigJson();
        }

        private Result CreateConfigJson() {
            // Set hostName according to pattern
            var sampleServer = new Dictionary<string, string>();
            sampleServer.Add("hostName", $"ArmaForces {ModlistName} edition");
            var sampleJSON = new Dictionary<string, Dictionary<string, string>>();
            sampleJSON.Add("server", sampleServer);
            // Write to file
            using (StreamWriter file = File.CreateText(ConfigJson))
            {
                var serializerOptions = new JsonSerializerOptions();
                serializerOptions.WriteIndented = true;
                var serializedJson = JsonSerializer.Serialize(sampleJSON, serializerOptions);
                file.Write(serializedJson);
            }
            return Result.Success();
        }

        /// <summary>
        /// Prepares modlist cfg files for server to load.
        /// </summary>
        private Result PrepareModlistConfig() {
            // Apply modlist config on top of default config
            var modlistConfig = new ConfigurationBuilder()
                .AddJsonFile(_serverConfig.ConfigJson)
                .AddJsonFile(ConfigJson)
                .Build();

            // Process configuration files for modlist
            var configs = new List<string> {"server", "basic"};
            foreach (var config in configs) {
                Console.WriteLine($"Loading {config}.cfg for {ModlistName} modlist.");
                var cfgFile = ServerConfig.FillCfg(
                    File.ReadAllText($"{_serverConfig.DirectoryPath}\\{config}.cfg"),
                    modlistConfig.GetSection(config));
                File.WriteAllText(Path.Join(DirectoryPath, $"{config}.cfg"), cfgFile);
                Console.WriteLine($"{config}.cfg successfully exported to {DirectoryPath}");
            }

            return Result.Success();
        }
    }
}