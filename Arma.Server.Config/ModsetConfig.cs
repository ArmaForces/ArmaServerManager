using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Arma.Server.Config {
    public class ModsetConfig : IConfig {
        private readonly IConfig _serverConfig;
        private readonly string _modsetConfigDirPath;
        private readonly string _modsetName;
        private readonly ISettings _settings;

        public ModsetConfig(ISettings settings, string modsetName) {
            _settings = settings;
            _serverConfig = new ServerConfig(_settings);
            _serverConfig.LoadConfig();
            _modsetName = modsetName;
            _modsetConfigDirPath = CreateModsetConfigDirPath();
        }

        public string GetConfigDir() {
            return _modsetConfigDirPath;
        }

        private string CreateModsetConfigDirPath() {
            return Path.Join(_serverConfig.GetConfigDir(), "modsetConfigs", _modsetName);
        }

        public string GetServerCfgPath() {
            return Path.Join(_modsetConfigDirPath, "server.cfg");
        }

        public string GetBasicCfgPath() {
            return Path.Join(_modsetConfigDirPath, "basic.cfg");
        }

        public string GetServerProfileDir() {
            return Path.Join(_modsetConfigDirPath, "profiles", "server");
        }

        public string GetHCProfileDir() {
            return Path.Join(_modsetConfigDirPath, "profiles", "HC");
        }

        public Result LoadConfig() {
            return GetOrCreateModsetConfigDir(_serverConfig.GetConfigDir(), _modsetName)
                .Bind(() => PrepareModsetConfig(_modsetConfigDirPath, _modsetName));
        }

        /// <summary>
        /// Prepares config directory and files for current modset
        /// </summary>
        /// <returns>path to modsetConfig</returns>
        private Result GetOrCreateModsetConfigDir(string serverConfigDir, string modsetName) {
            // Get modset config directory based on serverConfig
            var modsetConfigDir = Path.Join(serverConfigDir, "modsetConfigs", modsetName);

            // Check for directory if present
            if (!Directory.Exists(modsetConfigDir)) {
                Directory.CreateDirectory(modsetConfigDir);
            }

            // Prepare modset specific config.json if not exists
            if (!File.Exists(Path.Join(modsetConfigDir, "config.json"))) {
                // Set hostName according to pattern
                var sampleServer = new Dictionary<string, string>();
                sampleServer.Add("hostName", $"ArmaForces {modsetName} edition");
                var sampleJSON = new Dictionary<string, Dictionary<string, string>>();
                sampleJSON.Add("server", sampleServer);
                // Write to file
                using (StreamWriter file = File.CreateText(Path.Join(modsetConfigDir, "config.json"))) {
                    var serializerOptions = new JsonSerializerOptions();
                    serializerOptions.WriteIndented = true;
                    var serializedJson = JsonSerializer.Serialize(sampleJSON, serializerOptions);
                    file.Write(serializedJson);
                }
            }

            return Result.Success();
        }

        /// <summary>
        /// Prepares modset cfg files for server to load.
        /// </summary>
        private Result PrepareModsetConfig(string modsetConfigDir, string modsetName) {
            // Apply modset config on top of default config
            var serverConfigDir = Path.GetFullPath(Path.Join("..", ".."), modsetConfigDir);
            var modsetConfig = new ConfigurationBuilder()
                .AddJsonFile(Path.Join(serverConfigDir, "common.json"))
                .AddJsonFile(Path.Join(modsetConfigDir, "config.json"))
                .Build();

            // Process configuration files for modset
            var configs = new List<string> {"server", "basic"};
            foreach (var config in configs) {
                Console.WriteLine($"Loading {config}.cfg for {modsetName} modset.");
                var cfgFile = ServerConfig.FillCfg(File.ReadAllText($"{serverConfigDir}\\{config}.cfg"),
                    modsetConfig.GetSection(config));
                File.WriteAllText(Path.Join(modsetConfigDir, $"{config}.cfg"), cfgFile);
                Console.WriteLine($"{config}.cfg successfully exported to {modsetConfigDir}");
            }

            return Result.Success();
        }
    }
}