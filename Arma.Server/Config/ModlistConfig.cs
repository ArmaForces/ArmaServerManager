using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Arma.Server.Config {
    public class ModlistConfig : IConfig {
        private readonly IConfig _serverConfig;
        private readonly string _modlistConfigDirPath;
        private readonly string _modlistName;
        private readonly ISettings _settings;

        public ModlistConfig(ISettings settings, string modlistName) {
            _settings = settings;
            _serverConfig = new ServerConfig(_settings);
            _serverConfig.LoadConfig();
            _modlistName = modlistName;
            _modlistConfigDirPath = CreateModlistConfigDirPath();
        }

        public string GetConfigDir() {
            return _modlistConfigDirPath;
        }

        private string CreateModlistConfigDirPath() {
            return Path.Join(_serverConfig.GetConfigDir(), "modlistConfigs", _modlistName);
        }

        public string GetServerCfgPath() {
            return Path.Join(_modlistConfigDirPath, "server.cfg");
        }

        public string GetBasicCfgPath() {
            return Path.Join(_modlistConfigDirPath, "basic.cfg");
        }

        public string GetServerProfileDir() {
            return Path.Join(_modlistConfigDirPath, "profiles", "server");
        }

        public string GetHCProfileDir() {
            return Path.Join(_modlistConfigDirPath, "profiles", "HC");
        }

        public Result LoadConfig() {
            return GetOrCreateModlistConfigDir(_serverConfig.GetConfigDir(), _modlistName)
                .Bind(() => PrepareModlistConfig(_modlistConfigDirPath, _modlistName));
        }

        /// <summary>
        /// Prepares config directory and files for current modlist
        /// </summary>
        /// <returns>path to modlistConfig</returns>
        private Result GetOrCreateModlistConfigDir(string serverConfigDir, string modlistName) {
            // Get modlist config directory based on serverConfig
            var modlistConfigDir = Path.Join(serverConfigDir, "modlistConfigs", modlistName);

            // Check for directory if present
            if (!Directory.Exists(modlistConfigDir)) {
                Directory.CreateDirectory(modlistConfigDir);
            }

            // Prepare modlist specific config.json if not exists
            if (!File.Exists(Path.Join(modlistConfigDir, "config.json"))) {
                // Set hostName according to pattern
                var sampleServer = new Dictionary<string, string>();
                sampleServer.Add("hostName", $"ArmaForces {modlistName} edition");
                var sampleJSON = new Dictionary<string, Dictionary<string, string>>();
                sampleJSON.Add("server", sampleServer);
                // Write to file
                using (StreamWriter file = File.CreateText(Path.Join(modlistConfigDir, "config.json"))) {
                    var serializerOptions = new JsonSerializerOptions();
                    serializerOptions.WriteIndented = true;
                    var serializedJson = JsonSerializer.Serialize(sampleJSON, serializerOptions);
                    file.Write(serializedJson);
                }
            }

            return Result.Success();
        }

        /// <summary>
        /// Prepares modlist cfg files for server to load.
        /// </summary>
        private Result PrepareModlistConfig(string modlistConfigDir, string modlistName) {
            // Apply modlist config on top of default config
            var serverConfigDir = Path.GetFullPath(Path.Join("..", ".."), modlistConfigDir);
            var modlistConfig = new ConfigurationBuilder()
                .AddJsonFile(Path.Join(serverConfigDir, "common.json"))
                .AddJsonFile(Path.Join(modlistConfigDir, "config.json"))
                .Build();

            // Process configuration files for modlist
            var configs = new List<string> {"server", "basic"};
            foreach (var config in configs) {
                Console.WriteLine($"Loading {config}.cfg for {modlistName} modlist.");
                var cfgFile = ServerConfig.FillCfg(File.ReadAllText($"{serverConfigDir}\\{config}.cfg"),
                    modlistConfig.GetSection(config));
                File.WriteAllText(Path.Join(modlistConfigDir, $"{config}.cfg"), cfgFile);
                Console.WriteLine($"{config}.cfg successfully exported to {modlistConfigDir}");
            }

            return Result.Success();
        }
    }
}