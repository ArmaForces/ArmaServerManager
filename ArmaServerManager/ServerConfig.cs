using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace ArmaServerManager {
    public class ServerConfig {
        private readonly Settings _settings;
        private readonly Modset _modset;
        private string _serverConfigDir;
        private string _modsetConfigDir;

        /// <summary>
        /// Class prepares server configuration for given modset
        /// </summary>
        /// <param name="settings">Server Settings Object</param>
        /// <param name="modset">Modset object</param>
        public ServerConfig(Settings settings, Modset modset) {
            Console.WriteLine("Loading ServerConfig.");
            _settings = settings;
            _modset = modset;
            // Load config directory and create if it not exists
            PrepareCommonFiles();
            PrepareModsetConfig();

            Console.WriteLine("ServerConfig loaded.");
        }

        /// <summary>
        /// Prepares all config directories for server and modset.
        /// </summary>
        public void PrepareCommonFiles() {
            _serverConfigDir = GetOrCreateServerConfigDir();
            _modsetConfigDir = GetOrCreateModsetConfigDir();
        }

        /// <summary>
        /// Prepares serverConfig directory with files.
        /// </summary>
        /// <returns>path to serverConfig</returns>
        private string GetOrCreateServerConfigDir() {
            var serverPath = _settings.GetServerPath();
            var serverConfigDirName = _settings.GetSettingsValue("serverConfigDirName").ToString();
            var serverConfigDir = Path.Join(serverPath, serverConfigDirName);
            if (!Directory.Exists(serverConfigDir)) {
                Console.WriteLine($"Config directory {serverConfigDirName} does not exists, creating.");
                Directory.CreateDirectory(serverConfigDir);
            }

            // Prepare files
            var filesList = new List<string>() { "basic.cfg", "server.cfg", "common.Arma3Profile", "common.json" };
            foreach (var fileName in filesList) {
                var destFileName = Path.Join(_serverConfigDir, fileName);
                if (File.Exists(destFileName)) continue;
                Console.WriteLine($"{fileName} not found, copying.");
                File.Copy(Path.Join(Directory.GetCurrentDirectory(), $"example_{fileName}"), destFileName);
            }

            return serverConfigDir;
        }

        /// <summary>
        /// Prepares config directory and files for current modset
        /// </summary>
        /// <returns>path to modsetConfig</returns>
        private string GetOrCreateModsetConfigDir() {
            // Get modset config directory based on serverConfig
            var modsetConfigDir = Path.Join(_serverConfigDir, "modsetConfigs", _modset.GetName());

            // Check for directory if present
            if (!Directory.Exists(modsetConfigDir)) {
                Directory.CreateDirectory(modsetConfigDir);
            }

            // Prepare modset specific config.json if not exists
            if (!File.Exists(Path.Join(_modsetConfigDir, "config.json"))) {
                // Set hostName according to pattern
                var sampleServer = new Dictionary<string, string>();
                sampleServer.Add("hostName",
                    string.Format(Environment.GetEnvironmentVariable("hostNamePattern"), _modset.GetName()));
                var sampleJSON = new Dictionary<string, Dictionary<string, string>>();
                sampleJSON.Add("server", sampleServer);
                // Write to file
                using (StreamWriter file = File.CreateText(Path.Join(_modsetConfigDir, "config.json"))) {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(file, sampleJSON);
                }
            }

            return modsetConfigDir;
        }

        /// <summary>
        /// Prepares modset cfg files for server to load.
        /// </summary>
        private void PrepareModsetConfig() {
            // Apply modset config on top of default config
            var modsetConfig = new ConfigurationBuilder()
                .AddJsonFile(Path.Join(_serverConfigDir, "common.json"))
                .AddJsonFile(Path.Join(_modsetConfigDir, "config.json"))
                .Build();

            // Process configuration files for modset
            var configs = new List<string> {"server", "basic"};
            foreach (var config in configs) {
                Console.WriteLine($"Loading {config}.cfg for {_modset.GetName()} modset.");
                var cfgFile = FillCfg(File.ReadAllText($"{_serverConfigDir}\\{config}.cfg"),
                    modsetConfig.GetSection(config));
                File.WriteAllText(Path.Join(_modsetConfigDir, $"{config}.cfg"), cfgFile);
                Console.WriteLine($"{config}.cfg successfully exported to {_modsetConfigDir}");
            }
        }

        /// <summary>
        /// In given cfgFile string replaces values for keys present in given config. Returns filled string.
        /// </summary>
        public static string FillCfg(string cfgFile, IConfigurationSection config) {
            foreach (var section in config.GetChildren()) {
                var key = section.Key;
                var value = config.GetSection(key).GetChildren().ToList();
                // Replace default value in cfg with value from loaded configs
                if (value.Count != 0) {
                    // If value is array, it needs changing to string
                    var stringValue = String.Join(", ", value.Select(p => p.Value.ToString()));
                    cfgFile = ServerConfigReplacer.ReplaceValue(cfgFile, key, stringValue);
                } else {
                    cfgFile = ServerConfigReplacer.ReplaceValue(cfgFile, key, config[key]);
                }
            }

            return cfgFile;
        }
    }
}