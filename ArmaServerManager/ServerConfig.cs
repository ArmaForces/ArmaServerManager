using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Linq;
using CSharpFunctionalExtensions;

namespace ArmaServerManager {
    public class ServerConfig {
        private readonly ISettings _settings;
        private readonly string _modsetName;
        private readonly string _serverConfigDirPath;
        private readonly string _modsetConfigDirPath;

        /// <summary>
        /// Class prepares server configuration for given modset
        /// </summary>
        /// <param name="settings">Server Settings Object</param>
        /// <param name="modset">Modset object</param>
        public ServerConfig(ISettings settings, string modsetName) {
            _settings = settings;
            _modsetName = modsetName;

            // Initialize paths
            _serverConfigDirPath = CreateServerConfigDirPath();
            _modsetConfigDirPath = CreateModsetConfigDirPath();
        }

        public string GetServerConfigDir() {
            return _serverConfigDirPath;
        }

        public string GetModsetConfigDir() {
            return _modsetConfigDirPath;
        }

        public string GetServerCfgPath() {
            return Path.Join(_serverConfigDirPath, "server.cfg");
        }

        public string GetBasicCfgPath() {
            return Path.Join(_serverConfigDirPath, "basic.cfg");
        }

        public string GetServerProfileDir() {
            return Path.Join(_modsetConfigDirPath, "profiles", "server");
        }
        
        public string GetHCProfileDir() {
            return Path.Join(_modsetConfigDirPath, "profiles", "HC");
        }

        private string CreateServerConfigDirPath()
        {
            var serverPath = _settings.GetServerPath();
            var serverConfigDirName = _settings.GetSettingsValue("serverConfigDirName").ToString();
            return Path.Join(serverPath, serverConfigDirName);
        }

        private string CreateModsetConfigDirPath() {
            return Path.Join(_serverConfigDirPath, "modsetConfigs", _modsetName);
        }
        
        /// <summary>
        /// Handles preparation of all config files.
        /// </summary>
        /// <returns></returns>
        public Result LoadConfig() {
            Console.WriteLine("Loading ServerConfig.");
            return GetOrCreateServerConfigDir()
                .Bind(() => GetOrCreateModsetConfigDir(_serverConfigDirPath, _modsetName))
                .Bind(() => PrepareModsetConfig(_modsetConfigDirPath, _modsetName))
                .Tap(() => Console.WriteLine("ServerConfig loaded."))
                .OnFailure(e => Console.WriteLine("ServerConfig could not be loaded with {e}.", e));
        }

        /// <summary>
        /// Prepares serverConfig directory with files.
        /// </summary>
        /// <returns>path to serverConfig</returns>
        private Result GetOrCreateServerConfigDir() {
            var serverPath = _settings.GetServerPath();
            var serverConfigDirName = _settings.GetSettingsValue("serverConfigDirName").ToString();
            var serverConfigDir = Path.Join(serverPath, serverConfigDirName);
            if (!Directory.Exists(serverConfigDir)) {
                Console.WriteLine($"Config directory {serverConfigDirName} does not exists, creating.");
                Directory.CreateDirectory(serverConfigDir);
            }

            // Prepare files
            var filesList = new List<string>() {"basic.cfg", "server.cfg", "common.Arma3Profile", "common.json"};
            foreach (var fileName in filesList) {
                var destFileName = Path.Join(serverConfigDir, fileName);
                if (File.Exists(destFileName)) continue;
                Console.WriteLine($"{fileName} not found, copying.");
                File.Copy(Path.Join(Directory.GetCurrentDirectory(), $"example_{fileName}"), destFileName);
            }

            return Result.Success();
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
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(file, sampleJSON);
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
                var cfgFile = FillCfg(File.ReadAllText($"{serverConfigDir}\\{config}.cfg"),
                    modsetConfig.GetSection(config));
                File.WriteAllText(Path.Join(modsetConfigDir, $"{config}.cfg"), cfgFile);
                Console.WriteLine($"{config}.cfg successfully exported to {modsetConfigDir}");
            }

            return Result.Success();
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