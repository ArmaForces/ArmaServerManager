using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace ArmaServerManager {
    public class ServerConfig
    {
        private readonly Settings _settings;
        private readonly Modset _modset;
        private string _serverConfigDir;
        private IConfigurationRoot _commonConfig;
        private IConfigurationRoot _modsetConfig;

        /// <summary>
        /// Class prepares server configuration for given modset
        /// </summary>
        /// <param name="settings">Server Settings Object</param>
        /// <param name="modset">Modset object</param>
        public ServerConfig(Settings settings, Modset modset)
        {
            Console.WriteLine("Loading ServerConfig.");
            _settings = settings;
            _modset = modset;
            // Load config directory and create if it not exists
            PrepareServerConfig();
            PrepareModsetConfig();

            Console.WriteLine("ServerConfig loaded.");
        }

        /// <summary>
        /// Prepares common serverConfig directory and files
        /// </summary>
        private void PrepareServerConfig()
        {
            var serverPath = _settings.GetServerPath();
            var serverConfigDirName = _settings.GetSettingsValue("serverConfigDirName").ToString();
            _serverConfigDir = $"{serverPath}\\{serverConfigDirName}";
            if (!Directory.Exists(_serverConfigDir)) {
                Console.WriteLine($"Config directory {serverConfigDirName} does not exists, creating.");
                Directory.CreateDirectory(_serverConfigDir);
            }
            // Check if common server configs are in place
            var filesList = new List<string>() { "basic.cfg", "server.cfg", "common.Arma3Profile", "common.json" };
            foreach (var fileName in filesList)
            {
                var filePath = $"{_serverConfigDir}\\{fileName}";
                if (File.Exists(filePath)) continue;
                Console.WriteLine($"{fileName} not found, copying.");
                File.Copy($"{Directory.GetCurrentDirectory()}\\example_{fileName}", filePath);
            }
            // Load common config from JSON
            _commonConfig = new ConfigurationBuilder()
                .SetBasePath(_serverConfigDir)
                .AddJsonFile("common.json")
                .Build();
        }

        /// <summary>
        /// Prepares modset specific config directory and files.
        /// </summary>
        private void PrepareModsetConfig()
        {
            // Get modset config directory based on serverConfig
            var modsetConfigDir = _serverConfigDir + $"\\modsetConfigs\\{_modset.GetName()}";

            // Check for directory and files present
            if (!Directory.Exists(modsetConfigDir)) {
                Directory.CreateDirectory(modsetConfigDir);
            }
            if (!File.Exists($"{modsetConfigDir}\\config.json")) {
                // Set hostName according to pattern
                var sampleServer = new Dictionary<string, string>();
                sampleServer.Add("hostName", string.Format(Environment.GetEnvironmentVariable("hostNamePattern"), _modset.GetName()));
                var sampleJSON = new Dictionary<string, Dictionary<string, string>>();
                sampleJSON.Add("server", sampleServer);
                // Write to file
                using (StreamWriter file = File.CreateText($"{modsetConfigDir}\\config.json")) {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(file, sampleJSON);
                }
            }

            // Load modset specific config from JSON
            _modsetConfig = new ConfigurationBuilder()
                .SetBasePath(modsetConfigDir)
                .AddInMemoryCollection(_commonConfig.AsEnumerable())
                .AddJsonFile("config.json")
                .Build();
            var cfgFile = FillCfg(File.ReadAllText($"{_serverConfigDir}\\server.cfg"), _modsetConfig.GetSection("server"));
            File.WriteAllText($"{modsetConfigDir}\\server.cfg", cfgFile);
        }

        /// <summary>
        /// In given cfgFile string replaces values for keys present in given config. Returns filled string.
        /// </summary>
        private string FillCfg(string cfgFile, IConfigurationSection config) {
            foreach (var section in config.GetChildren()) {
                var key = section.Key;
                var value = section.Value;
            }
            return cfgFile;
        }
    }

}